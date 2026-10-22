"""Leitura/escrita minima de BVH + cinematica direta (FK) com numpy.

Suporta o formato dos BVH da conversao "MotionBuilder-friendly" do CMU (cgspeed):
raiz com 6 canais (posicao XYZ + rotacao em qualquer ordem) e juntas com 3 canais de rotacao.
Rotacoes sao guardadas internamente como quaternions (x, y, z, w) via scipy.
"""
from __future__ import annotations

from dataclasses import dataclass, field
from typing import List, Optional

import numpy as np
from scipy.spatial.transform import Rotation as R


@dataclass
class Joint:
    name: str
    parent: int  # -1 para a raiz
    offset: np.ndarray  # (3,)
    channels: List[str]
    end_site: Optional[np.ndarray] = None  # offset do End Site, se houver
    children: List[int] = field(default_factory=list)

    @property
    def rot_order(self) -> str:
        """Ordem das rotacoes como aparece nos canais, ex. 'ZYX'."""
        return "".join(c[0].upper() for c in self.channels if c.lower().endswith("rotation"))

    @property
    def has_position(self) -> bool:
        return any(c.lower().endswith("position") for c in self.channels)


@dataclass
class Motion:
    joints: List[Joint]
    frame_time: float
    root_pos: np.ndarray  # (F, 3)
    rot: np.ndarray  # (F, J, 4) quaternions locais (x, y, z, w)

    @property
    def num_frames(self) -> int:
        return self.root_pos.shape[0]

    @property
    def fps(self) -> float:
        return 1.0 / self.frame_time

    def index(self, name: str) -> int:
        for i, j in enumerate(self.joints):
            if j.name == name:
                return i
        raise KeyError(name)

    def copy(self) -> "Motion":
        return Motion(self.joints, self.frame_time, self.root_pos.copy(), self.rot.copy())

    def slice(self, start: int, end: int) -> "Motion":
        """Frames [start, end) — end exclusivo."""
        return Motion(self.joints, self.frame_time, self.root_pos[start:end].copy(), self.rot[start:end].copy())


# --------------------------------------------------------------------------- parse

def parse(path: str) -> Motion:
    with open(path, "r", encoding="utf-8", errors="replace") as f:
        tokens_lines = f.read().splitlines()

    joints: List[Joint] = []
    stack: List[int] = []
    i = 0
    pending_end = False
    while i < len(tokens_lines):
        line = tokens_lines[i].strip()
        i += 1
        if not line:
            continue
        parts = line.split()
        key = parts[0].upper()
        if key == "HIERARCHY":
            continue
        if key in ("ROOT", "JOINT"):
            parent = stack[-1] if stack else -1
            joints.append(Joint(parts[1], parent, np.zeros(3), []))
            if parent >= 0:
                joints[parent].children.append(len(joints) - 1)
            pending_end = False
        elif key == "END":
            pending_end = True
        elif key == "{":
            if pending_end:
                stack.append(-2)  # marcador de End Site
            else:
                stack.append(len(joints) - 1)
        elif key == "}":
            stack.pop()
            pending_end = False
        elif key == "OFFSET":
            off = np.array([float(v) for v in parts[1:4]])
            if stack and stack[-1] == -2:
                owner = [s for s in stack if s >= 0][-1]
                joints[owner].end_site = off
            else:
                joints[-1].offset = off
        elif key == "CHANNELS":
            joints[-1].channels = parts[2:2 + int(parts[1])]
        elif key == "MOTION":
            break

    # MOTION
    num_frames = 0
    frame_time = 1.0 / 30.0
    while i < len(tokens_lines):
        line = tokens_lines[i].strip()
        i += 1
        if line.lower().startswith("frames:"):
            num_frames = int(line.split(":")[1])
        elif line.lower().startswith("frame time:"):
            frame_time = float(line.split(":")[1])
            break
    data = []
    while i < len(tokens_lines) and len(data) < num_frames:
        line = tokens_lines[i].strip()
        i += 1
        if line:
            data.append([float(v) for v in line.split()])
    data = np.asarray(data, dtype=np.float64)

    F, J = data.shape[0], len(joints)
    root_pos = np.zeros((F, 3))
    rot = np.zeros((F, J, 4))
    col = 0
    for j, jt in enumerate(joints):
        pos = np.zeros((F, 3))
        euler = np.zeros((F, 3))
        k = 0
        for ch in jt.channels:
            c = ch.lower()
            axis = "xyz".index(c[0])
            if c.endswith("position"):
                pos[:, axis] = data[:, col]
            else:
                euler[:, k] = data[:, col]
                k += 1
            col += 1
        if jt.has_position and jt.parent < 0:
            root_pos = pos
        order = jt.rot_order
        if order:
            # BVH: rotacoes intrinsecas na ordem dos canais -> maiusculas no scipy
            rot[:, j] = R.from_euler(order, euler[:, :len(order)], degrees=True).as_quat()
        else:
            rot[:, j] = np.array([0, 0, 0, 1.0])
    return Motion(joints, frame_time, root_pos, rot)


