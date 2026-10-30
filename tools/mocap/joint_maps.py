"""Renomeia juntas de esqueletos que nao sao do CMU para os nomes CMU/cgspeed que o importador do Unity espera.

O importador (`MocapMath.CmuLinks`) so conhece estes nomes:
    Hips, LowerBack, Spine, Spine1, Neck, Head,
    Left/RightShoulder, Left/RightArm, Left/RightForeArm, Left/RightHand,
    Left/RightUpLeg, Left/RightLeg, Left/RightFoot, Left/RightToeBase

Mapas prontos ("--joint-map mixamo" etc.) cobrem os nomes mais comuns. Os nomes exatos variam com a versao do
programa e com o perfil de exportacao: confira a HIERARCHY do seu BVH e, se precisar, passe um JSON proprio
({"nome no arquivo": "nome CMU", ...}) com "--joint-map caminho.json". Juntas sem mapeamento ficam com o nome
original e o importador as ignora (dedos, pontas, ossos extras da coluna).
"""
from __future__ import annotations

import json
import os
import re
from typing import Dict, List

import bvh

# Nomes CMU usados pelo importador do Unity.
CMU_NAMES = (
    "Hips", "LowerBack", "Spine", "Spine1", "Neck", "Head",
    "LeftShoulder", "LeftArm", "LeftForeArm", "LeftHand",
    "RightShoulder", "RightArm", "RightForeArm", "RightHand",
    "LeftUpLeg", "LeftLeg", "LeftFoot", "LeftToeBase",
    "RightUpLeg", "RightLeg", "RightFoot", "RightToeBase",
)


def _sided(table: Dict[str, str]) -> Dict[str, str]:
    """Expande chaves com '{S}' para Left/Right (valor com '{S}' tambem)."""
    out: Dict[str, str] = {}
    for k, v in table.items():
        if "{S}" in k:
            for side in ("Left", "Right"):
                out[k.replace("{S}", side)] = v.replace("{S}", side)
        else:
            out[k] = v
    return out


# Mixamo (e DeepMotion/Plask quando exportam "Mixamo compatible"): Spine/Spine1/Spine2 sobem a coluna;
# o prefixo "mixamorig:" e removido antes de procurar no mapa.
MIXAMO = _sided({
    "Hips": "Hips", "Spine": "LowerBack", "Spine1": "Spine", "Spine2": "Spine1",
    "Neck": "Neck", "Head": "Head",
    "{S}Shoulder": "{S}Shoulder", "{S}Arm": "{S}Arm", "{S}ForeArm": "{S}ForeArm", "{S}Hand": "{S}Hand",
    "{S}UpLeg": "{S}UpLeg", "{S}Leg": "{S}Leg", "{S}Foot": "{S}Foot", "{S}ToeBase": "{S}ToeBase",
})

# Rokoko Studio / Rokoko Video (perfil "Rokoko"/"Newton"): coxa/canela e quatro vertebras. Spine3 fica sem
# mapa: o importador usa rotacoes de mundo, entao a vertebra pulada ja entra na de cima (Spine4, onde nascem
# pescoco e ombros). Conferir no arquivo: algumas versoes exportam nomes estilo Mixamo (use "mixamo" entao).
ROKOKO = _sided({
    "Hips": "Hips", "Spine1": "LowerBack", "Spine2": "Spine", "Spine4": "Spine1",
    "Neck": "Neck", "Head": "Head",
    "{S}Shoulder": "{S}Shoulder", "{S}Arm": "{S}Arm", "{S}ForeArm": "{S}ForeArm", "{S}Hand": "{S}Hand",
    "{S}Thigh": "{S}UpLeg", "{S}Shin": "{S}Leg", "{S}Foot": "{S}Foot", "{S}Toe": "{S}ToeBase",
})

# Nomes genericos de "Unity Humanoid"/Blender-Rigify-like (UpperLeg/LowerLeg, UpperArm/LowerArm, Chest).
HUMANOID = _sided({
    "Hips": "Hips", "Spine": "LowerBack", "Chest": "Spine", "UpperChest": "Spine1",
    "Neck": "Neck", "Head": "Head",
    "{S}Shoulder": "{S}Shoulder", "{S}UpperArm": "{S}Arm", "{S}LowerArm": "{S}ForeArm", "{S}Hand": "{S}Hand",
    "{S}UpperLeg": "{S}UpLeg", "{S}LowerLeg": "{S}Leg", "{S}Foot": "{S}Foot", "{S}Toes": "{S}ToeBase",
})

PRESETS = {"cmu": {}, "mixamo": MIXAMO, "rokoko": ROKOKO, "humanoid": HUMANOID}

_PREFIX = re.compile(r"^(mixamorig\d*[:_]|[A-Za-z0-9]+:)")


def load_map(spec: str) -> Dict[str, str]:
    """`spec` e um preset (cmu, mixamo, rokoko, humanoid) ou o caminho de um JSON {"origem": "CMU"}."""
    if spec in PRESETS:
        return dict(PRESETS[spec])
    if os.path.exists(spec):
        with open(spec, "r", encoding="utf-8") as f:
            data = json.load(f)
        bad = [v for v in data.values() if v not in CMU_NAMES]
        if bad:
            raise ValueError(f"mapa {spec}: destinos que o importador nao conhece: {bad}")
        return data
    raise ValueError(f"--joint-map: '{spec}' nao e preset ({', '.join(PRESETS)}) nem arquivo JSON")


def rename_joints(motion: bvh.Motion, mapping: Dict[str, str]) -> List[str]:
    """Renomeia as juntas no lugar e retorna os nomes CMU que faltaram.

    Quando dois nomes de origem caem no mesmo destino (num mapa JSON proprio), fica o primeiro na hierarquia. Uma junta nao mapeada cujo nome coincide com um destino ganha o prefixo "src_" (senao o
    importador acharia dois "Spine")."""
    if mapping:
        new_names = []
        used = set()
        for jt in motion.joints:
            target = mapping.get(_PREFIX.sub("", jt.name), mapping.get(jt.name))
            if target is not None and target not in used:
                used.add(target)
                new_names.append(target)
            else:
                new_names.append(None)
        for jt, new in zip(motion.joints, new_names):
            if new is not None:
                jt.name = new
            elif jt.name in used:
                jt.name = "src_" + jt.name
    dup = [n for n in CMU_NAMES if sum(1 for j in motion.joints if j.name == n) > 1]
    if dup:
        raise ValueError(f"juntas com nome repetido depois do mapeamento: {dup}")
    names = {j.name for j in motion.joints}
    return [n for n in CMU_NAMES if n not in names]
