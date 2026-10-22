"""Pre-processa um BVH do CMU (conversao cgspeed) para importacao no Unity.

Uso (um clipe):
    python bvh_prep.py cmu/35_01.bvh --out out/walk.bvh --cycle --min-len 0.9 --max-len 1.4 --in-place

Uso (conjunto inteiro definido em clips.json):
    python bvh_prep.py --set clips.json --out-dir out

Etapas: separa o frame 0 (T-pose da conversao cgspeed) -> escala p/ metros -> corte (start/end em s, no
tempo original) -> velocidade -> reamostragem p/ 30 fps -> janela de loop (opcional) -> orienta p/ +Z ->
pes no chao -> remove deslocamento XZ (in-place, opcional) -> recoloca a T-pose no frame 0 -> grava BVH.
"""
from __future__ import annotations

import argparse
import json
import os
import sys

import numpy as np

import bvh
import prep


def process(src: str, out: str, *, start=None, end=None, fps=30.0, speed=1.0, cycle=False,
            min_len=0.8, max_len=2.0, in_place=False, facing="start", tpose_frame0=True,
            unit_scale=prep.CMU_UNIT_TO_METERS) -> dict:
    m = bvh.parse(src)
    m = prep.scale(m, unit_scale)
    if tpose_frame0:
        tpose, motion = m.slice(0, 1), m.slice(1, m.num_frames)
    else:
        tpose = m.slice(0, 1)
        tpose.rot[:] = np.array([0, 0, 0, 1.0])
        tpose.root_pos[:] = m.root_pos[:1]
        motion = m

    motion = prep.trim(motion, start, end)
    if speed and abs(speed - 1.0) > 1e-6:
        motion.frame_time /= speed
    motion = prep.resample(motion, fps)

    info = {"source_frames": int(m.num_frames)}
    if cycle:
        i, j, cost = prep.find_loop(motion, min_len, max_len)
        motion = motion.slice(i, j + 1)
        info.update(loop_start_frame=i, loop_end_frame=j, loop_cost=round(cost, 4))

    motion = prep.face_forward(motion, "travel" if facing == "travel" else "start")
    motion = prep.ground(motion)

    if in_place:
        F = motion.num_frames
        a, b = motion.root_pos[0, [0, 2]], motion.root_pos[-1, [0, 2]]
        motion.root_pos[:, [0, 2]] -= np.linspace(a, b, F) - a  # mantem o balanco lateral, remove a deriva

    tpose = prep.ground(tpose)
    tpose.root_pos[:, [0, 2]] = 0.0
    tpose.frame_time = motion.frame_time

    final = bvh.Motion(motion.joints, motion.frame_time,
                       np.concatenate([tpose.root_pos, motion.root_pos]),
                       np.concatenate([tpose.rot, motion.rot]))
    os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
    bvh.write(final, out)
    info.update(frames=int(motion.num_frames), fps=fps, duration=round((motion.num_frames - 1) / fps, 3))
    return info


def run_set(cfg_path: str, out_dir: str) -> None:
    base = os.path.dirname(os.path.abspath(cfg_path))
    with open(cfg_path, "r", encoding="utf-8") as f:
        cfg = json.load(f)
    manifest = {"fps": cfg.get("fps", 30), "clips": []}
    for c in cfg["clips"]:
        src = os.path.join(base, c["source"])
        out = os.path.join(out_dir, c["name"] + ".bvh")
        info = process(src, out, start=c.get("start"), end=c.get("end"), fps=cfg.get("fps", 30),
                       speed=c.get("speed", 1.0), cycle=c.get("cycle", False),
                       min_len=c.get("min_len", 0.8), max_len=c.get("max_len", 2.0),
                       in_place=c.get("in_place", False), facing=c.get("facing", "start"))
        entry = {
            "name": c["name"],
            "file": c["name"] + ".bvh",
            "loop": bool(c.get("loop", False)),
            "inPlace": bool(c.get("in_place", False)),
            "rootMotion": bool(c.get("root_motion", False)),
            "tposeFrame0": True,
            "source": c["source"],
            "cmuTrial": c.get("cmu"),
            "description": c.get("description", ""),
            **info,
        }
        manifest["clips"].append(entry)
        print(f"{c['name']:<16} {info['duration']:5.2f}s  frames={info['frames']:4d}"
              + (f"  loop_cost={info['loop_cost']}" if 'loop_cost' in info else ""))
    with open(os.path.join(out_dir, "manifest.json"), "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)
    print(f"manifest: {os.path.join(out_dir, 'manifest.json')}")


def main(argv=None) -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("src", nargs="?", help="BVH de entrada")
    ap.add_argument("--out", help="BVH de saida")
    ap.add_argument("--set", help="clips.json com o conjunto a processar")
    ap.add_argument("--out-dir", default="out")
    ap.add_argument("--start", type=float)
    ap.add_argument("--end", type=float)
    ap.add_argument("--fps", type=float, default=30.0)
    ap.add_argument("--speed", type=float, default=1.0)
    ap.add_argument("--cycle", action="store_true", help="procura a melhor janela de loop")
    ap.add_argument("--min-len", type=float, default=0.8)
    ap.add_argument("--max-len", type=float, default=2.0)
    ap.add_argument("--in-place", action="store_true")
    ap.add_argument("--facing", choices=["start", "travel"], default="start")
    ap.add_argument("--no-tpose", action="store_true", help="o frame 0 NAO e uma T-pose (BVH que nao e cgspeed)")
    ap.add_argument("--unit-scale", type=float, default=prep.CMU_UNIT_TO_METERS,
                    help="fator unidade->metros (CMU: 0.05644; BVH ja em cm: 0.01)")
    a = ap.parse_args(argv)
    if a.set:
        run_set(a.set, a.out_dir)
        return 0
    if not a.src or not a.out:
        ap.error("informe <src> e --out, ou --set clips.json")
    info = process(a.src, a.out, start=a.start, end=a.end, fps=a.fps, speed=a.speed, cycle=a.cycle,
                   min_len=a.min_len, max_len=a.max_len, in_place=a.in_place, facing=a.facing,
                   tpose_frame0=not a.no_tpose, unit_scale=a.unit_scale)
    print(json.dumps(info))
    return 0


if __name__ == "__main__":
    sys.exit(main())
