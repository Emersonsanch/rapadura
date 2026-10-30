"""Power stacking check (Docs/Balance/README.md §14): python -I tools/balance/power_stack.py [--runs N]

The r8 model (balance_model.py, §10) balanced a hero with level, attributes, skill points and gear. After
r8 the game gained more sources of power, all of them modifiers on the same BuffController:

  * meals from the Cozinha da Vó Zefa (MealCatalog.cs, §12): one at a time, plus the cook's favourite bonus;
  * permanent relic passives (RelicDefinition.cs / RelicPassiveApplier, §14.1): x OwnerMultiplier for the
    relic's own hero;
  * road-event "next fight" buffs (WorldEventCatalog.cs, §13);
  * Caramelo's cuddle (CarameloCuddle.cs): MoveSpeed only, so 0 in a model without movement;
  * Nova Jornada+ (NewGamePlusRules.cs, §11): enemy HP/damage and kill XP per cycle.

Every number is parsed from the C# sources, so this report follows the code. It stacks the best meal (per
hero and fight), every relic found so far (relics of the regions opened up to the current one at NG+0, all
12 at NG+1) and the best event buff, on top of the r8 quest-route hero, at NG+0 and NG+1, and checks the
power budget (§14.3, `violations`): the stack may shorten the r8 boss fights by at most ~22% and commons by
~30%, a basic-attack-only hero must still be on a knife's edge, and the boss kills a parked hero in 12-40 s
(BalanceInvariantsTests band). The absolute r8 targets (commons 4-7 s, bosses 15-22 s) are printed too.
"""

from __future__ import annotations

import argparse
import math
import os
import re
import statistics
import sys
from dataclasses import dataclass

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import balance_model as bm  # noqa: E402
import rapadura_data as rd  # noqa: E402
import run_r8  # noqa: E402

SCRIPTS = os.path.join(rd.ROOT, "Assets", "Scripts")
NG_RULES = os.path.join(SCRIPTS, "Gameplay", "Story", "NewGamePlusRules.cs")
MEAL_CATALOG = os.path.join(SCRIPTS, "Gameplay", "Cooking", "MealCatalog.cs")
RELIC_DEFINITION = os.path.join(SCRIPTS, "Gameplay", "Secrets", "RelicDefinition.cs")
EVENT_CATALOG = os.path.join(SCRIPTS, "Gameplay", "World", "Events", "WorldEventCatalog.cs")
CUDDLE = os.path.join(SCRIPTS, "Gameplay", "Pets", "CarameloCuddle.cs")
REGION_QUESTS = os.path.join(SCRIPTS, "Gameplay", "Quests", "RegionQuestCatalog.cs")

HERO_IDS = {"Maithe": "Maithe", "Lavine": "Lavine", "Icaro": "Icaro", "Joaquim": "Joaquim", "Maria": "Maria"}
HEALTH_REGEN_BASE = 1.0  # PlayerStats._healthRegenPerSecond: what a PercentAdditive HealthRegen bonus scales.
# The r8 model (and BalanceInvariantsTests) leave this base regen out of every fight; so does this check.
PARKED_TTD_BAND = (12.0, 40.0)  # BalanceInvariantsTests.Boss_KillsAnUndefendedPlayerInAFewRotations_NotInstantly


def _read(path: str) -> str:
    with open(path, encoding="utf-8-sig") as handle:
        return handle.read()


def _const(source: str, name: str) -> float:
    match = re.search(r"const\s+(?:float|int)\s+" + name + r"\s*=\s*([-\d.]+)f?\s*;", source)
    if not match:
        raise ValueError(f"const {name} not found")
    return float(match.group(1))


# ----------------------------------------------------------------------------- sources (parsed from C#)

@dataclass
class Modifier:
    stat: str
    percent: bool
    value: float
    seconds: float = 0.0  # 0 = the whole fight


@dataclass
class Meal:
    id: str
    effects: list
    burst: list
    favourite_of: str | None
    favourite: list
    favourite_is_burst: bool
    unlock_region: str | None  # None = starter


