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
HUNTING_RULES = os.path.join(SCRIPTS, "Gameplay", "Hunting", "HuntingRules.cs")
HUNTING_CHARMS = os.path.join(SCRIPTS, "Gameplay", "Hunting", "HuntingCharmCatalog.cs")

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
    health_bonus: dict  # region -> one-off extra HP from NG+1 on (NewGamePlusRules.<Region>HealthBonus)
    health: float  # extra HP per cycle, every region
    damage_bonus: dict
    damage: float
    experience: float
    regionless: str = "Desert"  # NewGamePlusRules.RegionlessScaling

    def clamp(self, cycle: int) -> int:
        return max(0, min(self.max_cycle, cycle))

    def health_mult(self, cycle: int, region: str | None = None) -> float:
        c = self.clamp(cycle)
        return 1.0 if c == 0 else 1 + self.health_bonus[region or self.regionless] + self.health * c

    def damage_mult(self, cycle: int, region: str | None = None) -> float:
        c = self.clamp(cycle)
        return 1.0 if c == 0 else 1 + self.damage_bonus[region or self.regionless] + self.damage * c

    def xp_mult(self, cycle: int) -> float:
        return 1 + self.experience * self.clamp(cycle)


def load_ng_rules() -> NgRules:
    source = _read(NG_RULES)
    health = {b: _const(source, f"{b}HealthBonus") for b in rd.BIOME_ORDER}
    damage = {b: _const(source, f"{b}DamageBonus") for b in rd.BIOME_ORDER}
    regionless = re.search(r"RegionlessScaling\s*=\s*BiomeType\.(\w+)", source).group(1)
    return NgRules(int(_const(source, "MaxCycle")), health, _const(source, "HealthPerCycle"), damage,
                   _const(source, "DamagePerCycle"), _const(source, "ExperiencePerCycle"), regionless)


def region_of(data: rd.GameData, enemy_id: str) -> str | None:
    """BiomeRegistry.FindBiomeOf: the first region (BiomeType order) whose spawn table lists the enemy."""
    for biome in rd.BIOME_ORDER:
        if any(eid == enemy_id for eid, _ in data.biomes[biome].spawn):
            return biome
    return None


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


@dataclass
class Hunting:
    """Hunting bonuses that reach combat (Docs/Balance/README.md, section "Caça"), parsed from HuntingCharmCatalog.cs and HuntingRules.cs.

    Charms and the "Presa do dia" only work against commons (never bosses). Today they are all protective (less damage
    taken, dodge, heal on a win, longer stuns): none shortens a fight, so `best_vs_common` is 0. The commons of the
    Cavernas sit on a hit breakpoint where even +2% damage breaks COMMON_BUDGET, so any damage charm or prey
    damage pick added later is applied here, to every common of the fight (conservative), and fails the budget test."""
    damage_charms: dict  # archetype -> damage values of HuntingCharmEffect.*Damage charms
    protection_charms: dict  # archetype -> protection values (informative: commons' damage is not in the TTK budget)
    max_equipped: int
    prey_damage: float
    prey_protection: float
    max_protection: float

    def best_vs_common(self) -> float:
        """Best cord against one common: the damage charms of its archetype that fit, plus a prey damage pick."""
        best = max((sum(sorted(v, reverse=True)[:self.max_equipped]) for v in self.damage_charms.values()), default=0.0)
        return best + self.prey_damage

    def best_protection(self) -> float:
        best = max((sum(sorted(v, reverse=True)[:self.max_equipped]) for v in self.protection_charms.values()), default=0.0)
        return min(self.max_protection, best + self.prey_protection)


def _const_or(source: str, name: str, default: float) -> float:
    try:
        return _const(source, name)
    except ValueError:
        return default


def load_hunting() -> Hunting:
    charms = _read(HUNTING_CHARMS)
    rules = _read(HUNTING_RULES)
    consts = {name: float(value) for name, value in re.findall(r"const\s+float\s+(\w+)\s*=\s*([-\d.]+)f\s*;", charms)}
    damage: dict = {}
    protection: dict = {}
    spec = re.compile(r"new HuntingCharmSpec\(\w+,\s*HuntingCharmEffect\.(\w+),\s*(\w+|[\d.]+f?),\s*\w+(?:,\s*EnemyMotionArchetype\.(\w+))?\)")
    for effect, value, archetype in spec.findall(charms):
        v = consts[value] if value in consts else float(value.rstrip("f"))
        if "Damage" in effect:
            damage.setdefault(archetype or "any", []).append(v)
        elif "Protection" in effect:
            protection.setdefault(archetype or "any", []).append(v)
    return Hunting(damage, protection, int(_const(charms, "MaxEquipped")), _const_or(rules, "PreyDamageBonus", 0.0),
                   _const_or(rules, "PreyProtectionBonus", 0.0), _const_or(rules, "MaxProtection", 1.0))


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
    creature_damage: float = 0.0  # hunting charms + prey vs commons (§18), an AttackDamage % in the model

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
        g.ad_pct += self.creature_damage
        return build


def ng_data(data: rd.GameData, rules: NgRules, cycle: int) -> rd.GameData:
    """Enemies as EnemyController sees them on cycle `cycle`, by their region (HP rounded, damage to 0.1, before buffs)."""
    if cycle == 0:
        return data
    scaled = bm.clone_data(data)
    for enemy in scaled.enemies.values():
        region = region_of(data, enemy.id)
        enemy.max_health = round(enemy.max_health * rules.health_mult(cycle, region))
        enemy.attack_damage = round(enemy.attack_damage * rules.damage_mult(cycle, region), 1)
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
    hunting: Hunting | None = None

    @property
    def hunting_common(self) -> float:
        return self.hunting.best_vs_common() if self.hunting else 0.0


def load_context(data: rd.GameData | None = None, k_common: float | None = None, k_boss: float | None = None) -> Context:
    data = data or rd.load_all()
    if k_common is None or k_boss is None:
        k_common, k_boss, _ = run_r8.calibrate(data)
    relics, owner = load_relics()
    events = [e for e in load_event_buffs() if e.stat in COMBAT_STATS]
    return Context(data, load_ng_rules(), load_meals(), relics, owner, events, k_common, k_boss, load_hunting())


