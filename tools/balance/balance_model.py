"""Rapadura balance model, round r8 (Docs/Balance/README.md §10; power stacking on top of it: §14, power_stack.py).

Pure Python port of the analytic model of BalanceInvariantsTests (§5.3/§5.4), extended with what
the game gained after r7:

  * a time-stepped fight (dt = 20 ms) instead of a DPS ratio, so stun can cost enemy swings:
    enemies strike every attack_cooldown after a telegraphed windup (EnemyAttackState), a stun
    (StunRules: x0.5 on bosses) stops them, and the attack timer after a stun follows either the
    shipped r7 code ("reset": the next swing comes one windup after the stun ends) or the r8 fix
    ("pause": the cooldown left is kept, at least one full windup is re-telegraphed);
  * attack speed, thorns, crit, flat/percent AttackDamage, flat Defense and MaxHealth from gear;
  * gear from biome loot and boss bonus drops (Monte Carlo over the shipped tables, rarities and
    affix weights, equipping the best piece per slot);
  * XP from kills, region quests (RegionQuestCatalog) and boss kills along two routes through
    the map (rush: 3 wins + boss per region; quests: both region quests + boss);
  * attribute points (1 Vitalidade + 1 Poder Arcano per level) and skill points (greedy into the
    hero's damaging skills; the ultimate is learned at level 10).

Not modelled (same as r5-r7): movement, dodging, misses, enemy flee, % Defense buffs/debuffs,
enemy AttackDamage debuffs, heals. They are why the in-game times are longer than the model's;
`calibrate()` measures that gap against the r7 playtest.
"""

from __future__ import annotations

import copy
import math
import random
from dataclasses import dataclass, field

import rapadura_data as rd

DT = 0.02
BASIC_COOLDOWN = 0.5
BASIC_STRIKE_DELAY = 0.22
ATTACK_SPEED_MIN, ATTACK_SPEED_MAX = -0.5, 1.0
THORNS_MAX = 0.5
MAX_PERCENT_DEFENSE = 0.8  # DamageCalculator.MaxPercentDefense
MANA_POOL = 100.0
MANA_REGEN = 5.0
BOSS_STUN_MULTIPLIER = 0.5
VITALITY_HP = 10.0
ARCANE_POWER_PCT = 0.01
COMMONS_PER_ENCOUNTER = 1.5  # WorldMap.unity: 1-2
VICTORIES_TO_CHALLENGE = 3  # WorldMapGraph.DefaultVictoriesToUnlock
BOSS_MINIONS = 1.0  # RegionBossProgress: 0-2
AOE_RADIUS = 2.5  # skills at or above this area radius hit every enemy of the fight
HEROES = ["Maithe", "Lavine", "Icaro", "Joaquim", "Maria"]

COMMON_RARITY = [(0, 65), (1, 28), (2, 7)]
BOSS_RARITY = [(1, 45), (2, 40), (3, 15)]
AFFIX_COUNT = {0: (0, 0), 1: (0, 1), 2: (1, 2), 3: (2, 3), 4: (3, 4)}
RARITY_NAMES = ["Comum", "Incomum", "Raro", "Épico", "Lendário"]


# ----------------------------------------------------------------------------- player build

@dataclass
class Gear:
    ad_flat: float = 0.0
    ad_pct: float = 0.0
    crit: float = 0.0
    crit_dmg: float = 0.0
    attack_speed: float = 0.0
    thorns: float = 0.0
    defense: float = 0.0
    max_hp: float = 0.0
    regen: float = 0.0
    cdr: float = 0.0
    max_mana: float = 0.0
    # Not rolled by affixes; used by the power-stacking sources (power_stack.py: meals, relics, events).
    defense_pct: float = 0.0  # PercentAdditive Defense (DamageCalculator: x (1 - clamp(pct, -1, 0.8)))
    max_hp_pct: float = 0.0  # PercentAdditive MaxHealth: (base + flat) x (1 + pct)
    rcr: float = 0.0  # ResourceCostReduction (0..1 fraction)
    mana_regen_pct: float = 0.0  # PercentAdditive ManaRegen

    STAT_FIELDS = {
        "AttackDamage": None, "CriticalChance": "crit", "CriticalDamage": "crit_dmg", "AttackSpeed": "attack_speed",
        "Thorns": "thorns", "Defense": "defense", "MaxHealth": "max_hp", "HealthRegen": "regen",
        "CooldownReduction": "cdr", "MaxMana": "max_mana",
    }

    def add_affix(self, affix: rd.Affix, value: float):
        if affix.stat == "AttackDamage":
            if affix.percent:
                self.ad_pct += value
            else:
                self.ad_flat += value
            return
        name = self.STAT_FIELDS.get(affix.stat)
        if name:
            setattr(self, name, getattr(self, name) + value)

    def plus(self, other: "Gear") -> "Gear":
        return Gear(**{k: getattr(self, k) + getattr(other, k) for k in self.__dataclass_fields__})

    def scaled(self, factor: float) -> "Gear":
        return Gear(**{k: getattr(self, k) * factor for k in self.__dataclass_fields__})

    def score(self) -> float:
        """Rough equip preference (DPS-equivalent), used only to pick the best piece per slot."""
        return (self.ad_flat * 1.0 + self.ad_pct * 10 + self.attack_speed * 12 + self.crit * 8 + self.crit_dmg * 1.5
                + self.thorns * 6 + self.defense * 0.25 + self.max_hp * 0.08 + self.regen * 0.8 + self.cdr * 8
                + self.max_mana * 0.03)