@dataclass
class Relic:
    id: str
    source: str
    biome: str
    hero: str | None
    modifier: Modifier


@dataclass
class NgRules:
    max_cycle: int
    health: float
    damage: float
    experience: float

    def clamp(self, cycle: int) -> int:
        return max(0, min(self.max_cycle, cycle))

    def health_mult(self, cycle: int) -> float:
        return 1 + self.health * self.clamp(cycle)

    def damage_mult(self, cycle: int) -> float:
        return 1 + self.damage * self.clamp(cycle)

    def xp_mult(self, cycle: int) -> float:
        return 1 + self.experience * self.clamp(cycle)


def load_ng_rules() -> NgRules:
    source = _read(NG_RULES)
    return NgRules(int(_const(source, "MaxCycle")), _const(source, "HealthPerCycle"),
                   _const(source, "DamagePerCycle"), _const(source, "ExperiencePerCycle"))


_EFFECT = re.compile(r'Effect\("[^"]*",\s*StatType\.(\w+),\s*ModifierApplication\.(\w+),\s*([-\d.]+)f(?:,\s*(\w+))?\)')


def _effects(text: str, consts: dict) -> list:
    out = []
    for stat, application, value, seconds in _EFFECT.findall(text):
        out.append(Modifier(stat, application == "PercentAdditive", float(value),
                            consts.get(seconds, float(seconds) if seconds and seconds[0].isdigit() else 0.0)))
    return out


def load_meals() -> list:
    source = _read(MEAL_CATALOG)
    consts = {"RapaduraBurstSeconds": _const(source, "RapaduraBurstSeconds")}
    ids = dict(re.findall(r'const string (\w+) = "(meal_\w+)"', source))
    quests = _read(REGION_QUESTS)
    quest_const = {}
    for name, region in re.findall(r"public const string (\w+)\s*=\s*\"quest_region_(\w+?)_", quests):
        quest_const[name] = {"forest": "Forest", "desert": "Desert", "mountain": "Mountain", "caves": "Caves"}.get(region)
    meals = []
    token = re.compile(r"new\[\]\s*\{\s*(?:Effect\(.*?\)\s*,?\s*)+\}|CharacterId\.\w+|null")
    for block in source.split("new MealRecipeSpec(")[1:]:
        block = block.split("),\n\n")[0]
        meal_id = ids[block.split(",", 1)[0].strip()]
        head, _, tail = block.partition("starter:")
        head = re.sub(r"new\[\]\s*\{\s*(?:new MealIngredientSpec\([^)]*\)\s*,?\s*)+\}", "", head, count=1)
        tokens = token.findall(head)
        effects, burst, favourite_of, favourite = (tokens + ["null"] * 4)[:4]
        starter = tail.strip().startswith("true")
        unlock = re.search(r"unlockQuestId:\s*RegionQuestCatalog\.(\w+)", tail)
        meals.append(Meal(
            meal_id,
            _effects(effects, consts) if effects != "null" else [],
            _effects(burst, consts) if burst != "null" else [],
            favourite_of.split(".")[1] if favourite_of.startswith("CharacterId") else None,
            _effects(favourite, consts) if favourite != "null" else [],
            any(m.seconds > 0 for m in _effects(favourite, consts)) if favourite != "null" else False,
            None if starter else (quest_const.get(unlock.group(1)) if unlock else None),
        ))
    return meals


def load_relics() -> tuple:
    source = _read(RELIC_DEFINITION)
    ids = dict(re.findall(r'const string (\w+) = "(relic_\w+)"', source))
    owner = _const(source, "OwnerMultiplier")
    relics = []
    pattern = re.compile(r"new RelicDefinition\((\w+),\s*RelicSource\.(\w+),\s*BiomeType\.(\w+),\s*(null|CharacterId\.\w+),\s*"
                         r"StatType\.(\w+),\s*ModifierApplication\.(\w+),\s*([-\d.]+)f\)")
    for const, src, biome, hero, stat, application, value in pattern.findall(source):
        relics.append(Relic(ids[const], src, biome, None if hero == "null" else hero.split(".")[1],
                            Modifier(stat, application == "PercentAdditive", float(value))))
    return relics, owner


