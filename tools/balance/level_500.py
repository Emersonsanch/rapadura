"""Bands 100-500 against the real world (Docs/Progressao500.md §4, Docs/Balanco100a500.md):
python -I tools/balance/level_500.py [--runs N]   (also printed at the end of power_stack.py)

level_scaling.py stops at 100 and plays the campaign archetypes. This one reads what the open world actually has:

  * creatures: every Resources/Enemies asset named by a region layout (OverworldLayouts.*.cs) whose level band reaches the
    level, times EnemyLevelScaling; the world bosses of RegionBossRules from level 100 up (those with an asset) at their own level, with their real abilities;
  * reference hero at level L: the curve, 2 attribute points per level (Vitality + Arcane Power), skill points, the talent
    wheel and training, the graduations' active skills unlocked by L, and gear the game can really give: campaign loot
    farmed (Monte Carlo over the shipped tables) and the best head+chest pair OBTAINABLE at L (Ossuário from its entry
    level, each region set from the first level of its region);
  * two readings of that hero: "real" is what the code does today (item base armour is NOT applied to the hero, affixes
    do not grow with the level, group buffs of other players excluded) and "doc" is what §4.2 assumes (flat affixes rolled
    per band and the armour of the pieces counted);
  * "Perigoso" (LevelDangerRules) with the hero 10/20/30 levels below the creature.

Commons are read three ways: "abertura" (every cooldown ready, the ultimate included: the first pull after a rest),
"caça" (sustained farming: only skills whose cooldown fits a pull, <= FARM_COOLDOWN s) and basic attack only. The target
is judged on "caça".

Targets are §4.1: commons 4-7 s, bosses 15-22 s, parked hero dies to the boss in 12-40 s, basic-attack-only needs at least
75% of that time. Misses are reported, not fixed: this module never touches the game; `suggestions` turns them into numbers
(health/damage factors that would put each band back on target) for Docs/Balanco100a500.md.
"""

from __future__ import annotations

import argparse
import dataclasses
import glob
import math
import os
import random
import re
import statistics
import sys
from dataclasses import dataclass

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import balance_model as bm  # noqa: E402
import level_scaling as ls  # noqa: E402
import power_stack as ps  # noqa: E402
import rapadura_data as rd  # noqa: E402
import run_r8  # noqa: E402

SCRIPTS = os.path.join(rd.ROOT, "Assets", "Scripts", "Gameplay")
LAYOUTS = os.path.join(SCRIPTS, "World", "Map", "Overworld")
BOSS_RULES = os.path.join(SCRIPTS, "Enemies", "RegionBossRules.cs")
DANGER_RULES = os.path.join(SCRIPTS, "Combat", "LevelDangerRules.cs")
DUNGEON_RULES = os.path.join(SCRIPTS, "Dungeon", "DungeonRules.cs")
SET_RULES = os.path.join(SCRIPTS, "Items", "Sets", "ArmorSetRules.cs")
ITEMS_DIR = os.path.join(rd.ROOT, "Assets", "Resources", "Items")

LEVELS = [100, 120, 140, 160, 180, 200, 220, 240, 260, 280, 300, 320, 340, 360, 380, 400, 420, 440, 460, 478, 500]
PERIGOSO_LEVELS = [100, 200, 300, 400, 478]
PERIGOSO_DELTAS = [0, 10, 20, 30]

COMMON_TARGET = (4.0, 7.0)
BOSS_TARGET = (15.0, 22.0)
PARKED_TARGET = (12.0, 40.0)
BASIC_EDGE = 0.75
MIN_SWINGS_TO_DIE = 3  # below this a boss "kills in a few blows"
MIN_HERO_HITS_ON_BOSS = 8  # below this a boss "dies in a few blows"
FARM_COOLDOWN = 12.0  # "caça": a skill with a longer cooldown is not up for every pull (fight + walk to the next pack)
COMMON_GOAL = 5.5  # middle of COMMON_TARGET: what the suggested health factor aims at
PARKED_GOAL = 15.0  # what the suggested boss damage factor aims at (low in 12-40 s so Perigoso still bites)
BOSS_GOAL = 18.5  # middle of BOSS_TARGET
SUGGEST_LEVELS = [100, 200, 300, 400, 500]
ARCHETYPE_COMMON_HEALTH = 170.0  # Docs/Progressao500.md §4.1: the common archetype (Enxame de Morcegos) the scaling assumes
TOUGHNESS_CANDIDATES = [0.0025, 0.0]  # EnemyLevelScaling.HeroToughnessGrowthPerLevel (today 0.005) tried by the what-if
FARM_KILLS = 400  # commons farmed on top of the campaign before the band's gear is read
FARM_BOSSES = 20
DEFAULT_RUNS = 120

