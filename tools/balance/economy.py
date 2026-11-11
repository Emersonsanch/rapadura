"""Economy of levels 1-500 against the real content (Docs/Balanco100a500.md "Economia"):
python -I tools/balance/economy.py   (also printed at the end of power_stack.py)

level_500.py checks the fights; this one checks what the fights pay and what the world charges, all read from the C#/assets:

  * XP: the curve (LevelCurveMath), the kill XP of a common of the hero's level (EnemyLevelScaling.KillExperience, the
    open maps' table; the commons of the campaign keep their asset XP, which the table matches up to ~15) and the quest XP
    by the level it opens at: the 15 steps of the middle story (MidStoryCatalog), the letters (LateStoryCatalog), the city
    quests (CityQuestCatalog, at the walk level of the city's map: CityQuestCatalog.LevelOf) and the 8 campaign region
    quests (RegionQuestCatalog, at their region's band); above level 50 the steps, letters and city quests pay a share of
    the XP of their level (QuestXpRules.For: 50% / 150% / 25%, never less than the authored number).
    Quest XP is spent first, the rest is kills at KILLS_PER_HOUR (Docs/Progressao500.md §1.3: 200 kills/h of active
    farming, ×1.4 for town, travel and deaths). Fôlego/Descansado/Caça diária are left out (the "folga" of §1.3, on top).
  * gold: item_gold of every common's base loot (EnemyDefinition._lootItem at _lootDropChance) plus the biome table of the
    biome that lists it (BiomeRegistry.FindBiomeOf) sold back at the shop rate (ShopManager: value × 0.5), plus quest
    item_gold. Against: the Barca fare (MapAccessRules.CalculateWarpCost: BaseWarpFee + min(warp level, WarpFeeLevelCap) × WarpFeePerLevel), the
    special gates of the maps (MapRequirementRules: the cheapest shop line of any item that meets the tag, CityShopCatalog,
    unless a quest the hero has done by then already gave one) and shop equipment.
  * loot: the world bosses' own keepsakes (item_chefe_*, their asset LootItem) against what HouseCraftRules spends of
    each (outfits + trophies), the region armour sets (ArmorSetRules.DropChance) and the biome loot value per band.
  * "Perigoso +30" (LevelDangerRules) on the commons with HeroToughnessGrowthPerLevel = 0 (level_500's hero and creatures).

Budgets (BUDGET_* below) are what test_economy.py checks; misses are reported, the game is never touched from here.
"""

from __future__ import annotations

import dataclasses
import glob
import math
import os
import re
import statistics
import sys
from dataclasses import dataclass, field

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import rapadura_data as rd  # noqa: E402

GAMEPLAY = os.path.join(rd.ROOT, "Assets", "Scripts", "Gameplay")
CURVE_CS = os.path.join(GAMEPLAY, "Player", "LevelCurveMath.cs")
SCALING_CS = os.path.join(GAMEPLAY, "Enemies", "EnemyLevelScaling.cs")
MID_CS = os.path.join(GAMEPLAY, "Quests", "MidStoryCatalog.cs")
QUEST_XP_CS = os.path.join(GAMEPLAY, "Quests", "QuestXpRules.cs")
LATE_CS = os.path.join(GAMEPLAY, "Quests", "LateStoryCatalog.cs")
CITY_QUEST_CS = os.path.join(GAMEPLAY, "World", "Cities", "CityQuestCatalog.cs")
CITY_CS = os.path.join(GAMEPLAY, "World", "Cities", "CityCatalog.cs")
SHOP_CS = os.path.join(GAMEPLAY, "World", "Cities", "CityShopCatalog.cs")
MAP_CS = os.path.join(GAMEPLAY, "World", "Map", "MapCatalog.cs")
ACCESS_CS = os.path.join(GAMEPLAY, "World", "Map", "MapAccessRules.cs")
REQUIREMENT_CS = os.path.join(GAMEPLAY, "World", "Map", "MapRequirementRules.cs")
HOUSE_CRAFT_CS = os.path.join(GAMEPLAY, "Housing", "Sources", "HouseCraftRules.cs")
BOSS_CS = os.path.join(GAMEPLAY, "Enemies", "RegionBossRules.cs")
MAP_BOSS_CS = os.path.join(GAMEPLAY, "Enemies", "RegionBossRules.MapBosses.cs")
SET_CS = os.path.join(GAMEPLAY, "Items", "Sets", "ArmorSetRules.cs")
ITEMS_DIR = os.path.join(rd.ROOT, "Assets", "Resources", "Items")

GOLD = "item_gold"
KILLS_PER_HOUR = 200.0  # Docs/Progressao500.md §1.3 (~3.5 kills a minute, walking included)
PLAY_FACTOR = 1.4  # §1.3: hours of play = kill hours × 1.4 (town, travel, death and back, events)
SELL_BACK_RATE = 0.5  # CityShopSpec / ShopDefinition default
BAND = 10
MAX_LEVEL = 500

# Docs/Progressao500.md §1.3: cumulative hours of play at each milestone (the design target of the curve).
DOC_PLAY_HOURS = {50: 8.0, 100: 45.0, 200: 157.0, 300: 310.0, 400: 513.0, 500: 860.0}

# Budgets (test_economy.py).
BUDGET_MILESTONE_TOLERANCE = 0.20  # cumulative play hours within ±20% of DOC_PLAY_HOURS
BUDGET_BAND_STEP = (0.85, 1.60)  # hours per level of a band / of the band before: no wall, no free ride (above the campaign curve, 51+)
BUDGET_QUEST_SHARE_MAX = 0.60  # quests never pay more than 60% of a band's XP ("passa voando")
# Above 100 (QuestXpRules): every band has a quest and gets 3-40% of its XP from quests (the bands without a letter sit at
# the floor, two city quests alone: ~5%). Quests pay 8-15% of all the XP of 101-500 (decided 2026-11-11, Progressao500 §1.3
# and Docs/Balanco100a500.md §5: MU style, past 100 the hero climbs by hunting and quests are a bonus; 0% before the rule,
# ~9% with the city quests on the road of sugestão 5). The old 20-30% target is dropped.
BUDGET_QUEST_SHARE_BAND_ABOVE_100 = (0.03, 0.40)
BUDGET_QUEST_SHARE_ABOVE_100 = (0.08, 0.15)
BUDGET_FARE_MINUTES = 60.0  # a Barca fare costs at most an hour of farming gold at its warp level
BUDGET_KEEPSAKE_KILLS = 20.0  # expected kills of a world boss to collect what the house recipes spend of its keepsake
BUDGET_BOSS_CAMP = (1.1, 1.5)  # camping a world boss pays 1.1-1.5x the XP/h of farming the commons of its level (sugestão 4)
BUDGET_MAP_BOSS_CAMP = (0.8, 1.2)  # a map boss of the fichas (MapSpawns) about the same as the commons
BUDGET_SET_KILLS = 1500.0  # expected kills farming the set's region for both pieces (~7.5 h of kills)
PERIGOSO_DELTA = 30
# "Ainda assusta?": at +30 one pull of two commons should take at least this share of the hero's health (a second pull in
# a row kills), or kill a hero who just stands there in this many seconds.
PERIGOSO_PULL_SHARE = 0.60
PERIGOSO_PARKED_SECONDS = 15.0


def _read(path: str) -> str:
    with open(path, encoding="utf-8-sig", errors="replace") as handle:
        return handle.read()


def _const(source: str, name: str) -> float:
    match = re.search(r"const\s+(?:double|float|int)\s+" + name + r"\s*=\s*([-\d.]+)f?\s*;", source)
    if not match:
        raise ValueError(f"const {name} not found")
    return float(match.group(1))


def _string_consts(source: str) -> dict:
    return dict(re.findall(r'const string (\w+) = "([^"]*)";', source))


# ----------------------------------------------------------------------------- XP


@dataclass
class Curve:
    base: int
    growth: float
    campaign: int
    wall_start: int
    wall_width: float

    def to_next(self, level: int) -> int:
        """LevelCurveMath.ShippedExperienceToNext."""
        level = max(1, min(MAX_LEVEL, level))
        if level <= self.campaign:
            return int(math.floor(self.base * self.growth ** (level - 1) + 0.5))
        anchor = int(math.floor(self.base * self.growth ** (self.campaign - 1) + 0.5))
        wall = 1.0 if level <= self.wall_start else 1.0 + ((level - self.wall_start) / self.wall_width) ** 2
        return int(math.floor(anchor * (level / self.campaign) ** 3 * wall + 0.5))


def load_curve() -> Curve:
    src = _read(CURVE_CS)
    return Curve(int(_const(src, "ShippedBaseExperience")), _const(src, "ShippedExperienceGrowth"), int(_const(src, "CampaignLevels")),
                 int(_const(src, "WallStartLevel")), _const(src, "WallWidth"))