def cycle_setup(ctx: Context, cycle: int, runs: int, seed: int = 8, base_gear: dict | None = None, xp_mult: float = 1.0):
    """(model, visits by biome, gear) for the quest route on `cycle`: NG+0 = r8; NG+1 keeps the end-of-campaign
    level and the Caves gear (new drops ignored: conservative), kill/boss XP x the rules. `xp_mult` scales kill and
    boss XP on every cycle (the "fôlego diário" tier, §23: 1.3 = fôlego cheio the whole campaign)."""
    base_model = bm.Model(ctx.data)
    gear0 = base_gear or bm.gear_by_region(base_model, "quests", runs, seed)
    if cycle == 0:
        visits = bm.route_xp(base_model, "quests", kill_xp_mult=xp_mult)
        return base_model, {v.biome: v for v in visits}, gear0
    end = bm.route_xp(base_model, "quests", kill_xp_mult=xp_mult)[-1].exit_xp
    for c in range(1, cycle):
        end = bm.route_xp(bm.Model(ng_data(ctx.data, ctx.rules, c)), "quests", start_xp=end, kill_xp_mult=ctx.rules.xp_mult(c) * xp_mult)[-1].exit_xp
    model = bm.Model(ng_data(ctx.data, ctx.rules, cycle))
    visits = bm.route_xp(model, "quests", start_xp=end, kill_xp_mult=ctx.rules.xp_mult(cycle) * xp_mult)
    caves = gear0[("Caves", "boss")]
    gear = {(b, phase): caves for b in rd.BIOME_ORDER for phase in ("entry", "boss")}
    return model, {v.biome: v for v in visits}, gear


def best_stack(ctx: Context, model, hero, level, gear, enemies, biome, cycle, base: Stack) -> tuple:
    """Greedy: the meal that kills `enemies` fastest, then the event buff that does, on top of `base`."""
    best_meal, best_time = None, model.fight(base.apply(hero, level, gear), enemies)["time"]
    for meal in ctx.meals:
        if not meal_available(meal, biome, cycle):
            continue
        stack = Stack(meal, base.relics, base.owner_multiplier, base.event, base.creature_damage)
        t = model.fight(stack.apply(hero, level, gear), enemies)["time"]
        if t < best_time - 1e-9:
            best_meal, best_time = meal, t
    best_event = None
    for event in ctx.events:
        stack = Stack(best_meal, base.relics, base.owner_multiplier, event, base.creature_damage)
        t = model.fight(stack.apply(hero, level, gear), enemies)["time"]
        if t < best_time - 1e-9:
            best_event, best_time = event, t
    return Stack(best_meal, base.relics, base.owner_multiplier, best_event, base.creature_damage), best_time


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


def evaluate(ctx: Context, cycle: int, runs: int = 3000, base_gear: dict | None = None, stamina_mult: float = 1.0) -> dict:
    """Per biome: r8 vs full stack (best meal + relics so far + best event; cuddle = 0), calibrated in-game seconds.
    `stamina_mult` > 1: the stacked hero also levelled with that kill-XP tier ("fôlego cheio", §23) while the r8
    reference keeps the plain levels, so the budget covers the extra levels too."""
    model, visits, gear = cycle_setup(ctx, cycle, runs, base_gear=base_gear, xp_mult=stamina_mult)
    base_visits = visits if stamina_mult == 1.0 else cycle_setup(ctx, cycle, runs, base_gear=gear if cycle == 0 else base_gear)[1]
    out = {}
    for biome in rd.BIOME_ORDER:
        v = visits[biome]
        lvl_common = int(model.level_for_xp((v.entry_xp + v.boss_xp_before) / 2))
        lvl_boss = int(model.level_for_xp(v.boss_xp_before))
        bv = base_visits[biome]
        r8_lvl_common = int(model.level_for_xp((bv.entry_xp + bv.boss_xp_before) / 2))
        r8_lvl_boss = int(model.level_for_xp(bv.boss_xp_before))
        g_common = gear[(biome, "entry")].plus(gear[(biome, "boss")]).scaled(0.5)
        g_boss = gear[(biome, "boss")]
        relics = tuple(relics_available(ctx.relics, biome, cycle))
        rows = {}
        for hero in bm.HEROES:
            commons_fight, boss_fight = model.common_fight(biome), model.boss_fight(biome)
            boss = model.biome_boss(biome)
            plain = Stack()
            r8_common = model.fight(plain.apply(hero, r8_lvl_common, g_common), commons_fight)["time"]
            r8_boss = model.fight(plain.apply(hero, r8_lvl_boss, g_boss), boss_fight)["time"]
            with_relics = Stack(None, relics, ctx.owner_multiplier)
            relic_boss = model.fight(with_relics.apply(hero, lvl_boss, g_boss), boss_fight)["time"]
            hunted = Stack(None, relics, ctx.owner_multiplier, creature_damage=ctx.hunting_common)
            _, stack_common = best_stack(ctx, model, hero, lvl_common, g_common, commons_fight, biome, cycle, hunted)
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
                "parked_ttd_plain": parked_time_to_die(model, plain.apply(hero, r8_lvl_boss, g_boss), boss),
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
    hair at best (kill >= BASIC_ONLY_EDGE x time-to-die); on every cycle the boss kills a parked hero in 12-40 s."""
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
        if not low <= s["parked_ttd_stack"] <= high:
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
    if ctx.hunting is not None:
        lines.append(f"| amuletos do Caderno + presa do dia (seção Caça) | AttackDamage +{ctx.hunting_common:g} contra comuns (nenhum amuleto de dano); "
                     f"proteção até {ctx.hunting.best_protection():g} contra um tipo de bicho comum (fora do modelo de TTK) | "
                     f"páginas do Caderno / Seu Zé |")
    cuddle = load_cuddle()
    lines.append(f"| carinho no Caramelo | {cuddle.stat} +{cuddle.value:g} por {cuddle.seconds:g} s (fora do modelo: 0 em combate) | Vila |")
    return "\n".join(lines)


# ----------------------------------------------------------------------------- refine +0..+15 (README "Refino de item")

REFINE_RULES = os.path.join(SCRIPTS, "Gameplay", "Refining", "RefineRules.cs")
ITEMS_DIR = os.path.join(rd.ROOT, "Assets", "Resources", "Items")
# Refine level and Pedra da Vida option the budget assumes on EVERY piece (weapon + head/chest/boots) when the hero
# reaches each region: a player who farmed the stones and bought some at the Forja, i.e. an upper bound. The quest
# route alone drops fewer (README "Refino de item"). NG+1 keeps the Cavernas gear (cycle_setup), so also its refine.
REFINE_BY_REGION = {"Forest": (4, 0), "Desert": (6, 1), "Mountain": (7, 1), "Caves": (9, 2)}
REFINE_BOSS_BUDGET = 0.92  # r8 hero + refine: boss TTK >= 92% of the plain r8 one
# Commons die in 3-6 hits: one hit less is already -15..-20%, so their bands are wider (quantised, not a trend).
REFINE_COMMON_BUDGET = 0.80
FULL_BOSS_BUDGET = 0.74  # full stack + refine: boss TTK >= 74% of the plain r8 one (the whole optional ceiling)
FULL_COMMON_BUDGET = 0.55
REFINE_BASIC_EDGE = 0.75  # NG+0, stack + refine, basic attacks only: kill >= 75% of the time the boss needs (stack: 90%)
FULL_BOSS_FLOOR = 12.0  # NG+0 and NG+1: a stacked + refined hero still needs >= 12 s for any boss


@dataclass
class RefineCurve:
    stat_bonus: list  # cumulative fraction at +0..+15
    weapon_share: float
    weapon_option: float
    armour_option: float
    armour: dict  # equip slot -> best ArmorDefense among Resources/Items

    def refined(self, base: float, level: int) -> float:
        if base <= 0 or level <= 0:
            return base
        return round(base * (1 + self.stat_bonus[max(0, min(15, level))]) + 1e-9, 1)

    def gear(self, level: int, option: int) -> bm.Gear:
        """What RefineRules/InventoryManager.ApplyRefineEffects add with a weapon and every armour piece at `level`."""
        g = bm.Gear()
        g.ad_pct = self.stat_bonus[level] * self.weapon_share + option * self.weapon_option
        g.defense = sum(self.refined(d, level) - d for d in self.armour.values())
        g.max_hp_pct = len(self.armour) * option * self.armour_option
        return g


def load_refine() -> RefineCurve:
    source = _read(REFINE_RULES)
    body = re.search(r"StatBonuses\s*=\s*\{(.*?)\};", source, re.S).group(1)
    bonus = [float(v) for v in re.findall(r"([\d.]+)f", body)]
    armour = {}
    for name in sorted(os.listdir(ITEMS_DIR)):
        if not name.endswith(".asset"):
            continue
        text = _read(os.path.join(ITEMS_DIR, name))
        slot = int(re.search(r"_equipSlot: (\d+)", text).group(1))
        defense = float(re.search(r"_armorDefense: ([\d.]+)", text).group(1))
        if slot and defense > 0:
            armour[slot] = max(armour.get(slot, 0.0), defense)
    return RefineCurve(bonus, _const(source, "WeaponAttackShare"), _const(source, "WeaponOptionAttackPercent"),
                       _const(source, "ArmourOptionHealthPercent"), armour)


def refined_gear(base_gear: dict, curve: RefineCurve, levels: dict | None = None) -> dict:
    """base_gear (gear_by_region) with every region's expected refine added (other keys kept)."""
    levels = levels or REFINE_BY_REGION
    out = dict(base_gear)
    for biome in rd.BIOME_ORDER:
        for phase in ("entry", "boss"):
            out[(biome, phase)] = base_gear[(biome, phase)].plus(curve.gear(*levels[biome]))
    return out