# Which layout file holds each set's region (ArmorSetRules: Regions 13-16).
SET_REGION = {"papiro": "Rio", "tijolo": "Mistura", "selo": "Profundo", "luz": "Ceu"}
SET_NAMES = {"bone": "Marfim (Ossuário)", "papiro": "Cais do Papiro", "tijolo": "Tijolo Azul", "selo": "Selo dos Artífices",
             "luz": "Cais de Luz", None: "peças do saque"}
LOOT_ARMOR = 12.0  # item_leather_helmet + item_leather_chest: what the loot table drops for those slots
BOOTS_ARMOR = 3.0  # item_leather_boots / item_botas_raiz: the only boots there are


def _read(path: str) -> str:
    with open(path, encoding="utf-8-sig", errors="replace") as handle:
        return handle.read()


def _const(source: str, name: str) -> float:
    match = re.search(r"const\s+(?:double|float|int)\s+" + name + r"\s*=\s*([-\d.]+)f?\s*;", source)
    if not match:
        raise ValueError(f"const {name} not found")
    return float(match.group(1))


# ----------------------------------------------------------------------------- world


@dataclass
class Region:
    name: str
    low: int
    high: int
    species: list  # enemy ids


def load_regions(data: rd.GameData) -> dict:
    """Band and common species of each overworld region, read from its layout file."""
    regions = {}
    zone = re.compile(r"OverworldTerrain\.\w+,\s*(\d+),\s*(\d+),\s*\d+")
    for path in glob.glob(os.path.join(LAYOUTS, "OverworldLayouts.*.cs")):
        name = os.path.basename(path).split(".")[1]
        source = _read(path)
        bands = [(int(a), int(b)) for a, b in zone.findall(source)]
        if not bands:
            continue
        ids = sorted({i for i in re.findall(r'"(enemy_[a-z0-9_]+)"', source) if i in data.enemies and not data.enemies[i].is_boss})
        if ids:
            regions[name] = Region(name, min(b[0] for b in bands), max(b[1] for b in bands), ids)
    return regions


def commons_at(data: rd.GameData, regions: dict, level: int) -> list:
    """Every common that can stand at `level`: the species of the regions whose band reaches it."""
    ids = sorted({i for r in regions.values() if r.low <= level <= r.high for i in r.species})
    return [data.enemies[i] for i in ids]


def scaled(enemy: rd.Enemy, scaling: ls.Scaling, level: int) -> rd.Enemy:
    return dataclasses.replace(enemy, max_health=enemy.max_health * scaling.health_mult(level),
                               attack_damage=enemy.attack_damage * scaling.damage_mult(level))


# ----------------------------------------------------------------------------- world bosses


@dataclass
class BossRules:
    ring: float
    barrage_factor: float
    barrage_interval: float
    great_factor: float
    great_interval: float
    night: float
    drink_factor: float
    drink_interval: float
    midnight: float
    dream_factor: float
    dream_interval: float
    greed_fraction: float
    greed_interval: float
    storm_shots: int = 3
    last_shots: int = 5
    signature_factor: dict = dataclasses.field(default_factory=dict)  # signature -> damage factor of its own blow


@dataclass
class WorldBoss:
    enemy_id: str
    level: int
    night: bool
    signature: str


# The signature blows of the bosses added after the first seven (Regions 5-9): one hit of factor x the swing each.
SIGNATURE_FACTOR_CONSTS = {"GluttonBite": "BiteDamageFactor", "SwallowingMire": "MireDamageFactor",
                           "PassBreath": "BreathDamageFactor", "FourAppetites": "AppetiteDamageFactor",
                           "ChainSweep": "ChainDamageFactor"}


def load_boss_rules() -> BossRules:
    src = _read(BOSS_RULES)
    c = lambda name: _const(src, name)  # noqa: E731
    factors = {}
    for signature, const in SIGNATURE_FACTOR_CONSTS.items():
        try:
            factors[signature] = c(const)
        except ValueError:
            pass
    return BossRules(c("RingDamageFactor"), c("BarrageDamageFactor"), c("BarrageIntervalSeconds"), c("GreatBlowDamageFactor"),
                     c("GreatBlowIntervalSeconds"), c("NightAbilityMultiplier"), c("DrinkDamageFactor"), c("DrinkIntervalSeconds"),
                     c("MidnightPeakMultiplier"), c("DreamFallDamageFactor"), c("DreamFallIntervalSeconds"),
                     c("GreedHealthFraction"), c("GreedIntervalSeconds"), signature_factor=factors)


def load_world_bosses() -> list:
    src = _read(BOSS_RULES)
    ids = dict(re.findall(r'const string (\w+Id) = "(enemy_\w+)"', src))
    out = []
    pattern = re.compile(r"new RegionBossSpawn\((\w+Id),[^;]*?,\s*(\d+),\s*EnemyProjectileKind\.\w+,\s*(true|false)"
                         r"(?:,\s*RegionBossSignature\.(\w+))?")
    for name, level, night, signature in pattern.findall(src):
        out.append(WorldBoss(ids[name], int(level), night == "true", signature or "None"))
    return out