@dataclass
class KillXp:
    low_scale: float
    low_exp: float
    high_at_cap: float
    high_exp: float
    campaign: int
    ranks: dict

    def common(self, level: int) -> int:
        """EnemyLevelScaling.KillExperience(level, Common)."""
        if level <= self.campaign:
            value = self.low_scale * ((level + 1) / 3.0) ** self.low_exp
        else:
            value = self.high_at_cap * (level / self.campaign) ** self.high_exp
        return int(math.floor(value + 0.5))

    def of(self, level: int, rank: str) -> int:
        return self.times(level, self.ranks[rank])

    def times(self, level: int, multiplier: int) -> int:
        """EnemyLevelScaling.KillExperienceTimes(level, multiplier)."""
        if level <= self.campaign:
            value = self.low_scale * ((level + 1) / 3.0) ** self.low_exp
        else:
            value = self.high_at_cap * (level / self.campaign) ** self.high_exp
        return int(math.floor(value * max(0, multiplier) + 0.5))


def load_kill_xp() -> KillXp:
    src = _read(SCALING_CS)
    ranks = dict(re.findall(r"case EnemyRank\.(\w+): return (\d+);", src))
    ranks = {k: int(v) for k, v in ranks.items()}
    ranks["Common"] = 1
    return KillXp(_const(src, "LowKillXpScale"), _const(src, "LowKillXpExponent"), _const(src, "HighKillXpAtCampaignCap"),
                  _const(src, "HighKillXpExponent"), int(_const(_read(CURVE_CS), "CampaignLevels")), ranks)


# ----------------------------------------------------------------------------- quests


@dataclass
class QuestReward:
    source: str  # "meio" / "cartas" / "cidade" / "campanha"
    quest_id: str
    level: int
    xp: int
    gold: int
    items: list  # [(item_id, qty)] other than gold
    authored: int = 0  # the XP written in the catalog (xp is what QuestXpRules makes of it)


def _rewards(text: str) -> list:
    return [(i, int(q)) for i, q in re.findall(r'(?:Reward|RegionRewardSpec)\("(\w+)", (\d+)\)', text)]


def _split(rewards: list) -> tuple:
    gold = sum(q for i, q in rewards if i == GOLD)
    return gold, [(i, q) for i, q in rewards if i != GOLD]


@dataclass
class QuestXp:
    """QuestXpRules: above `above`, a share of the XP to pass the quest's level, never less than the authored number."""
    shares: dict  # source → share
    above: int

    def apply(self, curve: "Curve", source: str, level: int, authored: int) -> int:
        share = self.shares.get(source, 0.0) if level > self.above else 0.0
        return max(authored, int(math.floor(share * curve.to_next(level) + 0.5))) if share > 0 else authored


def load_quest_xp() -> QuestXp:
    src = _read(QUEST_XP_CS)
    return QuestXp({"meio": _const(src, "MidStoryShare"), "cartas": _const(src, "RoadLetterShare"), "cidade": _const(src, "CityShare")},
                   int(_const(src, "ScaledAboveLevel")))


def scale_quests(quests: list, rule: QuestXp, curve: "Curve") -> list:
    return [dataclasses.replace(q, authored=q.xp, xp=rule.apply(curve, q.source, q.level, q.xp)) for q in quests]


def load_story(path: str, source: str) -> list:
    """The steps of MidStoryCatalog / LateStoryCatalog: Step(region, city, giver, zone, map, minLevel, questId, prereq, xp, ...) and Seal(...)."""
    src = _read(path)
    block = src[src.index("Steps = new List<LateStoryStep>"):]
    block = block[: block.index("};")]
    out = []
    for chunk in re.split(r"\n\s*(?=(?:Step|Seal)\()", block)[1:]:
        step = re.match(r'Step\(\d+, [\w.]+, "[^"]*", "[^"]*", "map_\w+", (\d+), (\w+), [\w.]+, (\d+),', chunk)
        seal = re.match(r'Seal\(\d+, [\w.]+, [\w"]+, "[^"]*", "map_\w+", (\d+), (\w+), (\d+),', chunk)
        match = step or seal
        if not match:
            raise ValueError(f"{path}: step not parsed: {chunk[:80]}")
        gold, items = _split(_rewards(chunk))
        out.append(QuestReward(source, match.group(2), int(match.group(1)), int(match.group(3)), gold, items))
    return out


@dataclass
class MapInfo:
    number: int
    id: str
    low: int
    high: int
    walk: int
    warp: int
    base_fee: int
    tag: str
    city: str
    settlement: str

    def fare(self, access: "Access") -> int:
        return 0 if self.base_fee <= 0 else self.base_fee + min(access.level_cap, max(1, self.warp)) * access.fee_per_level


@dataclass
class Access:
    base_fee: int
    fee_per_level: int
    level_cap: int  # MapAccessRules.WarpFeeLevelCap


def load_access() -> Access:
    src = _read(ACCESS_CS)
    return Access(int(_const(src, "BaseWarpFee")), int(_const(src, "WarpFeePerLevel")), int(_const(src, "WarpFeeLevelCap")))


def load_maps(access: Access) -> list:
    src = _read(MAP_CS)
    names = {**_string_consts(src), **{"CityCatalog." + k: v for k, v in _string_consts(_read(CITY_CS)).items()}}
    names.update(_string_consts(src))
    pattern = re.compile(r'new MapDefinition\((\d+), ("[\w]+"|\w+), R\.\w+, Y\.\w+, S\.\w+, (\d+), (\d+), (\d+), (\d+), (\w+), T\.(\w+), '
                         r'([\w.]+), K\.(\w+)')
    maps = []
    for number, ident, low, high, walk, warp, fee, tag, city, kind in pattern.findall(src):
        ident = ident.strip('"') if ident.startswith('"') else names[ident]
        base = access.base_fee if fee == "Fee" else int(fee)
        city_id = names.get(city, names.get(city.split(".")[-1], city))
        maps.append(MapInfo(int(number), ident, int(low), int(high), int(walk), int(warp), base, "" if tag == "None" else tag, city_id, kind))
    return sorted(maps, key=lambda m: m.number)


def city_levels(maps: list) -> dict:
    """city id → the walk level of its own map (the first non-outpost map that returns to it, else any)."""
    levels = {}
    for m in sorted(maps, key=lambda m: m.walk):
        if m.settlement != "Outpost":
            levels.setdefault(m.city, m.walk)
    for m in sorted(maps, key=lambda m: m.walk):
        levels.setdefault(m.city, m.walk)
    return levels


def load_city_quests(maps: list) -> list:
    src = _read(CITY_QUEST_CS)
    cities = {k: v for k, v in _string_consts(_read(CITY_CS)).items()}
    levels = city_levels(maps)
    out = []
    chunks = re.split(r"(?=new CityQuestSpec\()", src)[1:]
    for chunk in chunks:
        # A quest on the road names its map, zone and level (CityQuestSpec.MinLevel) and is paid at that level.
        match = re.match(r'new CityQuestSpec\(CityCatalog\.(\w+), "\w+", (?:"\w+", "\w+", (\d+), )?new RegionQuestSpec\((\w+), BiomeType\.\w+, [\w.]+, (\d+),', chunk)
        if not match:
            continue
        city, road_level, quest, xp = match.groups()
        gold, items = _split(_rewards(chunk))
        out.append(QuestReward("cidade", quest, int(road_level) if road_level else levels[cities[city]], int(xp), gold, items))
    return out


def load_campaign_quests() -> list:
    """The 8 region quests of the four campaign regions, at their region's band (EnemyLevelScaling.*BandLevel)."""
    src = _read(SCALING_CS)
    band = {"Forest": int(_const(src, "ForestBandLevel")), "Desert": int(_const(src, "DesertBandLevel")),
            "Mountain": int(_const(src, "MountainBandLevel")), "Caves": int(_const(src, "CavesBandLevel"))}
    out = []
    for q in rd.load_region_quests():
        gold, items = _split(q.rewards)
        out.append(QuestReward("campanha", q.id, band[q.region], q.xp, gold, items))
    return out


# ----------------------------------------------------------------------------- items, shops, gates


def load_item_values() -> dict:
    values = {}
    for path in glob.glob(os.path.join(ITEMS_DIR, "**", "*.asset"), recursive=True):
        match = re.search(r"^\s*_value:\s*(\d+)", _read(path), re.MULTILINE)
        values[os.path.basename(path)[:-6]] = int(match.group(1)) if match else 0
    return values


@dataclass
class ShopLine:
    item: str
    price: int
    stock: int
    gate: str