def refine_violations(plain: dict, refined: dict, cycle: int) -> list:
    """README "Refino de item" (budget): the refined hero keeps the §14.3 stack budget (relative to the refined r8 hero; the basic-only edge loosens
    to REFINE_BASIC_EDGE), refine alone shortens the r8 fights by at most REFINE_*_BUDGET, stack + refine together by
    at most FULL_*_BUDGET, and no boss drops under FULL_BOSS_FLOOR seconds."""
    # The §14.3 stack budget on top of the refined r8 hero; the commons part is checked against the plain r8 hero below
    # (FULL_COMMON_BUDGET): on a refined hero the stack often removes exactly one hit, which reads as -35%.
    out = [f"stack on refined hero: {v}" for v in violations(refined, cycle) if "basic-only" not in v and "commons" not in v]
    for biome in plain:
        p, r = biome_summary(plain, biome), biome_summary(refined, biome)
        if cycle == 0 and r["basic_kill"] < REFINE_BASIC_EDGE * r["parked_ttd_stack"]:
            out.append(f"{biome}: basic-only kill {r['basic_kill']:.1f} s < {REFINE_BASIC_EDGE:.0%} of time-to-die {r['parked_ttd_stack']:.1f} s")
        if r["stack_boss"] < FULL_BOSS_FLOOR:
            out.append(f"{biome}: stack+refine boss {r['stack_boss']:.1f} s < {FULL_BOSS_FLOOR:g} s")
        if r["r8_boss"] < REFINE_BOSS_BUDGET * p["r8_boss"]:
            out.append(f"{biome}: refined boss {r['r8_boss']:.1f} s < {REFINE_BOSS_BUDGET:.0%} of r8 {p['r8_boss']:.1f} s")
        if r["r8_common"] < REFINE_COMMON_BUDGET * p["r8_common"]:
            out.append(f"{biome}: refined commons {r['r8_common']:.1f} s < {REFINE_COMMON_BUDGET:.0%} of r8 {p['r8_common']:.1f} s")
        if r["stack_boss"] < FULL_BOSS_BUDGET * p["r8_boss"]:
            out.append(f"{biome}: stack+refine boss {r['stack_boss']:.1f} s < {FULL_BOSS_BUDGET:.0%} of r8 {p['r8_boss']:.1f} s")
        if r["stack_common"] < FULL_COMMON_BUDGET * p["r8_common"]:
            out.append(f"{biome}: stack+refine commons {r['stack_common']:.1f} s < {FULL_COMMON_BUDGET:.0%} of r8 {p['r8_common']:.1f} s")
    return out


def refine_report(plain_results: dict, refined_results: dict) -> str:
    lines = ["| Ciclo | Bioma | Refino (opção) | Chefe: r8 → r8+refino → pilha+refino | 2 comuns: r8 → r8+refino → pilha+refino | "
             "Parado, chefe mata em (pilha+refino) |", "|---|---|---|---|---|---|"]
    for cycle in refined_results:
        for biome in refined_results[cycle]:
            p, r = biome_summary(plain_results[cycle], biome), biome_summary(refined_results[cycle], biome)
            level, option = REFINE_BY_REGION["Caves" if cycle > 0 else biome]
            lines.append(f"| NG+{cycle} | {run_r8.NAMES[biome]} | +{level} ({option}) | "
                         f"{f1(p['r8_boss'])} → {f1(r['r8_boss'])} → **{f1(r['stack_boss'])}** s | "
                         f"{f1(p['r8_common'])} → {f1(r['r8_common'])} → **{f1(r['stack_common'])}** s | {f1(r['parked_ttd_stack'])} s |")
    return "\n".join(lines)


DAILY_STAMINA = os.path.join(SCRIPTS, "Gameplay", "Daily", "DailyStaminaRules.cs")


def load_stamina() -> dict:
    """The "fôlego diário" (§23), parsed from DailyStaminaRules.cs: the tier multipliers (full/normal/tired) and the
    campaign pace of kill XP (`KillXpPace`, "1f / FullMultiplier" or a literal); kill XP = tier x pace."""
    text = _read(DAILY_STAMINA)
    out = {tier: _const(text, name) for tier, name in
           (("full", "FullMultiplier"), ("normal", "NormalMultiplier"), ("tired", "TiredMultiplier"))}
    if re.search(r"const\s+float\s+KillXpPace\s*=\s*1f\s*/\s*FullMultiplier\s*;", text):
        out["pace"] = 1.0 / out["full"]
    else:
        out["pace"] = _const(text, "KillXpPace")
    return out