def load_event_buffs() -> list:
    """Every "next fight" buff a road event can give (one event's buff per fight in practice: §13.1)."""
    source = _read(EVENT_CATALOG)
    out = []
    for stat, value, application in re.findall(r"WorldEventEffect\.Buff\(StatType\.(\w+),\s*([-\d.]+)f(?:,\s*ModifierApplication\.(\w+))?\)", source):
        out.append(Modifier(stat, application != "Flat", float(value)))
    return out


def load_cuddle() -> Modifier:
    source = _read(CUDDLE)
    return Modifier("MoveSpeed", True, _const(source, "BuffSpeedBonus"), _const(source, "BuffSeconds"))


# ----------------------------------------------------------------------------- modifiers -> model

COMBAT_STATS = {"AttackDamage", "CriticalChance", "CriticalDamage", "AttackSpeed", "Thorns", "Defense", "MaxHealth",
                "HealthRegen", "ManaRegen", "CooldownReduction", "ResourceCostReduction", "MaxMana"}


def add_modifier(gear: bm.Gear, build: bm.Build, mod: Modifier, scale: float = 1.0) -> None:
    """Adds one modifier to the model build, the way the game's reader sees it."""
    v = mod.value * scale
    if mod.seconds > 0:
        if mod.stat == "AttackSpeed":
            build.opening_attack_speed += v
            build.opening_seconds = max(build.opening_seconds, mod.seconds)
        return  # other timed effects (MoveSpeed) are not modelled
    s = mod.stat
    if s == "AttackDamage":
        if mod.percent:
            gear.ad_pct += v
        else:
            gear.ad_flat += v
    elif s == "Defense":
        if mod.percent:
            gear.defense_pct += v
        else:
            gear.defense += v
    elif s == "MaxHealth":
        if mod.percent:
            gear.max_hp_pct += v
        else:
            gear.max_hp += v
    elif s == "HealthRegen":
        gear.regen += v * HEALTH_REGEN_BASE if mod.percent else v
    elif s == "ManaRegen":
        gear.mana_regen_pct += v if mod.percent else v / bm.MANA_REGEN
    # 0-based fractions: Flat and PercentAdditive are the same "+v" (BuffController.GetAdditiveFractionTotal, §7.6).
    elif s == "CriticalChance":
        gear.crit += v
    elif s == "CriticalDamage":
        gear.crit_dmg += v
    elif s == "AttackSpeed":
        gear.attack_speed += v
    elif s == "Thorns":
        gear.thorns += v
    elif s == "CooldownReduction":
        gear.cdr += v
    elif s == "ResourceCostReduction":
        gear.rcr += v
    elif s == "MaxMana":
        gear.max_mana += v
    # MoveSpeed, MaxStamina, StaminaRegen: no effect in a model without movement.


def meal_available(meal: Meal, biome: str, cycle: int) -> bool:
    if meal.unlock_region is None or cycle > 0:
        return True
    return rd.BIOME_ORDER.index(meal.unlock_region) <= rd.BIOME_ORDER.index(biome)


def relics_available(relics: list, biome: str, cycle: int) -> list:
    if cycle > 0:
        return list(relics)
    return [r for r in relics if rd.BIOME_ORDER.index(r.biome) <= rd.BIOME_ORDER.index(biome)]


@dataclass
class Stack:
    meal: Meal | None = None
    relics: tuple = ()
    owner_multiplier: float = 2.0
    event: Modifier | None = None

    def apply(self, hero: str, level: int, gear: bm.Gear) -> bm.Build:
        g = bm.Gear(**{k: getattr(gear, k) for k in gear.__dataclass_fields__})
        build = bm.Build(hero, level, g)
        if self.meal is not None:
            for mod in self.meal.effects + self.meal.burst:
                add_modifier(g, build, mod)
            if self.meal.favourite_of == HERO_IDS[hero]:
                for mod in self.meal.favourite:
                    add_modifier(g, build, mod)
        for relic in self.relics:
            add_modifier(g, build, relic.modifier, self.owner_multiplier if relic.hero == HERO_IDS[hero] else 1.0)
        if self.event is not None:
            add_modifier(g, build, self.event)
        return build


