import os
import sys

import numpy as np
from scipy.spatial.transform import Rotation as R

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))

import bvh  # noqa: E402
import prep  # noqa: E402

SAMPLE = os.path.join(os.path.dirname(HERE), "cmu", "35_01.bvh")


def _synthetic(period_frames=40, frames=200, fps=30.0):
    """Cadeia de 3 juntas cuja rotacao oscila com periodo conhecido."""
    joints = [bvh.Joint("Hips", -1, np.zeros(3), ["Xposition", "Yposition", "Zposition",
                                                     "Zrotation", "Yrotation", "Xrotation"]),
              bvh.Joint("LeftUpLeg", 0, np.array([0.1, -0.1, 0]), ["Zrotation", "Yrotation", "Xrotation"]),
              bvh.Joint("LeftFoot", 1, np.array([0, -0.8, 0]), ["Zrotation", "Yrotation", "Xrotation"],
                        end_site=np.array([0, 0, 0.1]))]
    joints[0].children = [1]
    joints[1].children = [2]
    t = np.arange(frames)
    ang = 30 * np.sin(2 * np.pi * t / period_frames)
    rot = np.zeros((frames, 3, 4))
    rot[:, 0] = R.identity(frames).as_quat()
    rot[:, 1] = R.from_euler("X", ang[:, None], degrees=True).as_quat()
    rot[:, 2] = R.from_euler("X", -ang[:, None] * 0.5, degrees=True).as_quat()
    root = np.stack([np.zeros(frames), np.full(frames, 0.9), t * 0.04], axis=1)
    return bvh.Motion(joints, 1 / fps, root, rot)


def test_find_loop_recovers_period():
    m = _synthetic(period_frames=40)
    i, j, cost = prep.find_loop(m, min_len_s=0.8, max_len_s=2.0)
    assert (j - i) % 40 == 0, (i, j)
    assert cost < 1e-3


def test_resample_halves_frames():
    m = _synthetic(frames=121, fps=120.0)
    r = prep.resample(m, 30.0)
    assert r.num_frames == 31
    assert abs(r.frame_time - 1 / 30) < 1e-9
    np.testing.assert_allclose(r.root_pos[-1], m.root_pos[-1], atol=1e-9)


def test_face_forward_travel_aligns_plus_z():
    m = _synthetic()
    yaw = R.from_euler("Y", 70, degrees=True)
    m.root_pos = yaw.apply(m.root_pos)
    m.rot[:, 0] = (yaw * R.from_quat(m.rot[:, 0])).as_quat()
    f = prep.face_forward(m, "travel")
    d = f.root_pos[-1] - f.root_pos[0]
    assert abs(d[0]) < 1e-6 and d[2] > 0


def test_write_parse_roundtrip(tmp_path):
    m = _synthetic(frames=20)
    p = tmp_path / "x.bvh"
    bvh.write(m, str(p))
    back = bvh.parse(str(p))
    assert back.num_frames == 20
    g0, _ = bvh.forward_kinematics(m)
    g1, _ = bvh.forward_kinematics(back)
    np.testing.assert_allclose(g0, g1, atol=1e-4)


def test_cmu_sample_is_meter_scale():
    if not os.path.exists(SAMPLE):
        return
    m = prep.scale(bvh.parse(SAMPLE), prep.CMU_UNIT_TO_METERS)
    assert m.num_frames > 100 and abs(m.fps - 120) < 1
    g, _ = bvh.forward_kinematics(m.slice(1, 2))
    hips_h = g[0, 0, 1] - min(g[0, m.index("LeftToeBase"), 1], g[0, m.index("RightToeBase"), 1])
    assert 0.8 < hips_h < 1.2, hips_h  # altura do quadril de um adulto em metros
