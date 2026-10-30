"""pytest for the refine layer of the power-stacking check (Docs/Balance/README.md "Refino de item").

Run from the repo root:
    uv run --no-project --with pytest python -m pytest -q -p no:cacheprovider tools/balance
"""

from __future__ import annotations

import os
import sys

import pytest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import balance_model as bm  # noqa: E402
import power_stack as ps  # noqa: E402
import rapadura_data as rd  # noqa: E402

RUNS = 600


@pytest.fixture(scope="module")
def ctx():
    return ps.load_context()


@pytest.fixture(scope="module")
def gear(ctx):
    return bm.gear_by_region(bm.Model(ctx.data), "quests", RUNS)


@pytest.fixture(scope="module")
def curve():
    return ps.load_refine()


@pytest.fixture(scope="module")
def results(ctx, gear, curve):
    plain = {c: ps.evaluate(ctx, c, RUNS, gear) for c in (0, 1)}
    refined = {c: ps.evaluate(ctx, c, RUNS, ps.refined_gear(gear, curve)) for c in (0, 1)}
    return plain, refined


def test_curve_is_parsed_from_refine_rules(curve):
    assert len(curve.stat_bonus) == 16
    assert curve.stat_bonus[0] == 0 and curve.stat_bonus[15] == pytest.approx(1.0)
    assert all(b > a for a, b in zip(curve.stat_bonus, curve.stat_bonus[1:]))
    assert curve.weapon_share == pytest.approx(0.10)
    # Head, Chest, Boots of Resources/Items (leather: 4, 8, 3).
    assert sorted(curve.armour.values()) == [3.0, 4.0, 8.0]
    assert curve.refined(18, 9) == pytest.approx(25.6)


def test_refine_gear_grows_with_the_level(curve):
    low, high = curve.gear(4, 0), curve.gear(9, 2)
    assert 0 < low.ad_pct < high.ad_pct
    assert 0 < low.defense < high.defense
    assert low.max_hp_pct == 0 and high.max_hp_pct > 0


def test_regions_refine_more_the_deeper_they_are():
    levels = [ps.REFINE_BY_REGION[b] for b in rd.BIOME_ORDER]
    assert levels == sorted(levels)
    assert levels[0][0] == 4 and levels[-1][0] == 9


def test_refine_never_makes_a_fight_longer(results):
    plain, refined = results
    for cycle in plain:
        for biome in plain[cycle]:
            assert ps.biome_summary(refined[cycle], biome)["r8_boss"] <= ps.biome_summary(plain[cycle], biome)["r8_boss"] + 1e-6


@pytest.mark.parametrize("cycle", [0, 1])
def test_refine_stays_within_its_budget(results, cycle):
    plain, refined = results
    assert ps.refine_violations(plain[cycle], refined[cycle], cycle) == []
