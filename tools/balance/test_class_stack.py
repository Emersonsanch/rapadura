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
    assert len(cls.skills) == 15, "every Skill(...) of ClassEvolutionCatalog.cs on one line, positional"
    assert len(cls.min_level) == 15
    assert {s.kind for s in cls.skills if s.tier == 1} == {"GroupBuff"}
    assert len(cls.combos) == 5 and cls.combo_tier == 2
    assert len(cls.master_nodes) == 6
    assert sum(rank for _, _, rank in cls.master_nodes) == 30


def test_graduations_are_gated_by_level(cls):
    assert cls.gear(1) == bm.Gear()
    first = min(lv for (_, t), lv in cls.min_level.items() if t == 1)
    assert cls.gear(first).crit > 0 and cls.gear(first).ad_pct == 0, "combo only from the 2nd graduation"
    assert cls.gear(50).ad_pct > 0


def test_master_tree_is_modest(cls):
    g = cls.master_gear()
    assert g.ad_pct <= 0.03 and g.crit <= 0.02 and g.max_hp_pct <= 0.05 and g.defense_pct <= 0.03


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
