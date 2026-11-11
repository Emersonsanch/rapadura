"""Creature level scaling budget (Docs/Progressao500.md §4): python -I tools/balance/level_scaling.py [--runs N]

EnemyLevelScaling.cs scales a creature's health by Rd(L)/Rd(15) and its damage by Rh(L)/Rh(15) (reference hero damage /
toughness). This check plays the r8 reference hero (quest route, Grutas do Limiar end-of-campaign gear with flat affixes
rolled "por faixa", attributes and skill points from progression.csv) at hero level L against the Grutas do Limiar
archetypes (the level-15 campaign numbers) scaled to the same level L, and checks that the r8 invariants hold at every
level of the "até 100" scope: boss and commons TTK stay near the level-15 numbers, the boss kills a parked hero in
12-40 s, and a basic-attack-only hero still needs most of that time (the boss wins).
"""

from __future__ import annotations

import argparse
import os
import re
import statistics
import sys
from dataclasses import dataclass

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import balance_model as bm  # noqa: E402
import rapadura_data as rd  # noqa: E402
import run_r8  # noqa: E402

SCALING = os.path.join(rd.ROOT, "Assets", "Scripts", "Gameplay", "Enemies", "EnemyLevelScaling.cs")
ARCHETYPE_BIOME = "Caves"  # the level-15 campaign region: its enemies are the archetypes
LEVELS = [15, 20, 25, 30, 40, 50, 60, 70, 80, 90, 100]  # "até 100" (LevelCurveMath.ContentScopeLevel)

BOSS_BAND = (0.85, 1.20)  # boss TTK at level L / at level 15
COMMON_BAND = (0.80, 1.30)  # commons are a few hits: quantised
PARKED_TTD_BAND = (12.0, 40.0)
BASIC_EDGE = 0.75  # basic-attack-only kill time >= 75% of the parked time-to-die


@dataclass
class Scaling:
    archetype_level: int
    dmg: float
    off: float
    hp: float
    tough: float
    common_floor: float = 0.0  # EnemyLevelScaling.CommonHealthFloor: base health floor of a common...
    floor_from: int = 10 ** 9  # ...from this level up (CommonHealthFloorFromLevel)

    def base_health(self, health: float, level: int, is_boss: bool) -> float:
        """EnemyLevelScaling.FlooredBaseHealth: a common of level floor_from or more starts from at least common_floor."""
        return health if is_boss or level < self.floor_from else max(health, self.common_floor)

    def rd(self, level: int) -> float:
        s = max(1, level) - 1
        return (1 + self.dmg * s) * (1 + self.off * s)

    def rh(self, level: int) -> float:
        s = max(1, level) - 1
        return (1 + self.hp * s) * (1 + self.tough * s)

    def health_mult(self, level: int) -> float:
        return 1.0 if level <= self.archetype_level else self.rd(level) / self.rd(self.archetype_level)

    def damage_mult(self, level: int) -> float:
        return 1.0 if level <= self.archetype_level else self.rh(level) / self.rh(self.archetype_level)

    def gear_offense_band(self, level: int) -> float:
        """Flat offensive affixes rolled per band (§4.2: value x Rd(L)/Rd(15))."""
        return max(1.0, self.rd(level) / self.rd(self.archetype_level))

    def gear_defense_band(self, level: int) -> float:
        """Flat defensive affixes per band: the gear part of Rh only (the curve's +8 HP is the hero's own)."""
        s, s0 = max(1, level) - 1, self.archetype_level - 1
        return max(1.0, (1 + self.tough * s) / (1 + self.tough * s0))


def _const(source: str, name: str) -> float:
    match = re.search(r"const\s+(?:double|float|int)\s+" + name + r"\s*=\s*([-\d.]+)f?\s*;", source)
    if not match:
        raise ValueError(f"const {name} not found in EnemyLevelScaling.cs")
    return float(match.group(1))


def load_scaling() -> Scaling:
    with open(SCALING, encoding="utf-8-sig") as handle:
        src = handle.read()
    return Scaling(int(_const(src, "ArchetypeLevel")), _const(src, "HeroDamageGrowthPerLevel"),
                   _const(src, "HeroOffenseGrowthPerLevel"), _const(src, "HeroHealthGrowthPerLevel"),
                   _const(src, "HeroToughnessGrowthPerLevel"), _const(src, "CommonHealthFloor"),
                   int(_const(src, "CommonHealthFloorFromLevel")))


