"""Read Rapadura balance data straight from the Unity project (no Editor needed).

Sources (all relative to the repository root):
  - Assets/Resources/Enemies/*.asset          EnemyDefinition (HP, damage, cooldown, XP, loot)
  - Assets/Resources/Biomes/*.asset           BiomeDefinition (spawn table, loot table, rolls)
  - Assets/Resources/Items/*.asset            ItemDefinition (rarity, equip slot)
  - Assets/Resources/Affixes/*.asset          ItemAffixDefinition (stat, range, weight, slot)
  - Assets/Resources/Skills/Characters/**     the 25 hero skills (damage, cooldown, mana, DoT, stun)
  - Assets/Resources/Quests/**                personal / convergence / daily quest XP
  - Assets/Scripts/Gameplay/Quests/RegionQuestCatalog.cs   the 8 region quests (code data)
  - Docs/Balance/progression.csv              the player curve (source of truth)

The .asset files are Unity YAML. Only the handful of fields the model needs are parsed, with plain
regular expressions, so this works without PyYAML and ignores Unity's custom tags.
"""

from __future__ import annotations

import csv
import glob
import os
import re
from dataclasses import dataclass, field

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))

BIOME_ORDER = ["Forest", "Desert", "Mountain", "Caves"]
BIOME_TYPE_INDEX = {0: "Forest", 1: "Desert", 2: "Mountain", 3: "Caves"}

# EquipmentSlot enum order (Assets/Scripts/Gameplay/Items/EquipmentSlot.cs).
EQUIP_SLOTS = ["None", "Head", "Chest", "Legs", "Boots", "MainHand", "OffHand", "Tool", "Accessory", "Wings"]  # EquipmentSlot.cs (append only)

# StatType enum (Assets/Scripts/Gameplay/Skills/StatType.cs) is read from source below.

# EnemyMotionArchetype → BaseWindup (Assets/Scripts/Gameplay/Enemies/EnemyMotion.cs).
WINDUP_BY_ARCHETYPE = {0: 0.42, 1: 0.42, 2: 0.40, 3: 0.36, 4: 0.65}
MIN_TELEGRAPH = 0.35
MIN_RECOVER_WINDOW = 0.25


def _read(path: str) -> str:
    with open(path, "r", encoding="utf-8", errors="replace") as handle:
        return handle.read()


def _scalar(text: str, name: str, default=None, cast=float):
    match = re.search(r"^\s*" + re.escape(name) + r":\s*(.*)$", text, re.MULTILINE)
    if not match:
        return default
    raw = match.group(1).strip()
    try:
        return cast(raw)
    except ValueError:
        return default


def _guid_of_ref(raw: str):
    match = re.search(r"guid:\s*([0-9a-f]+)", raw or "")
    return match.group(1) if match else None


def build_guid_map() -> dict:
    """guid → asset name (file name without extension) for every asset under Assets/Resources."""
    mapping = {}
    for meta in glob.glob(os.path.join(ROOT, "Assets", "Resources", "**", "*.asset.meta"), recursive=True):
        guid = _scalar(_read(meta), "guid", cast=str)
        if guid:
            mapping[guid] = os.path.basename(meta)[: -len(".asset.meta")]
    return mapping


def load_stat_types() -> list:
    source = _read(os.path.join(ROOT, "Assets", "Scripts", "Gameplay", "Skills", "StatType.cs"))
    body = source[source.index("{", source.index("enum StatType")) + 1:]
    body = body[: body.index("}")]
    body = re.sub(r"//.*", "", body)
    names = []
    for token in body.split(","):
        token = token.strip()
        if not token:
            continue
        names.append(token.split("=")[0].strip())
    return names


@dataclass
class Enemy:
    id: str
    name: str
    is_boss: bool
    max_health: float
    attack_damage: float
    attack_cooldown: float
    attack_range: float
    xp: int
    boss_xp_mult: float
    archetype: int
    loot_item: str
    loot_chance: float
    loot_min: int
    loot_max: int
    bonus_item: str
    bonus_rarity: int
    path: str = ""

    @property
    def xp_reward(self) -> int:
        if not self.is_boss:
            return self.xp
        return int(round(self.xp * max(1.0, self.boss_xp_mult)))

    @property
    def windup(self) -> float:
        base = WINDUP_BY_ARCHETYPE.get(self.archetype, 0.42)
        high = max(MIN_TELEGRAPH, self.attack_cooldown - MIN_RECOVER_WINDOW)
        return min(max(base, MIN_TELEGRAPH), high)


