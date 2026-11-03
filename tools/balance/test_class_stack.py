"""pytest for the class-evolution layer of the power-stacking check (Docs/ClassEvolution.md §6).

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

RUNS = 600


@pytest.fixture(scope="module")
def ctx():
    return ps.load_context()


@pytest.fixture(scope="module")
def cls():
    return ps.load_class_evolution()


@pytest.fixture(scope="module")
def results(ctx, cls):
    gear = bm.gear_by_region(bm.Model(ctx.data), "quests", RUNS)
    plain = {c: ps.evaluate(ctx, c, RUNS, gear) for c in (0, 1)}
    classed, mastered = ps.class_evaluate(ctx, cls, RUNS, gear, plain)
    return plain, classed, mastered


def test_catalog_is_parsed(cls):
    assert len(cls.skills) == 20, "every Skill(...) of ClassEvolutionCatalog.cs on one line, positional (G4 is a passive)"
    assert len(cls.min_level) == 25
    assert {s.kind for s in cls.skills if s.tier == 1} == {"GroupBuff"}
    assert len(cls.combos) == 5 and cls.combo_tier == 2
    assert len(cls.upgrades) == 5, "every Upgrade(...) on one line, positional"
    assert {lv for (_, t), lv in cls.min_level.items() if t == 4} == {150}
    assert {lv for (_, t), lv in cls.min_level.items() if t == 5} == {350}
    assert sum(cls.master_ranks.values()) == 100
    assert sum(r for n, r in cls.master_ranks.items() if cls.master_branch[n] == "Core") == 30


def test_high_tiers_and_new_master_lines_stay_within_their_budget(cls):
    assert ps.high_tier_violations(cls) == []


def test_high_tier_skills_never_reach_the_campaign(cls):
    """G4/G5 open at 150/350: nothing of them is in the 4-biome fights (bosses at levels 3-15) nor in the "até 100" scale."""
    assert cls.gear(100) == cls.gear(50)
    data = ps.class_data(ps.load_context().data, cls)
    for s in cls.skills:
        if s.tier == 5:
            assert any(x.id == s.id and x.min_player_level == 350 for x in data.skills[s.hero])


def test_graduations_are_gated_by_level(cls):
    assert cls.gear(1) == bm.Gear()
    first = min(lv for (_, t), lv in cls.min_level.items() if t == 1)
    assert cls.gear(first).crit > 0 and cls.gear(first).ad_pct == 0, "combo only from the 2nd graduation"
    assert cls.gear(50).ad_pct > 0


def test_master_tree_is_modest(cls):
    g = cls.master_gear()
    assert g.ad_pct <= 0.03 and g.crit <= 0.02 and g.max_hp_pct <= 0.08 and g.defense_pct <= 0.05
    assert g.cdr == 0 and g.rcr == 0 and g.attack_speed == 0 and g.crit_dmg == 0


def test_graduations_never_make_a_fight_longer(results):
    plain, classed, _ = results
    for cycle in plain:
        for biome in plain[cycle]:
            assert ps.biome_summary(classed[cycle], biome)["r8_boss"] <= ps.biome_summary(plain[cycle], biome)["r8_boss"] + 0.3


@pytest.mark.parametrize("cycle", [0, 1])
def test_class_evolution_stays_within_its_budget(results, cycle):
    plain, classed, mastered = results
    assert ps.class_violations(plain[cycle], classed[cycle], cycle) == []
    if cycle in mastered:
        assert ps.master_violations(classed[cycle], mastered[cycle]) == []
