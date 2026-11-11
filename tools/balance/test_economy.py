"""pytest for the economy of levels 1-500 against the real content (economy.py, Docs/Balanco100a500.md "Economia").

Run from the repo root:
    uv run --no-project --with pytest python -m pytest -q -p no:cacheprovider tools/balance
"""

from __future__ import annotations

import math
import os
import sys

import pytest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import economy as ec  # noqa: E402
import level_500 as l5  # noqa: E402

# What Docs/Balanco100a500.md "Economia" reports as off budget. The only miss is the Barca fare: gold per hour is flat
# (~930-1130/h at every level: item_gold and the 4 campaign biome tables do not scale), the fare grows 15 per warp level,
# so from map 29 (warp 65) on it costs more than an hour of farming (never more than the gold earned by then). The fix
# is a design call in MapAccessRules (a cap, or gold that grows with level), not a MapCatalog Fee, so it stays a
# suggestion. When this moves, rerun `python -I tools/balance/economy.py`, update the doc, then this snapshot.
DOCUMENTED_EXPENSIVE_FARES = set(range(29, 101))


@pytest.fixture(scope="module")
def world():
    return l5.World(20)


@pytest.fixture(scope="module")
def eco(world):
    return ec.build(world.base)


@pytest.fixture(scope="module")
def perigoso_rows(world):
    return ec.perigoso(world)


def test_content_is_read_from_the_code(eco):
    by_source = {}
    for q in eco.quests:
        by_source[q.source] = by_source.get(q.source, 0) + 1
    assert by_source == {"meio": 15, "cartas": 17, "cidade": 46, "campanha": 8}
    assert len(eco.maps) == 100 and [m.number for m in eco.maps] == list(range(1, 101))
    assert len(eco.fare_rows) == 99  # map 01 is the free tutorial map
    assert eco.curve.campaign == 50 and len(eco.levels) == ec.MAX_LEVEL - 1
    assert all(q.xp > 0 for q in eco.quests if q.source != "campanha")
    assert len(eco.keepsakes) == 12 and len(eco.sets) == 4


def test_milestones_follow_progressao500(eco):
    for level, hours in ec.milestone_hours(eco).items():
        target = ec.DOC_PLAY_HOURS[level]
        assert abs(hours / target - 1) <= ec.BUDGET_MILESTONE_TOLERANCE, (level, hours, target)


def test_no_wall_and_no_free_ride_above_the_campaign(eco):
    low, high = ec.BUDGET_BAND_STEP
    for before, band in zip(eco.bands, eco.bands[1:]):
        if band.low <= eco.curve.campaign:
            continue
        step = band.hours_per_level / before.hours_per_level
        assert low <= step <= high, (band.low, step)
        assert band.quest_xp <= ec.BUDGET_QUEST_SHARE_MAX * band.xp, band.low


def test_every_gate_is_affordable_on_arrival(eco):
    for g in eco.gate_rows:
        assert g.how in ("presente", "missão", "já tinha", "compra", "sem compra"), g.how
        if g.how == "compra":
            assert 0 < g.cost <= g.balance_before, (g.map.number, g.item, g.cost, g.balance_before)


def test_no_fare_above_all_the_gold_earned(eco):
    for f in eco.fare_rows:
        assert f.fare <= f.cum_gold, (f.map.number, f.fare, f.cum_gold)


def test_expensive_fares_are_the_documented_ones(eco):
    over = {f.map.number for f in eco.fare_rows if f.minutes > ec.BUDGET_FARE_MINUTES}
    assert over == DOCUMENTED_EXPENSIVE_FARES


def test_boss_keepsakes_and_sets_are_reachable(eco):
    for k in eco.keepsakes:
        assert k.needed > 0, k.item  # every keepsake is spent by some house recipe
        assert 0 < k.chance and k.kills <= ec.BUDGET_KEEPSAKE_KILLS, (k.item, k.kills)
    for s in eco.sets:
        assert s.farm_kills <= ec.BUDGET_SET_KILLS, (s.name, s.farm_kills)


def test_no_shop_line_sells_below_its_buy_back(eco):
    assert eco.arbitrage == []


def test_gold_per_kill_is_positive_everywhere(eco):
    assert all(p.total > 0 and not math.isnan(p.total) for p in eco.pay.values())


def test_perigoso_plus_30_still_scares(perigoso_rows):
    assert ec.perigoso_problems(perigoso_rows) == []
    for r in perigoso_rows:
        if r.delta == ec.PERIGOSO_DELTA:
            assert r.pull_share >= ec.PERIGOSO_PULL_SHARE and r.parked <= ec.PERIGOSO_PARKED_SECONDS, r


def test_only_the_documented_misses(eco, perigoso_rows):
    miss = ec.problems(eco) + ec.perigoso_problems(perigoso_rows)
    assert {(k, m) for k, _, m in miss} <= {("fare", "expensive")}
