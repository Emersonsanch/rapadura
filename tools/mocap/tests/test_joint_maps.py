import json
import os
import sys

import numpy as np
import pytest
from scipy.spatial.transform import Rotation as R

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))

import bvh  # noqa: E402
import bvh_prep  # noqa: E402
import joint_maps  # noqa: E402


def _skeleton(names, frames=90, fps=30.0):
    """Corpo minimo em cm com os nomes dados (dict papel -> nome), andando para +Z com as pernas balancando."""
    rot3 = ["Zrotation", "Xrotation", "Yrotation"]
    spec = [  # (papel, pai, offset em cm)
        ("hips", None, (0, 95, 0)),
        ("spine", "hips", (0, 10, 0)), ("spine1", "spine", (0, 12, 0)), ("spine2", "spine1", (0, 12, 0)),
        ("neck", "spine2", (0, 15, 0)), ("head", "neck", (0, 10, 0)),
        ("lsh", "spine2", (5, 12, 0)), ("larm", "lsh", (12, 0, 0)), ("lfore", "larm", (28, 0, 0)), ("lhand", "lfore", (25, 0, 0)),
        ("rsh", "spine2", (-5, 12, 0)), ("rarm", "rsh", (-12, 0, 0)), ("rfore", "rarm", (-28, 0, 0)), ("rhand", "rfore", (-25, 0, 0)),
        ("lup", "hips", (9, -5, 0)), ("lleg", "lup", (0, -42, 0)), ("lfoot", "lleg", (0, -40, 0)), ("ltoe", "lfoot", (0, -6, 12)),
        ("rup", "hips", (-9, -5, 0)), ("rleg", "rup", (0, -42, 0)), ("rfoot", "rleg", (0, -40, 0)), ("rtoe", "rfoot", (0, -6, 12)),
    ]
    roles = [s[0] for s in spec if s[0] in names]
    joints = []
    for role, parent, off in spec:
        if role not in names:
            continue
        p = roles.index(parent) if parent else -1
        ch = (["Xposition", "Yposition", "Zposition"] if p < 0 else []) + rot3
        joints.append(bvh.Joint(names[role], p, np.array(off, float), ch,
                                end_site=np.array([0, 5.0, 0]) if role in ("head", "lhand", "rhand", "ltoe", "rtoe") else None))
        if p >= 0:
            joints[p].children.append(len(joints) - 1)
    t = np.arange(frames)
    swing = 25 * np.sin(2 * np.pi * t / 30)
    rot = np.tile(R.identity().as_quat(), (frames, len(joints), 1))
    for role, sign in (("lup", 1), ("rup", -1)):
        if role in names:
            rot[:, roles.index(role)] = R.from_euler("X", sign * swing[:, None], degrees=True).as_quat()
    root = np.stack([np.zeros(frames), np.full(frames, 95.0), t * 4.0], axis=1)  # 1,2 m/s em cm a 30 fps
    return bvh.Motion(joints, 1 / fps, root, rot)


MIXAMO_NAMES = {
    "hips": "mixamorig:Hips", "spine": "mixamorig:Spine", "spine1": "mixamorig:Spine1", "spine2": "mixamorig:Spine2",
    "neck": "mixamorig:Neck", "head": "mixamorig:Head",
    "lsh": "mixamorig:LeftShoulder", "larm": "mixamorig:LeftArm", "lfore": "mixamorig:LeftForeArm", "lhand": "mixamorig:LeftHand",
    "rsh": "mixamorig:RightShoulder", "rarm": "mixamorig:RightArm", "rfore": "mixamorig:RightForeArm", "rhand": "mixamorig:RightHand",
    "lup": "mixamorig:LeftUpLeg", "lleg": "mixamorig:LeftLeg", "lfoot": "mixamorig:LeftFoot", "ltoe": "mixamorig:LeftToeBase",
    "rup": "mixamorig:RightUpLeg", "rleg": "mixamorig:RightLeg", "rfoot": "mixamorig:RightFoot", "rtoe": "mixamorig:RightToeBase",
}