def load_shop_lines() -> list:
    lines = []
    for item, price, stock, rest in re.findall(r'new CityShopEntry\("(item_\w+)", (\d+), (-?\d+)(.*?)\)\s*,?\s*\n', _read(SHOP_CS)):
        gate = re.search(r"ShopUnlock\.(\w+\([^)]*\))", rest)
        lines.append(ShopLine(item, int(price), int(stock), gate.group(1) if gate else ""))
    return lines


def cheapest(lines: list) -> dict:
    best = {}
    for line in lines:
        if line.item not in best or line.price < best[line.item].price:
            best[line.item] = line
    return best


@dataclass
class Gate:
    tag: str
    primary: str | None  # MapRequirementRules.PrimaryItemOf
    alternatives: list  # every item IsMet accepts (any one), or both for PickaxeTorch
    needs_all: bool
    gifts: list  # cousins that pass without items (CharacterId)
    other: list  # non-item ways (wings, mounts)


def load_gates() -> dict:
    src = _read(REQUIREMENT_CS)
    items = _string_consts(src[: src.index("class MapRequirementState")])
    is_met = src[src.index("public static bool IsMet"):src.index("public static Func<string, bool> Lookup")]
    primary = dict(re.findall(r"case MapRequirementTags\.(\w+): return MapRequirementItems\.(\w+);", src))
    gates = {}
    for tag, body in re.findall(r"case MapRequirementTags\.(\w+):(.*?)(?=case MapRequirementTags\.|default:)", is_met, re.DOTALL):
        alternatives = [items[n] for n in dict.fromkeys(re.findall(r"MapRequirementItems\.(\w+)", body))]
        other = []
        if "HasWings" in body:
            other.append("asas do Tacho")
        if "MountType.Voador" in body:
            other.append("montaria voadora")
        if "MountType.Aquatico" in body:
            other.append("montaria aquática")
        if "WearsFeltCape" in body:
            other.append("Capa de Feltro")
        if "HasLamp" in body:
            alternatives += [items["OilLamp"], items["RunicLamp"]]
        if not alternatives and tag in primary:  # "any boots worn" (LeatherBoots): the primary item is the one on sale
            alternatives = [items[primary[tag]]]
        gates[tag] = Gate(tag, items.get(primary.get(tag)), list(dict.fromkeys(alternatives)), tag == "PickaxeTorch",
                          re.findall(r"CharacterId\.(\w+)", body), other)
    return gates


def gate_met(gate: Gate, owned: set) -> bool:
    if gate.needs_all:  # Picareta e Tocha: the pickaxe AND a lamp
        lamps = [a for a in gate.alternatives if "lamparina" in a]
        return gate.primary in owned and any(lamp in owned for lamp in lamps)
    return any(a in owned for a in gate.alternatives)


def gate_purchase(gate: Gate, owned: set, shop: dict) -> list:
    """The cheapest shop lines that meet `gate` given what is already owned ([] when nothing on sale does)."""
    if gate.needs_all:
        lamps = [a for a in gate.alternatives if "lamparina" in a and a in shop]
        buy = [] if gate.primary in owned else [shop[gate.primary]] if gate.primary in shop else None
        if buy is None:
            return []
        if not any(lamp in owned for lamp in gate.alternatives if "lamparina" in lamp):
            if not lamps:
                return []
            buy.append(min((shop[lamp] for lamp in lamps), key=lambda s: s.price))
        return buy
    sold = [shop[a] for a in gate.alternatives if a in shop]
    return [min(sold, key=lambda s: s.price)] if sold else []


# ----------------------------------------------------------------------------- gold and loot per kill


def biome_of(data: rd.GameData) -> dict:
    """enemy id → the first biome (BiomeType order) whose spawn table lists it: BiomeRegistry.FindBiomeOf."""
    out = {}
    for name in rd.BIOME_ORDER:
        for enemy_id, _ in data.biomes[name].spawn:
            out.setdefault(enemy_id, name)
    return out


def biome_sale_per_kill(biome: rd.Biome, values: dict, boss: bool = False) -> float:
    """Expected sale value of one kill's rolls on the biome table (EnemyLootRoller step 2), at SELL_BACK_RATE."""
    total = sum(e.weight for e in biome.loot)
    if total <= 0:
        return 0.0
    per_entry = sum(e.weight * (e.qmin + e.qmax) / 2.0 * values.get(e.item, 0) for e in biome.loot) / total
    rolls = biome.boss_rolls if boss else biome.rolls_per_kill
    chance = 1.0 if boss else biome.loot_chance
    return rolls * chance * per_entry * SELL_BACK_RATE


def base_gold(enemy: rd.Enemy) -> float:
    return enemy.loot_chance * (enemy.loot_min + enemy.loot_max) / 2.0 if enemy.loot_item == GOLD else 0.0


@dataclass
class KillPay:
    level: int
    species: int
    gold: float  # item_gold of the base loot
    sale: float  # biome table sold back
    biomes: dict  # biome name → share of the species

    @property
    def total(self) -> float:
        return self.gold + self.sale


# ----------------------------------------------------------------------------- the simulation


@dataclass
class LevelRow:
    level: int
    xp: int
    quest_xp: int
    kill_xp: int
    kills: float
    hours: float
    kill_gold: float
    quest_gold: int
    coin: float = 0.0  # the item_gold part of kill_gold (nothing sold)


@dataclass
class BandRow:
    low: int
    high: int
    xp: int
    quest_xp: int
    kills: float
    kill_hours: float
    play_hours: float
    cum_play_hours: float
    hours_per_level: float
    gold: float
    quest_gold: int
    cum_gold: float
    gold_per_hour: float
    biggest_cost: str


@dataclass
class GateRow:
    map: MapInfo
    gate: Gate
    how: str  # "presente" / "missão" / "já tinha" / "compra" / "sem compra"
    item: str
    cost: int
    balance_before: float  # gold in the bag at the walk level, before this purchase
    gold_hour: float
    gated: str


@dataclass
class FareRow:
    map: MapInfo
    fare: int
    cum_gold: float
    gold_hour: float
    cum_coin: float = 0.0

    @property
    def minutes(self) -> float:
        return self.fare / self.gold_hour * 60.0 if self.gold_hour > 0 else math.inf


@dataclass
class KeepsakeRow:
    boss_id: str
    boss_name: str
    level: int
    item: str
    chance: float
    mean_qty: float
    value: int
    needed: int
    recipes: int

    @property
    def kills(self) -> float:
        per_kill = self.chance * self.mean_qty
        return math.inf if per_kill <= 0 else self.needed / per_kill

    @property
    def hours(self) -> float:
        return self.kills * RESPAWN_SECONDS / 3600.0


RESPAWN_SECONDS = _const(_read(BOSS_CS), "RespawnSeconds")


@dataclass
class Economy:
    curve: Curve
    kill_xp: KillXp
    quests: list
    maps: list
    access: Access
    values: dict
    shop: dict
    shop_lines: list
    gates: dict
    pay: dict  # level → KillPay
    levels: list = field(default_factory=list)  # [LevelRow] 1..499
    bands: list = field(default_factory=list)
    gate_rows: list = field(default_factory=list)
    fare_rows: list = field(default_factory=list)
    keepsakes: list = field(default_factory=list)
    sets: list = field(default_factory=list)
    biome_bands: list = field(default_factory=list)
    arbitrage: list = field(default_factory=list)
    equipment: list = field(default_factory=list)

    def cum_gold(self, level: int) -> float:
        """Gold earned from level 1 up to reaching `level` (kills + quests opened below it)."""
        return sum(r.kill_gold + r.quest_gold for r in self.levels if r.level < level)

    def cum_coin(self, level: int) -> float:
        """The same without selling any loot: item_gold of the kills and the quests only."""
        return sum(r.coin + r.quest_gold for r in self.levels if r.level < level)

    def gold_per_hour(self, level: int) -> float:
        return self.pay[max(1, min(MAX_LEVEL, level))].total * KILLS_PER_HOUR


def kill_pay(data: rd.GameData, regions: dict, values: dict, level: int) -> KillPay:
    import level_500 as l5  # noqa: E402 (sibling module)
    species = l5.commons_at(data, regions, level)
    biomes = biome_of(data)
    gold = statistics.mean(base_gold(e) for e in species)
    sale = statistics.mean(biome_sale_per_kill(data.biomes[biomes[e.id]], values) if e.id in biomes else 0.0 for e in species)
    share = {}
    for e in species:
        name = biomes.get(e.id, "—")
        share[name] = share.get(name, 0) + 1 / len(species)
    return KillPay(level, len(species), gold, sale, share)


