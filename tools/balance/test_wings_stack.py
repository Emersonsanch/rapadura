"""pytest for the wings + Frutas do Sertão layer of the power-stacking check (Docs/Balance/README.md "Tacho Encantado e asas").

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
def wings():
    return ps.load_wings()


@pytest.fixture(scope="module")
def results(ctx, gear, wings):
    refine = ps.load_refine()
    plain = {c: ps.evaluate(ctx, c, RUNS, gear) for c in (0, 1)}
    refined = {c: ps.evaluate(ctx, c, RUNS, ps.refined_gear(gear, refine)) for c in (0, 1)}
    winged = {c: ps.evaluate(ctx, c, RUNS, ps.winged_gear(gear, refine, wings, c)) for c in (0, 1)}
    return plain, refined, winged


def test_wings_are_parsed_from_the_catalog(wings):
    assert set(wings.styles) == {"Carcara", "CapaVaqueiro", "BeijaFlor", "AsaBranca", "VagaLume"}
    assert wings.level_scale[:4] == [0.0, 1.0, 1.25, 1.5]
    assert wings.absorb[1] == 0 and 0 < wings.absorb[2] < wings.absorb[3]
    assert wings.fruit_cap == 2
    assert wings.arcane_pct == pytest.approx(0.01)
    # Small bonuses: at level 3 the best style adds 2.25% damage, about 1/3 of a +9 refine weapon with option 2 (6.2%).
    assert wings.damage(3) == pytest.approx(0.0225)
    assert wings.gear(0, 0).ad_pct == 0 and wings.gear(0, 0).defense_pct == 0


def test_wings_come_late_and_grow_the_deeper_the_region():
    levels = [ps.WINGS_BY_REGION[b] for b in rd.BIOME_ORDER]
    fruits = [ps.FRUITS_BY_REGION[b] for b in rd.BIOME_ORDER]
    assert levels == sorted(levels) and fruits == sorted(fruits)
    assert levels[0] == 0 and levels[1] == 0, "no Pedra do Caos before the Montanha"
    assert ps.WINGS_NG1 == 3


def test_wings_never_make_a_fight_longer(results):
    _, refined, winged = results
    for cycle in refined:
        for biome in refined[cycle]:
            assert ps.biome_summary(winged[cycle], biome)["r8_boss"] <= ps.biome_summary(refined[cycle], biome)["r8_boss"] + 1e-6


@pytest.mark.parametrize("cycle", [0, 1])
def test_wings_stay_within_their_budget(results, cycle):
    plain, refined, winged = results
    assert ps.wings_violations(plain[cycle], refined[cycle], winged[cycle], cycle) == []
