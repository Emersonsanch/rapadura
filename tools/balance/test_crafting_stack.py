"""pytest for the advanced crafting layer (Docs/Balance/README.md "Crafting avançado").

Run from the repo root:
    uv run --no-project --with pytest python -m pytest -q -p no:cacheprovider tools/balance
"""

from __future__ import annotations

import os
import sys

import pytest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import crafting_stack as cs  # noqa: E402
import power_stack as ps  # noqa: E402

RUNS = 600


@pytest.fixture(scope="module")
def results():
    return cs.run(ps.load_context(), RUNS)


def test_imbues_are_parsed_and_never_beat_the_best_road_blessing():
    imbues = cs.load_imbues()
    assert {i for i, _, _ in imbues} == {"imbue_presa_lobo", "imbue_seda_aranha", "imbue_ferrao", "imbue_lasca_golem", "imbue_asa_morcego"}
    assert sorted(t for _, t, _ in imbues) == [1, 1, 2, 3, 4]
    best_event_damage = max(e.value for e in ps.load_event_buffs() if e.stat == "AttackDamage")
    for _, _, mod in imbues:
        if mod.stat == "AttackDamage":
            assert mod.value <= best_event_damage


def test_caps_grow_with_the_tier_and_stay_low():
    caps = cs.load_caps()
    assert len(caps) == 4 and caps == sorted(caps)
    assert max(caps) <= 2, "Raro at most: Épico from crafting breaks the budget"


def test_crafted_gear_never_makes_a_fight_longer(results):
    for c in (0, 1):
        for biome in results["plain"][c]:
            assert ps.biome_summary(results["crafted"][c], biome)["r8_boss"] <= ps.biome_summary(results["plain"][c], biome)["r8_boss"] + 1e-6


def test_crafting_stays_within_the_budget(results):
    assert cs.problems(results) == []