ROKOKO_NAMES = {
    "hips": "Hips", "spine": "Spine1", "spine1": "Spine2", "spine2": "Spine4",
    "neck": "Neck", "head": "Head",
    "lsh": "LeftShoulder", "larm": "LeftArm", "lfore": "LeftForeArm", "lhand": "LeftHand",
    "rsh": "RightShoulder", "rarm": "RightArm", "rfore": "RightForeArm", "rhand": "RightHand",
    "lup": "LeftThigh", "lleg": "LeftShin", "lfoot": "LeftFoot", "ltoe": "LeftToe",
    "rup": "RightThigh", "rleg": "RightShin", "rfoot": "RightFoot", "rtoe": "RightToe",
}


def test_mixamo_names_become_cmu_and_keep_the_motion(tmp_path):
    src = tmp_path / "mixamo.bvh"
    bvh.write(_skeleton(MIXAMO_NAMES), str(src))
    out = tmp_path / "out.bvh"
    info = bvh_prep.process(str(src), str(out), joint_map="mixamo", unit_scale=0.01, tpose_frame0=False,
                            cycle=True, min_len=0.8, max_len=1.2, in_place=True, facing="travel")
    assert "missing_joints" not in info
    m = bvh.parse(str(out))
    names = [j.name for j in m.joints]
    for n in joint_maps.CMU_NAMES:
        assert names.count(n) == 1, n
    # Coluna do Mixamo sobe uma posicao: Spine -> LowerBack, Spine1 -> Spine, Spine2 -> Spine1 (pai dos ombros).
    assert m.joints[m.joints[m.index("LeftShoulder")].parent].name == "Spine1"
    assert m.joints[m.index("LowerBack")].parent == m.index("Hips")
    # Loop de 1 s (periodo do balanco), em metros, com o frame 0 neutro inserido.
    assert info["frames"] == 31
    g, _ = bvh.forward_kinematics(m.slice(0, 1))
    assert 0.8 < g[0, m.index("Hips"), 1] < 1.1


def test_rokoko_preset_maps_thigh_shin_and_upper_vertebra():
    m = _skeleton(ROKOKO_NAMES, frames=4)
    missing = joint_maps.rename_joints(m, joint_maps.load_map("rokoko"))
    assert missing == []
    names = [j.name for j in m.joints]
    assert "LeftUpLeg" in names and "RightLeg" in names and "LeftToeBase" in names
    assert m.joints[m.index("Neck")].parent == m.index("Spine1")  # Spine4 virou Spine1


def test_unmapped_joint_with_a_target_name_is_moved_out_of_the_way():
    # Esqueleto tipo Mixamo sem prefixo: "Spine" original vira LowerBack; nenhum nome CMU pode repetir.
    names = {k: v.split(":")[1] for k, v in MIXAMO_NAMES.items()}
    m = _skeleton(names, frames=2)
    joint_maps.rename_joints(m, {"Spine1": "Spine"})  # mapa parcial de proposito
    got = [j.name for j in m.joints]
    assert got.count("Spine") == 1 and "src_Spine" in got


def test_json_map_is_validated(tmp_path):
    good = tmp_path / "map.json"
    good.write_text(json.dumps({"pelvis": "Hips", "thigh_l": "LeftUpLeg"}), encoding="utf-8")
    assert joint_maps.load_map(str(good))["pelvis"] == "Hips"
    bad = tmp_path / "bad.json"
    bad.write_text(json.dumps({"pelvis": "Pelvis"}), encoding="utf-8")
    with pytest.raises(ValueError):
        joint_maps.load_map(str(bad))
    with pytest.raises(ValueError):
        joint_maps.load_map("deepmotion-que-nao-existe")


def test_cmu_files_pass_through_unchanged():
    m = _skeleton({k: v.split(":")[1] for k, v in MIXAMO_NAMES.items()}, frames=2)
    before = [j.name for j in m.joints]
    joint_maps.rename_joints(m, joint_maps.load_map("cmu"))
    assert [j.name for j in m.joints] == before
