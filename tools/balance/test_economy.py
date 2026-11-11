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

# What Docs/Balanco100a500.md "Economia" reports as off budget, after sugestões 1 and 2 (§5.3). The fare is capped at
# warp level 57 (MapAccessRules.WarpFeeLevelCap, 905 at most): no fare costs more than an hour of farming any more.
# The quest XP of QuestXpRules (50% / 150% / 25% of the level's XP above 50; up to 50 the campaign curve keeps the authored
# numbers, so level 50 stays at 6.7 h, -16%) makes one documented miss: 251-261 (three letters, 35% of its XP) is ×0.67
# the hours per level of 241-251. When this moves, rerun `python -I tools/balance/economy.py`, update the doc, then this
# snapshot.
DOCUMENTED_EXPENSIVE_FARES: set = set()
DOCUMENTED_MISSES = {("band", 251, "drop")}


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
    assert by_source == {"meio": 15, "cartas": 17, "cidade": 86, "campanha": 8}  # 46 + 40 on the road (regions 9-16)
    assert len(eco.maps) == 100 and [m.number for m in eco.maps] == list(range(1, 101))
    assert len(eco.fare_rows) == 99  # map 01 is the free tutorial map
    assert eco.curve.campaign == 50 and len(eco.levels) == ec.MAX_LEVEL - 1
    assert all(q.xp > 0 for q in eco.quests if q.source != "campanha")
    assert len(eco.keepsakes) == 12 and len(eco.sets) == 9  # Regions 5-9 and 13-16 (ArmorSetRules)


def test_milestones_follow_progressao500(eco):
    for level, hours in ec.milestone_hours(eco).items():
        if ("xp", level, "fast") in DOCUMENTED_MISSES:
            target = ec.DOC_PLAY_HOURS[level]
            assert target * (1 - 1.25 * ec.BUDGET_MILESTONE_TOLERANCE) <= hours < target * (1 - ec.BUDGET_MILESTONE_TOLERANCE), (level, hours)  # -20% to -25%
            continue
        target = ec.DOC_PLAY_HOURS[level]
        assert abs(hours / target - 1) <= ec.BUDGET_MILESTONE_TOLERANCE, (level, hours, target)


def test_no_wall_and_no_free_ride_above_the_campaign(eco):
    low, high = ec.BUDGET_BAND_STEP
    for before, band in zip(eco.bands, eco.bands[1:]):
        if band.low <= eco.curve.campaign:
            continue
        step = band.hours_per_level / before.hours_per_level
        if ("band", band.low, "drop") in DOCUMENTED_MISSES:
            assert 0.6 <= step < low, (band.low, step)
        else:
            assert low <= step <= high, (band.low, step)
        assert band.quest_xp <= ec.BUDGET_QUEST_SHARE_MAX * band.xp, band.low


def test_quests_pay_their_share_above_100(eco):
    low, high = ec.BUDGET_QUEST_SHARE_BAND_ABOVE_100
    above = [b for b in eco.bands if b.low > 100]
    assert all(b.quest_xp > 0 for b in above), [b.low for b in above if b.quest_xp == 0]  # sugestão 5: no band of 101-500 without a quest
    for band in above:
        if band.quest_xp > 0:
            assert low <= band.quest_xp / band.xp <= high, (band.low, band.quest_xp / band.xp)
    # MU style (Progressao500 §1.3, decided 2026-11-11): past 100 the hero climbs by hunting, quests pay 8-15% of 101-500.
    assert ec.BUDGET_QUEST_SHARE_ABOVE_100 == (0.08, 0.15)
    assert ec.BUDGET_QUEST_SHARE_ABOVE_100[0] <= ec.quest_share_above_100(eco) <= ec.BUDGET_QUEST_SHARE_ABOVE_100[1]
    assert ec.quest_share_above_100(ec.with_authored_quest_xp(eco)) < 0.01  # the authored numbers alone: ~0%


def test_quest_xp_follows_quest_xp_rules(eco):
    rule = ec.load_quest_xp()
    assert rule.above == 50 and rule.shares == {"meio": 0.5, "cartas": 1.5, "cidade": 0.25}
    for q in eco.quests:
        assert q.xp >= q.authored, q.quest_id
        if q.level <= rule.above or q.source == "campanha":
            assert q.xp == q.authored, q.quest_id
        else:
            assert q.xp == max(q.authored, int(rule.shares[q.source] * eco.curve.to_next(q.level) + 0.5)), q.quest_id
    letters = [q for q in eco.quests if q.source == "cartas"]
    assert all(q.xp > q.authored for q in letters)  # 150% of a level above 100 is always more than the 2 300-4 600 written


def test_fare_is_capped(eco):
    assert eco.access.level_cap == 57
    assert max(f.fare for f in eco.fare_rows) == 905
    assert eco.access.level_cap <= ec.fare_cap(eco)
    assert all(f.minutes <= ec.BUDGET_FARE_MINUTES for f in eco.fare_rows)


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
    assert set(miss) == DOCUMENTED_MISSES


def test_gold_has_destinations_above_100(eco, world):
    """Sugestão 3 (Docs/Balanco100a500.md §5.3): refine fee, capped repair and talent reset take 30-60% of the gold of 101-500,
    no band of 10 levels outside 25-70%, and the three grow with the band (none forced)."""
    rows = ec.gold_sinks(eco, world.base)
    assert rows[0].low == ec.SINK_FROM and rows[-1].high == ec.MAX_LEVEL
    low, high = ec.BUDGET_SINK_SHARE
    assert low <= ec.sink_total_share(rows) <= high, ec.sink_total_share(rows)
    band_low, band_high = ec.BUDGET_SINK_BAND
    for r in rows:
        assert band_low <= r.share <= band_high, (r.low, r.share)
        assert r.refine > 0 and r.repair > 0 and r.respec > 0, r.low
    rules = ec.load_sink_rules()
    assert rules.fee_multiplier(100) == 1.0 and rules.fee_multiplier(500) > 4  # campaign price up to 100, x4.2 at 500
    assert rules.respec(rules.points(100)) == 44 * rules.respec_point and rules.respec(rules.points(500)) == 14700
    assert rows[-1].respec > rows[0].respec and rows[-1].refine > rows[0].refine
    # the cap is what keeps the repair of the sets of Regions 13-16 (4 700-10 400 of value) below a farming hour of gold
    assert all(r.repair / r.income <= 0.30 for r in rows)


def test_bosses_pay_their_xp_per_hour(eco, world):
    """Sugestão 4 (Docs/Balanco100a500.md §5.3): camping a world boss (RegionBossRules.WorldBossKillXpMultiplier) pays 1.1-1.5x the
    XP per hour of farming the commons of its level; a map boss of the fichas (MapBossKillXpMultiplier) 0.8-1.2x."""
    assert len(world.bosses) == 12 and len(ec.load_map_bosses()) == 12
    assert ec.boss_camp_problems(eco, world.bosses) == []
    world_rank, map_rank = ec.boss_xp_ranks()
    assert world_rank > map_rank > eco.kill_xp.ranks["MapBoss"]
    for _b, _xp, before, camp, commons in ec.world_boss_xp(eco, world.bosses):
        assert before / commons < 0.6 < ec.BUDGET_BOSS_CAMP[0] <= camp / commons  # was ~0.51x with the generic x9