FORM_MULT = {"Lion": 1.3, "Eagle": 1.0, "Dragon": 1.15, "Fish": 0.85, "Demon": 1.2}


def form_factor(boss: WorldBoss) -> float:
    """Average of what the signature adds to every ability (A Grande Treva's five forms; the swing itself is untouched)."""
    return statistics.mean(FORM_MULT.values()) if boss.signature == "FiveForms" else 1.0


# ----------------------------------------------------------------------------- Perigoso (LevelDangerRules)


@dataclass
class Danger:
    crit_delta: int
    hit_delta: int
    lethal_delta: int
    crit_chance: float
    crit_mult: float
    hit_penalty: float
    heavy: float
    lethal: float

    def damage_mult(self, delta: int) -> float:
        if delta < self.hit_delta:
            return 1.0
        if delta >= self.lethal_delta:
            return self.lethal
        t = (delta - self.hit_delta) / (self.lethal_delta - self.hit_delta)
        return self.heavy + (self.lethal - self.heavy) * t

    def crit(self, delta: int) -> float:
        return self.crit_chance if delta >= self.crit_delta else 0.0

    def hit_rate(self, delta: int) -> float:
        return 1.0 - self.hit_penalty if delta >= self.hit_delta else 1.0

    def expected_hit(self, delta: int) -> float:
        """Average multiplier on the creature's blow (damage ramp and crits)."""
        return self.damage_mult(delta) * (1 + self.crit(delta) * (self.crit_mult - 1))

    def worst_hit(self, delta: int) -> float:
        return self.damage_mult(delta) * (self.crit_mult if self.crit(delta) > 0 else 1.0)


def load_danger() -> Danger:
    src = _read(DANGER_RULES)
    c = lambda name: _const(src, name)  # noqa: E731
    return Danger(int(c("CritDelta")), int(c("HitRateDelta")), int(c("LethalDelta")), c("CritChanceBonus"),
                  c("CreatureCriticalMultiplier"), c("HitRatePenalty"), c("HeavyDamageTaken"), c("LethalDamageTaken"))


# ----------------------------------------------------------------------------- gear obtainable at a level


@dataclass
class ArmorPiece:
    id: str
    slot: str
    armor: float
    gear: bm.Gear


def _piece(path: str, stat_types: list) -> ArmorPiece:
    text = _read(path)
    gear = bm.Gear()
    block = re.search(r"_equipStatModifiers:(.*?)\n  _healthRestore", text, re.DOTALL)
    for chunk in re.split(r"\n  - ", "\n" + (block.group(1) if block else ""))[1:]:
        stat = stat_types[int(re.search(r"affectedStat:\s*(\d+)", chunk).group(1))]
        percent = int(re.search(r"application:\s*(\d+)", chunk).group(1)) == 1
        value = float(re.search(r"\bvalue:\s*([-\d.]+)", chunk).group(1))
        if stat == "MaxHealth" and percent:
            gear.max_hp_pct += value
        elif stat == "MaxHealth":
            gear.max_hp += value
        elif stat == "MaxMana":
            gear.max_mana += value
        elif stat == "CriticalChance":
            gear.crit += value
        elif stat == "Defense" and not percent:
            gear.defense += value
        else:
            raise ValueError(f"{path}: stat {stat} not modelled")
    name = os.path.basename(path)[:-6]
    return ArmorPiece(name, rd.EQUIP_SLOTS[int(re.search(r"_equipSlot:\s*(\d+)", text).group(1))],
                      float(re.search(r"_armorDefense:\s*([\d.]+)", text).group(1)), gear)


@dataclass
class ArmorSet:
    key: str
    min_level: int
    head: ArmorPiece
    chest: ArmorPiece
    thorns: float = 0.0  # the set bonus that moves a fight's numbers (Reflexo Vidrado)


def load_sets(regions: dict) -> list:
    """The two-piece sets with the first level each can be had. Bonuses that are only a heal on a kill, mana back or a splash
    on OTHER targets (Hora da Barca, Selo Reaberto, Raio da Coluna, Toque Espectral) do not change these single-target numbers."""
    stat_types = rd.load_stat_types()
    rules = _read(SET_RULES)
    reflect = _const(rules, "GlazedReflectChance") * _const(rules, "GlazedReflectFraction")
    sets = []

    def build(key, folder, chest, helm, level, thorns=0.0):
        sets.append(ArmorSet(key, level, _piece(os.path.join(ITEMS_DIR, folder, helm + ".asset"), stat_types),
                             _piece(os.path.join(ITEMS_DIR, folder, chest + ".asset"), stat_types), thorns))

    build("bone", "Dungeon", "armor_bone_chest", "armor_bone_helm", int(_const(_read(DUNGEON_RULES), "MinLevel")))
    for key, region in SET_REGION.items():
        build(key, "Sets", f"armor_{key}_chest", f"armor_{key}_helm", regions[region].low, reflect if key == "tijolo" else 0.0)
    return sets