def stamina_xp_mult(stamina: dict, tier: str = "full") -> float:
    """Kill-XP multiplier of a tier as the game applies it (EnemyRewardGranter): tier x campaign pace."""
    return stamina[tier] * stamina["pace"]


# ----------------------------------------------------------------------------- wings + fruits (README "Tacho Encantado e asas")

WING_CATALOG = os.path.join(SCRIPTS, "Gameplay", "Tacho", "WingCatalog.cs")
FRUIT_RULES = os.path.join(SCRIPTS, "Gameplay", "Tacho", "FruitRules.cs")
ATTRIBUTE_SET = os.path.join(SCRIPTS, "Gameplay", "Player", "AttributeSet.cs")
# Wing level and Frutas do Mandacaru eaten that the budget assumes when the hero reaches each region: a player who farmed
# the Pedras do Caos (they only drop from the Montanha boss on: 15%, Cavernas 1% / boss 25%) and won every Tacho roll, i.e.
# an upper bound. Each wing level 1 / fruit costs 1 Caos, level 2 costs 2 more: with ~3 stones by the end of the Cavernas
# the most offensive spend is wings 1 + both fruits. NG+1 keeps the Cavernas gear (cycle_setup), so it gets WINGS_NG1 and
# the fruit cap.
WINGS_BY_REGION = {"Forest": 0, "Desert": 0, "Mountain": 1, "Caves": 1}
WINGS_NG1 = 3
FRUITS_BY_REGION = {"Forest": 0, "Desert": 0, "Mountain": 0, "Caves": 2}
WINGS_BOSS_BUDGET = 0.95  # refined r8 hero + wings + fruits: boss TTK >= 95% of the refined r8 one
WINGS_COMMON_BUDGET = 0.75  # NG+1 commons die in 2-3 hits: +4% damage already removes one (-21%, quantised; README)
ALL_BOSS_BUDGET = 0.70  # full stack + refine + wings + fruits: boss TTK >= 70% of the plain r8 one
ALL_COMMON_BUDGET = 0.50
ALL_BASIC_EDGE = 0.75  # NG+0, everything on, basic attacks only: kill >= 75% of the time the boss needs (as the refine)
ALL_BOSS_FLOOR = 12.0  # no boss under 12 s, NG+0 and NG+1


@dataclass
class WingCurve:
    """WingCatalog.cs (styles at level 1, level scale, absorb) + the fruit cap and Poder Arcano per point."""
    styles: dict  # style -> (damage, defense, move speed) at level 1
    level_scale: list  # index = wing level (0 unused, 4 = legendary, documented only)
    absorb: list
    fruit_cap: int
    arcane_pct: float

    def damage(self, level: int) -> float:
        """Best AttackDamage % of any style at `level` (0 = no wings)."""
        return max(d for d, _, _ in self.styles.values()) * self.level_scale[level] if level > 0 else 0.0

    def defense(self, level: int) -> float:
        """Best Defense % of any style at `level` plus the absorb (same percent channel)."""
        return max(f for _, f, _ in self.styles.values()) * self.level_scale[level] + self.absorb[level] if level > 0 else 0.0

    def gear(self, level: int, fruits: int) -> bm.Gear:
        """Upper bound: the most offensive style's damage AND the most defensive style's defense on the same wing, plus the
        fruits as Poder Arcano points (the most offensive spend). MoveSpeed: not modelled (no movement)."""
        g = bm.Gear()
        g.ad_pct = self.damage(level) + min(fruits, self.fruit_cap) * self.arcane_pct
        g.defense_pct = self.defense(level)
        return g


def load_wings() -> WingCurve:
    source = _read(WING_CATALOG)
    styles = {}
    for style, damage, defense, move in re.findall(
            r"new WingStyleSpec\(WingStyle\.(\w+),\s*CharacterId\.\w+,\s*\"\w+\",\s*([\d.]+)f,\s*([\d.]+)f,\s*([\d.]+)f", source):
        styles[style] = (float(damage), float(defense), float(move))

    def table(name: str) -> list:
        body = re.search(name + r"\s*=\s*\{(.*?)\};", source, re.S).group(1)
        return [float(v) for v in re.findall(r"([\d.]+)f", body)]

    attributes = _read(ATTRIBUTE_SET)
    return WingCurve(styles, table("LevelScale"), table("AbsorbByLevel"), int(_const(_read(FRUIT_RULES), "MaxGrowthPoints")),
                     _const(attributes, "ArcanePowerAttackDamagePerPoint"))


def winged_gear(base_gear: dict, refine: RefineCurve, wings: WingCurve, cycle: int) -> dict:
    """refined_gear + each region's wings and fruits. NG+1 reads only the Cavernas boss gear, which gets WINGS_NG1 and the cap."""
    out = refined_gear(base_gear, refine)
    for biome in rd.BIOME_ORDER:
        level, fruits = (WINGS_NG1, wings.fruit_cap) if cycle > 0 else (WINGS_BY_REGION[biome], FRUITS_BY_REGION[biome])
        for phase in ("entry", "boss"):
            out[(biome, phase)] = out[(biome, phase)].plus(wings.gear(level, fruits))
    return out


def wings_violations(plain: dict, refined: dict, winged: dict, cycle: int) -> list:
    """README "Tacho Encantado e asas": wings + fruits on the refined r8 hero cost at most WINGS_*_BUDGET; everything
    (stack + refine + wings + fruits) at most ALL_*_BUDGET of the plain r8 hero; no boss under ALL_BOSS_FLOOR; at NG+0 a
    basic-attack-only hero keeps ALL_BASIC_EDGE; parked, the boss still kills in 12-40 s."""
    out = []
    low, high = PARKED_TTD_BAND
    for biome in plain:
        p, r, w = biome_summary(plain, biome), biome_summary(refined, biome), biome_summary(winged, biome)
        if w["r8_boss"] < WINGS_BOSS_BUDGET * r["r8_boss"]:
            out.append(f"{biome}: wings boss {w['r8_boss']:.1f} s < {WINGS_BOSS_BUDGET:.0%} of refined r8 {r['r8_boss']:.1f} s")
        if w["r8_common"] < WINGS_COMMON_BUDGET * r["r8_common"]:
            out.append(f"{biome}: wings commons {w['r8_common']:.1f} s < {WINGS_COMMON_BUDGET:.0%} of refined r8 {r['r8_common']:.1f} s")
        if w["stack_boss"] < ALL_BOSS_BUDGET * p["r8_boss"]:
            out.append(f"{biome}: everything boss {w['stack_boss']:.1f} s < {ALL_BOSS_BUDGET:.0%} of r8 {p['r8_boss']:.1f} s")
        if w["stack_common"] < ALL_COMMON_BUDGET * p["r8_common"]:
            out.append(f"{biome}: everything commons {w['stack_common']:.1f} s < {ALL_COMMON_BUDGET:.0%} of r8 {p['r8_common']:.1f} s")
        if w["stack_boss"] < ALL_BOSS_FLOOR:
            out.append(f"{biome}: everything boss {w['stack_boss']:.1f} s < {ALL_BOSS_FLOOR:g} s")
        if cycle == 0 and w["basic_kill"] < ALL_BASIC_EDGE * w["parked_ttd_stack"]:
            out.append(f"{biome}: basic-only kill {w['basic_kill']:.1f} s < {ALL_BASIC_EDGE:.0%} of time-to-die {w['parked_ttd_stack']:.1f} s")
        if not low <= w["parked_ttd_stack"] <= high:
            out.append(f"{biome}: parked time-to-die {w['parked_ttd_stack']:.1f} s outside {low}-{high}")
    return out