def load_keepsakes(data: rd.GameData, values: dict) -> list:
    import level_500 as l5  # noqa: E402
    src = _read(HOUSE_CRAFT_CS)
    aliases = dict(re.findall(r'private const string (\w+) = "(item_chefe_\w+)";', src))
    keep = dict(re.findall(r'\{ "(enemy_\w+)", "(item_chefe_\w+)" \}', src))
    needed, recipes = {}, {}
    for recipe in re.findall(r"(?:R|Trophy)\(\"\w+\",[^;\n]*", src):
        for alias, qty in re.findall(r"\((\w+), (\d+)\)", recipe):
            item = aliases.get(alias)
            if item:
                needed[item] = needed.get(item, 0) + int(qty)
                recipes[item] = recipes.get(item, 0) + 1
    levels = {b.enemy_id: b.level for b in l5.load_world_bosses()}
    rows = []
    for boss_id, item in keep.items():
        enemy = data.enemies[boss_id]
        rows.append(KeepsakeRow(boss_id, enemy.name.strip('"'), levels.get(boss_id, 0), item, enemy.loot_chance if enemy.loot_item == item else 0.0,
                                (enemy.loot_min + enemy.loot_max) / 2.0, values.get(item, 0), needed.get(item, 0), recipes.get(item, 0)))
    return sorted(rows, key=lambda r: r.level)


@dataclass
class SetRow:
    name: str
    beasts: int
    region_species: int
    kills: float  # kills of the set's own beasts for both pieces (2 equally likely pieces: 3 drops)
    farm_kills: float  # kills farming the whole region (only its beasts drop)
    farm_hours: float


def load_sets(regions: dict) -> list:
    src = _read(SET_CS)
    chance = _const(src, "DropChance")
    middle = _const(src, "MiddleDropChance")
    region_of = {"Coral": "Costa", "Lotus": "Delta", "Yak": "Teto", "Bronze": "Mingcheng", "Kurgan": "Nartes",
                 "Papyrus": "Rio", "BlueBrick": "Mistura", "Seal": "Profundo", "Light": "Ceu"}
    rows = []
    for name, body in re.findall(r"ArmorSetDefinition (\w+) = new ArmorSetDefinition\((.*?)\}\);", src, re.DOTALL):
        beasts = re.findall(r'"(enemy_\w+)"', body)
        region = regions[region_of[name]]
        own = len([b for b in beasts if b in region.species])
        p = middle if "MiddleDropChance" in body else chance  # Regions 5-9: ArmorSetRules.MiddleDropChance
        kills = 3.0 / p  # coupon collector, 2 pieces: 1/p + 2/p
        farm = kills * len(region.species) / max(1, own)
        rows.append(SetRow(re.search(r'"(\w+)"', body).group(1), len(beasts), len(region.species), kills, farm, farm / KILLS_PER_HOUR))
    return rows


def build(data: rd.GameData | None = None) -> Economy:
    import level_500 as l5  # noqa: E402
    data = data or rd.load_all()
    access = load_access()
    maps = load_maps(access)
    curve = load_curve()
    authored = load_story(MID_CS, "meio") + load_story(LATE_CS, "cartas") + load_city_quests(maps) + load_campaign_quests()
    quests = scale_quests(authored, load_quest_xp(), curve)
    values = load_item_values()
    lines = load_shop_lines()
    regions = l5.load_regions(data)
    pay = {lv: kill_pay(data, regions, values, lv) for lv in range(1, MAX_LEVEL + 1)}
    eco = Economy(curve, load_kill_xp(), quests, maps, access, values, cheapest(lines), lines, load_gates(), pay)
    simulate(eco)
    eco.gate_rows = gates_along_the_way(eco)
    eco.fare_rows = [FareRow(m, m.fare(access), eco.cum_gold(m.warp), eco.gold_per_hour(m.warp), eco.cum_coin(m.warp)) for m in maps if m.fare(access) > 0]
    eco.bands = bands(eco)
    eco.keepsakes = load_keepsakes(data, values)
    eco.sets = load_sets(regions)
    eco.biome_bands = [(lv, pay[lv]) for lv in (5, 25, 50, 100, 200, 300, 400, 500)]
    eco.arbitrage = [line for line in lines if line.price < values.get(line.item, 0) * SELL_BACK_RATE]
    eco.equipment = sorted(((i, s.price) for i, s in eco.shop.items() if i in data.items and data.items[i].is_equipment), key=lambda t: t[1])
    return eco


def simulate(eco: Economy) -> None:
    by_level = {}
    for q in eco.quests:
        by_level.setdefault(q.level, []).append(q)
    pool = 0  # quest XP not yet used (a step worth more than its level carries over)
    for level in range(1, MAX_LEVEL):
        opened = by_level.get(level, [])
        pool += sum(q.xp for q in opened)
        need = eco.curve.to_next(level)
        used = min(pool, need)
        pool -= used
        kxp = eco.kill_xp.common(level)
        kills = (need - used) / kxp
        eco.levels.append(LevelRow(level, need, used, kxp, kills, kills / KILLS_PER_HOUR, kills * eco.pay[level].total,
                                   sum(q.gold for q in opened), kills * eco.pay[level].gold))