def ng_data(data: rd.GameData, rules: NgRules, cycle: int) -> rd.GameData:
    """Enemies as EnemyController sees them on cycle `cycle` (HP rounded, damage per hit, before buffs)."""
    if cycle == 0:
        return data
    scaled = bm.clone_data(data)
    for enemy in scaled.enemies.values():
        enemy.max_health = round(enemy.max_health * rules.health_mult(cycle))
        enemy.attack_damage = enemy.attack_damage * rules.damage_mult(cycle)
    return scaled


def parked_time_to_die(model: bm.Model, build: bm.Build, boss: rd.Enemy) -> float:
    """Boss alone vs a hero who never attacks nor dodges, with the regen bonuses (base regen left out, as in r8)."""
    p = model.player(build)
    dps = boss.attack_damage * p.defense_mult / max(boss.attack_cooldown, boss.windup)
    net = dps - p.regen
    return p.hp / net if net > 0 else math.inf


# ----------------------------------------------------------------------------- the check

@dataclass
class Context:
    data: rd.GameData
    rules: NgRules
    meals: list
    relics: list
    owner_multiplier: float
    events: list
    k_common: float
    k_boss: float


def load_context(data: rd.GameData | None = None, k_common: float | None = None, k_boss: float | None = None) -> Context:
    data = data or rd.load_all()
    if k_common is None or k_boss is None:
        k_common, k_boss, _ = run_r8.calibrate(data)
    relics, owner = load_relics()
    events = [e for e in load_event_buffs() if e.stat in COMBAT_STATS]
    return Context(data, load_ng_rules(), load_meals(), relics, owner, events, k_common, k_boss)


def cycle_setup(ctx: Context, cycle: int, runs: int, seed: int = 8, base_gear: dict | None = None):
    """(model, visits by biome, gear) for the quest route on `cycle`: NG+0 = r8; NG+1 keeps the end-of-campaign
    level and the Caves gear (new drops ignored: conservative), kill/boss XP x the rules."""
    base_model = bm.Model(ctx.data)
    gear0 = base_gear or bm.gear_by_region(base_model, "quests", runs, seed)
    if cycle == 0:
        visits = bm.route_xp(base_model, "quests")
        return base_model, {v.biome: v for v in visits}, gear0
    end = bm.route_xp(base_model, "quests")[-1].exit_xp
    for c in range(1, cycle):
        end = bm.route_xp(bm.Model(ng_data(ctx.data, ctx.rules, c)), "quests", start_xp=end, kill_xp_mult=ctx.rules.xp_mult(c))[-1].exit_xp
    model = bm.Model(ng_data(ctx.data, ctx.rules, cycle))
    visits = bm.route_xp(model, "quests", start_xp=end, kill_xp_mult=ctx.rules.xp_mult(cycle))
    caves = gear0[("Caves", "boss")]
    gear = {(b, phase): caves for b in rd.BIOME_ORDER for phase in ("entry", "boss")}
    return model, {v.biome: v for v in visits}, gear


def best_stack(ctx: Context, model, hero, level, gear, enemies, biome, cycle, base: Stack) -> tuple:
    """Greedy: the meal that kills `enemies` fastest, then the event buff that does, on top of `base`."""
    best_meal, best_time = None, model.fight(base.apply(hero, level, gear), enemies)["time"]
    for meal in ctx.meals:
        if not meal_available(meal, biome, cycle):
            continue
        stack = Stack(meal, base.relics, base.owner_multiplier, base.event)
        t = model.fight(stack.apply(hero, level, gear), enemies)["time"]
        if t < best_time - 1e-9:
            best_meal, best_time = meal, t
    best_event = None
    for event in ctx.events:
        stack = Stack(best_meal, base.relics, base.owner_multiplier, event)
        t = model.fight(stack.apply(hero, level, gear), enemies)["time"]
        if t < best_time - 1e-9:
            best_event, best_time = event, t
    return Stack(best_meal, base.relics, base.owner_multiplier, best_event), best_time


