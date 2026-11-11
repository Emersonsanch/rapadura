"""pytest for the 500-level curve and the creature level scaling (Docs/Progressao500.md §1, §2, §4).

Run from the repo root:
    uv run --no-project --with pytest python -m pytest -q -p no:cacheprovider tools/balance
"""

from __future__ import annotations

import csv
import os
import sys

import pytest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import balance_model as bm  # noqa: E402
import gen_progression as gp  # noqa: E402
import level_scaling as ls  # noqa: E402
import rapadura_data as rd  # noqa: E402

# Docs/Progressao500.md §1.3: level -> (xp to next, total to reach)
MILESTONES = {1: (100, 0), 10: (236, 1357), 25: (985, 8848), 50: (10672, 105715), 51: (11325, 116387),
              100: (85376, 2069521), 150: (288144, 10639402), 200: (683008, 33787346), 300: (2305152, 171713342),
              400: (6830080, 580789956), 500: (21344000, 1853225538)}


@pytest.fixture(scope="module")
def rows():
    return rd.load_progression()


def test_csv_covers_the_cap_and_matches_the_doc(rows):
    assert len(rows) == gp.MAX_LEVEL
    for level, (xp, total) in MILESTONES.items():
        row = rows[level - 1]
        assert row["xp_to_next"] == xp, level
        assert row["xp_total"] == total, level


def test_csv_is_what_the_generator_writes(rows):
    with open(gp.CSV_PATH, newline="", encoding="utf-8") as handle:
        text_rows = list(csv.DictReader(handle))
    regenerated = gp.curve_rows(text_rows, gp.shipped_params())
    assert [r[:2] + r[3:] for r in regenerated] == [[t[h] for h in gp.HEADER if h != "xp_total"] for t in text_rows]


def test_points_per_level(rows):
    assert sum(r["attribute_points"] for r in rows[:100]) == 198
    assert sum(r["attribute_points"] for r in rows) == 998
    assert sum(r["skill_points"] for r in rows[:50]) == 49
    assert sum(r["skill_points"] for r in rows[:100]) == 59
    assert sum(r["skill_points"] for r in rows) == 139
    for a, b in zip(rows, rows[1:]):
        assert b["xp_to_next"] > a["xp_to_next"] and b["hp"] > a["hp"] and b["damage_mult"] > a["damage_mult"]


def test_skill_points_past_fifty_follow_the_curve():
    model = bm.Model(rd.load_all())
    build = bm.Build("Maithe", 100)
    spent = sum(lv - 1 for _, lv in model.skill_levels(build))
    assert spent <= 59  # 59 points at 100 (the ultimate costs one), skills cap at 5 long before


def test_scaling_constants_match_the_doc_table():
    s = ls.load_scaling()
    assert s.archetype_level == 15 and s.tough == 0.0  # HeroToughnessGrowthPerLevel (0.005 until 2026-11-11)
    for level, (hp, dmg) in {25: (1.26, 1.38), 50: (2.02, 2.32), 100: (4.06, 4.21), 500: (45.1, 19.3)}.items():
        assert s.health_mult(level) == pytest.approx(hp, abs=0.06 if level == 500 else 0.01)
        assert s.damage_mult(level) == pytest.approx(dmg, abs=0.06 if level == 500 else 0.01)
    assert all(s.health_mult(level) == 1.0 and s.damage_mult(level) == 1.0 for level in (3, 7, 10, 15))


@pytest.fixture(scope="module")
def result():
    return ls.evaluate(runs=300)


def test_creature_level_budget_holds_up_to_100(result):
    assert max(result) == 100
    assert ls.violations(result) == []
