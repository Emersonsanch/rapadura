"""Regenerates Docs/Balance/progression.csv rows 51..500 (Docs/Progressao500.md §1): python -I tools/balance/gen_progression.py

Rows 1..50 (the campaign table, designer-edited, mirrored in Resources/Balance/ProgressionCurve.asset) are kept as they
are. Above 50 the rows are what ProgressionCurve.BuildFormulaRow / LevelCurveMath produce: XP = round(xp(50) x (n/50)^3 x
W(n)), W = 1 up to 300 and 1 + ((n - 300)/200)^2 after; vitals and damage_mult computed in double and rounded once to
float (written as the shortest text that parses back to the same float, like ProgressionCsv's "R"); 2 attribute points
per level; 1 skill point every 5 levels. The Unity menu "Export Progression CSV" writes the same file.
"""

from __future__ import annotations

import csv
import math
import os
import struct

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
CSV_PATH = os.path.join(ROOT, "Docs", "Balance", "progression.csv")
MAX_LEVEL = 500  # LevelCurveMath.MaxLevel
CAMPAIGN = 50  # LevelCurveMath.CampaignLevels
WALL_START, WALL_WIDTH = 300, 200.0
SKILL_EVERY = 5  # LevelCurveMath.HighLevelSkillPointEvery
HEADER = ["level", "xp_to_next", "xp_total", "hp", "mana", "stamina", "damage_mult", "attribute_points", "skill_points"]


def f32(value: float) -> float:
    return struct.unpack("f", struct.pack("f", value))[0]


def fmt_float(value: float) -> str:
    value = f32(value)
    for decimals in range(0, 10):
        text = f"{value:.{decimals}f}"
        if decimals:
            text = text.rstrip("0").rstrip(".")
        if f32(float(text)) == value:
            return text
    return repr(value)


def wall(level: int) -> float:
    if level <= WALL_START:
        return 1.0
    d = (level - WALL_START) / WALL_WIDTH
    return 1.0 + d * d


def high_xp(level: int, anchor: int) -> int:
    ratio = min(level, MAX_LEVEL) / float(CAMPAIGN)
    return int(min(2**31 - 1, math.floor(max(1, anchor) * (ratio * ratio * ratio) * wall(level) + 0.5)))


def skill_points(level: int, per_level: int) -> int:
    if level <= 1:
        return 0
    if level <= CAMPAIGN:
        return per_level
    return 1 if level % SKILL_EVERY == 0 else 0


def curve_rows(campaign_rows: list, params: dict) -> list:
    """`campaign_rows`: the csv rows 1..50 (dicts of strings). Returns rows 1..500 as lists of strings (xp_total filled)."""
    anchor = int(campaign_rows[CAMPAIGN - 1]["xp_to_next"])
    out = [[r[h] for h in HEADER] for r in campaign_rows[:CAMPAIGN]]
    hp_step, mana_step, stamina_step, dmg_step = (f32(params[k]) for k in ("hp_step", "mana_step", "stamina_step", "dmg_step"))
    for level in range(CAMPAIGN + 1, MAX_LEVEL + 1):
        steps = level - 1
        out.append([
            str(level), str(high_xp(level, anchor)), "0",
            fmt_float(params["hp"] + hp_step * steps), fmt_float(params["mana"] + mana_step * steps),
            fmt_float(params["stamina"] + stamina_step * steps), fmt_float(1.0 + dmg_step * steps),
            str(params["attr"]), str(skill_points(level, params["skill"])),
        ])
    total = 0
    for row in out:
        row[2] = str(total)
        total += int(row[1])
    return out


def shipped_params() -> dict:
    """Formula parameters of Resources/Balance/ProgressionCurve.asset."""
    path = os.path.join(ROOT, "Assets", "Resources", "Balance", "ProgressionCurve.asset")
    values = {}
    with open(path, encoding="utf-8") as handle:
        for line in handle:
            line = line.strip()
            if line.startswith("_") and ":" in line:
                key, value = line.split(":", 1)
                try:
                    values[key] = float(value)
                except ValueError:
                    pass
    return {
        "hp": values["_baseMaxHealth"], "hp_step": values["_maxHealthPerLevel"],
        "mana": values["_baseMaxMana"], "mana_step": values["_maxManaPerLevel"],
        "stamina": values["_baseMaxStamina"], "stamina_step": values["_maxStaminaPerLevel"],
        "dmg_step": values["_damageMultiplierPerLevel"],
        "attr": int(values["_attributePointsPerLevel"]), "skill": int(values["_skillPointsPerLevel"]),
    }


def main() -> None:
    with open(CSV_PATH, newline="", encoding="utf-8") as handle:
        rows = list(csv.DictReader(line for line in handle if line.strip() and not line.startswith("#")))
    if len(rows) < CAMPAIGN:
        raise SystemExit(f"progression.csv has {len(rows)} rows; the campaign table needs {CAMPAIGN}")
    lines = [",".join(HEADER)] + [",".join(r) for r in curve_rows(rows, shipped_params())]
    with open(CSV_PATH, "w", newline="", encoding="utf-8") as handle:
        handle.write("\n".join(lines) + "\n")
    print(f"wrote {len(lines) - 1} levels to {CSV_PATH}")


if __name__ == "__main__":
    main()