def available_sets(sets: list, level: int) -> list:
    return [s for s in sets if level >= s.min_level]


def farmed_slots(model: bm.Model, runs: int = DEFAULT_RUNS, farm_kills: int = FARM_KILLS, farm_bosses: int = FARM_BOSSES,
                 seed: int = 500) -> dict:
    """Average best piece per slot after the campaign and a long farm of the last region's tables. There is no item level
    (Docs/Dungeons.md §6): affixes roll the same at level 400 as at 15, so this is the ceiling of loot, not a band's own loot."""
    data = model.data
    rng = random.Random(seed)
    sums = {}
    for _ in range(runs):
        equipped = {}

        def offer(drops):
            for slot, gear in drops:
                if slot not in equipped or gear.score() > equipped[slot].score():
                    equipped[slot] = gear

        for name in rd.BIOME_ORDER:
            biome = data.biomes[name]
            for _ in range(int(bm.region_fights(model, name, "quests") * bm.COMMONS_PER_ENCOUNTER)):
                offer(bm.roll_kill(data, rng, biome, False, None))
            offer(bm.roll_kill(data, rng, biome, True, model.biome_boss(name)))
        caves = data.biomes["Caves"]
        for _ in range(farm_kills):
            offer(bm.roll_kill(data, rng, caves, False, None))
        for _ in range(farm_bosses):
            offer(bm.roll_kill(data, rng, caves, True, model.biome_boss("Caves")))
        for slot, gear in equipped.items():
            sums[slot] = sums.get(slot, bm.Gear()).plus(gear)
    return {slot: gear.scaled(1.0 / runs) for slot, gear in sums.items()}


@dataclass
class Loadout:
    gear: bm.Gear
    armor: float  # base armour of the pieces worn: the hero does NOT get it today
    set_key: str | None


def loadout(slots: dict, sets: list, level: int) -> Loadout:
    """The farmed pieces, with head and chest swapped for the set that scores best among those obtainable at `level`."""
    rest = bm.Gear()
    for slot, gear in slots.items():
        if slot not in ("Head", "Chest"):
            rest = rest.plus(gear)
    # The tooltip shows the armour even though the hero does not get it (see the module docstring), so a player picks by it too.
    rank = lambda pair, armor: pair.score() + armor * 0.25  # noqa: E731
    best_key, best_pair, best_armor = None, slots.get("Head", bm.Gear()).plus(slots.get("Chest", bm.Gear())), LOOT_ARMOR
    for candidate in available_sets(sets, level):
        pair = candidate.head.gear.plus(candidate.chest.gear)
        pair.thorns += candidate.thorns
        armor = candidate.head.armor + candidate.chest.armor
        if rank(pair, armor) >= rank(best_pair, best_armor):
            best_key, best_pair, best_armor = candidate.key, pair, armor
    return Loadout(rest.plus(best_pair), best_armor + BOOTS_ARMOR, best_key)


@dataclass
class Hero:
    build_gear: bm.Gear
    set_key: str | None


class World:
    """Everything the evaluation reads, loaded once."""

    def __init__(self, runs: int = DEFAULT_RUNS, data: rd.GameData | None = None):
        base = data or rd.load_all()
        self.base = base
        self.cls = ps.load_class_evolution()
        self.data = ps.class_data(base, self.cls)  # the graduations' active skills, each from its own level
        self.model = bm.Model(self.data)
        self.farm = FarmModel(self.data)
        self.scaling = ls.load_scaling()
        self.danger = load_danger()
        self.boss_rules = load_boss_rules()
        listed = [b for b in load_world_bosses() if b.level >= LEVELS[0]]  # the 100-500 bands only
        self.bosses = [b for b in listed if b.enemy_id in self.data.enemies]
        self.pending_bosses = [b for b in listed if b.enemy_id not in self.data.enemies]  # spawn listed, asset not made yet
        self.regions = load_regions(self.data)
        self.sets = load_sets(self.regions)
        self.talents = ps.load_talents()
        self.k_common, self.k_boss, _ = run_r8.calibrate(base)
        self.slots = farmed_slots(self.model, runs)
        self._wheel = {}

    def wheel(self, level: int) -> bm.Gear:
        points = self.talents.points(level)
        if points not in self._wheel:
            self._wheel[points] = self.talents.best_wheel(points, ps.talent_offense)
        return self._wheel[points]

    def hero_gear(self, level: int, variant: str = "real") -> Hero:
        kit = loadout(self.slots, self.sets, level)
        gear = kit.gear.plus(self.talents.training_gear(self.talents.max_level)).plus(self.wheel(level))
        if variant == "doc":
            gear = ls.band_gear(gear, self.scaling, level)
            gear.defense += kit.armor
        return Hero(gear, kit.set_key)


# ----------------------------------------------------------------------------- measurements


