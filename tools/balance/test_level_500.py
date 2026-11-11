"""pytest for the 100-500 bands against the open world (level_500.py, Docs/Balanco100a500.md).

Run from the repo root:
    uv run --no-project --with pytest python -m pytest -q -p no:cacheprovider tools/balance
"""

from __future__ import annotations

import os
import sys

import pytest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import level_500 as l5  # noqa: E402

# What Docs/Balanco100a500.md reports as off target (real reading, same-level fights). When the game changes and this
# list moves, rerun `python -I tools/balance/level_500.py`, update the doc's tables, then this snapshot.
DOCUMENTED_MISSES = sorted(
    [("common", lv, "fast") for lv in (100, 120, 140, 160, 260, 280, 300, 320, 340, 360, 380, 400, 420, 440, 460, 478, 500)]
    + [("boss", lv, "parked_short") for lv in (200, 240, 280, 320, 360, 440, 478)])


@pytest.fixture(scope="module")
def world():
    return l5.World()


@pytest.fixture(scope="module")
def result(world):
    return l5.evaluate(world)


def test_world_bosses_and_regions_are_read_from_the_code(world):
    levels = [b.level for b in world.bosses]
    assert {200, 240, 280, 320, 360, 440, 478} <= set(levels) and min(levels) >= l5.LEVELS[0]
    assert all(b.enemy_id in world.data.enemies and world.data.enemies[b.enemy_id].is_boss for b in world.bosses)
    assert all(b.enemy_id not in world.data.enemies for b in world.pending_bosses)
    for level in l5.LEVELS:
        assert l5.commons_at(world.data, world.regions, level), level
    assert world.regions["Ceu"].high == 500


def test_sets_only_from_the_level_they_can_be_had(world):
    keys = {s.key: s.min_level for s in world.sets}
    assert keys["bone"] == 35  # DungeonRules.MinLevel: the Ossuário
    assert keys["papiro"] == world.regions["Rio"].low and keys["luz"] == world.regions["Ceu"].low
    assert l5.loadout(world.slots, world.sets, keys["papiro"] - 1).set_key in ("bone", None)
    assert all(l5.loadout(world.slots, world.sets, lv).set_key in [k for k, m in keys.items() if m <= lv] + [None]
               for lv in l5.LEVELS)


def test_creatures_are_scaled_by_enemy_level_scaling(world):
    enemy = l5.commons_at(world.data, world.regions, 300)[0]
    foe = l5.scaled(enemy, world.scaling, 300)
    assert foe.max_health == pytest.approx(enemy.max_health * world.scaling.health_mult(300))
    assert foe.attack_damage == pytest.approx(enemy.attack_damage * world.scaling.damage_mult(300))


def test_perigoso_matches_level_danger_rules(world):
    d = world.danger
    assert d.expected_hit(0) == 1.0 and d.hit_rate(19) == 1.0
    assert d.damage_mult(20) == pytest.approx(1.5) and d.damage_mult(25) == pytest.approx(2.25) and d.damage_mult(40) == 3.0
    assert d.hit_rate(20) == pytest.approx(0.6) and d.crit(10) == pytest.approx(0.25)


def test_perigoso_only_makes_fights_worse(result):
    for by in result.perigoso_commons.values():
        hits = [by[delta]["hits"] for delta in l5.PERIGOSO_DELTAS]
        farm = [by[delta]["farm"] for delta in l5.PERIGOSO_DELTAS]
        assert hits == sorted(hits, reverse=True) and farm == sorted(farm)
    for _, by in result.perigoso_bosses:
        parked = [by[delta]["parked"] for delta in l5.PERIGOSO_DELTAS]
        assert parked == sorted(parked, reverse=True)
        assert by[30]["swings"] <= 2  # +30 is "quase hit-kill" by design


def test_off_target_bands_are_the_documented_ones(result):
    assert sorted(l5.misses(result)) == DOCUMENTED_MISSES


def test_world_boss_kill_time_is_on_target_today(result):
    for boss, by in result.bosses:
        assert l5.BOSS_TARGET[0] <= by["real"]["kill"] <= l5.BOSS_TARGET[1], boss.level


def test_suggested_toughness_puts_every_boss_on_target_without_breaking_up_to_100(world, result):
    whatif = l5.toughness_whatif(world, result, 0.0)
    assert whatif.up_to_100 == []
    for boss, by in result.bosses:
        assert l5.PARKED_TARGET[0] <= whatif.parked[boss.level] <= l5.PARKED_TARGET[1], boss.level
        assert whatif.swings[boss.level] >= l5.MIN_SWINGS_TO_DIE
        assert by["real"]["basic"] >= l5.BASIC_EDGE * whatif.parked[boss.level]


def test_suggested_health_floor_puts_sustained_commons_on_target(world):
    for level in l5.SUGGEST_LEVELS:
        row = l5.common_row(world, level, "real", 0, 1.0, l5.ARCHETYPE_COMMON_HEALTH)
        assert l5.COMMON_TARGET[0] <= row["farm"] <= l5.COMMON_TARGET[1], level