def load_enemies(guids: dict) -> dict:
    enemies = {}
    for path in sorted(glob.glob(os.path.join(ROOT, "Assets", "Resources", "Enemies", "*.asset"))):
        text = _read(path)
        enemy_id = os.path.basename(path)[:-6]
        enemies[enemy_id] = Enemy(
            id=enemy_id,
            name=_scalar(text, "_enemyName", "", str),
            is_boss=_scalar(text, "_isBoss", 0, int) == 1,
            max_health=_scalar(text, "_maxHealth", 0.0),
            attack_damage=_scalar(text, "_attackDamage", 0.0),
            attack_cooldown=_scalar(text, "_attackCooldown", 1.0),
            attack_range=_scalar(text, "_attackRange", 1.5),
            xp=_scalar(text, "_experienceReward", 0, int),
            boss_xp_mult=_scalar(text, "_bossExperienceMultiplier", 1.0),
            archetype=_scalar(text, "_motionArchetype", 0, int),
            loot_item=guids.get(_guid_of_ref(_scalar(text, "_lootItem", "", str))),
            loot_chance=_scalar(text, "_lootDropChance", 0.0),
            loot_min=_scalar(text, "_lootQuantityMin", 1, int),
            loot_max=_scalar(text, "_lootQuantityMax", 1, int),
            bonus_item=guids.get(_guid_of_ref(_scalar(text, "_bossBonusDropItem", "", str))),
            bonus_rarity=_scalar(text, "_bossBonusDropRarity", 0, int),
            path=path,
        )
    return enemies


@dataclass
class LootEntry:
    item: str
    weight: int
    qmin: int
    qmax: int


@dataclass
class Biome:
    name: str
    spawn: list  # [(enemy_id, weight)]
    loot: list  # [LootEntry]
    loot_chance: float
    rolls_per_kill: int
    boss_rolls: int


def load_biomes(guids: dict) -> dict:
    biomes = {}
    for name in BIOME_ORDER:
        text = _read(os.path.join(ROOT, "Assets", "Resources", "Biomes", name + ".asset"))
        spawn_block = re.search(r"_enemies:\n(.*?)\n  _loot:", text, re.DOTALL).group(1)
        spawn = [
            (guids.get(g), int(w))
            for g, w in re.findall(r"_enemy: \{[^}]*guid: ([0-9a-f]+)[^}]*\}\n\s+_weight: (\d+)", spawn_block)
        ]
        loot_block = re.search(r"\n  _loot:\n(.*?)\n  _lootDropChance", text, re.DOTALL).group(1)
        loot = [
            LootEntry(guids.get(g), int(w), int(a), int(b))
            for g, w, a, b in re.findall(
                r"_item: \{[^}]*guid: ([0-9a-f]+)[^}]*\}\n\s+_weight: (\d+)\n\s+_quantityMin: (\d+)\n\s+_quantityMax: (\d+)",
                loot_block,
            )
        ]
        biomes[name] = Biome(
            name=name,
            spawn=spawn,
            loot=loot,
            loot_chance=_scalar(text, "_lootDropChance", 0.0),
            rolls_per_kill=_scalar(text, "_lootRollsPerKill", 0, int),
            boss_rolls=_scalar(text, "_bossLootRolls", 0, int),
        )
    return biomes


@dataclass
class Item:
    id: str
    rarity: int
    equip_slot: str
    max_stack: int

    @property
    def is_equipment(self) -> bool:
        return self.equip_slot != "None" and self.max_stack <= 1