@dataclass
class Build:
    hero: str
    level: int
    gear: Gear = field(default_factory=Gear)
    attributes: bool = True
    skill_points: bool = True
    # A timed opening buff (meal "Rapadura": AttackSpeed +25% for the first 20 s of the fight).
    opening_attack_speed: float = 0.0
    opening_seconds: float = 0.0


@dataclass
class PlayerStats:
    hp: float
    mana: float
    damage_mult: float
    ad_flat: float
    ad_pct: float
    crit_ev: float
    basic_interval: float
    defense_mult: float
    thorns: float
    regen: float
    cdr: float
    skills: list  # [(Skill, level)]
    mana_regen: float = 5.0
    rcr: float = 0.0
    opening_interval: float = 0.0  # basic interval while t < opening_seconds (0 = no opening buff)
    opening_seconds: float = 0.0

    def hit(self, base: float) -> float:
        return (base * self.damage_mult + self.ad_flat) * (1 + self.ad_pct) * self.crit_ev

    @property
    def basic_dps(self) -> float:
        return self.hit(BASIC_BASE[0]) / self.basic_interval


BASIC_BASE = [10.0]  # filled from PlayerCombatBalance.asset by Model


def resolve_cooldown(attack_speed: float) -> float:
    bonus = min(max(attack_speed, ATTACK_SPEED_MIN), ATTACK_SPEED_MAX)
    return max(BASIC_COOLDOWN / (1 + bonus), BASIC_STRIKE_DELAY)