def tankiest_stack(ctx: Context, model, hero, level, gear, boss, biome, cycle, relics) -> tuple:
    """The meal + event that keep a parked hero alive longest (for the 12-40 s band's upper end)."""
    best, best_t = Stack(None, relics, ctx.owner_multiplier, None), 0.0
    for meal in [None] + [m for m in ctx.meals if meal_available(m, biome, cycle)]:
        for event in [None] + ctx.events:
            stack = Stack(meal, relics, ctx.owner_multiplier, event)
            t = parked_time_to_die(model, stack.apply(hero, level, gear), boss)
            if t > best_t:
                best, best_t = stack, t
    return best, best_t


def evaluate(ctx: Context, cycle: int, runs: int = 3000, base_gear: dict | None = None) -> dict:
    """Per biome: r8 vs full stack (best meal + relics so far + best event; cuddle = 0), calibrated in-game seconds."""
    model, visits, gear = cycle_setup(ctx, cycle, runs, base_gear=base_gear)
    out = {}
    for biome in rd.BIOME_ORDER:
        v = visits[biome]
        lvl_common = int(model.level_for_xp((v.entry_xp + v.boss_xp_before) / 2))
        lvl_boss = int(model.level_for_xp(v.boss_xp_before))
        g_common = gear[(biome, "entry")].plus(gear[(biome, "boss")]).scaled(0.5)
        g_boss = gear[(biome, "boss")]
        relics = tuple(relics_available(ctx.relics, biome, cycle))
        rows = {}
        for hero in bm.HEROES:
            commons_fight, boss_fight = model.common_fight(biome), model.boss_fight(biome)
            boss = model.biome_boss(biome)
            plain = Stack()
            r8_common = model.fight(plain.apply(hero, lvl_common, g_common), commons_fight)["time"]
            r8_boss = model.fight(plain.apply(hero, lvl_boss, g_boss), boss_fight)["time"]
            with_relics = Stack(None, relics, ctx.owner_multiplier)
            relic_boss = model.fight(with_relics.apply(hero, lvl_boss, g_boss), boss_fight)["time"]
            _, stack_common = best_stack(ctx, model, hero, lvl_common, g_common, commons_fight, biome, cycle, with_relics)
            stack, stack_boss = best_stack(ctx, model, hero, lvl_boss, g_boss, boss_fight, biome, cycle, with_relics)
            basic = model.fight(stack.apply(hero, lvl_boss, g_boss), [boss], use_skills=False)
            _, tank_ttd = tankiest_stack(ctx, model, hero, lvl_boss, g_boss, boss, biome, cycle, relics)
            rows[hero] = {
                "r8_common": r8_common * ctx.k_common, "r8_boss": r8_boss * ctx.k_boss,
                "relic_boss": relic_boss * ctx.k_boss,
                "stack_common": stack_common * ctx.k_common, "stack_boss": stack_boss * ctx.k_boss,
                "meal": stack.meal.id if stack.meal else None,
                "event": f"{stack.event.stat} +{stack.event.value:g}" if stack.event else None,
                "basic_kill": basic["time"], "basic_death": basic["death_time"],
                "parked_ttd_plain": parked_time_to_die(model, plain.apply(hero, lvl_boss, g_boss), boss),
                "parked_ttd_stack": parked_time_to_die(model, stack.apply(hero, lvl_boss, g_boss), boss),
                "parked_ttd_tank": tank_ttd,
            }
        out[biome] = {"levels": (lvl_common, lvl_boss), "heroes": rows, "relics": len(relics)}
    return out


def mean_of(result: dict, biome: str, key: str) -> float:
    return statistics.mean(r[key] for r in result[biome]["heroes"].values())


