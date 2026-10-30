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
    # NG+ by region (§11.1): one-off region bonus from NG+1 + an equal step per cycle; 1 on cycle 0.
    assert [ctx.rules.health_mult(1, b) for b in rd.BIOME_ORDER] == pytest.approx([2.2, 1.5, 1.35, 1.15])
    assert [ctx.rules.damage_mult(1, b) for b in rd.BIOME_ORDER] == pytest.approx([2.7, 1.8, 1.6, 1.45])
    assert ctx.rules.health_mult(1) == ctx.rules.health_mult(1, "Desert")
    assert all(ctx.rules.health_mult(0, b) == 1.0 and ctx.rules.damage_mult(0, b) == 1.0 for b in rd.BIOME_ORDER)
    assert ctx.rules.xp_mult(1) == pytest.approx(1.5)
    assert ctx.rules.health_mult(99, "Forest") == ctx.rules.health_mult(ctx.rules.max_cycle, "Forest")


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
    for enemy_id, region in (("enemy_forest_alpha_wolf", "Forest"), ("enemy_cave_dweller", "Caves")):
        assert ps.region_of(ctx.data, enemy_id) == region
        enemy, base = scaled.enemies[enemy_id], ctx.data.enemies[enemy_id]
        assert enemy.max_health == round(base.max_health * ctx.rules.health_mult(1, region))
        assert enemy.attack_damage == pytest.approx(round(base.attack_damage * ctx.rules.damage_mult(1, region), 1))
    assert scaled.enemies["enemy_forest_alpha_wolf"].max_health == 1144
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


@pytest.mark.parametrize("cycle", [1, 2])
def test_ng_plus_bosses_are_even_across_regions(ctx, gear, results, cycle):
    """The r9 shape fix (§11.1, §14.4): without the stack, every NG+1/NG+2 boss is a 15-22 s fight (r8 band),
    and no region is more than 4 s off another (before: NG+1 Floresta 14 s, Cavernas 33 s)."""
    result = results[cycle] if cycle in results else ps.evaluate(ctx, cycle, RUNS, gear)
    bosses = [ps.biome_summary(result, b)["r8_boss"] for b in rd.BIOME_ORDER]
    assert all(15.0 <= t <= 22.0 for t in bosses), bosses
    assert max(bosses) - min(bosses) <= 4.0, bosses


def test_ng1_parked_hero_still_dies_in_the_band(results):
    for biome in rd.BIOME_ORDER:
        ttd = ps.biome_summary(results[1], biome)["parked_ttd_stack"]
        assert ps.PARKED_TTD_BAND[0] <= ttd <= ps.PARKED_TTD_BAND[1], (biome, ttd)


# ----------------------------------------------------------------------------- hunting: charms + prey (section "Caça")

def test_hunting_is_parsed_and_adds_no_damage(ctx):
    """Amuletos do Caderno and the Presa do dia protect, never hit harder: the best cord adds 0 damage vs commons."""
    hunting = ctx.hunting
    assert hunting is not None and hunting.max_equipped == 2
    assert set(hunting.protection_charms) == {"Quadruped", "Arthropod", "Flyer", "Heavy"}
    assert all(v == [pytest.approx(0.08)] for v in hunting.protection_charms.values())
    assert hunting.prey_protection == pytest.approx(0.10)
    assert hunting.best_protection() == pytest.approx(0.18)
    assert hunting.damage_charms == {} and hunting.prey_damage == 0.0
    assert ctx.hunting_common == 0.0


def test_hunting_damage_would_break_the_commons_budget(ctx, gear):
    """Why the charms are protective: +4% damage vs commons (a damage charm) already breaks the NG+0 budget."""
    hunting = ps.Hunting({"Quadruped": [0.04]}, {}, 2, 0.0, 0.0, 1.0)
    probe = ps.Context(ctx.data, ctx.rules, ctx.meals, ctx.relics, ctx.owner_multiplier, ctx.events, ctx.k_common, ctx.k_boss, hunting)
    assert probe.hunting_common == pytest.approx(0.04)
    assert ps.violations(ps.evaluate(probe, 0, RUNS, gear), 0)

# ----------------------------------------------------------------------------- fôlego diário (§23)

def test_stamina_is_parsed_from_the_code():
    """DailyStaminaRules: +30% / x1 / -50% between tiers; kill XP at the campaign pace (fôlego cheio = the r8 XP)."""
    stamina = ps.load_stamina()
    assert stamina["full"] == pytest.approx(1.3)
    assert stamina["normal"] == pytest.approx(1.0)
    assert stamina["tired"] == pytest.approx(0.5)
    assert ps.stamina_xp_mult(stamina, "full") == pytest.approx(1.0)
    assert ps.stamina_xp_mult(stamina, "normal") < 1.0
    assert ps.stamina_xp_mult(stamina, "tired") < ps.stamina_xp_mult(stamina, "normal")


def test_full_stamina_the_whole_campaign_stays_in_the_power_budget(ctx, gear):
    """A child always in "fôlego cheio" (under 3 h a day) + the full optional stack: inside the §14.3 budget."""
    mult = ps.stamina_xp_mult(ps.load_stamina())
    for cycle in (0, 1):
        assert ps.violations(ps.evaluate(ctx, cycle, RUNS, gear, stamina_mult=mult), cycle) == []


def test_stamina_on_top_of_r8_without_the_pace_would_break_the_budget(ctx, gear):
    """Why the pace exists: x1.3 kill XP over the r8 route puts the hero 1-2 levels ahead and breaks the budget."""
    assert ps.violations(ps.evaluate(ctx, 0, RUNS, gear, stamina_mult=1.3), 0)
