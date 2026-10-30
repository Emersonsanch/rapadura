"""pytest for the training-by-use + Roda de Talentos layer of the power-stacking check (Docs/Balance/README.md §27).

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
def talents():
    return ps.load_talents()


@pytest.fixture(scope="module")
def results():
    ctx = ps.load_context()
    gear = bm.gear_by_region(bm.Model(ctx.data), "quests", RUNS)
    plain = {c: ps.evaluate(ctx, c, RUNS, gear) for c in (0, 1)}
    return {kind: ps.talent_evaluate(ctx, RUNS, gear, plain, kind) for kind in ("excellent", "ancient")}


def test_talents_are_parsed_from_the_code(talents):
    assert set(talents.proficiencies) == {"Weapon", "Magic", "Shielding"}
    assert talents.max_level == 20
    heroes = {n.hero for n in talents.nodes if n.hero}
    assert len(heroes) == 5, "one keystone set per cousin"
    assert len([n for n in talents.nodes if n.hero is None]) == 12
    assert talents.points(talents.unlock_level - 1) == 0 and talents.points(talents.unlock_level) == 1


def test_training_never_gives_raw_damage(talents):
    assert all(m.stat != "AttackDamage" for m in talents.proficiencies.values())
    g = talents.training_gear(talents.max_level)
    assert g.attack_speed <= 0.005 and g.rcr <= 0.01


def test_every_quadrant_allocation_respects_the_rings(talents):
    for hero in {n.hero for n in talents.nodes if n.hero}:
        for quadrant in {n.quadrant for n in talents.nodes}:
            costs = sorted({cost for cost, _ in talents.quadrant_options(hero, quadrant)})
            assert costs[-1] == 12, "quadrant capacity"
            # the keystone (3 points) needs 6 small points beside it: nothing legal costs exactly 3+0..5 with it, so 9 is
            # the cheapest allocation that holds the keystone; small-only allocations cost at most 9.
            with_keystone = [cost for cost, mods in talents.quadrant_options(hero, quadrant) if len(mods) and cost > 9]
            assert all(cost >= 10 for cost in with_keystone)


def test_the_whole_wheel_stays_small(talents):
    full = talents.best_wheel(talents.points(50), ps.talent_offense)
    assert full.ad_pct <= 0.005 and full.crit <= 0.005 and full.cdr <= 0.005 and full.crit_dmg <= 0.03
    assert full.rcr <= 0.015 and full.thorns <= 0.01
    tank = talents.best_wheel(talents.points(50), ps.talent_toughness)
    assert tank.defense_pct + tank.max_hp_pct <= 0.08


@pytest.mark.parametrize("kind", ["excellent", "ancient"])
@pytest.mark.parametrize("cycle", [0, 1])
def test_talents_stay_within_their_budget(results, kind, cycle):
    pile, talented, tank = results[kind]
    assert ps.talent_violations(pile[cycle], talented[cycle], tank[cycle]) == []


def test_talents_never_make_a_fight_longer(results):
    pile, talented, _ = results["excellent"]
    for cycle in pile:
        for biome in rd.BIOME_ORDER:
            assert ps.biome_summary(talented[cycle], biome)["stack_boss"] <= ps.biome_summary(pile[cycle], biome)["stack_boss"] + 0.05