class FarmModel(bm.Model):
    """The same hero in sustained farming: skills on a cooldown longer than FARM_COOLDOWN are left out of the pull."""

    def player(self, build: bm.Build) -> bm.PlayerStats:
        p = super().player(build)
        return dataclasses.replace(p, skills=[(s, lv) for s, lv in p.skills if s.cycle * (1 - p.cdr) <= FARM_COOLDOWN])


def _ceil_hits(hp: float, hit: float) -> int:
    return math.inf if hit <= 0 else max(1, math.ceil(hp / hit))


def common_row(world: World, creature: int, variant: str = "real", delta: int = 0, health_factor: float = 1.0,
               health_floor: float = 0.0) -> dict:
    """A pair of every common of the world at level `creature`, the hero `delta` levels below: means over species and cousins.
    `time` is the opening pull (whole kit ready), `farm` the sustained pull (FarmModel), `basic` the basic attack alone."""
    level = creature - delta
    species = commons_at(world.data, world.regions, creature)
    gear = world.hero_gear(level, variant).build_gear
    d = world.danger
    times, farms, basics, hits = [], [], [], []
    for enemy in species:
        foe = scaled(dataclasses.replace(enemy, max_health=max(enemy.max_health, health_floor)), world.scaling, creature)
        foe = dataclasses.replace(foe, max_health=foe.max_health * health_factor)
        t, f, b, h = [], [], [], []
        for hero in bm.HEROES:
            build = bm.Build(hero, level, gear)
            player = world.model.player(build)
            miss = world.k_common / d.hit_rate(delta)
            t.append(world.model.fight(build, [foe, foe])["time"] * miss)
            f.append(world.farm.fight(build, [foe, foe])["time"] * miss)
            b.append(world.model.fight(build, [foe, foe], use_skills=False)["time"] / d.hit_rate(delta))
            h.append(_ceil_hits(player.hp, foe.attack_damage * player.defense_mult * d.worst_hit(delta)))
        times.append(statistics.mean(t))
        farms.append(statistics.mean(f))
        basics.append(statistics.mean(b))
        hits.append(statistics.mean(h))
    bases = [e.max_health for e in species]
    return {"level": level, "creature": creature, "species": len(species), "time": statistics.mean(times), "time_min": min(times),
            "time_max": max(times), "farm": statistics.mean(farms), "farm_min": min(farms), "farm_max": max(farms),
            "basic": statistics.mean(basics), "hits": statistics.mean(hits), "hits_min": min(hits),
            "base_health": statistics.mean(bases), "base_health_min": min(bases), "base_health_max": max(bases)}


def boss_row(world: World, boss: WorldBoss, delta: int = 0, variant: str = "real") -> dict:
    """The hero `delta` levels below a world boss, alone with it (the director spawns no minions)."""
    level = boss.level - delta
    gear = world.hero_gear(level, variant)
    foe = scaled(world.data.enemies[boss.enemy_id], world.scaling, boss.level)
    r, d = world.boss_rules, world.danger
    form = form_factor(boss)
    night = r.night if boss.night else 1.0
    rows = []
    for hero in bm.HEROES:
        build = bm.Build(hero, level, gear.build_gear)
        p = world.model.player(build)
        swing = foe.attack_damage * p.defense_mult
        cycle = max(foe.attack_cooldown, foe.windup)
        expected, worst = d.expected_hit(delta), d.worst_hit(delta)
        ring = swing * r.ring * form * night
        kill = world.model.fight(build, [foe])["time"] * world.k_boss / d.hit_rate(delta)
        basic = world.model.fight(build, [foe], use_skills=False)["time"] / d.hit_rate(delta)
        parked = p.hp / ((swing + ring) / cycle * expected)
        great = swing * r.great_factor * form * night
        biggest = max(swing + ring, great)
        extra = {"DrinkTheRiver": swing * r.drink_factor * r.midnight, "DreamFall": swing * r.dream_factor}.get(
            boss.signature, swing * r.signature_factor.get(boss.signature, 0.0))
        biggest = max(biggest, extra)
        # The fight as played: Wrath, Storm and Last Stand take a third of the kill each; the hero stands next to the boss.
        strikes = kill / cycle
        total = (swing + ring) * strikes
        total += swing * r.barrage_factor * form * night * ((kill / 3 / r.barrage_interval) * r.storm_shots
                                                            + (kill / 3 / r.barrage_interval) * r.last_shots)
        total += great * (kill / 3 / r.great_interval)
        if boss.signature == "GreedOfTheArk":
            total += p.hp * r.greed_fraction * p.defense_mult * (kill * 2 / 3 / r.greed_interval)
        rows.append({"kill": kill, "basic": basic, "parked": parked, "swings": _ceil_hits(p.hp, swing * worst), "swing": swing,
                     "blow": biggest * worst / p.hp, "hero_hits": _ceil_hits(foe.max_health, p.hit(bm.BASIC_BASE[0])),
                     "taken": total * expected / p.hp, "hp": p.hp})
    out = {k: statistics.mean(r2[k] for r2 in rows) for k in rows[0]}
    out.update(boss=boss, name=world.data.enemies[boss.enemy_id].name, hero_level=level, delta=delta, set=gear.set_key,
               health=foe.max_health, damage=foe.attack_damage)
    return out


