"""pytest for the MU-style item options layer of the power-stacking check (Docs/Balance/README.md "Opções de item").

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
def options():
    return ps.load_options()


@pytest.fixture(scope="module")
def results():
    ctx = ps.load_context()
    gear = bm.gear_by_region(bm.Model(ctx.data), "quests", RUNS)
    refine, wings, options = ps.load_refine(), ps.load_wings(), ps.load_options()
    plain = {c: ps.evaluate(ctx, c, RUNS, gear) for c in (0, 1)}
    winged = {c: ps.evaluate(ctx, c, RUNS, ps.winged_gear(gear, refine, wings, c)) for c in (0, 1)}
    optioned = {kind: {c: ps.evaluate(ctx, c, RUNS, ps.optioned_gear(gear, refine, wings, options, c, kind)) for c in (0, 1)}
                for kind in ("excellent", "ancient")}
    return plain, winged, optioned


def test_options_are_parsed_from_the_catalog(options):
    assert len(options.weapon_lines) == 3 and len(options.armour_lines) == 3
    assert len(options.sets) == 5, "one ancient set per cousin"
    assert all([need for need, _ in tiers] == [2, 3, 4] for tiers in options.sets.values())
    assert len(options.engravings) == 4
    assert {w for w, _ in options.engravings.values()} == {True, False}, "weapon and armour engravings"
    assert options.lines_max == 2 and options.sockets_max == 2
    assert options.luck.stat == "CriticalChance" and options.luck.value <= 0.01


def test_option_bonuses_are_small(options):
    # Every single line is a fraction of a +9 refine weapon with option 2 (6.2% damage, README "Refino de item").
    singles = options.weapon_lines + options.armour_lines + [m for _, m in options.engravings.values()]
    singles += [m for tiers in options.sets.values() for _, m in tiers]
    assert all(m.value <= 0.04 for m in singles if m.percent)
    # Offensive engravings only go in weapons: eight armour sockets never stack damage.
    for weapon, mod in options.engravings.values():
        if mod.stat in ("AttackDamage", "CriticalChance", "AttackSpeed", "CriticalDamage"):
            assert weapon


def test_more_pieces_never_give_less(options):
    for kind in ("excellent", "ancient"):
        scores = [options.offense(options.gear(n, kind)) for n in range(5)]
        assert scores == sorted(scores) and scores[0] == 0


def test_options_grow_the_deeper_the_region():
    pieces = [ps.OPTION_PIECES_BY_REGION[b] for b in rd.BIOME_ORDER]
    assert pieces == sorted(pieces) and pieces[-1] == 4


def test_options_never_make_a_fight_longer(results):
    _, winged, optioned = results
    for kind in optioned:
        for cycle in winged:
            for biome in winged[cycle]:
                assert ps.biome_summary(optioned[kind][cycle], biome)["stack_boss"] <= ps.biome_summary(winged[cycle], biome)["stack_boss"] + 1e-6


@pytest.mark.parametrize("cycle", [0, 1])
@pytest.mark.parametrize("kind", ["excellent", "ancient"])
def test_options_stay_within_their_budget(results, cycle, kind):
    plain, winged, optioned = results
    assert ps.options_violations(plain[cycle], winged[cycle], optioned[kind][cycle], cycle) == []
