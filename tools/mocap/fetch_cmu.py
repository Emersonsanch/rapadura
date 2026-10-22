"""Baixa trials do CMU Motion Capture Database (conversao BVH cgspeed) do espelho no GitHub.

Uso:
    python fetch_cmu.py 35_01 09_02 143_23          # salva em cmu/<trial>.bvh
    python fetch_cmu.py --search "punch"            # procura no indice (cmu/cmu-mocap-index.txt)

Espelho: https://github.com/una-dinosauria/cmu-mocap (data/<sujeito com 3 digitos>/<trial>.bvh)
Licenca: ver cmu/SOURCES.md (uso livre, inclusive comercial).
"""
from __future__ import annotations

import argparse
import os
import sys
import urllib.request

MIRROR = "https://raw.githubusercontent.com/una-dinosauria/cmu-mocap/master/data/{subject:03d}/{trial}.bvh"
HERE = os.path.dirname(os.path.abspath(__file__))
CMU_DIR = os.path.join(HERE, "cmu")


def fetch(trial: str, out_dir: str = CMU_DIR, overwrite: bool = False) -> str:
    subject = int(trial.split("_")[0])
    trial = f"{subject:02d}_{trial.split('_')[1]}"
    dst = os.path.join(out_dir, trial + ".bvh")
    if os.path.exists(dst) and not overwrite:
        return dst
    os.makedirs(out_dir, exist_ok=True)
    url = MIRROR.format(subject=subject, trial=trial)
    with urllib.request.urlopen(url, timeout=60) as r, open(dst, "wb") as f:
        f.write(r.read())
    return dst


def search(term: str) -> None:
    with open(os.path.join(CMU_DIR, "cmu-mocap-index.txt"), encoding="utf-8", errors="replace") as f:
        for line in f:
            if "\t" in line and term.lower() in line.lower():
                print(line.rstrip())


def main(argv=None) -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("trials", nargs="*")
    ap.add_argument("--search")
    ap.add_argument("--overwrite", action="store_true")
    a = ap.parse_args(argv)
    if a.search:
        search(a.search)
    for t in a.trials:
        print(fetch(t, overwrite=a.overwrite))
    return 0


if __name__ == "__main__":
    sys.exit(main())