class Model:
    def __init__(self, data: rd.GameData, stun_mode: str = "pause"):
        self.data = data
        self.stun_mode = stun_mode
        BASIC_BASE[0] = data.basic_damage

    # -- curve
    def row(self, level: int) -> dict:
        rows = self.data.progression
        return rows[min(max(level, 1), len(rows)) - 1]

    def total_xp(self, level: int) -> float:
        return self.row(level)["xp_total"]

    def level_for_xp(self, xp: float) -> float:
        """Fractional level for an XP total (level + progress to the next)."""
        rows = self.data.progression
        for index in range(len(rows) - 1):
            if xp < rows[index + 1]["xp_total"]:
                return rows[index]["level"] + (xp - rows[index]["xp_total"]) / rows[index]["xp_to_next"]
        return rows[-1]["level"]

    # -- build
    def damaging_skills(self, hero: str, level: int) -> list:
        return [s for s in self.data.skills[hero] if s.is_damaging and s.skill_type != 1 and s.min_player_level <= level]

    def skill_levels(self, build: Build) -> list:
        skills = self.damaging_skills(build.hero, build.level)
        levels = {s.id: 1 for s in skills}
        points = int(build.level - 1) if build.skill_points else 0
        if any(s.skill_type == 2 for s in skills):
            points -= 1  # learning the ultimate at level 10
        while points > 0:
            best, gain = None, 0.0
            for skill in skills:
                if levels[skill.id] >= skill.max_level:
                    continue
                g = skill.damage_per_level / skill.cycle
                if g > gain:
                    best, gain = skill, g
            if best is None:
                break
            levels[best.id] += 1
            points -= 1
        return [(s, levels[s.id]) for s in skills]

    def player(self, build: Build) -> PlayerStats:
        row = self.row(build.level)
        gear = build.gear
        vitality = (build.level - 1) if build.attributes else 0
        arcane = (build.level - 1) if build.attributes else 0
        crit = min(1.0, gear.crit)
        defense_mult = 1 - gear.defense / (gear.defense + 100) if gear.defense > 0 else 1.0
        defense_mult *= 1 - min(max(gear.defense_pct, -1.0), MAX_PERCENT_DEFENSE)
        opening = build.opening_seconds > 0 and build.opening_attack_speed != 0
        return PlayerStats(
            hp=(row["hp"] + vitality * VITALITY_HP + gear.max_hp) * (1 + gear.max_hp_pct),
            mana=row["mana"] + gear.max_mana,
            damage_mult=row["damage_mult"],
            ad_flat=gear.ad_flat,
            ad_pct=gear.ad_pct + arcane * ARCANE_POWER_PCT,
            crit_ev=1 + crit * (0.5 + gear.crit_dmg),
            basic_interval=resolve_cooldown(gear.attack_speed),
            defense_mult=defense_mult,
            thorns=min(max(gear.thorns, 0.0), THORNS_MAX),
            regen=gear.regen,
            cdr=min(max(gear.cdr, 0.0), 1.0),
            skills=self.skill_levels(build),
            mana_regen=MANA_REGEN * (1 + gear.mana_regen_pct),
            rcr=min(max(gear.rcr, 0.0), 1.0),
            opening_interval=resolve_cooldown(gear.attack_speed + build.opening_attack_speed) if opening else 0.0,
            opening_seconds=build.opening_seconds if opening else 0.0,
        )

    # -- fight
    def fight(self, build: Build, enemies: list, use_skills: bool = True, max_time: float = 240.0,
              overrides: dict | None = None) -> dict:
        """Time-stepped fight against `enemies` (EnemyDefinition list) fought together.

        Returns time to kill all, player HP left (may be negative: the fight keeps going so the
        "would have died at" time is known), death time, swings the enemies made / lost to stun.
        """
        p = self.player(build)
        foes = []
        for e in enemies:
            foes.append({
                "e": e, "hp": e.max_health, "next": e.windup, "stun_until": -1.0, "swings": 0,
                "stun_time": 0.0,
            })
        hp = p.hp
        mana = p.mana
        t = 0.0
        next_basic = 0.0
        cooldowns = {s.id: 0.0 for s, _ in p.skills}
        death_time = None
        min_hp = hp
        damage_taken = 0.0
        stun_count = 0
        busy_until = 0.0  # cast + recovery: no other skill nor basic swing meanwhile

        def alive():
            return [f for f in foes if f["hp"] > 0]

        def focus():
            living = alive()
            return min(living, key=lambda f: f["hp"]) if living else None

        def damage(target, amount):
            target["hp"] -= amount

        def stun(target, seconds):
            nonlocal stun_count
            if target["hp"] <= 0 or seconds <= 0:
                return
            duration = seconds * (BOSS_STUN_MULTIPLIER if target["e"].is_boss else 1.0)
            end = max(target["stun_until"], t + duration)
            if target["stun_until"] > t:  # already stunned: just extend
                if self.stun_mode == "reset":
                    target["next"] = end + target["e"].windup
                else:
                    target["next"] += end - target["stun_until"]
                target["stun_until"] = end
                return
            stun_count += 1
            remaining = target["next"] - t
            if self.stun_mode == "reset":
                target["next"] = end + target["e"].windup
            else:
                target["next"] = end + max(remaining, target["e"].windup)
            target["stun_until"] = end

        while alive() and t < max_time:
            # player: skills (highest damage ready first), then the basic swing
            if use_skills and t >= busy_until:
                ready = [(s, lv) for s, lv in p.skills if cooldowns[s.id] <= t and mana >= s.mana * (1 - p.rcr)]
                ready.sort(key=lambda x: -(x[0].damage(x[1]) + x[0].dot_total))
                for skill, lv in ready[:1]:
                    mana -= skill.mana * (1 - p.rcr)
                    busy_until = t + skill.cast + skill.recovery
                    cooldowns[skill.id] = t + skill.cycle * (1 - p.cdr)
                    targets = alive() if skill.area >= AOE_RADIUS else [focus()]
                    for target in targets:
                        damage(target, p.hit(skill.damage(lv)) + skill.dot_total)
                        if skill.stun > 0:
                            stun(target, skill.stun)
            target = focus()
            if target is not None and t >= next_basic and t >= busy_until:
                damage(target, p.hit(BASIC_BASE[0]))
                next_basic = t + (p.opening_interval if t < p.opening_seconds else p.basic_interval)
            # enemies
            for f in alive():
                if f["stun_until"] > t:
                    f["stun_time"] += DT
                    continue
                if t >= f["next"]:
                    hit = f["e"].attack_damage * p.defense_mult
                    hp -= hit
                    damage_taken += hit
                    f["swings"] += 1
                    if p.thorns > 0:
                        damage(f, hit * p.thorns)
                    f["next"] = t + max(f["e"].attack_cooldown, f["e"].windup)
            hp = min(p.hp, hp + p.regen * DT)
            mana = min(p.mana, mana + p.mana_regen * DT)
            min_hp = min(min_hp, hp)
            if death_time is None and hp <= 0:
                death_time = t
            t += DT
        return {
            "time": t if not alive() else math.inf,
            "hp_left": hp / p.hp,
            "min_hp": min_hp / p.hp,
            "death_time": death_time,
            "swings": sum(f["swings"] for f in foes),
            "stun_time": sum(f["stun_time"] for f in foes),
            "stuns": stun_count,
            "damage_taken": damage_taken,
            "player": p,
        }

    def time_to_kill_player(self, build: Build, enemy: rd.Enemy) -> float:
        p = self.player(build)
        return p.hp / (enemy.attack_damage * p.defense_mult / max(enemy.attack_cooldown, enemy.windup))

    # -- world
    def biome_commons(self, biome: str) -> list:
        b = self.data.biomes[biome]
        return [(self.data.enemies[eid], w) for eid, w in b.spawn if not self.data.enemies[eid].is_boss]

    def biome_boss(self, biome: str) -> rd.Enemy:
        b = self.data.biomes[biome]
        return next(self.data.enemies[eid] for eid, w in b.spawn if self.data.enemies[eid].is_boss and w > 0)

    def common_fight(self, biome: str):
        """The playtest's normal fight: 2 commons. Forest's two commons share stats, so the heavier-weighted one is used."""
        commons = sorted(self.biome_commons(biome), key=lambda x: -x[1])
        return [commons[0][0], commons[0][0]]

    def boss_fight(self, biome: str):
        commons = sorted(self.biome_commons(biome), key=lambda x: -x[1])
        return [self.biome_boss(biome), commons[0][0]]