@dataclass
class Result:
    commons: dict  # level -> {variant -> row}
    bosses: list  # [(WorldBoss, {variant -> row})]
    perigoso_commons: dict  # level -> {delta -> row}
    perigoso_bosses: list  # [(WorldBoss, {delta -> row})]


def evaluate(world: World) -> Result:
    commons = {lv: {v: common_row(world, lv, v) for v in ("real", "doc")} for lv in LEVELS}
    bosses = [(b, {v: boss_row(world, b, 0, v) for v in ("real", "doc")}) for b in world.bosses]
    perigoso_commons = {lv: {d: common_row(world, lv, "real", d) for d in PERIGOSO_DELTAS} for lv in PERIGOSO_LEVELS}
    perigoso_bosses = [(b, {d: boss_row(world, b, d) for d in PERIGOSO_DELTAS}) for b in world.bosses]
    return Result(commons, bosses, perigoso_commons, perigoso_bosses)


def misses(result: Result, variant: str = "real") -> list:
    """Stable keys of what is off target, the same every run: (kind, level, metric). Same-level fights only (Perigoso is meant to hurt)."""
    out = []
    for level, by in result.commons.items():
        row = by[variant]
        if row["farm"] < COMMON_TARGET[0]:
            out.append(("common", level, "fast"))
        elif row["farm"] > COMMON_TARGET[1]:
            out.append(("common", level, "slow"))
    for boss, by in result.bosses:
        row = by[variant]
        if row["kill"] < BOSS_TARGET[0]:
            out.append(("boss", boss.level, "fast"))
        elif row["kill"] > BOSS_TARGET[1]:
            out.append(("boss", boss.level, "slow"))
        if row["parked"] < PARKED_TARGET[0]:
            out.append(("boss", boss.level, "parked_short"))
        elif row["parked"] > PARKED_TARGET[1]:
            out.append(("boss", boss.level, "parked_long"))
        if row["basic"] < BASIC_EDGE * row["parked"]:
            out.append(("boss", boss.level, "basic_wins"))
        if row["blow"] >= 1.0:
            out.append(("boss", boss.level, "one_shot"))
        if row["swings"] < MIN_SWINGS_TO_DIE:
            out.append(("boss", boss.level, "few_swings"))
        if row["hero_hits"] < MIN_HERO_HITS_ON_BOSS:
            out.append(("boss", boss.level, "dies_fast"))
    return out


# ----------------------------------------------------------------------------- suggested adjustments


def _solve_increasing(fn, goal: float, lo: float = 0.25, hi: float = 16.0, iterations: int = 9) -> float:
    """x in [lo, hi] with fn(x) ~= goal, fn increasing (bisection on a log scale)."""
    if fn(hi) < goal:
        return hi
    if fn(lo) > goal:
        return lo
    for _ in range(iterations):
        mid = math.sqrt(lo * hi)
        if fn(mid) < goal:
            lo = mid
        else:
            hi = mid
    return math.sqrt(lo * hi)


@dataclass
class CommonFix:
    level: int
    farm: float
    health_factor: float  # on the commons' health at this level so "caça" lands on COMMON_GOAL
    after: dict  # common_row with that factor


@dataclass
class BossFix:
    boss: WorldBoss
    name: str
    damage_factor: float  # on the boss's damage so the parked hero dies in PARKED_GOAL s
    swings_after: float
    blow_after: float
    health_factor_doc: float  # on its health if the "doc" hero (band affixes + armour) ever ships


@dataclass
class ToughnessWhatIf:
    tough: float
    parked: dict  # boss level -> parked time-to-die with that constant
    swings: dict  # boss level -> boss swings to kill the hero
    common_hits: dict  # SUGGEST_LEVELS -> hits a common needs at its level
    up_to_100: list  # level_scaling.violations (levels 15-100) with that constant


def toughness_whatif(world: World, result: Result, tough: float) -> ToughnessWhatIf:
    """Boss and common damage with another HeroToughnessGrowthPerLevel: health is untouched, so only the damage side moves."""
    other = dataclasses.replace(world.scaling, tough=tough)
    parked, swings, hits = {}, {}, {}
    for boss, by in result.bosses:
        ratio = world.scaling.damage_mult(boss.level) / other.damage_mult(boss.level)
        real = by["real"]
        parked[boss.level] = real["parked"] * ratio
        swings[boss.level] = _ceil_hits(real["hp"], real["swing"] / ratio * world.danger.worst_hit(0))
    for level in SUGGEST_LEVELS:
        hits[level] = result.commons[level]["real"]["hits"] * world.scaling.damage_mult(level) / other.damage_mult(level)
    original = ls.load_scaling
    ls.load_scaling = lambda: other  # noqa: E731 (level_scaling reads the constant itself)
    try:
        problems = ls.violations(ls.evaluate(300, world.base))
    finally:
        ls.load_scaling = original
    return ToughnessWhatIf(tough, parked, swings, hits, problems)


