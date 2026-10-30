"""pytest for the power-stacking check (Docs/Balance/README.md §14).

Run from the repo root (no venv in the project; uv brings pytest for the run):
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

RUNS = 600  # gear Monte Carlo: enough for stable means, fixed seed


@pytest.fixture(scope="module")
def ctx():
    return ps.load_context()


@pytest.fixture(scope="module")
def gear(ctx):
    return bm.gear_by_region(bm.Model(ctx.data), "quests", RUNS)


@pytest.fixture(scope="module")
def results(ctx, gear):
    return {cycle: ps.evaluate(ctx, cycle, RUNS, gear) for cycle in (0, 1)}


# ----------------------------------------------------------------------------- the sources, as parsed from C#

def test_sources_are_parsed_from_the_code(ctx):
    # The 6 stove dishes of §12 (more may come, e.g. the fishing ones: every one is tried as "best meal").
    assert {m.id for m in ctx.meals} >= {"meal_cuscuz", "meal_cafe_coado", "meal_pamonha", "meal_rapadura",
                                         "meal_baiao_de_dois", "meal_queijo_coalho"}
    assert all(m.effects or m.burst for m in ctx.meals)
    assert len(ctx.relics) == 12
    assert ctx.owner_multiplier == 2.0
    assert {e.stat for e in ctx.events} >= {"AttackDamage", "Defense", "CriticalChance", "HealthRegen"}
    assert ctx.rules.health_mult(1) == pytest.approx(1.8)
    assert ctx.rules.damage_mult(1) == pytest.approx(1.5)
    assert ctx.rules.xp_mult(1) == pytest.approx(1.5)
    assert ctx.rules.health_mult(99) == ctx.rules.health_mult(ctx.rules.max_cycle)


def test_retuned_coefficients_r9(ctx):
    """The §14.3 retune: Canivete +2 -> +1 AttackDamage, Cantoria +15% -> +8%, Rapadura AttackSpeed +25% -> +12%."""
    canivete = next(r for r in ctx.relics if r.id == "relic_canivete_vo_chico")
    assert (canivete.modifier.stat, canivete.modifier.percent, canivete.modifier.value) == ("AttackDamage", False, 1.0)
    rapadura = next(m for m in ctx.meals if m.id == "meal_rapadura")
    assert [(b.stat, b.value, b.seconds) for b in rapadura.burst] == [("AttackSpeed", 0.12, 20.0)]
    damage_events = [e.value for e in ctx.events if e.stat == "AttackDamage"]
    assert damage_events == [0.08]


def test_cuddle_has_no_combat_stat():
    assert ps.load_cuddle().stat == "MoveSpeed"
    assert ps.load_cuddle().stat not in ps.COMBAT_STATS


# ----------------------------------------------------------------------------- how a modifier reaches the model

def test_zero_based_fractions_count_flat_and_percent_alike():
    """BuffController.GetAdditiveFractionTotal (§7.6): a PercentAdditive CDR is +0.05, like a Flat one."""
    for percent in (False, True):
        gear, build = bm.Gear(), bm.Build("Maithe", 5)
        ps.add_modifier(gear, build, ps.Modifier("CooldownReduction", percent, 0.05))
        assert gear.cdr == pytest.approx(0.05)


def test_percent_health_and_defense_scale_the_hero():
    model = bm.Model(rd.load_all())
    plain = model.player(bm.Build("Maria", 5))
    gear, build = bm.Gear(), bm.Build("Maria", 5)
    ps.add_modifier(gear, build, ps.Modifier("MaxHealth", True, 0.10))
    ps.add_modifier(gear, build, ps.Modifier("Defense", True, 0.10))
    build.gear = gear
    buffed = model.player(build)
    assert buffed.hp == pytest.approx(plain.hp * 1.10)
    assert buffed.defense_mult == pytest.approx(plain.defense_mult * 0.90)


def test_opening_attack_speed_only_lasts_its_seconds():
    model = bm.Model(rd.load_all())
    build = bm.Build("Maithe", 5)
    ps.add_modifier(build.gear, build, ps.Modifier("AttackSpeed", True, 0.5, 20.0))
    p = model.player(build)
    assert build.gear.attack_speed == 0.0
    assert p.opening_seconds == 20.0
    assert p.opening_interval < p.basic_interval


def test_relics_follow_the_regions_and_the_owner_bonus(ctx):
    assert len(ps.relics_available(ctx.relics, "Forest", 0)) == 3
    assert len(ps.relics_available(ctx.relics, "Caves", 0)) == 12
    assert len(ps.relics_available(ctx.relics, "Forest", 1)) == 12
    arco = next(r for r in ctx.relics if r.id == "relic_arco_maithe")
    for hero, crit in (("Maithe", 0.02), ("Lavine", 0.01)):
        build = ps.Stack(None, (arco,), ctx.owner_multiplier).apply(hero, 5, bm.Gear())
        assert build.gear.crit == pytest.approx(crit)


def test_meals_unlock_by_region_at_ng0_and_all_at_ng1(ctx):
    queijo = next(m for m in ctx.meals if m.id == "meal_queijo_coalho")
    assert not ps.meal_available(queijo, "Mountain", 0)
    assert ps.meal_available(queijo, "Caves", 0)
    assert ps.meal_available(queijo, "Forest", 1)


def test_ng_plus_scales_enemies_like_enemy_controller(ctx):
    scaled = ps.ng_data(ctx.data, ctx.rules, 1)
    wolf, base = scaled.enemies["enemy_forest_alpha_wolf"], ctx.data.enemies["enemy_forest_alpha_wolf"]
    assert wolf.max_health == round(base.max_health * 1.8)
    assert wolf.attack_damage == pytest.approx(base.attack_damage * 1.5)
    assert ps.ng_data(ctx.data, ctx.rules, 0) is ctx.data


# ----------------------------------------------------------------------------- the stack against the budget

def test_stack_never_makes_a_fight_longer(results):
    for result in results.values():
        for biome in result:
            s = ps.biome_summary(result, biome)
            assert s["stack_boss"] <= s["r8_boss"] + 1e-6, biome
            assert s["stack_common"] <= s["r8_common"] + 1e-6, biome


@pytest.mark.parametrize("cycle", [0, 1])
def test_full_stack_stays_within_the_power_budget(results, cycle):
    assert ps.violations(results[cycle], cycle) == []


def test_ng0_full_stack_bosses_stay_near_the_r8_band(results):
    """Absolute floor: a fully stacked hero still needs 13.5 s (90% of the r8 15 s floor) for every boss."""
    for biome in results[0]:
        assert ps.biome_summary(results[0], biome)["stack_boss"] >= 13.5, biome


def test_ng1_is_harder_than_ng0_from_the_desert_on(results):
    for biome in rd.BIOME_ORDER[1:]:
        assert ps.biome_summary(results[1], biome)["r8_boss"] > ps.biome_summary(results[0], biome)["r8_boss"], biome
