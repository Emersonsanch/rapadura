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
# Since HeroToughnessGrowthPerLevel = 0 and the commons' health floor 170 from level 100 up (2026-11-11) only the three
# Regions 5-9 bosses below 100 were off (Hedammu 72, Lodo-que-Engole 88, Aydahar 98: 13-14 s to kill, target 15-22). Their
# base health went up x1.2 (2026-11-11: 1152 / 1176; Aydahar x1.18 = 1180, the RegionBossTests cap: no world boss above
# Tages, enemy_cave_dweller), so they now die in 16.2-17.0 s and nothing is off.
DOCUMENTED_MISSES: list = []


@pytest.fixture(scope="module")
def world():
    return l5.World()


@pytest.fixture(scope="module")
def result(world):
    return l5.evaluate(world)


def test_world_bosses_and_regions_are_read_from_the_code(world):
    levels = [b.level for b in world.bosses]
    assert {72, 88, 98, 140, 150, 200, 240, 280, 320, 360, 440, 478} <= set(levels) and levels == sorted(levels)
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
        assert by[30]["swings"] <= 3 and by[30]["parked"] < l5.PARKED_TARGET[0]  # +30 is "quase hit-kill" by design


def test_off_target_bands_are_the_documented_ones(result):
    assert sorted(l5.misses(result)) == DOCUMENTED_MISSES


def test_world_boss_kill_time_is_on_target_today(result):
    for boss, by in result.bosses:
        if ("boss", boss.level, "fast") in DOCUMENTED_MISSES:
            continue
        assert l5.BOSS_TARGET[0] <= by["real"]["kill"] <= l5.BOSS_TARGET[1], boss.level


def test_shipped_toughness_puts_every_boss_on_target_without_breaking_up_to_100(world, result):
    assert world.scaling.tough == 0.0  # Docs/Balanco100a500.md suggestion 1, applied
    whatif = l5.toughness_whatif(world, result, 0.0)
    assert whatif.up_to_100 == []
    for boss, by in result.bosses:
        assert l5.PARKED_TARGET[0] <= whatif.parked[boss.level] <= l5.PARKED_TARGET[1], boss.level
        assert whatif.swings[boss.level] >= l5.MIN_SWINGS_TO_DIE
        assert by["real"]["basic"] >= l5.BASIC_EDGE * whatif.parked[boss.level]


def test_commons_health_floor_is_read_from_the_code(world):
    s = world.scaling
    assert s.common_floor == 170.0 and s.floor_from == 100
    assert s.base_health(70.0, 99, False) == 70.0 and s.base_health(70.0, 100, False) == 170.0
    assert s.base_health(70.0, 300, True) == 70.0 and s.base_health(210.0, 300, False) == 210.0
    enemy = min(l5.commons_at(world.data, world.regions, 300), key=lambda e: e.max_health)
    assert l5.scaled(enemy, s, 300).max_health == pytest.approx(170.0 * s.health_mult(300))


def test_suggested_health_floor_puts_sustained_commons_on_target(world):
    for level in l5.SUGGEST_LEVELS:
        row = l5.common_row(world, level, "real", 0, 1.0, l5.ARCHETYPE_COMMON_HEALTH)
        assert l5.COMMON_TARGET[0] <= row["farm"] <= l5.COMMON_TARGET[1], level