def suggestions(world: World, result: Result) -> tuple:
    commons = []
    for level in SUGGEST_LEVELS:
        row = result.commons[level]["real"]
        factor = _solve_increasing(lambda f: common_row(world, level, "real", 0, f)["farm"], COMMON_GOAL)
        commons.append(CommonFix(level, row["farm"], factor, common_row(world, level, "real", 0, factor)))
    bosses = []
    for boss, by in result.bosses:
        real, doc = by["real"], by["doc"]
        factor = min(1.0, real["parked"] / PARKED_GOAL)
        swings = _ceil_hits(real["hp"], real["swing"] * factor * world.danger.worst_hit(0))
        bosses.append(BossFix(boss, real["name"], factor, swings, real["blow"] * factor, BOSS_GOAL / doc["kill"]))
    floor = {lv: common_row(world, lv, "real", 0, 1.0, ARCHETYPE_COMMON_HEALTH) for lv in SUGGEST_LEVELS}
    whatifs = [toughness_whatif(world, result, t) for t in [world.scaling.tough] + TOUGHNESS_CANDIDATES]
    return commons, bosses, floor, whatifs


def report_suggestions(commons: list, bosses: list, floor: dict, whatifs: list) -> str:
    lines = ["| Nível | 2 comuns, caça: hoje | ×vida dos comuns para 5,5 s | Depois: caça / abertura / só básico | Golpes para morrer |",
             "|---|---|---|---|---|"]
    for c in commons:
        a = c.after
        lines.append(f"| {c.level} | {_s(c.farm)} | ×{c.health_factor:.2f} | {_s(a['farm'])} / {_s(a['time'])} / {_s(a['basic'])} | "
                     f"{a['hits']:.0f} |")
    lines += ["", "| Chefe (nível) | ×dano para o herói parado durar 15 s | Golpes para morrer depois | Maior golpe depois | "
              "×vida se o herói \"doc\" for implementado |", "|---|---|---|---|---|"]
    for b in bosses:
        lines.append(f"| {b.name} ({b.boss.level}) | ×{b.damage_factor:.2f} | {b.swings_after:.0f} | {b.blow_after:.0%} | "
                     f"×{b.health_factor_doc:.2f} |")
    lines += ["", f"Piso de vida base {ARCHETYPE_COMMON_HEALTH:.0f} (o comum-arquétipo da escala) nos comuns do mundo aberto:", "",
              "| Nível | Caça (mais rápido–mais lento) | Abertura | Só básico | Golpes para morrer |", "|---|---|---|---|---|"]
    for level, row in floor.items():
        lines.append(f"| {level} | {_s(row['farm'])}{_common_mark(row['farm'])} ({row['farm_min']:.1f}–{row['farm_max']:.1f}) | "
                     f"{_s(row['time'])} | {_s(row['basic'])} | {row['hits']:.0f} |")
    order = [b.boss.level for b in bosses]
    lines += ["", "`EnemyLevelScaling.HeroToughnessGrowthPerLevel` (só mexe no dano dos bichos): herói parado morre em · "
              "golpes do chefe para morrer (pior crítico):", "",
              "| Valor | " + " | ".join(f"chefe {lv}" for lv in order) + " | Golpes de comum p/ morrer ("
              + "/".join(str(lv) for lv in SUGGEST_LEVELS) + ") | Orçamento 15–100 (level_scaling) |",
              "|---|" + "---|" * len(order) + "---|---|"]
    for w in whatifs:
        cells = [f"{_s(w.parked[lv])}{_parked_mark(w.parked[lv])} · {w.swings[lv]}" for lv in order]
        hits = "/".join(f"{w.common_hits[lv]:.0f}" for lv in SUGGEST_LEVELS)
        budget = "dentro" if not w.up_to_100 else "; ".join(w.up_to_100)
        lines.append(f"| {w.tough:g} | " + " | ".join(cells) + f" | {hits} | {budget} |")
    return "\n".join(lines)


# ----------------------------------------------------------------------------- report


def _s(x: float) -> str:
    return f"{x:.1f} s" if math.isfinite(x) else "—"


def _common_mark(t: float) -> str:
    return "" if COMMON_TARGET[0] <= t <= COMMON_TARGET[1] else " **(fora)**"


def _boss_mark(t: float) -> str:
    return "" if BOSS_TARGET[0] <= t <= BOSS_TARGET[1] else " **(fora)**"


def _parked_mark(t: float) -> str:
    return "" if PARKED_TARGET[0] <= t <= PARKED_TARGET[1] else " **(fora)**"