def load_items() -> dict:
    items = {}
    for path in glob.glob(os.path.join(ROOT, "Assets", "Resources", "Items", "*.asset")):
        text = _read(path)
        item_id = os.path.basename(path)[:-6]
        items[item_id] = Item(
            id=item_id,
            rarity=_scalar(text, "_rarity", 0, int),
            equip_slot=EQUIP_SLOTS[_scalar(text, "_equipSlot", 0, int)],
            max_stack=_scalar(text, "_maxStack", 1, int),
        )
    return items


@dataclass
class Affix:
    id: str
    prefix: bool
    stat: str
    percent: bool
    vmin: float
    vmax: float
    weight: int
    slot: str
    path: str = ""

    def compatible(self, slot: str) -> bool:
        return self.slot == "None" or self.slot == slot


def load_affixes(stat_types: list) -> dict:
    affixes = {}
    for path in sorted(glob.glob(os.path.join(ROOT, "Assets", "Resources", "Affixes", "*.asset"))):
        text = _read(path)
        affix_id = os.path.basename(path)[:-6]
        affixes[affix_id] = Affix(
            id=affix_id,
            prefix=_scalar(text, "_slotKind", 0, int) == 0,
            stat=stat_types[_scalar(text, "_affectedStat", 0, int)],
            percent=_scalar(text, "_application", 0, int) == 1,
            vmin=_scalar(text, "_minValue", 0.0),
            vmax=_scalar(text, "_maxValue", 0.0),
            weight=_scalar(text, "_rarityWeight", 0, int),
            slot=EQUIP_SLOTS[_scalar(text, "_requiredEquipSlot", 0, int)],
            path=path,
        )
    return affixes


@dataclass
class Skill:
    id: str
    hero: str
    skill_type: int  # 0 Active, 1 Passive, 2 Ultimate
    cooldown: float
    mana: float
    cast: float
    recovery: float
    base_damage: float
    damage_per_level: float
    area: float
    max_level: int
    min_player_level: int
    dot_total: float
    stun: float
    buffs: list = field(default_factory=list)  # [(stat, application, value, duration)]
    path: str = ""

    def damage(self, level: int) -> float:
        return self.base_damage + self.damage_per_level * (max(1, level) - 1)

    @property
    def cycle(self) -> float:
        return max(self.cooldown, self.cast + self.recovery)

    @property
    def is_damaging(self) -> bool:
        return self.base_damage + self.dot_total > 0


def _effects(block: str, stat_types: list) -> list:
    effects = []
    for chunk in re.split(r"\n  - ", "\n" + block)[1:]:
        stat = _scalar(chunk, "affectedStat", 0, int)
        effects.append({
            "stat": stat_types[stat] if stat < len(stat_types) else str(stat),
            "percent": _scalar(chunk, "application", 0, int) == 1,
            "value": _scalar(chunk, "value", 0.0),
            "duration": _scalar(chunk, "durationSeconds", 0.0),
            "ticks": _scalar(chunk, "ticksOverTime", 0, int) == 1,
            "interval": _scalar(chunk, "tickIntervalSeconds", 1.0),
            "tick": _scalar(chunk, "tickDamageOrHeal", 0.0),
        })
    return effects


def load_hero_skills(stat_types: list) -> dict:
    """hero name → [Skill]."""
    heroes = {}
    for path in sorted(glob.glob(os.path.join(ROOT, "Assets", "Resources", "Skills", "Characters", "*", "*.asset"))):
        text = _read(path)
        hero = os.path.basename(os.path.dirname(path))
        buffs_match = re.search(r"\n  _buffs:(.*?)\n  _debuffs:", text, re.DOTALL)
        debuffs_match = re.search(r"\n  _debuffs:(.*?)\n  _[a-zA-Z]", text, re.DOTALL)
        buffs = _effects(buffs_match.group(1), stat_types) if buffs_match and "- " in buffs_match.group(1) else []
        debuffs = _effects(debuffs_match.group(1), stat_types) if debuffs_match and "- " in debuffs_match.group(1) else []
        dot = 0.0
        for effect in debuffs:
            if effect["ticks"] and effect["interval"] > 0:
                dot += int(effect["duration"] / effect["interval"] + 1e-6) * effect["tick"]
        min_level = re.search(r"minimumPlayerLevel:\s*(\d+)", text)
        heroes.setdefault(hero, []).append(Skill(
            id=os.path.basename(path)[:-6],
            hero=hero,
            skill_type=_scalar(text, "_skillType", 0, int),
            cooldown=_scalar(text, "_cooldownSeconds", 0.0),
            mana=_scalar(text, "_manaCost", 0.0),
            cast=_scalar(text, "_castTimeSeconds", 0.0),
            recovery=_scalar(text, "_recoverySeconds", 0.0),
            base_damage=_scalar(text, "_baseDamage", 0.0),
            damage_per_level=_scalar(text, "_damagePerLevel", 0.0),
            area=_scalar(text, "_areaRadius", 0.0),
            max_level=_scalar(text, "_maxLevel", 1, int),
            min_player_level=int(min_level.group(1)) if min_level else 1,
            dot_total=dot,
            stun=_scalar(text, "_stunDurationSeconds", 0.0),
            buffs=buffs,
            path=path,
        ))
    return heroes