def wings_report(plain_results: dict, refined_results: dict, winged_results: dict) -> str:
    lines = ["| Ciclo | Bioma | Asas (frutas) | Chefe: r8 → r8+refino → r8+refino+asas → tudo | 2 comuns: r8 → r8+refino+asas → tudo | "
             "Parado, chefe mata em (tudo) |", "|---|---|---|---|---|---|"]
    for cycle in winged_results:
        for biome in winged_results[cycle]:
            p = biome_summary(plain_results[cycle], biome)
            r = biome_summary(refined_results[cycle], biome)
            w = biome_summary(winged_results[cycle], biome)
            level, fruits = (WINGS_NG1, "cap") if cycle > 0 else (WINGS_BY_REGION[biome], FRUITS_BY_REGION[biome])
            lines.append(f"| NG+{cycle} | {run_r8.NAMES[biome]} | {level or '—'} ({fruits}) | "
                         f"{f1(p['r8_boss'])} → {f1(r['r8_boss'])} → {f1(w['r8_boss'])} → **{f1(w['stack_boss'])}** s | "
                         f"{f1(p['r8_common'])} → {f1(w['r8_common'])} → **{f1(w['stack_common'])}** s | {f1(w['parked_ttd_stack'])} s |")
    return "\n".join(lines)


def wings_main(ctx: Context, runs: int, base_gear: dict, results: dict, refined: dict, refine: RefineCurve) -> None:
    wings = load_wings()
    winged = {cycle: evaluate(ctx, cycle, runs, winged_gear(base_gear, refine, wings, cycle)) for cycle in (0, 1)}
    print("\n## Asas e Frutas do Sertão por região (README: Tacho Encantado e asas)\n")
    print(wings_report(results, refined, winged))
    print("\n## Orçamento das asas\n")
    problems = [f"NG+{c}: {p}" for c in winged for p in wings_violations(results[c], refined[c], winged[c], c)]
    print("\n".join(f"- {p}" for p in problems) if problems else "- dentro do orçamento")


# ----------------------------------------------------------------------------- MU-style item options (README "Opções de item")

ITEM_OPTION_CATALOG = os.path.join(SCRIPTS, "Gameplay", "Items", "Options", "ItemOptionCatalog.cs")
_MOD = r"Mod\(StatType\.(\w+),\s*ModifierApplication\.(\w+),\s*([-\d.]+)f\)"
# Pieces (of weapon + hat + chest + boots) that carry options when the hero reaches each region: a player who farmed
# (excellent off a common enemy is 1.5% per equipment drop, ancient only off bosses), i.e. an upper bound. Every piece
# with options is assumed perfect: max excellent lines, luck and both sockets engraved with the best engraving.
OPTION_PIECES_BY_REGION = {"Forest": 1, "Desert": 2, "Mountain": 3, "Caves": 4}
OPTIONS_BOSS_BUDGET = 0.90  # everything + perfect options: boss TTK >= 90% of everything without options (+1% damage is already -4%: rotation steps)
OPTIONS_COMMON_BUDGET = 0.70  # NG+1 commons die in 2-3 hits: one hit less is -26% (quantised, as the wings)
TOTAL_BOSS_BUDGET = 0.66  # stack + refine + wings + fruits + options: boss TTK >= 66% of the plain r8 one
TOTAL_BOSS_FLOOR = 12.0


def _option_mod(stat: str, application: str, value: str) -> Modifier:
    return Modifier(stat, application != "Flat", float(value))


@dataclass
class OptionCatalog:
    """ItemOptionCatalog.cs: excellent lines (weapon/armour), luck, ancient sets (tiers), engravings, caps."""
    weapon_lines: list
    armour_lines: list
    luck: Modifier
    sets: dict  # id -> [(pieces, Modifier)]
    engravings: dict  # id -> (weapon only?, Modifier)
    lines_max: int
    sockets_max: int

    @staticmethod
    def as_gear(mods: list) -> bm.Gear:
        g = bm.Gear()
        for mod in mods:
            add_modifier(g, None, mod)  # whole-fight modifiers only: the build is never touched
        return g

    @staticmethod
    def offense(g: bm.Gear) -> float:
        """The offensive part of Gear.score (the budget is about kill time, so the upper bound picks damage)."""
        return g.ad_flat + g.ad_pct * 10 + g.attack_speed * 12 + g.crit * 8 + g.crit_dmg * 1.5 + g.thorns * 6 + g.cdr * 8

    def _best(self, candidates: list, count: int) -> list:
        """The `count` most offensive candidates."""
        return sorted(candidates, key=lambda m: -self.offense(self.as_gear([m])))[:count]

    def piece_extras(self, weapon: bool) -> list:
        """Luck + every socket with the best engraving that fits the piece (offensive ones only go in weapons)."""
        fits = [mod for for_weapon, mod in self.engravings.values() if for_weapon == weapon]
        return [self.luck] + self._best(fits, 1) * self.sockets_max

    def excellent_gear(self, pieces: int) -> bm.Gear:
        """`pieces` perfect excellent pieces: the weapon first (best lines_max weapon lines), then armour."""
        mods = []
        for i in range(pieces):
            mods += self._best(self.weapon_lines if i == 0 else self.armour_lines, self.lines_max) + self.piece_extras(i == 0)
        return self.as_gear(mods)

    def ancient_gear(self, pieces: int) -> bm.Gear:
        """`pieces` pieces of the strongest ancient set (all active tiers) + luck and sockets on each."""
        best = None
        for tiers in self.sets.values():
            mods = [m for need, m in tiers if pieces >= need]
            for i in range(pieces):
                mods += self.piece_extras(i == 0)
            g = self.as_gear(mods)
            if best is None or self.offense(g) > self.offense(best):
                best = g
        return best

    def gear(self, pieces: int, kind: str) -> bm.Gear:
        return self.excellent_gear(pieces) if kind == "excellent" else self.ancient_gear(pieces)