def report_commons(world: World, result: Result) -> str:
    lines = ["| Nível | Conjunto obtível | Espécies (vida base média, mín–máx) | ×vida / ×dano | 2 comuns, caça: real (mais rápido–mais lento) | "
             "Caça: doc | Abertura (kit todo) | Só básico | Golpes para morrer: real / doc |", "|---|---|---|---|---|---|---|---|---|"]
    for level, by in result.commons.items():
        real, doc = by["real"], by["doc"]
        lines.append(f"| {level} | {SET_NAMES[world.hero_gear(level).set_key]} | {real['species']} "
                     f"({real['base_health']:.0f}, {real['base_health_min']:.0f}–{real['base_health_max']:.0f}) | "
                     f"{world.scaling.health_mult(level):.1f} / {world.scaling.damage_mult(level):.1f} | "
                     f"{_s(real['farm'])}{_common_mark(real['farm'])} ({real['farm_min']:.1f}–{real['farm_max']:.1f}) | "
                     f"{_s(doc['farm'])}{_common_mark(doc['farm'])} | {_s(real['time'])} | {_s(real['basic'])} | "
                     f"{real['hits']:.0f} / {doc['hits']:.0f} |")
    return "\n".join(lines)


def report_bosses(result: Result) -> str:
    lines = ["| Chefe (nível) | Vida / dano | Mata: real | Mata: doc | Parado morre em: real / doc | Só básico mata em (real) | "
             "Golpes do chefe para morrer: real / doc | Maior golpe (% da vida): real / doc | Dano da luta (× vida): real / doc | "
             "Básicos do herói no chefe (real) |", "|---|---|---|---|---|---|---|---|---|---|"]
    for boss, by in result.bosses:
        r, d = by["real"], by["doc"]
        lines.append(f"| {r['name']} ({boss.level}) | {r['health']:,.0f} / {r['damage']:,.0f} | {_s(r['kill'])}{_boss_mark(r['kill'])} | "
                     f"{_s(d['kill'])}{_boss_mark(d['kill'])} | {_s(r['parked'])}{_parked_mark(r['parked'])} / {_s(d['parked'])}{_parked_mark(d['parked'])} | "
                     f"{_s(r['basic'])} | {r['swings']:.0f} / {d['swings']:.0f} | {r['blow']:.0%} / {d['blow']:.0%} | "
                     f"{r['taken']:.1f} / {d['taken']:.1f} | {r['hero_hits']:.0f} |")
    return "\n".join(lines)


def report_perigoso(result: Result) -> str:
    head = ["| Bicho | " + " | ".join(f"+{d}" for d in PERIGOSO_DELTAS) + " |", "|---|" + "---|" * len(PERIGOSO_DELTAS)]
    lines = ["Comuns (2 por luta): tempo para matar (caça) · golpes para morrer no pior crítico. O bicho fica no nível da linha; o herói está delta níveis abaixo.", ""] + head
    for level, by in result.perigoso_commons.items():
        cells = [f"{_s(by[d]['farm'])} · {by[d]['hits']:.0f}" for d in PERIGOSO_DELTAS]
        lines.append(f"| comuns {level} | " + " | ".join(cells) + " |")
    lines += ["", "Chefes do mundo: tempo para matar · golpes do chefe para morrer (pior crítico) · herói morre parado em.", ""] + head
    for boss, by in result.perigoso_bosses:
        cells = [f"{_s(by[d]['kill'])} · {by[d]['swings']:.0f} · {_s(by[d]['parked'])}" for d in PERIGOSO_DELTAS]
        lines.append(f"| {by[0]['name']} ({boss.level}) | " + " | ".join(cells) + " |")
    return "\n".join(lines)


def report(world: World, result: Result, with_suggestions: bool = True) -> str:
    miss = misses(result)
    blocks = [f"Fator jogo/modelo (r7): comuns ×{world.k_common:.2f}, chefes ×{world.k_boss:.2f}. \"Caça\" = só skills com recarga ≤ "
              f"{FARM_COOLDOWN:g} s; \"abertura\" = kit inteiro pronto, a suprema incluída.\n",
              "### Herói de referência × comuns do mundo\n", report_commons(world, result),
              "\n### Chefes do mundo (herói no nível do chefe)\n", report_bosses(result)
              + "".join(f"\n\nAinda sem asset em `Resources/Enemies` (fora da tabela): `{b.enemy_id}` (nível {b.level})"
                        for b in world.pending_bosses),
              "\n### Regra \"Perigoso\"\n", report_perigoso(result),
              "\n### Fora da meta (leitura \"real\")\n",
              "\n".join(f"- {k} {lv}: {m}" for k, lv, m in miss) if miss else "- nada"]
    if with_suggestions:
        blocks += ["\n### Ajustes que levariam à meta (sugestão, não aplicada)\n", report_suggestions(*suggestions(world, result))]
    return "\n".join(blocks)


def main() -> int:
    sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--runs", type=int, default=DEFAULT_RUNS)
    args = parser.parse_args()
    world = World(args.runs)
    print(report(world, evaluate(world)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