def load_progression() -> list:
    """Rows of Docs/Balance/progression.csv as dicts with numeric values, index 0 = level 1."""
    rows = []
    with open(os.path.join(ROOT, "Docs", "Balance", "progression.csv"), newline="", encoding="utf-8") as handle:
        for row in csv.DictReader(line for line in handle if line.strip() and not line.startswith("#")):
            rows.append({key: float(value) for key, value in row.items()})
    return rows


@dataclass
class Quest:
    id: str
    region: str
    xp: int
    prerequisite: str
    kills: list  # [(enemy_id, amount)]
    collects: list  # [(item_id, amount)]
    rewards: list  # [(item_id, qty)]


REGION_CATALOG = os.path.join(ROOT, "Assets", "Scripts", "Gameplay", "Quests", "RegionQuestCatalog.cs")


def load_region_quests() -> list:
    source = _read(REGION_CATALOG)
    constants = dict(re.findall(r'public const string (\w+) = "(quest_[a-z_]+)";', source))
    quests = []
    pattern = re.compile(
        r"new RegionQuestSpec\((\w+), BiomeType\.(\w+), (\w+|null), (\d+),(.*?)\}\),\s*\n", re.DOTALL)
    for const, region, prereq, xp, body in pattern.findall(source):
        objectives = re.findall(r'QuestObjectiveType\.(\w+), "([a-z_]+)", (\d+)\)', body)
        rewards = re.findall(r'new RegionRewardSpec\("([a-z_]+)", (\d+)\)', body)
        quests.append(Quest(
            id=constants[const],
            region=region,
            xp=int(xp),
            prerequisite=None if prereq == "null" else constants.get(prereq),
            kills=[(t, int(n)) for kind, t, n in objectives if kind == "KillEnemyType"],
            collects=[(t, int(n)) for kind, t, n in objectives if kind == "CollectItem"],
            rewards=[(i, int(q)) for i, q in rewards],
        ))
    return quests


def load_asset_quests() -> dict:
    """quest id → XP for the personal, convergence and daily quest assets."""
    quests = {}
    for path in glob.glob(os.path.join(ROOT, "Assets", "Resources", "Quests", "**", "*.asset"), recursive=True):
        text = _read(path)
        quests[os.path.basename(path)[:-6]] = _scalar(text, "_rewardExperience", 0, int)
    return quests


@dataclass
class GameData:
    enemies: dict
    biomes: dict
    items: dict
    affixes: dict
    skills: dict
    progression: list
    region_quests: list
    asset_quests: dict
    basic_damage: float


def load_all() -> GameData:
    guids = build_guid_map()
    stat_types = load_stat_types()
    balance = _read(os.path.join(ROOT, "Assets", "Resources", "Balance", "PlayerCombatBalance.asset"))
    return GameData(
        enemies=load_enemies(guids),
        biomes=load_biomes(guids),
        items=load_items(),
        affixes=load_affixes(stat_types),
        skills=load_hero_skills(stat_types),
        progression=load_progression(),
        region_quests=load_region_quests(),
        asset_quests=load_asset_quests(),
        basic_damage=_scalar(balance, "_basicAttackDamage", 10.0),
    )