def load_options() -> OptionCatalog:
    source = _read(ITEM_OPTION_CATALOG)
    weapon, armour = [], []
    for _, is_weapon, stat, application, value in re.findall(
            r"new ExcellentLine\(ExcellentOption\.(\w+),\s*(true|false),\s*" + _MOD, source):
        (weapon if is_weapon == "true" else armour).append(_option_mod(stat, application, value))
    luck = _option_mod(*re.search(r"LuckModifier\s*=\s*" + _MOD, source).groups())
    sets = {}
    for block in source.split("new AncientSet(")[1:]:
        set_id = re.match(r'\s*"(\w+)"', block).group(1)
        sets[set_id] = [(int(n), _option_mod(s, a, v)) for n, s, a, v in
                        re.findall(r"new AncientTier\((\d+),\s*" + _MOD, block.split("})")[0])]
    engravings = {e: (w == "true", _option_mod(s, a, v)) for e, w, s, a, v in
                  re.findall(r'new Engraving\("(\w+)",\s*(true|false),\s*"\w+",\s*\d+,\s*\d+,\s*' + _MOD, source)}
    return OptionCatalog(weapon, armour, luck, sets, engravings,
                         int(_const(source, "ExcellentLinesBossMax")), int(_const(source, "MaxSockets")))


def optioned_gear(base_gear: dict, refine: RefineCurve, wings: WingCurve, options: OptionCatalog, cycle: int, kind: str) -> dict:
    """winged_gear + each region's perfect options of `kind` ("excellent" or "ancient"). NG+1: all 4 pieces."""
    out = winged_gear(base_gear, refine, wings, cycle)
    for biome in rd.BIOME_ORDER:
        extra = options.gear(4 if cycle > 0 else OPTION_PIECES_BY_REGION[biome], kind)
        for phase in ("entry", "boss"):
            out[(biome, phase)] = out[(biome, phase)].plus(extra)
    return out


def options_violations(plain: dict, winged: dict, optioned: dict, cycle: int) -> list:
    """README "Opções de item": perfect options on top of everything cost at most OPTIONS_*_BUDGET; the whole pile at
    most TOTAL_BOSS_BUDGET of the plain r8 hero; no boss under TOTAL_BOSS_FLOOR; parked, the boss still kills in 12-40 s."""
    out = []
    low, high = PARKED_TTD_BAND
    for biome in plain:
        p, w, o = biome_summary(plain, biome), biome_summary(winged, biome), biome_summary(optioned, biome)
        if o["stack_boss"] < OPTIONS_BOSS_BUDGET * w["stack_boss"]:
            out.append(f"{biome}: options boss {o['stack_boss']:.1f} s < {OPTIONS_BOSS_BUDGET:.0%} of everything {w['stack_boss']:.1f} s")
        if o["stack_common"] < OPTIONS_COMMON_BUDGET * w["stack_common"]:
            out.append(f"{biome}: options commons {o['stack_common']:.1f} s < {OPTIONS_COMMON_BUDGET:.0%} of everything {w['stack_common']:.1f} s")
        if o["stack_boss"] < TOTAL_BOSS_BUDGET * p["r8_boss"]:
            out.append(f"{biome}: total boss {o['stack_boss']:.1f} s < {TOTAL_BOSS_BUDGET:.0%} of r8 {p['r8_boss']:.1f} s")
        if o["stack_boss"] < TOTAL_BOSS_FLOOR:
            out.append(f"{biome}: total boss {o['stack_boss']:.1f} s < {TOTAL_BOSS_FLOOR:g} s")
        if not low <= o["parked_ttd_stack"] <= high:
            out.append(f"{biome}: parked time-to-die {o['parked_ttd_stack']:.1f} s outside {low}-{high}")
    return out


def options_report(plain_results: dict, winged_results: dict, optioned: dict) -> str:
    lines = ["| Ciclo | Bioma | Peças | Chefe: r8 → tudo → tudo+excelente / tudo+antigo | 2 comuns: tudo → excelente / antigo | "
             "Parado, chefe mata em (excelente / antigo) |", "|---|---|---|---|---|---|"]
    for cycle in winged_results:
        for biome in winged_results[cycle]:
            p = biome_summary(plain_results[cycle], biome)
            w = biome_summary(winged_results[cycle], biome)
            e = biome_summary(optioned["excellent"][cycle], biome)
            a = biome_summary(optioned["ancient"][cycle], biome)
            pieces = 4 if cycle > 0 else OPTION_PIECES_BY_REGION[biome]
            lines.append(f"| NG+{cycle} | {run_r8.NAMES[biome]} | {pieces} | {f1(p['r8_boss'])} → {f1(w['stack_boss'])} → "
                         f"**{f1(e['stack_boss'])}** / **{f1(a['stack_boss'])}** s | {f1(w['stack_common'])} → "
                         f"**{f1(e['stack_common'])}** / **{f1(a['stack_common'])}** s | "
                         f"{f1(e['parked_ttd_stack'])} / {f1(a['parked_ttd_stack'])} s |")
    return "\n".join(lines)


def options_main(ctx: Context, runs: int, base_gear: dict, results: dict, refine: RefineCurve) -> None:
    wings, options = load_wings(), load_options()
    winged = {c: evaluate(ctx, c, runs, winged_gear(base_gear, refine, wings, c)) for c in (0, 1)}
    optioned = {kind: {c: evaluate(ctx, c, runs, optioned_gear(base_gear, refine, wings, options, c, kind)) for c in (0, 1)}
                for kind in ("excellent", "ancient")}
    print("\n## Opções de item (excelente, sorte, conjunto antigo, engaste) por região (README: Opções de item)\n")
    print(options_report(results, winged, optioned))
    print("\n## Orçamento das opções\n")
    problems = [f"NG+{c} {kind}: {p}" for kind in optioned for c in (0, 1)
                for p in options_violations(results[c], winged[c], optioned[kind][c], c)]
    print("\n".join(f"- {p}" for p in problems) if problems else "- dentro do orçamento")


# ----------------------------------------------------------------------------- class evolution (Docs/ClassEvolution.md)

CLASS_CATALOG = os.path.join(SCRIPTS, "Gameplay", "ClassEvolution", "ClassEvolutionCatalog.cs")
MASTER_RULES = os.path.join(SCRIPTS, "Gameplay", "ClassEvolution", "MasterLevelRules.cs")
CLASS_RECOVERY = 0.2  # GraduationSkillFactory: _recoverySeconds
CLASS_MAX_LEVEL = 5  # GraduationSkillFactory: _maxLevel
CLASS_BOSS_BUDGET = 0.88  # r8 hero + graduations (skills, every group buff, best combo): boss TTK >= 88% of the r8 one
CLASS_COMMON_BUDGET = 0.75  # commons die in a few hits: one hit less is already -15..-20% (quantised, as the refine)
CLASS_ALL_BOSS_BUDGET = 0.70  # meal/relic/event stack + graduations: boss TTK >= 70% of the r8 one (as "everything" of the wings)
CLASS_ALL_COMMON_BUDGET = 0.55
CLASS_BASIC_EDGE = 0.75  # NG+0, stack + graduations, basic attacks only: kill >= 75% of the time the boss needs
CLASS_BOSS_FLOOR = 12.0
MASTER_BOSS_BUDGET = 0.95  # the whole master tree (30 points) on top of everything else: boss TTK >= 95% of without it