# ----------------------------------------------------------------------------- loot (Monte Carlo)

def _weighted(rng: random.Random, entries):
    total = sum(w for _, w in entries)
    roll = rng.randrange(total) if total > 0 else 0
    acc = 0
    for value, weight in entries:
        acc += weight
        if roll < acc:
            return value
    return entries[-1][0]


def roll_item(data: rd.GameData, rng: random.Random, item_id: str, rarity: int) -> tuple:
    item = data.items[item_id]
    pool = [a for a in data.affixes.values() if a.compatible(item.equip_slot)]
    lo, hi = AFFIX_COUNT[rarity]
    count = rng.randint(lo, hi)
    gear = Gear()
    remaining = list(pool)
    for _ in range(min(count, len(remaining))):
        affix = _weighted(rng, [(a, a.weight) for a in remaining])
        remaining.remove(affix)
        gear.add_affix(affix, affix.vmin + rng.random() * (affix.vmax - affix.vmin))
    return item.equip_slot, gear


def roll_kill(data: rd.GameData, rng: random.Random, biome: rd.Biome, boss: bool, bonus: rd.Enemy | None) -> list:
    drops = []
    rolls = biome.boss_rolls if boss else biome.rolls_per_kill
    for _ in range(rolls):
        if not boss and rng.random() >= biome.loot_chance:
            continue
        entry = _weighted(rng, [(e, e.weight) for e in biome.loot])
        item = data.items.get(entry.item)
        if item is None or not item.is_equipment:
            continue
        table = BOSS_RARITY if boss else COMMON_RARITY
        rarity = max(_weighted(rng, table), item.rarity)
        drops.append(roll_item(data, rng, entry.item, rarity))
    if boss and bonus is not None and bonus.bonus_item:
        drops.append(roll_item(data, rng, bonus.bonus_item, bonus.bonus_rarity))
    return drops


# ----------------------------------------------------------------------------- progression routes