# The budget of the optional power sources (meals + relics + event buffs; the cuddle adds nothing in combat):
# a full stack may make the r8 quest-route hero this much faster, never more. -20% is about what a lucky (p90)
# weapon roll already does to a boss (§10.4); commons are shorter, so rounding to the 20 ms step weighs more.
BOSS_BUDGET = 0.78  # stacked boss TTK >= 78% of the r8 one
COMMON_BUDGET = 0.70  # stacked commons TTK >= 70% of the r8 one
BASIC_ONLY_EDGE = 0.9  # NG+0: a basic-attack-only stacked hero needs >= 90% of the time the boss needs to kill him


def biome_summary(result: dict, biome: str) -> dict:
    heroes = result[biome]["heroes"].values()
    return {
        "r8_common": statistics.mean(h["r8_common"] for h in heroes),
        "r8_boss": statistics.mean(h["r8_boss"] for h in heroes),
        "stack_common": statistics.mean(h["stack_common"] for h in heroes),
        "stack_boss": statistics.mean(h["stack_boss"] for h in heroes),
        "basic_kill": statistics.mean(h["basic_kill"] for h in heroes),
        "parked_ttd_stack": statistics.mean(h["parked_ttd_stack"] for h in heroes),
    }


def violations(result: dict, cycle: int) -> list:
    """The power budget (§14.3) a full offensive stack must keep, per biome (means of the 5 heroes):
    boss/commons TTK within BOSS_BUDGET/COMMON_BUDGET of the r8 hero; at NG+0 a basic-attack-only hero wins by a
    hair at best (kill >= BASIC_ONLY_EDGE x time-to-die) and the boss kills a parked hero in 12-40 s; at NG+1 in
    at least 12 s (the upper end is the NG+ scaling's, not the stack's: §14.4)."""
    out = []
    for biome in result:
        s = biome_summary(result, biome)
        if s["stack_boss"] < BOSS_BUDGET * s["r8_boss"]:
            out.append(f"{biome}: boss {s['stack_boss']:.1f} s < {BOSS_BUDGET:.0%} of r8 {s['r8_boss']:.1f} s")
        if s["stack_common"] < COMMON_BUDGET * s["r8_common"]:
            out.append(f"{biome}: commons {s['stack_common']:.1f} s < {COMMON_BUDGET:.0%} of r8 {s['r8_common']:.1f} s")
        if cycle == 0 and s["basic_kill"] < BASIC_ONLY_EDGE * s["parked_ttd_stack"]:
            out.append(f"{biome}: basic-only kill {s['basic_kill']:.1f} s < {BASIC_ONLY_EDGE:.0%} of time-to-die {s['parked_ttd_stack']:.1f} s")
        low, high = PARKED_TTD_BAND
        if s["parked_ttd_stack"] < low or (cycle == 0 and s["parked_ttd_stack"] > high):
            out.append(f"{biome}: parked time-to-die {s['parked_ttd_stack']:.1f} s outside {low}-{high}")
    return out


def target_misses(result: dict) -> list:
    """The r8 absolute targets (commons 4-7 s, bosses 15-22 s), informative: the budget above is what is enforced."""
    out = []
    for biome in result:
        s = biome_summary(result, biome)
        for key, target in (("stack_common", run_r8.TARGET["common"]), ("stack_boss", run_r8.TARGET["boss"])):
            if not target[0] - 0.05 <= s[key] <= target[1] + 0.05:
                out.append(f"{biome}: {key.split('_')[1]} {s[key]:.1f} s (meta {target[0]:g}-{target[1]:g})")
    return out


def f1(x):
    return run_r8.f1(x)