def bands(eco: Economy) -> list:
    rows = []
    cum_hours = cum_gold = 0.0
    gate_by_band = {}
    for g in eco.gate_rows:
        if g.cost > 0:
            gate_by_band.setdefault((g.map.walk - 1) // BAND, []).append(g)
    for start in range(1, MAX_LEVEL, BAND):
        chunk = [r for r in eco.levels if start <= r.level < start + BAND]
        kill_hours = sum(r.hours for r in chunk)
        play = kill_hours * PLAY_FACTOR
        cum_hours += play
        gold = sum(r.kill_gold for r in chunk)
        qgold = sum(r.quest_gold for r in chunk)
        cum_gold += gold + qgold
        costs = gate_by_band.get((start - 1) // BAND, [])
        biggest = max(costs, key=lambda g: g.cost) if costs else None
        rows.append(BandRow(start, start + len(chunk), sum(r.xp for r in chunk), sum(r.quest_xp for r in chunk), sum(r.kills for r in chunk),
                            kill_hours, play, cum_hours, play / len(chunk), gold, qgold, cum_gold,
                            gold / kill_hours if kill_hours else 0.0,
                            f"{biggest.item} {biggest.cost} (mapa {biggest.map.number})" if biggest else ""))
    return rows


def gates_along_the_way(eco: Economy) -> list:
    """Every special gate in walk-level order, for a cousin without the gift: owned already, given by a quest opened by
    then, or bought (the cheapest line). The bag is what came in up to the walk level minus what the gates cost before."""
    gifts = {}
    for q in eco.quests:
        for item, _ in q.items:
            gifts.setdefault(item, q.level)
    owned, spent, rows = set(), 0.0, []
    for m in sorted([m for m in eco.maps if m.tag], key=lambda m: (m.walk, m.number)):
        gate = eco.gates[m.tag]
        balance = eco.cum_gold(m.walk) - spent
        for item, level in gifts.items():
            if level <= m.walk:
                owned.add(item)
        if gate_met(gate, owned):
            how = "missão" if any(a in owned and gifts.get(a, 999) <= m.walk for a in gate.alternatives) else "já tinha"
            rows.append(GateRow(m, gate, how, next((a for a in gate.alternatives if a in owned), ""), 0, balance, eco.gold_per_hour(m.walk), ""))
            continue
        buy = gate_purchase(gate, owned, eco.shop)
        if not buy:
            rows.append(GateRow(m, gate, "sem compra", " / ".join(gate.other) or "—", 0, balance, eco.gold_per_hour(m.walk), ""))
            continue
        cost = sum(b.price for b in buy)
        rows.append(GateRow(m, gate, "compra", " + ".join(b.item for b in buy), cost, balance, eco.gold_per_hour(m.walk),
                            ", ".join(b.gate for b in buy if b.gate)))
        spent += cost
        owned.update(b.item for b in buy)
    return rows


# ----------------------------------------------------------------------------- Perigoso +30 on the commons


@dataclass
class PerigosoRow:
    creature: int
    delta: int
    farm: float  # time to kill the pair (caça)
    hits: float  # worst-crit blows of one common to kill the hero (mean over species)
    hits_min: int
    parked: float  # the hero standing still dies to the pair in
    pull_share: float  # share of the hero's health one pull costs (both hit for the first half, one for the second)


def perigoso(world, levels=(100, 200, 300, 400, 478), deltas=(0, 30), lethal: float | None = None) -> list:
    import balance_model as bm  # noqa: E402
    import level_500 as l5  # noqa: E402
    danger = world.danger if lethal is None else dataclasses.replace(world.danger, lethal=lethal)
    rows = []
    for creature in levels:
        for delta in deltas:
            level = creature - delta
            gear = world.hero_gear(level).build_gear
            row = l5.common_row(world, creature, "real", delta)
            parked, shares, hits = [], [], []
            for enemy in l5.commons_at(world.data, world.regions, creature):
                foe = l5.scaled(enemy, world.scaling, creature)
                for hero in bm.HEROES:
                    p = world.model.player(bm.Build(hero, level, gear))
                    blow = foe.attack_damage * p.defense_mult
                    dps = blow * danger.expected_hit(delta) / max(foe.attack_cooldown, foe.windup)
                    parked.append(p.hp / (2 * dps))
                    shares.append(0.75 * 2 * dps * row["farm"] / p.hp)
                    hits.append(math.ceil(p.hp / (blow * danger.worst_hit(delta))))
            rows.append(PerigosoRow(creature, delta, row["farm"], statistics.mean(hits), min(hits), statistics.mean(parked),
                                    statistics.mean(shares)))
    return rows


# ----------------------------------------------------------------------------- XP checks and suggested numbers


def campaign_xp_check(data: rd.GameData, kill_xp: KillXp) -> list:
    """Mean asset XP of each campaign biome's commons against KillExperience at its band (the table must match up to ~15)."""
    src = _read(SCALING_CS)
    out = []
    for name in rd.BIOME_ORDER:
        band = int(_const(src, f"{name if name != 'Caves' else 'Caves'}BandLevel"))
        commons = [data.enemies[e] for e, _ in data.biomes[name].spawn if e in data.enemies and not data.enemies[e].is_boss
                   and data.enemies[e].xp > 0]
        own = [e for e in commons if e.xp < 60]  # the campaign's own commons (the open-world reuse leaves them as they were)
        out.append((name, band, statistics.mean(e.xp for e in own) if own else 0.0, kill_xp.common(band)))
    return out


BOSS_KILL_SECONDS = 18.0  # the fight on top of RegionBossRules.RespawnSeconds when camping a boss


def boss_xp_ranks() -> tuple:
    """RegionBossRules.WorldBossKillXpMultiplier / MapBossKillXpMultiplier (kill XP in commons of the boss's level)."""
    src = _read(BOSS_CS)
    return int(_const(src, "WorldBossKillXpMultiplier")), int(_const(src, "MapBossKillXpMultiplier"))


def load_map_bosses() -> list:
    """RegionBossRules.MapSpawns (enemy id, level), the six map bosses of the fichas."""
    src = _read(MAP_BOSS_CS)
    ids = dict(re.findall(r'const string (\w+Id) = "(enemy_\w+)"', src))
    return [(ids[name], int(level)) for name, level in
            re.findall(r"new RegionBossSpawn\((\w+Id),[^;]*?,\s*(\d+),\s*EnemyProjectileKind", src)]


def boss_camp_rows(eco: Economy, levels: list, rank: int, kill_seconds: float = BOSS_KILL_SECONDS) -> list:
    """(level, kill XP, XP/h camping, XP/h farming commons, ratio) of a boss of each level paying `rank` commons a kill."""
    rows = []
    for level in levels:
        xp = eco.kill_xp.times(level, rank)
        camp = xp * 3600.0 / (RESPAWN_SECONDS + kill_seconds)
        commons = eco.kill_xp.common(level) * KILLS_PER_HOUR
        rows.append((level, xp, camp, commons, camp / commons))
    return rows


def world_boss_xp(eco: Economy, bosses: list, kill_seconds: float = BOSS_KILL_SECONDS) -> list:
    """XP of camping a world boss (RegionBossRules.WorldBossKillXpMultiplier commons a kill) against farming commons, per hour;
    also what the generic boss rank (MapBoss, ×9: EnemyLevelScaling.RankOf(isBoss)) paid before sugestão 4."""
    world_rank, _ = boss_xp_ranks()
    rows = []
    for b in bosses:
        level, xp, camp, commons, _ratio = boss_camp_rows(eco, [b.level], world_rank, kill_seconds)[0]
        before = eco.kill_xp.of(b.level, "MapBoss") * 3600.0 / (RESPAWN_SECONDS + kill_seconds)
        rows.append((b, xp, before, camp, commons))
    return rows


def boss_camp_problems(eco: Economy, bosses: list) -> list:
    """Bosses whose camping XP/h falls out of BUDGET_BOSS_CAMP (world) / BUDGET_MAP_BOSS_CAMP (map)."""
    world_rank, map_rank = boss_xp_ranks()
    out = []
    for level, _xp, _camp, _commons, ratio in boss_camp_rows(eco, [b.level for b in bosses], world_rank):
        if not BUDGET_BOSS_CAMP[0] <= ratio <= BUDGET_BOSS_CAMP[1]:
            out.append(("world_boss", level, round(ratio, 2)))
    for level, _xp, _camp, _commons, ratio in boss_camp_rows(eco, [lv for _, lv in load_map_bosses()], map_rank):
        if not BUDGET_MAP_BOSS_CAMP[0] <= ratio <= BUDGET_MAP_BOSS_CAMP[1]:
            out.append(("map_boss", level, round(ratio, 2)))
    return out


def with_authored_quest_xp(eco: Economy) -> Economy:
    """The same economy with every quest paying the XP written in its catalog (before QuestXpRules): the "antes"."""
    quests = [dataclasses.replace(q, xp=q.authored) for q in eco.quests]
    other = dataclasses.replace(eco, quests=quests, levels=[])
    simulate(other)
    other.gate_rows = gates_along_the_way(other)
    other.bands = bands(other)
    return other


def fare_cap(eco: Economy) -> int:
    """Highest warp level the fare can follow so no fare costs more than BUDGET_FARE_MINUTES of the poorest band's farming
    (MapAccessRules.WarpFeeLevelCap must stay at or under it)."""
    poorest = min(eco.gold_per_hour(lv) for lv in range(eco.curve.campaign, MAX_LEVEL + 1))
    return int((poorest * BUDGET_FARE_MINUTES / 60.0 - eco.access.base_fee) // eco.access.fee_per_level)


# ----------------------------------------------------------------------------- budgets


def milestone_hours(eco: Economy) -> dict:
    out = {}
    for level in DOC_PLAY_HOURS:
        out[level] = sum(r.hours for r in eco.levels if r.level < level) * PLAY_FACTOR
    return out


def quest_share_above_100(eco: Economy) -> float:
    """Share of the XP of levels 101-499 that quests pay."""
    rows = [r for r in eco.levels if r.level > 100]
    return sum(r.quest_xp for r in rows) / sum(r.xp for r in rows)


def problems(eco: Economy) -> list:
    """Stable keys of what is off budget: (kind, where, what)."""
    out = []
    for level, hours in milestone_hours(eco).items():
        target = DOC_PLAY_HOURS[level]
        if abs(hours / target - 1) > BUDGET_MILESTONE_TOLERANCE:
            out.append(("xp", level, "slow" if hours > target else "fast"))
    for before, band in zip(eco.bands, eco.bands[1:]):
        if band.low <= eco.curve.campaign:
            continue
        step = band.hours_per_level / before.hours_per_level
        if step > BUDGET_BAND_STEP[1]:
            out.append(("band", band.low, "wall"))
        elif step < BUDGET_BAND_STEP[0]:
            out.append(("band", band.low, "drop"))
        if band.quest_xp > BUDGET_QUEST_SHARE_MAX * band.xp:
            out.append(("band", band.low, "quests_carry"))
        if band.low > 100 and band.quest_xp > 0 and not BUDGET_QUEST_SHARE_BAND_ABOVE_100[0] <= band.quest_xp / band.xp <= BUDGET_QUEST_SHARE_BAND_ABOVE_100[1]:
            out.append(("band", band.low, "quest_share"))
    share = quest_share_above_100(eco)
    if not BUDGET_QUEST_SHARE_ABOVE_100[0] <= share <= BUDGET_QUEST_SHARE_ABOVE_100[1]:
        out.append(("xp", 100, "quest_share"))
    for g in eco.gate_rows:
        if g.how == "compra" and g.cost > g.balance_before:
            out.append(("gate", g.map.number, "unaffordable"))
    for f in eco.fare_rows:
        if f.fare > f.cum_gold:
            out.append(("fare", f.map.number, "unaffordable"))
        elif f.minutes > BUDGET_FARE_MINUTES:
            out.append(("fare", f.map.number, "expensive"))
    for k in eco.keepsakes:
        if k.needed and k.kills > BUDGET_KEEPSAKE_KILLS:
            out.append(("keepsake", k.level, "grind"))
        if k.needed == 0:
            out.append(("keepsake", k.level, "unused"))
    for s in eco.sets:
        if s.farm_kills > BUDGET_SET_KILLS:
            out.append(("set", s.name, "grind"))
    for line in eco.arbitrage:
        out.append(("shop", line.item, "arbitrage"))
    return out


def perigoso_problems(rows: list) -> list:
    out = []
    for r in rows:
        if r.delta == PERIGOSO_DELTA and r.pull_share < PERIGOSO_PULL_SHARE and r.parked > PERIGOSO_PARKED_SECONDS:
            out.append(("perigoso", r.creature, "tame"))
    return out


# ----------------------------------------------------------------------------- report


def _h(x: float) -> str:
    return f"{x:,.1f}".replace(",", " ") if x < 100 else f"{x:,.0f}".replace(",", " ")


def _n(x: float) -> str:
    return f"{x:,.0f}".replace(",", " ")


def report_bands(eco: Economy) -> str:
    lines = [f"Ritmo: {KILLS_PER_HOUR:.0f} abates/h de caça ativa, horas de jogo = abate × {PLAY_FACTOR:g} (Progressao500 §1.3); XP de "
             "missão gasto primeiro, o resto em comuns do nível do herói. Ouro = item_gold do saque base + tabela do bioma vendida a "
             f"{SELL_BACK_RATE:.0%}; o maior gasto é a exigência comprada mais cara da faixa.", "",
             "| Faixa | XP da faixa | XP de missão | Abates | Horas de jogo (h/nível) | Acumulado | Ouro na faixa: abate + missão | "
             "Ouro/h | Ouro acumulado | Maior exigência comprada |", "|---|---|---|---|---|---|---|---|---|---|"]
    prev = None
    for b in eco.bands:
        step = "" if prev is None else f", ×{b.hours_per_level / prev.hours_per_level:.2f}"
        mark = ""
        if prev is not None and b.low > eco.curve.campaign:
            s = b.hours_per_level / prev.hours_per_level
            mark = " **(parede)**" if s > BUDGET_BAND_STEP[1] else " **(queda)**" if s < BUDGET_BAND_STEP[0] else ""
        lines.append(f"| {b.low}–{b.high} | {_n(b.xp)} | {_n(b.quest_xp)} ({b.quest_xp / b.xp:.0%}) | {_n(b.kills)} | "
                     f"{_h(b.play_hours)} ({b.hours_per_level * 60:.0f} min{step}){mark} | {_h(b.cum_play_hours)} h | "
                     f"{_n(b.gold)} + {_n(b.quest_gold)} | {_n(b.gold_per_hour)} | {_n(b.cum_gold)} | {b.biggest_cost} |")
        prev = b
    marks = milestone_hours(eco)
    lines += ["", "Marcos (horas de jogo acumuladas): " + " · ".join(
        f"{lv}: {_h(h)} h (doc {DOC_PLAY_HOURS[lv]:g} h, {h / DOC_PLAY_HOURS[lv] - 1:+.0%})" for lv, h in marks.items())]
    total_quest = sum(q.xp for q in eco.quests)
    by_source = {}
    for q in eco.quests:
        by_source.setdefault(q.source, [0, 0, 0])
        by_source[q.source][0] += 1
        by_source[q.source][1] += q.xp
        by_source[q.source][2] += q.gold
    lines += ["", "XP e ouro de missão por fonte: " + " · ".join(
        f"{s}: {n} missões, {_n(x)} XP, {_n(g)} ouro" for s, (n, x, g) in by_source.items()) + f" (total {_n(total_quest)} XP)."]
    return "\n".join(lines)


def report_gates(eco: Economy) -> str:
    lines = ["| Mapa (andar / barca) | Exigência | Como (primo sem o dom) | Item | Custo | Ouro na bolsa ao chegar | Custo em min de caça |",
             "|---|---|---|---|---|---|---|"]
    for g in eco.gate_rows:
        gifts = f" ({', '.join(g.gate.gifts)} passa)" if g.gate.gifts else ""
        cost = _n(g.cost) if g.cost else "—"
        minutes = f"{g.cost / g.gold_hour * 60:.0f} min" if g.cost else "—"
        flag = " **(não dá)**" if g.how == "compra" and g.cost > g.balance_before else ""
        gated = f" [{g.gated}]" if g.gated else ""
        lines.append(f"| {g.map.number:02d} ({g.map.walk} / {g.map.warp}) | {g.gate.tag}{gifts} | {g.how} | {g.item}{gated} | {cost}{flag} | "
                     f"{_n(g.balance_before)} | {minutes} |")
    return "\n".join(lines)


def report_fares(eco: Economy) -> str:
    picks = [f for f in eco.fare_rows if f.map.number in (2, 3, 13, 25, 41, 47, 59, 71, 83, 91, 100)]
    lines = [f"Passagem = {eco.access.base_fee} + min(nível da barca, {eco.access.level_cap}) × {eco.access.fee_per_level} "
             "(`MapAccessRules.CalculateWarpCost`; o dobro com a mochila pesada).", "", "| Mapa | Nível da barca | Passagem | Ouro acumulado no nível (sem vender nada) | Ouro/h no nível | Passagem em min de caça |",
             "|---|---|---|---|---|---|"]
    for f in picks:
        mark = " **(fora)**" if f.minutes > BUDGET_FARE_MINUTES else ""
        lines.append(f"| {f.map.number:02d} | {f.map.warp} | {_n(f.fare)} | {_n(f.cum_gold)} ({_n(f.cum_coin)}) | {_n(f.gold_hour)} | {f.minutes:.0f} min{mark} |")
    over = [f for f in eco.fare_rows if f.minutes > BUDGET_FARE_MINUTES]
    if over:
        lines += ["", f"Acima de {BUDGET_FARE_MINUTES:.0f} min de caça: {len(over)} de {len(eco.fare_rows)} mapas, do {over[0].map.number:02d} "
                  f"(barca {over[0].map.warp}) ao {over[-1].map.number:02d}. Passagem acima de todo o ouro acumulado até o nível da barca: "
                  f"{', '.join(f'{f.map.number:02d}' for f in eco.fare_rows if f.fare > f.cum_gold) or 'nenhuma'}; acima do ouro sem vender "
                  f"o saque: {', '.join(f'{f.map.number:02d}' for f in eco.fare_rows if f.fare > f.cum_coin) or 'nenhuma'}."]
    return "\n".join(lines)


def report_loot(eco: Economy) -> str:
    lines = [f"Lembranças dos chefes do mundo (`item_chefe_*`, LootItem do asset; o chefe volta em {RESPAWN_SECONDS:.0f} s):", "",
             "| Chefe (nível) | Item | Chance × qtd | Valor (venda) | Receitas que usam / quantidade gasta | Abates esperados | Horas (só esperando o respawn) |",
             "|---|---|---|---|---|---|---|"]
    for k in eco.keepsakes:
        mark = " **(fora)**" if k.kills > BUDGET_KEEPSAKE_KILLS else ""
        lines.append(f"| {k.boss_name} ({k.level}) | {k.item} | {k.chance:.0%} × {k.mean_qty:g} | {k.value} ({int(k.value * SELL_BACK_RATE)}) | "
                     f"{k.recipes} / {k.needed} | {k.kills:.1f}{mark} | {k.hours:.1f} |")
    lines += ["", "Conjuntos das regiões 5–9 (`ArmorSetRules.MiddleDropChance`) e 13–16 (`DropChance`), só as feras do conjunto soltam:", "",
              "| Conjunto | Feras / espécies da região | Abates de fera p/ as 2 peças | Abates caçando a região | Horas de abate |", "|---|---|---|---|---|"]
    for s in eco.sets:
        lines.append(f"| {s.name} | {s.beasts} / {s.region_species} | {_n(s.kills)} | {_n(s.farm_kills)} | {s.farm_hours:.1f} |")
    lines += ["", "Saque de bioma por faixa (o valor por abate, venda a 50%; só existem as 4 tabelas da campanha):", "",
              "| Nível | Espécies | Bioma das espécies | item_gold por abate | Venda da tabela por abate | Total/abate | Ouro/h |", "|---|---|---|---|---|---|---|"]
    for level, p in eco.biome_bands:
        share = ", ".join(f"{k} {v:.0%}" for k, v in sorted(p.biomes.items(), key=lambda kv: -kv[1]))
        lines.append(f"| {level} | {p.species} | {share} | {p.gold:.2f} | {p.sale:.2f} | {p.total:.2f} | {p.total * KILLS_PER_HOUR:.0f} |")
    eq = ", ".join(f"{i} {p}" for i, p in eco.equipment)  # cheapest line of each piece
    lines += ["", f"Equipamento à venda nas lojas (preço em ouro): {eq}."]
    return "\n".join(lines)


def report_perigoso(rows: list) -> str:
    lines = ["| Comuns (nível) | Herói | Caça de 2 | Golpes p/ morrer (pior crítico, média / mín.) | Parado morre em | Vida gasta numa luta |",
             "|---|---|---|---|---|---|"]
    for r in rows:
        mark = " **(manso)**" if r.delta == PERIGOSO_DELTA and r.pull_share < PERIGOSO_PULL_SHARE and r.parked > PERIGOSO_PARKED_SECONDS else ""
        lines.append(f"| {r.creature} | +{r.delta} ({r.creature - r.delta}) | {r.farm:.1f} s | {r.hits:.0f} / {r.hits_min} | {r.parked:.1f} s | "
                     f"{r.pull_share:.0%}{mark} |")
    lines += ["", "\"Vida gasta numa luta\" = dano esperado dos dois comuns (os dois batem na primeira metade da caça de 2, um na "
              "segunda) em % da vida, sem poção nem cura; acima de 100% o herói morre antes de matar o par."]
    return "\n".join(lines)


def report_xp_extras(eco: Economy, data: rd.GameData, bosses: list) -> str:
    lines = ["Campanha (asset × tabela no nível da banda): " + " · ".join(
        f"{name} {band}: {asset:.0f} × {table}" for name, band, asset, table in campaign_xp_check(data, eco.kill_xp)) + ".", ""]
    world_rank, map_rank = boss_xp_ranks()
    lines += [f"Acampar o chefe = um abate a cada {RESPAWN_SECONDS:.0f} s de volta + {BOSS_KILL_SECONDS:.0f} s de luta; caçar comuns = "
              f"{KILLS_PER_HOUR:.0f} abates/h. Chefe do mundo paga ×{world_rank} um comum do nível "
              f"(`RegionBossRules.WorldBossKillXpMultiplier`, orçamento {BUDGET_BOSS_CAMP[0]:g}–{BUDGET_BOSS_CAMP[1]:g}× os comuns), "
              f"chefe de mapa ×{map_rank} (`MapBossKillXpMultiplier`, {BUDGET_MAP_BOSS_CAMP[0]:g}–{BUDGET_MAP_BOSS_CAMP[1]:g}×); "
              "antes, os dois ×9 (o posto genérico de chefe, `EnemyLevelScaling.RankOf`).", "",
              f"| Chefe do mundo (nível) | XP por abate (×{world_rank}) | XP/h acampando: antes (×9) → agora | XP/h caçando comuns | Acampar / caçar |",
              "|---|---|---|---|---|"]
    for b, xp, before, camp, commons in world_boss_xp(eco, bosses):
        lines.append(f"| {data.enemies[b.enemy_id].name.strip(chr(34))} ({b.level}) | {_n(xp)} | {_n(before)} → {_n(camp)} | {_n(commons)} | "
                     f"{before / commons:.2f}× → {camp / commons:.2f}× |")
    lines += ["", f"| Chefe de mapa (nível) | XP por abate (×{map_rank}) | XP/h acampando: antes (×9) → agora | XP/h caçando comuns | Acampar / caçar |",
              "|---|---|---|---|---|"]
    maps = load_map_bosses()
    for (enemy_id, _), (level, xp, camp, commons, ratio) in zip(maps, boss_camp_rows(eco, [lv for _, lv in maps], map_rank)):
        before = eco.kill_xp.of(level, "MapBoss") * 3600.0 / (RESPAWN_SECONDS + BOSS_KILL_SECONDS)
        name = data.enemies[enemy_id].name.strip(chr(34)) if enemy_id in data.enemies else enemy_id
        lines.append(f"| {name} ({level}) | {_n(xp)} | {_n(before)} → {_n(camp)} | {_n(commons)} | {before / commons:.2f}× → {ratio:.2f}× |")
    return "\n".join(lines)


def report_quest_rule(eco: Economy) -> str:
    rule = load_quest_xp()
    before = with_authored_quest_xp(eco)
    picks = [b for b in eco.bands if b.low in (21, 41, 51, 91, 101, 151, 201, 261, 301, 341, 421, 491)]
    base = {b.low: b for b in before.bands}
    above = [b for b in eco.bands if b.low > 100]
    lines = [f"`QuestXpRules.For`: acima do nível {rule.above}, etapa da história do meio = {rule.shares['meio']:.0%} do XP do nível em que abre, "
             f"Carta = {rule.shares['cartas']:.0%}, missão de cidade = {rule.shares['cidade']:.0%} (no nível de andar do mapa da cidade; na estrada, no nível da missão); nunca "
             "menos que o número escrito no catálogo. Campanha (Atos I–V, missões de região) sem mudança.", "",
             "| Faixa | XP de missão só com o escrito | Com a regra | Horas da faixa: antes → depois |", "|---|---|---|---|"]
    for b in picks:
        now = base[b.low]
        lines.append(f"| {b.low}–{b.high} | {now.quest_xp / now.xp:.0%} | {b.quest_xp / b.xp:.0%} | {_h(now.play_hours)} → {_h(b.play_hours)} |")
    with_quests = [b.quest_xp / b.xp for b in above if b.quest_xp > 0]
    lines += ["", f"Faixas 101–500: {min(with_quests):.0%}–{max(with_quests):.0%} de missão nas {len(with_quests)} faixas que têm missão, "
              f"0% nas outras {len(above) - len(with_quests)}; no total {quest_share_above_100(eco):.0%} do XP de 101 a 500 "
              f"(antes {quest_share_above_100(before):.0%}).",
              "", "Marcos só com o escrito: " + " · ".join(f"{lv}: {_h(h)} h" for lv, h in milestone_hours(before).items())]
    return "\n".join(lines)


def report(eco: Economy, perigoso_rows: list | None = None, data: rd.GameData | None = None, bosses: list | None = None) -> str:
    miss = problems(eco) + (perigoso_problems(perigoso_rows) if perigoso_rows else [])
    cap = fare_cap(eco)
    blocks = ["### XP e ouro por faixa de 10 níveis\n", report_bands(eco)]
    if data is not None and bosses:
        blocks += ["\n### XP por bicho\n", report_xp_extras(eco, data, bosses)]
    blocks += ["\n### XP de missão proporcional ao nível (QuestXpRules)\n", report_quest_rule(eco),
               "\n### Exigências dos mapas (na ordem do nível de andar)\n", report_gates(eco),
               "\n### Passagem da barca\n", report_fares(eco)
               + f"\n\nTeto: a passagem segue o nível da barca só até o {eco.access.level_cap} (`MapAccessRules.WarpFeeLevelCap`, passagem "
               f"máxima {eco.access.base_fee + eco.access.level_cap * eco.access.fee_per_level}); o teto que ainda cabe em "
               f"{BUDGET_FARE_MINUTES:.0f} min de caça na faixa mais pobre é o {cap}.",
               "\n### Loot\n", report_loot(eco)]
    if perigoso_rows:
        blocks += ["\n### \"Perigoso +30\" nos comuns\n", report_perigoso(perigoso_rows)]
    blocks += ["\n### Fora do orçamento\n", summarize(miss)]
    return "\n".join(blocks)


def summarize(miss: list) -> str:
    """One line per (kind, what), the places joined (72 fares in one line, not 72 lines)."""
    if not miss:
        return "- nada"
    grouped = {}
    for kind, where, what in miss:
        grouped.setdefault((kind, what), []).append(where)
    lines = []
    for (kind, what), places in grouped.items():
        shown = ", ".join(str(p) for p in places) if len(places) <= 8 else f"{len(places)} ({places[0]} a {places[-1]})"
        lines.append(f"- {kind} {what}: {shown}")
    return "\n".join(lines)


# ----------------------------------------------------------------------------- gold destinations above 100 (sugestão 3)

DURABILITY_CS = os.path.join(GAMEPLAY, "Items", "Durability", "DurabilityRules.cs")
REFINE_CS = os.path.join(GAMEPLAY, "Refining", "RefineRules.cs")
REFINE_DROPS_CS = os.path.join(GAMEPLAY, "Refining", "RefineStoneDrops.cs")
TALENT_CS = os.path.join(GAMEPLAY, "Talents", "TalentWheelRules.cs")
TALENT_CATALOG_CS = os.path.join(GAMEPLAY, "Talents", "TalentWheelCatalog.cs")
SINK_FROM = 101  # the bands the destinations are measured on (101-111 .. 491-500)
SINK_HITS_PER_KILL = 3.0  # hits a farming kill costs (2 commons, ~1.4 hits/s while both stand, ~3.3 s of fight a kill: level_500)
SINK_SET_REFINE = 9  # the worn set is at +9 (Pedra da Alma alone; the fee starts at +9)
SINK_LEVELS_PER_RESPEC = 50  # one talent reset every 50 levels (a new region's build)
BUDGET_SINK_SHARE = (0.30, 0.60)  # the destinations take 30-60% of the gold of 101-500
BUDGET_SINK_BAND = (0.25, 0.70)  # and no band of 10 levels leaves them idle or eats all its gold


@dataclass
class SinkRules:
    repair_cap: float
    repair_fraction: float
    refine_cost_per_level: float
    wear_per_hit: float
    rarity: list
    fee_start: int
    fee_step: int
    gold_per_soul: float  # average gold paid with each Pedra da Alma on the chain +0 -> +15 (RefineRules.ExpectedCostTo)
    soul_chance: dict  # biome -> Pedra da Alma chance per common kill
    respec_point: int
    respec_min: int
    respec_high_start: int
    respec_high_point: int
    unlock: int
    campaign_cap: int
    levels_per_point: int

    def fee_multiplier(self, level: int) -> float:
        return 1.0 + max(0, level - self.fee_start) / self.fee_step

    def points(self, level: int) -> int:
        """TalentWheelRules.EarnedPoints."""
        if level < self.unlock:
            return 0
        return (min(level, self.campaign_cap) - self.unlock + 1) + max(0, level - self.campaign_cap) // self.levels_per_point

    def respec(self, spent: int) -> int:
        """TalentWheelRules.RespecCostFor."""
        if spent <= 0:
            return 0
        return max(self.respec_min, spent * (self.respec_point + self.respec_high_point * max(0, spent - self.respec_high_start)))


def _floats(source: str, name: str) -> list:
    body = re.search(name + r"\s*=\s*\{(.*?)\};", source, re.DOTALL).group(1)
    return [float(x) for x in re.findall(r"\d+(?:\.\d+)?", re.sub(r"//[^\n]*", "", body))]


def load_sink_rules() -> SinkRules:
    dur, ref, drops, tal, cat = (_read(p) for p in (DURABILITY_CS, REFINE_CS, REFINE_DROPS_CS, TALENT_CS, TALENT_CATALOG_CS))
    chances, gold = _floats(ref, "Chances"), _floats(ref, "GoldCosts")
    blessing = int(_const(ref, "BlessingCap"))
    step_souls, step_gold = [0.0] * len(chances), [0.0] * len(chances)
    for i, p in enumerate(chances):  # the Markov chain of RefineRules.ExpectedCostTo: a failure above +6 drops one level
        back = i > blessing
        step_souls[i] = (1 + (1 - p) * (step_souls[i - 1] if back else 0)) / p
        step_gold[i] = (gold[i] + (1 - p) * (step_gold[i - 1] if back else 0)) / p
    souls = sum(step_souls[blessing:])
    row = re.search(r"RefineItems\.SoulStone, new\[\] \{([^}]*)\}", drops).group(1)
    soul = dict(zip(rd.BIOME_ORDER, (float(x.strip().rstrip("f")) for x in row.split(","))))
    return SinkRules(_const(dur, "RepairValueCap"), _const(dur, "RepairValueFraction"), _const(dur, "RefineCostPerLevel"),
                     _const(dur, "ArmourWearPerHit"), _floats(dur, "RarityMultipliers"), int(_const(ref, "FeeLevelStart")),
                     int(_const(ref, "FeeLevelsPerStep")), sum(step_gold[blessing:]) / souls, soul,
                     int(_const(tal, "RespecGoldPerPoint")), int(_const(tal, "RespecMinimumGold")), int(_const(tal, "RespecHighPointsStart")),
                     int(_const(tal, "RespecHighGoldPerPoint")), int(_const(cat, "UnlockLevel")), int(_const(cat, "HighLevelStart")),
                     int(_const(cat, "HighLevelLevelsPerPoint")))


def _set_piece(key: str, part: str) -> tuple:
    """(value, rarity, max durability) of a set piece's asset (Sets/ or Dungeon/ for the Marfim)."""
    for folder in ("Sets", "Dungeon"):
        path = os.path.join(ITEMS_DIR, folder, f"armor_{key}_{part}.asset")
        if os.path.exists(path):
            text = _read(path)

            def num(name: str) -> float:
                return float(re.search(name + r":\s*([\d.]+)", text).group(1))

            return num("_value"), int(num("_rarity")), num("_maxDurability")
    raise FileNotFoundError(f"armor_{key}_{part}")


@dataclass
class SinkRow:
    low: int
    high: int
    income: float  # kills + quests of the band
    souls: float  # Pedras da Alma dropped in the band (all go to the refine chain)
    refine: float
    set_key: str
    repair: float
    respec: float

    @property
    def spent(self) -> float:
        return self.refine + self.repair + self.respec

    @property
    def share(self) -> float:
        return self.spent / self.income if self.income else 0.0


def gold_sinks(eco: Economy, data: rd.GameData) -> list:
    """What the three destinations take out of each band of 101-500: the ferreiro's refine fee on every Pedra da Alma that
    drops (RefineRules.FeeFor), the repair of the worn set (DurabilityRules, capped value) and one talent reset every
    SINK_LEVELS_PER_RESPEC levels (TalentWheelRules.RespecCostFor). None is forced: this is a hero who refines, wears the set
    and changes the build."""
    import level_500 as l5  # noqa: E402
    rules = load_sink_rules()
    sets = l5.load_sets(l5.load_regions(data))
    rows = []
    for band in eco.bands:
        if band.low < SINK_FROM:
            continue
        chunk = [r for r in eco.levels if band.low <= r.level < band.high]

        def souls_of(r) -> float:
            return r.kills * sum(share * rules.soul_chance.get(b, 0.0) for b, share in eco.pay[r.level].biomes.items())

        souls = sum(souls_of(r) for r in chunk)
        refine = sum(souls_of(r) * rules.gold_per_soul * rules.fee_multiplier(r.level) for r in chunk)
        key = l5.available_sets(sets, band.low)[-1].key
        per_point = 0.0
        for part in ("chest", "helm"):
            value, rarity, durability = _set_piece(key, part)
            per_point += (min(value, rules.repair_cap) * rules.repair_fraction * rules.rarity[rarity]
                          * (1 + SINK_SET_REFINE * rules.refine_cost_per_level) / durability)
        repair = band.kills * SINK_HITS_PER_KILL * rules.wear_per_hit * per_point
        respec = rules.respec(rules.points(band.low)) * len(chunk) / SINK_LEVELS_PER_RESPEC
        rows.append(SinkRow(band.low, band.high, band.gold + band.quest_gold, souls, refine, key, repair, respec))
    return rows


def sink_total_share(rows: list) -> float:
    income = sum(r.income for r in rows)
    return sum(r.spent for r in rows) / income if income else 0.0


def report_gold_sinks(rows: list) -> str:
    rules = load_sink_rules()
    lines = [f"Destinos (nenhum obrigatório): taxa do ferreiro no refino (`RefineRules.FeeFor`: preço da campanha × (1 + (nível − "
             f"{rules.fee_start}) / {rules.fee_step}); cada Pedra da Alma que cai vai para a cadeia +0 → +15, {rules.gold_per_soul:.1f} "
             f"de ouro por pedra no preço da campanha), conserto do conjunto vestido (+{SINK_SET_REFINE}, {SINK_HITS_PER_KILL:.0f} golpes por "
             f"abate, valor no máximo {rules.repair_cap:.0f}: `DurabilityRules.RepairValueCap`) e um reset da Roda a cada "
             f"{SINK_LEVELS_PER_RESPEC} níveis (`TalentWheelRules.RespecCostFor`).", "",
             "| Faixa | Ouro que entra | Pedras da Alma | Refino | Conjunto | Conserto | Reset (rateado) | Gasto | % do ouro |",
             "|---|---|---|---|---|---|---|---|---|"]
    for r in rows:
        lines.append(f"| {r.low}–{r.high} | {_n(r.income)} | {r.souls:.0f} | {_n(r.refine)} | {r.set_key} | {_n(r.repair)} | {_n(r.respec)} | "
                     f"{_n(r.spent)} | {r.share:.0%} |")
    income = sum(r.income for r in rows)
    lines.append(f"| **{rows[0].low}–{rows[-1].high}** | **{_n(income)}** | {sum(r.souls for r in rows):.0f} | {_n(sum(r.refine for r in rows))} | | "
                 f"{_n(sum(r.repair for r in rows))} | {_n(sum(r.respec for r in rows))} | **{_n(sum(r.spent for r in rows))}** | "
                 f"**{sink_total_share(rows):.0%}** |")
    return "\n".join(lines)


def run(runs: int = 40) -> str:
    import level_500 as l5  # noqa: E402
    world = l5.World(runs)
    eco = build(world.base)
    return (report(eco, perigoso(world), world.base, world.bosses)
            + "\n\n### Destinos do ouro acima do 100 (sugestão 3)\n\n" + report_gold_sinks(gold_sinks(eco, world.base)))


def main() -> int:
    sys.stdout.reconfigure(encoding="utf-8")
    print(run())
    return 0


if __name__ == "__main__":
    sys.exit(main())