def band_gear(gear: bm.Gear, scaling: Scaling, level: int) -> bm.Gear:
    g = bm.Gear(**{k: getattr(gear, k) for k in gear.__dataclass_fields__})
    g.ad_flat *= scaling.gear_offense_band(level)
    for name in ("max_hp", "defense", "regen"):
        setattr(g, name, getattr(g, name) * scaling.gear_defense_band(level))
    return g


def scaled_data(data: rd.GameData, scaling: Scaling, level: int) -> rd.GameData:
    out = bm.clone_data(data)
    for enemy in out.enemies.values():
        enemy.max_health = round(scaling.base_health(enemy.max_health, level, enemy.is_boss) * scaling.health_mult(level))
        enemy.attack_damage = round(enemy.attack_damage * scaling.damage_mult(level), 1)
    return out


def evaluate(runs: int = 600, data: rd.GameData | None = None, levels: list | None = None) -> dict:
    data = data or rd.load_all()
    scaling = load_scaling()
    k_common, k_boss, _ = run_r8.calibrate(data)
    gear0 = bm.gear_by_region(bm.Model(data), "quests", runs)[(ARCHETYPE_BIOME, "boss")]
    out = {}
    for level in levels or LEVELS:
        model = bm.Model(scaled_data(data, scaling, level))
        gear = band_gear(gear0, scaling, level)
        boss = model.biome_boss(ARCHETYPE_BIOME)
        rows = []
        for hero in bm.HEROES:
            build = bm.Build(hero, level, gear)
            parked = model.time_to_kill_player(build, boss)
            rows.append({
                "boss": model.fight(build, model.boss_fight(ARCHETYPE_BIOME))["time"] * k_boss,
                "common": model.fight(build, model.common_fight(ARCHETYPE_BIOME))["time"] * k_common,
                "parked": parked,
                "basic": model.fight(build, [boss], use_skills=False)["time"],
            })
        out[level] = {key: statistics.mean(r[key] for r in rows) for key in rows[0]}
        out[level]["health_mult"] = scaling.health_mult(level)
        out[level]["damage_mult"] = scaling.damage_mult(level)
    return out


def violations(result: dict) -> list:
    base = result[min(result)]
    out = []
    for level, s in result.items():
        for key, band in (("boss", BOSS_BAND), ("common", COMMON_BAND)):
            ratio = s[key] / base[key]
            if not band[0] <= ratio <= band[1]:
                out.append(f"level {level}: {key} TTK {s[key]:.1f} s = {ratio:.0%} of level {min(result)} ({band[0]:.0%}-{band[1]:.0%})")
        if not PARKED_TTD_BAND[0] <= s["parked"] <= PARKED_TTD_BAND[1]:
            out.append(f"level {level}: parked time-to-die {s['parked']:.1f} s outside {PARKED_TTD_BAND}")
        if s["basic"] < BASIC_EDGE * s["parked"]:
            out.append(f"level {level}: basic-only kill {s['basic']:.1f} s < {BASIC_EDGE:.0%} of time-to-die {s['parked']:.1f} s")
    return out


def report(result: dict) -> str:
    lines = ["| Nível (herói = bicho) | ×vida / ×dano | 2 comuns | Chefe | Parado, chefe mata em | Só básico: mata em |",
             "|---|---|---|---|---|---|"]
    for level, s in result.items():
        lines.append(f"| {level} | {s['health_mult']:.2f} / {s['damage_mult']:.2f} | {run_r8.f1(s['common'])} s | "
                     f"{run_r8.f1(s['boss'])} s | {run_r8.f1(s['parked'])} s | {run_r8.f1(s['basic'])} s |")
    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--runs", type=int, default=600)
    args = parser.parse_args()
    result = evaluate(args.runs)
    print("## Nível do bicho (Grutas do Limiar como arquétipo, herói de referência no mesmo nível)\n")
    print(report(result))
    problems = violations(result)
    print("\n- " + ("\n- ".join(problems) if problems else "dentro do orçamento (1–100)"))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