def report(results: dict) -> str:
    lines = ["| Ciclo | Bioma (níveis) | Relíquias | 2 comuns: r8 → pilha | Chefe: r8 → relíquias → pilha | "
             "Só básico com a pilha: mata / morre | Parado, chefe mata em: r8 → pilha (pilha mais defensiva) | Refeição/evento mais usados |",
             "|---|---|---|---|---|---|---|---|"]
    for cycle, result in results.items():
        for biome, row in result.items():
            heroes = row["heroes"].values()
            meals = statistics.mode([h["meal"] or "—" for h in heroes])
            events = statistics.mode([h["event"] or "—" for h in heroes])
            kill = statistics.mean(h["basic_kill"] for h in heroes)
            deaths = [h["basic_death"] for h in heroes if h["basic_death"] is not None]
            lines.append(
                f"| NG+{cycle} | {run_r8.NAMES[biome]} ({row['levels'][0]} / {row['levels'][1]}) | {row['relics']} | "
                f"{f1(mean_of(result, biome, 'r8_common'))} → **{f1(mean_of(result, biome, 'stack_common'))}** s | "
                f"{f1(mean_of(result, biome, 'r8_boss'))} → {f1(mean_of(result, biome, 'relic_boss'))} → **{f1(mean_of(result, biome, 'stack_boss'))}** s | "
                f"{f1(kill)} / {f1(statistics.mean(deaths)) if len(deaths) == len(heroes) else 'sobrevive'} s | "
                f"{f1(statistics.mean(h['parked_ttd_plain'] for h in heroes))} → {f1(statistics.mean(h['parked_ttd_stack'] for h in heroes))} "
                f"({f1(min(h['parked_ttd_tank'] for h in heroes))}–{f1(max(h['parked_ttd_tank'] for h in heroes))}) s | "
                f"{meals.replace('meal_', '')} / {events} |")
    return "\n".join(lines)


def budget_table(ctx: Context) -> str:
    """Each source's combat stats, as the model reads them."""
    lines = ["| Fonte | Efeito em combate | Quando |", "|---|---|---|"]
    for meal in ctx.meals:
        fx = ", ".join(f"{m.stat} {'+' if m.value >= 0 else ''}{m.value:g}{'%' if m.percent else ''}"
                       f"{f' ({m.seconds:g} s)' if m.seconds else ''}" for m in meal.effects + meal.burst)
        fav = ", ".join(f"{m.stat} +{m.value:g}" for m in meal.favourite)
        lines.append(f"| {meal.id} | {fx}; favorito ({meal.favourite_of}): {fav} | {meal.unlock_region or 'início'} |")
    for relic in ctx.relics:
        m = relic.modifier
        lines.append(f"| {relic.id} | {m.stat} +{m.value:g}{' (dono ×' + format(ctx.owner_multiplier, 'g') + ': ' + relic.hero + ')' if relic.hero else ''} | "
                     f"{relic.biome} ({relic.source}) |")
    for event in ctx.events:
        lines.append(f"| evento | {event.stat} +{event.value:g} | próxima luta |")
    cuddle = load_cuddle()
    lines.append(f"| carinho no Caramelo | {cuddle.stat} +{cuddle.value:g} por {cuddle.seconds:g} s (fora do modelo: 0 em combate) | Vila |")
    return "\n".join(lines)


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser()
    parser.add_argument("--runs", type=int, default=3000)
    args = parser.parse_args()
    ctx = load_context()
    base_gear = bm.gear_by_region(bm.Model(ctx.data), "quests", args.runs)
    results = {cycle: evaluate(ctx, cycle, args.runs, base_gear) for cycle in (0, 1)}
    print("## Fontes de poder (lidas do código)\n")
    print(budget_table(ctx))
    print("\n## Pilha completa: melhor refeição + relíquias + melhor evento (+ carinho = 0), rota com missões\n")
    print(report(results))
    print("\n## Orçamento da pilha (§14.3)\n")
    problems = [f"NG+{c}: {p}" for c, r in results.items() for p in violations(r, c)]
    print("\n".join(f"- {p}" for p in problems) if problems else "- dentro do orçamento")
    print("\n## Metas absolutas do r8 com a pilha (informativo)\n")
    misses = [f"NG+{c}: {p}" for c, r in results.items() for p in target_misses(r)]
    print("\n".join(f"- {p}" for p in misses) if misses else "- todas dentro")


if __name__ == "__main__":
    main()