# --------------------------------------------------------------------------- write

def write(motion: Motion, path: str, decimals: int = 5) -> None:
    out: List[str] = ["HIERARCHY"]
    fmt = "{:.%df}" % decimals

    def emit(j: int, depth: int) -> None:
        jt = motion.joints[j]
        ind = "\t" * depth
        out.append(f"{ind}{'ROOT' if jt.parent < 0 else 'JOINT'} {jt.name}")
        out.append(f"{ind}{{")
        out.append(f"{ind}\tOFFSET " + " ".join(fmt.format(v) for v in jt.offset))
        out.append(f"{ind}\tCHANNELS {len(jt.channels)} " + " ".join(jt.channels))
        for c in jt.children:
            emit(c, depth + 1)
        if jt.end_site is not None:
            out.append(f"{ind}\tEnd Site")
            out.append(f"{ind}\t{{")
            out.append(f"{ind}\t\tOFFSET " + " ".join(fmt.format(v) for v in jt.end_site))
            out.append(f"{ind}\t}}")
        out.append(f"{ind}}}")

    emit(0, 0)
    out.append("MOTION")
    out.append(f"Frames: {motion.num_frames}")
    out.append(f"Frame Time: {motion.frame_time:.7f}")

    F = motion.num_frames
    cols = []
    for j, jt in enumerate(motion.joints):
        order = jt.rot_order
        euler = R.from_quat(motion.rot[:, j]).as_euler(order, degrees=True) if order else np.zeros((F, 0))
        k = 0
        for ch in jt.channels:
            c = ch.lower()
            if c.endswith("position"):
                axis = "xyz".index(c[0])
                cols.append(motion.root_pos[:, axis] if jt.parent < 0 else np.full(F, jt.offset[axis]))
            else:
                cols.append(euler[:, k])
                k += 1
    mat = np.stack(cols, axis=1) if cols else np.zeros((F, 0))
    for row in mat:
        out.append(" ".join(fmt.format(v) for v in row))
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out) + "\n")


# --------------------------------------------------------------------------- FK

def forward_kinematics(motion: Motion):
    """Retorna (posicoes globais (F,J,3), rotacoes globais como scipy Rotation por junta)."""
    F, J = motion.num_frames, len(motion.joints)
    gpos = np.zeros((F, J, 3))
    grot: List[R] = [None] * J  # type: ignore
    for j, jt in enumerate(motion.joints):
        local = R.from_quat(motion.rot[:, j])
        if jt.parent < 0:
            gpos[:, j] = motion.root_pos + jt.offset
            grot[j] = local
        else:
            p = jt.parent
            gpos[:, j] = gpos[:, p] + grot[p].apply(jt.offset)
            grot[j] = grot[p] * local
    return gpos, grot
