"""Pre-processamento de clipes de mocap: corte, reamostragem, escala, orientacao e deteccao de loop."""
from __future__ import annotations

from typing import Optional, Tuple

import numpy as np
from scipy.spatial.transform import Rotation as R
from scipy.spatial.transform import Slerp

from bvh import Motion, forward_kinematics

# CMU ASF/AMC usa "length unit" = (1/0.45) polegada -> metros
CMU_UNIT_TO_METERS = 0.0254 / 0.45


def scale(motion: Motion, factor: float) -> Motion:
    """Escala offsets e posicao da raiz (ex.: unidades CMU -> metros)."""
    joints = []
    for jt in motion.joints:
        nj = type(jt)(jt.name, jt.parent, jt.offset * factor, list(jt.channels),
                      None if jt.end_site is None else jt.end_site * factor, list(jt.children))
        joints.append(nj)
    return Motion(joints, motion.frame_time, motion.root_pos * factor, motion.rot.copy())


def resample(motion: Motion, fps: float) -> Motion:
    """Reamostra para `fps` (lerp na posicao da raiz, slerp nas rotacoes)."""
    F = motion.num_frames
    if F < 2:
        return motion.copy()
    t_src = np.arange(F) * motion.frame_time
    duration = t_src[-1]
    n = int(np.floor(duration * fps + 1e-6)) + 1
    t_dst = np.arange(n) / fps
    t_dst = np.clip(t_dst, 0, duration)
    root = np.stack([np.interp(t_dst, t_src, motion.root_pos[:, k]) for k in range(3)], axis=1)
    rot = np.zeros((n, motion.rot.shape[1], 4))
    for j in range(motion.rot.shape[1]):
        rot[:, j] = Slerp(t_src, R.from_quat(motion.rot[:, j]))(t_dst).as_quat()
    return Motion(motion.joints, 1.0 / fps, root, rot)


def trim(motion: Motion, start_s: Optional[float], end_s: Optional[float]) -> Motion:
    a = 0 if start_s is None else int(round(start_s / motion.frame_time))
    b = motion.num_frames if end_s is None else int(round(end_s / motion.frame_time)) + 1
    a = max(0, min(a, motion.num_frames - 1))
    b = max(a + 1, min(b, motion.num_frames))
    return motion.slice(a, b)


def hips_yaw(motion: Motion) -> np.ndarray:
    """Yaw (rad) do quadril por frame, medido pelo eixo +Z local projetado no plano XZ."""
    fwd = R.from_quat(motion.rot[:, 0]).apply([0, 0, 1])
    return np.arctan2(fwd[:, 0], fwd[:, 2])


def face_forward(motion: Motion, mode: str = "start", ref_frames: int = 10) -> Motion:
    """Gira o clipe inteiro em torno de Y para que o personagem olhe/ande para +Z.

    mode="start": usa o yaw medio dos primeiros frames do quadril.
    mode="travel": usa a direcao de deslocamento (inicio->fim) da raiz — bom para locomocao.
    Tambem recentra a raiz em XZ = 0 no primeiro frame.
    """
    m = motion.copy()
    if mode == "travel":
        d = m.root_pos[-1] - m.root_pos[0]
        if np.hypot(d[0], d[2]) < 1e-3:
            mode = "start"
        else:
            yaw = np.arctan2(d[0], d[2])
    if mode == "start":
        y = hips_yaw(m)[:ref_frames]
        yaw = np.arctan2(np.mean(np.sin(y)), np.mean(np.cos(y)))
    fix = R.from_euler("Y", np.atleast_1d(-yaw)[:, None]) if np.ndim(yaw) else R.from_euler("Y", -yaw)
    origin = m.root_pos[0].copy()
    origin[1] = 0.0
    m.root_pos = fix.apply(m.root_pos - origin)
    m.rot[:, 0] = (fix * R.from_quat(m.rot[:, 0])).as_quat()
    return m


def ground(motion: Motion, foot_joints=("LeftToeBase", "RightToeBase", "LeftFoot", "RightFoot")) -> Motion:
    """Desloca Y para que o ponto mais baixo dos pes (percentil 5) fique em y=0."""
    gpos, _ = forward_kinematics(motion)
    idx = [motion.index(n) for n in foot_joints if any(j.name == n for j in motion.joints)]
    if not idx:
        return motion
    low = np.percentile(gpos[:, idx, 1].min(axis=1), 5)
    m = motion.copy()
    m.root_pos[:, 1] -= low
    return m


# --------------------------------------------------------------------------- loop

def _canon(q: np.ndarray) -> np.ndarray:
    """Hemisferio canonico (w >= 0) para comparar quaternions."""
    return np.where(q[..., 3:4] < 0, -q, q)


def pose_features(motion: Motion) -> Tuple[np.ndarray, np.ndarray]:
    """(rotacoes locais sem a raiz (F, J-1, 4) com yaw da raiz removido, velocidade local da raiz (F,3))."""
    yaw = hips_yaw(motion)
    inv_yaw = R.from_euler("Y", np.atleast_1d(-yaw)[:, None]) if np.ndim(yaw) else R.from_euler("Y", -yaw)
    root_local = (inv_yaw * R.from_quat(motion.rot[:, 0])).as_quat()
    rots = motion.rot.copy()
    rots[:, 0] = root_local
    vel = np.gradient(motion.root_pos, axis=0) / motion.frame_time
    vel_local = inv_yaw.apply(vel)
    return _canon(rots), vel_local


def find_loop(motion: Motion, min_len_s: float, max_len_s: float,
              vel_weight: float = 0.15, search: Optional[Tuple[float, float]] = None) -> Tuple[int, int, float]:
    """Acha a janela [i, j] (j inclusivo, frame j ~= frame i) que melhor fecha um ciclo.

    Custo = angulo medio entre poses (rad) + vel_weight * |dif. de velocidade da raiz| (m/s)
            + mesmo custo para os frames vizinhos (i+1, j+1) -> casa tambem a "direcao" do movimento.
    Retorna (i, j, custo).
    """
    rots, vel = pose_features(motion)
    F = motion.num_frames
    fps = motion.fps
    lo = max(2, int(round(min_len_s * fps)))
    hi = max(lo, int(round(max_len_s * fps)))
    a0, a1 = 0, F - 1
    if search is not None:
        a0 = max(0, int(round(search[0] * fps)))
        a1 = min(F - 1, int(round(search[1] * fps)))
    best = (0, min(F - 1, lo), np.inf)
    flat = rots.reshape(F, -1, 4)
    for i in range(a0, a1 - lo):
        js = np.arange(i + lo, min(i + hi, a1 - 1) + 1)
        if js.size == 0:
            break
        dots = np.abs(np.einsum("jk,njk->nj", flat[i], flat[js])).clip(0, 1)
        ang = 2 * np.arccos(dots).mean(axis=1)
        dv = np.linalg.norm(vel[js] - vel[i], axis=1)
        cost = ang + vel_weight * dv
        if i + 1 < F:
            js1 = np.minimum(js + 1, F - 1)
            dots1 = np.abs(np.einsum("jk,njk->nj", flat[i + 1], flat[js1])).clip(0, 1)
            cost = cost + 2 * np.arccos(dots1).mean(axis=1)
        k = int(np.argmin(cost))
        if cost[k] < best[2]:
            best = (i, int(js[k]), float(cost[k]))
    return best