@dataclass
class RegionVisit:
    biome: str
    fights: float
    common_kills: float
    kill_xp: float
    quest_xp: float
    boss_xp: float
    entry_xp: float
    boss_xp_before: float  # total XP when the boss fight starts
    exit_xp: float


def region_fights(model: Model, biome: str, route: str) -> float:
    if route == "rush":
        return float(VICTORIES_TO_CHALLENGE)
    commons = model.biome_commons(biome)
    total_weight = sum(w for _, w in commons)
    fights = 0.0
    for quest in [q for q in model.data.region_quests if q.region == biome]:
        need = 0.0
        for enemy_id, amount in quest.kills:
            share = sum(w for e, w in commons if e.id == enemy_id) / total_weight
            need = max(need, amount / (COMMONS_PER_ENCOUNTER * share))
        fights += need
    return max(float(VICTORIES_TO_CHALLENGE), fights)


def route_xp(model: Model, route: str, quest_xp_scale: dict | None = None, start_xp: float = 0.0,
             kill_xp_mult: float = 1.0) -> list:
    """XP along a route. `start_xp`/`kill_xp_mult` model a Nova Jornada+ cycle (level kept, kill and
    boss XP x NewGamePlusRules.ExperienceMultiplier, quest XP unscaled)."""
    visits = []
    xp = start_xp
    for biome in rd.BIOME_ORDER:
        fights = region_fights(model, biome, route)
        commons = model.biome_commons(biome)
        total_weight = sum(w for _, w in commons)
        avg_xp = sum(e.xp_reward * w for e, w in commons) / total_weight * kill_xp_mult
        kills = fights * COMMONS_PER_ENCOUNTER
        quest_xp = sum(q.xp for q in model.data.region_quests if q.region == biome) if route == "quests" else 0.0
        boss = model.biome_boss(biome)
        entry = xp
        before_boss = xp + kills * avg_xp + quest_xp
        boss_total = boss.xp_reward * kill_xp_mult + BOSS_MINIONS * avg_xp
        xp = before_boss + boss_total
        visits.append(RegionVisit(biome, fights, kills, kills * avg_xp, quest_xp, boss_total, entry, before_boss, xp))
    return visits


def gear_by_region(model: Model, route: str, runs: int = 3000, seed: int = 8) -> dict:
    """Average equipped Gear at each region's entry and right before its boss, over `runs` playthroughs."""
    data = model.data
    rng = random.Random(seed)
    sums = {(b, phase): Gear() for b in rd.BIOME_ORDER for phase in ("entry", "boss")}
    rare_weapon = {b: 0 for b in rd.BIOME_ORDER}
    flat_samples = {b: [] for b in rd.BIOME_ORDER}
    for _ in range(runs):
        equipped = {}

        def offer(slot, gear):
            if slot not in equipped or gear.score() > equipped[slot].score():
                equipped[slot] = gear

        def total():
            g = Gear()
            for piece in equipped.values():
                g = g.plus(piece)
            return g

        for biome_name in rd.BIOME_ORDER:
            biome = data.biomes[biome_name]
            sums[(biome_name, "entry")] = sums[(biome_name, "entry")].plus(total())
            if "MainHand" in equipped and equipped["MainHand"].score() > 0:
                rare_weapon[biome_name] += 1
            fights = region_fights(model, biome_name, route)
            whole = int(fights)
            fights_to_play = whole + (1 if rng.random() < fights - whole else 0)
            for _ in range(fights_to_play):
                for _ in range(rng.randint(1, 2)):
                    for slot, gear in roll_kill(data, rng, biome, False, None):
                        offer(slot, gear)
            sums[(biome_name, "boss")] = sums[(biome_name, "boss")].plus(total())
            flat_samples[biome_name].append(total().ad_flat)
            boss = model.biome_boss(biome_name)
            for slot, gear in roll_kill(data, rng, biome, True, boss):
                offer(slot, gear)
            for _ in range(rng.randint(0, 2)):
                for slot, gear in roll_kill(data, rng, biome, False, None):
                    offer(slot, gear)
    result = {key: value.scaled(1.0 / runs) for key, value in sums.items()}
    result["weapon_with_affix_share"] = {b: rare_weapon[b] / runs for b in rd.BIOME_ORDER}
    result["flat_p90"] = {b: sorted(v)[int(len(v) * 0.9)] for b, v in flat_samples.items()}
    return result


def clone_data(data: rd.GameData) -> rd.GameData:
    return copy.deepcopy(data)