@dataclass
class ClassSkill:
    id: str
    hero: str
    tier: int
    kind: str  # GroupBuff | Active
    cooldown: float
    mana: float
    cast: float
    damage: float
    per_level: float
    area: float
    stun: float
    heal: float
    stat: str
    percent: bool
    value: float
    duration: float


@dataclass
class ClassEvolution:
    skills: list  # [ClassSkill]
    min_level: dict  # (hero, tier) -> level
    combos: list  # [(hero, bonus type, value, duration, last skill id)]
    master_nodes: list  # [(node id, Modifier at max rank, max rank)]
    combo_tier: int

    def level_of(self, skill: ClassSkill) -> int:
        return self.min_level[(skill.hero, skill.tier)]

    def gear(self, level: int) -> bm.Gear:
        """What the graduations add at `level`, hero-independent upper bound: a party of the five cousins (FASE 12) keeps
        EVERY tier-1 group buff up the whole fight (each lasts 12-15 s, about one boss fight), plus the best damage combo
        and the best cost combo at their uptime (bonus seconds / cooldown of the combo's last skill)."""
        g, build = bm.Gear(), bm.Build("Maithe", level)
        for s in self.skills:
            if s.kind == "GroupBuff" and level >= self.level_of(s):
                add_modifier(g, build, Modifier(s.stat, s.percent, s.value))
        best = {"DamageMultiplier": 0.0, "CostReduction": 0.0}
        by_id = {s.id: s for s in self.skills}
        for hero, kind, value, duration, last in self.combos:
            if kind in best and level >= self.min_level[(hero, self.combo_tier)]:
                best[kind] = max(best[kind], value * min(1.0, duration / by_id[last].cooldown))
        g.ad_pct += best["DamageMultiplier"]
        g.rcr += best["CostReduction"]
        return g

    def master_gear(self) -> bm.Gear:
        g, build = bm.Gear(), bm.Build("Maithe", 50)
        for _, mod, _ in self.master_nodes:
            add_modifier(g, build, mod)
        return g


def load_class_evolution() -> ClassEvolution:
    source = _read(CLASS_CATALOG)
    num = r"([-\d.]+)f"
    skills = []
    pattern = (r'Skill\("(\w+)",\s*CharacterId\.(\w+),\s*(\d+),\s*GraduationSkillKind\.(\w+),\s*' + r",\s*".join([num] * 8)
               + r",\s*StatType\.(\w+),\s*ModifierApplication\.(\w+),\s*" + num + r",\s*" + num + r"\)")
    for m in re.finditer(pattern, source):
        g = m.groups()
        skills.append(ClassSkill(g[0], g[1], int(g[2]), g[3], *map(float, g[4:12]), g[12], g[13] == "PercentAdditive",
                                 float(g[14]), float(g[15])))
    min_level = {(h, int(t)): int(lv) for h, t, lv in re.findall(r"Grad\(CharacterId\.(\w+),\s*(\d+),\s*(\d+),", source)}
    combos = [(h, kind, float(v), float(d), ids.split(",")[-1].strip().strip('"'))
              for h, ids, kind, v, d in re.findall(
                  r'Combo\("\w+",\s*CharacterId\.(\w+),\s*new\[\]\s*\{([^}]*)\},\s*[\d.]+f,\s*ComboBonusType\.(\w+),\s*([\d.]+)f,\s*([\d.]+)f\)',
                  source)]
    master = []
    for node, stat, app, value, rank in re.findall(
            r'new MasterNodeSpec\("(\w+)",\s*StatType\.(\w+),\s*ModifierApplication\.(\w+),\s*([\d.]+)f,\s*(\d+)', _read(MASTER_RULES)):
        master.append((node, Modifier(stat, app == "PercentAdditive", float(value) * int(rank)), int(rank)))
    return ClassEvolution(skills, min_level, combos, master, int(_const(source, "ComboTier")))


def class_data(data: rd.GameData, cls: ClassEvolution) -> rd.GameData:
    """GameData with every graduation active skill in its hero's list, unlocked at the graduation's level (the model
    learns a skill at min_player_level: an upper bound, the mission itself takes a while)."""
    out = bm.clone_data(data)
    for s in cls.skills:
        if s.kind != "Active":
            continue
        out.skills.setdefault(s.hero, []).append(rd.Skill(
            id=s.id, hero=s.hero, skill_type=0, cooldown=s.cooldown, mana=s.mana, cast=s.cast, recovery=CLASS_RECOVERY,
            base_damage=s.damage, damage_per_level=s.per_level, area=s.area, max_level=CLASS_MAX_LEVEL,
            min_player_level=cls.level_of(s), dot_total=0.0, stun=s.stun))
    return out


def class_context(ctx: Context, cls: ClassEvolution) -> Context:
    return load_context(class_data(ctx.data, cls), ctx.k_common, ctx.k_boss)


def class_gear(base_gear: dict, cls: ClassEvolution, levels: dict, master: bool = False) -> dict:
    """base_gear + the graduations each region allows at its boss level (`levels`: biome -> boss level)."""
    out = dict(base_gear)
    for biome in rd.BIOME_ORDER:
        extra = cls.gear(levels[biome])
        if master:
            extra = extra.plus(cls.master_gear())
        for phase in ("entry", "boss"):
            out[(biome, phase)] = base_gear[(biome, phase)].plus(extra)
    return out


def levels_of(result: dict) -> dict:
    return {biome: result[biome]["levels"][1] for biome in result}


def class_violations(plain: dict, classed: dict, cycle: int) -> list:
    """Docs/ClassEvolution.md §6: graduations on the r8 hero cost at most CLASS_*_BUDGET; stack + graduations at most
    CLASS_ALL_*_BUDGET of the plain r8 hero; no boss under CLASS_BOSS_FLOOR; at NG+0 a basic-attack-only stacked hero keeps
    CLASS_BASIC_EDGE; parked, the boss still kills in 12-40 s."""
    out = []
    low, high = PARKED_TTD_BAND
    for biome in plain:
        p, c = biome_summary(plain, biome), biome_summary(classed, biome)
        if c["r8_boss"] < CLASS_BOSS_BUDGET * p["r8_boss"]:
            out.append(f"{biome}: class boss {c['r8_boss']:.1f} s < {CLASS_BOSS_BUDGET:.0%} of r8 {p['r8_boss']:.1f} s")
        if c["r8_common"] < CLASS_COMMON_BUDGET * p["r8_common"]:
            out.append(f"{biome}: class commons {c['r8_common']:.1f} s < {CLASS_COMMON_BUDGET:.0%} of r8 {p['r8_common']:.1f} s")
        if c["stack_boss"] < CLASS_ALL_BOSS_BUDGET * p["r8_boss"]:
            out.append(f"{biome}: stack+class boss {c['stack_boss']:.1f} s < {CLASS_ALL_BOSS_BUDGET:.0%} of r8 {p['r8_boss']:.1f} s")
        if c["stack_common"] < CLASS_ALL_COMMON_BUDGET * p["r8_common"]:
            out.append(f"{biome}: stack+class commons {c['stack_common']:.1f} s < {CLASS_ALL_COMMON_BUDGET:.0%} of r8 {p['r8_common']:.1f} s")
        if c["stack_boss"] < CLASS_BOSS_FLOOR:
            out.append(f"{biome}: stack+class boss {c['stack_boss']:.1f} s < {CLASS_BOSS_FLOOR:g} s")
        if cycle == 0 and c["basic_kill"] < CLASS_BASIC_EDGE * c["parked_ttd_stack"]:
            out.append(f"{biome}: basic-only kill {c['basic_kill']:.1f} s < {CLASS_BASIC_EDGE:.0%} of time-to-die {c['parked_ttd_stack']:.1f} s")
        if not low <= c["parked_ttd_stack"] <= high:
            out.append(f"{biome}: parked time-to-die {c['parked_ttd_stack']:.1f} s outside {low}-{high}")
    return out


def master_violations(classed: dict, mastered: dict) -> list:
    """The full master tree (stress test on NG+1) shortens the stacked + graduated boss fight by at most MASTER_BOSS_BUDGET."""
    out = []
    for biome in mastered:
        c, m = biome_summary(classed, biome), biome_summary(mastered, biome)
        if m["stack_boss"] < MASTER_BOSS_BUDGET * c["stack_boss"]:
            out.append(f"{biome}: master tree boss {m['stack_boss']:.1f} s < {MASTER_BOSS_BUDGET:.0%} of {c['stack_boss']:.1f} s")
        if m["stack_boss"] < CLASS_BOSS_FLOOR:
            out.append(f"{biome}: master tree boss {m['stack_boss']:.1f} s < {CLASS_BOSS_FLOOR:g} s")
    return out


def class_evaluate(ctx: Context, cls: ClassEvolution, runs: int, base_gear: dict, plain: dict) -> tuple:
    """(classed, mastered) per cycle: graduations by level; mastered = classed + the full master tree on NG+1 (a stress
    test: the tree needs level 50, past the modelled campaign)."""
    cctx = class_context(ctx, cls)
    classed = {c: evaluate(cctx, c, runs, class_gear(base_gear, cls, levels_of(plain[c]))) for c in plain}
    mastered = {c: evaluate(cctx, c, runs, class_gear(base_gear, cls, levels_of(plain[c]), master=True)) for c in plain if c > 0}
    return classed, mastered


def class_report(plain: dict, classed: dict, mastered: dict) -> str:
    lines = ["| Ciclo | Bioma (nível do chefe) | Chefe: r8 → r8+classe → pilha+classe (+árvore de mestre) | "
             "2 comuns: r8 → r8+classe → pilha+classe | Parado, chefe mata em (pilha+classe) |", "|---|---|---|---|---|"]
    for cycle in classed:
        for biome in classed[cycle]:
            p, c = biome_summary(plain[cycle], biome), biome_summary(classed[cycle], biome)
            m = f" ({f1(biome_summary(mastered[cycle], biome)['stack_boss'])})" if cycle in mastered else ""
            lines.append(f"| NG+{cycle} | {run_r8.NAMES[biome]} ({plain[cycle][biome]['levels'][1]}) | "
                         f"{f1(p['r8_boss'])} → {f1(c['r8_boss'])} → **{f1(c['stack_boss'])}**{m} s | "
                         f"{f1(p['r8_common'])} → {f1(c['r8_common'])} → **{f1(c['stack_common'])}** s | {f1(c['parked_ttd_stack'])} s |")
    return "\n".join(lines)


def class_main(ctx: Context, runs: int, base_gear: dict, results: dict) -> None:
    cls = load_class_evolution()
    classed, mastered = class_evaluate(ctx, cls, runs, base_gear, results)
    print("\n## Evolução de classe: graduações + combo + buffs de grupo (Docs/ClassEvolution.md §6)\n")
    print(class_report(results, classed, mastered))
    print("\n## Orçamento da evolução de classe\n")
    problems = [f"NG+{c}: {p}" for c in classed for p in class_violations(results[c], classed[c], c)]
    problems += [f"NG+{c}: {p}" for c in mastered for p in master_violations(classed[c], mastered[c])]
    print("\n".join(f"- {p}" for p in problems) if problems else "- dentro do orçamento")


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
    curve = load_refine()
    refined = {cycle: evaluate(ctx, cycle, args.runs, refined_gear(base_gear, curve)) for cycle in (0, 1)}
    print("\n## Refino +0..+15 por região (README: Refino de item)\n")
    print(refine_report(results, refined))
    print("\n## Orçamento do refino\n")
    rproblems = [f"NG+{c}: {p}" for c in refined for p in refine_violations(results[c], refined[c], c)]
    print("\n".join(f"- {p}" for p in rproblems) if rproblems else "- dentro do orçamento")
    stamina = load_stamina()
    mult = stamina_xp_mult(stamina)
    full = {cycle: evaluate(ctx, cycle, args.runs, base_gear, stamina_mult=mult) for cycle in (0, 1)}
    print(f"\n## Fôlego cheio a campanha inteira + pilha completa (§23): XP de abate ×{stamina['full']:g} × ritmo "
          f"{stamina['pace']:.3f} = ×{mult:.3f}\n")
    print(report(full))
    print("\n## Orçamento da pilha com fôlego cheio (§23)\n")
    sproblems = [f"NG+{c}: {p}" for c, r in full.items() for p in violations(r, c)]
    print("\n".join(f"- {p}" for p in sproblems) if sproblems else "- dentro do orçamento")
    literal = {cycle: evaluate(ctx, cycle, args.runs, base_gear, stamina_mult=stamina["full"]) for cycle in (0, 1)}
    print(f"\n## Informativo: +{stamina['full'] - 1:.0%} por cima do r8, sem o ritmo de campanha (§23.3, não aplicado no jogo e fora do orçamento)\n")
    print(report(literal))
    lproblems = [f"NG+{c}: {p}" for c, r in literal.items() for p in violations(r, c)]
    print("\n" + ("\n".join(f"- {p}" for p in lproblems) if lproblems else "- dentro do orçamento"))
    wings_main(ctx, args.runs, base_gear, results, refined, curve)
    class_main(ctx, args.runs, base_gear, results)
    options_main(ctx, args.runs, base_gear, results, curve)


if __name__ == "__main__":
    main()
