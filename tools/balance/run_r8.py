"""Balance round r8 report: python -I tools/balance/run_r8.py [--stun reset|pause] [--candidate file.json]

Prints markdown tables (Docs/Balance/README.md §10 was written from this output):
  1. calibration against the r7 in-editor playtest (same protocol: levels 2/5/8/11, no gear, no stun);
  2. the r7 reference model (expected levels 2/4/6/8, no gear/quests);
  3. XP routes (rush / region quests): level at each region entry and boss, time-to-level;
  4. gear expected at each region (biome loot + boss bonus, Monte Carlo);
  5. TTK per hero x biome (2 commons; boss + 1 minion) at the level and gear the player really has,
     raw model and calibrated in-game estimate, plus the death-risk checks.

--candidate applies a JSON of data overrides in memory (used to try a retune before editing assets):
  {"enemies": {"enemy_x": {"max_health": 90}}, "region_quest_xp": {"quest_id": 120},
   "skills": {"skill_id": {"stun": 0.8}}, "affixes": {"affix_id": {"vmin": 1, "vmax": 3}}}
"""

from __future__ import annotations

import argparse
import json
import os
import statistics
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import balance_model as bm  # noqa: E402
import rapadura_data as rd  # noqa: E402

R7_PROTOCOL_LEVELS = {"Forest": 2, "Desert": 5, "Mountain": 8, "Caves": 11}
R7_EXPECTED_LEVELS = {"Forest": 2, "Desert": 4, "Mountain": 6, "Caves": 8}
R7_MEASURED = {"common": (4.3, 7.4), "boss": (15.0, 22.0)}
TARGET = {"common": (4.0, 7.0), "boss": (15.0, 22.0)}
MAP_OVERHEAD_SECONDS = 20.0  # walking the token, encounter intro, "Prepare-se/Já!", result screen
# Enemy stats the r7 playtest ran with (Docs/Balance/README.md §6): calibration and the r7 reference
# always use these, so they stay fixed whatever the assets hold now.
R7_ENEMIES = {
    "enemy_forest_wolf": {"max_health": 70, "attack_damage": 7}, "enemy_forest_spider": {"max_health": 70, "attack_damage": 7},
    "enemy_forest_alpha_wolf": {"max_health": 430, "attack_damage": 14},
    "enemy_desert_scorpion": {"max_health": 85, "attack_damage": 11}, "enemy_desert_ruin_guardian": {"max_health": 505, "attack_damage": 22},
    "enemy_mountain_stone_golem": {"max_health": 100, "attack_damage": 13}, "enemy_mountain_warlord": {"max_health": 580, "attack_damage": 16},
    "enemy_cave_bat_swarm": {"max_health": 80, "attack_damage": 7}, "enemy_cave_dweller": {"max_health": 700, "attack_damage": 18},
}
NAMES = {"Forest": "Bosque do Zarafshan", "Desert": "Areias Flamejantes", "Mountain": "Serra de Kaggen", "Caves": "Grutas do Limiar"}


def apply_candidate(data: rd.GameData, candidate: dict) -> rd.GameData:
    data = bm.clone_data(data)
    for enemy_id, fields in candidate.get("enemies", {}).items():
        for key, value in fields.items():
            setattr(data.enemies[enemy_id], key, value)
    for quest in data.region_quests:
        if quest.id in candidate.get("region_quest_xp", {}):
            quest.xp = candidate["region_quest_xp"][quest.id]
    for skills in data.skills.values():
        for skill in skills:
            for key, value in candidate.get("skills", {}).get(skill.id, {}).items():
                setattr(skill, key, value)
    for affix_id, fields in candidate.get("affixes", {}).items():
        for key, value in fields.items():
            setattr(data.affixes[affix_id], key, value)
    for biome, fields in candidate.get("biomes", {}).items():
        for key, value in fields.items():
            setattr(data.biomes[biome], key, value)
    return data


def without_stun(data: rd.GameData) -> rd.GameData:
    data = bm.clone_data(data)
    for skills in data.skills.values():
        for skill in skills:
            skill.stun = 0.0
    return data


def f1(x):
    return "—" if x is None or x == float("inf") else f"{x:.1f}".replace(".", ",")


def f2(x):
    return f"{x:.2f}".replace(".", ",")


def pct(x):
    return f"{x * 100:.0f}%"


def hero_fights(model, biome, level, gear, use_skills=True, attributes=True, skill_points=True):
    rows = {}
    for hero in bm.HEROES:
        build = bm.Build(hero, level, gear, attributes, skill_points)
        rows[hero] = {
            "common": model.fight(build, model.common_fight(biome), use_skills),
            "boss": model.fight(build, model.boss_fight(biome), use_skills),
            "build": build,
        }
    return rows


def calibrate(data):
    model = bm.Model(without_stun(apply_candidate(data, {"enemies": R7_ENEMIES})), stun_mode="reset")
    common_avgs, boss_avgs = [], []
    lines = ["| Bioma (nível) | 2 comuns: modelo (média dos 5) | Chefe + lacaio: modelo (pior–melhor herói) |", "|---|---|---|"]
    for biome in rd.BIOME_ORDER:
        rows = hero_fights(model, biome, R7_PROTOCOL_LEVELS[biome], bm.Gear(), attributes=False, skill_points=False)
        commons = [r["common"]["time"] for r in rows.values()]
        bosses = [r["boss"]["time"] for r in rows.values()]
        common_avgs.append(statistics.mean(commons))
        boss_avgs.append(statistics.mean(bosses))
        lines.append(f"| {NAMES[biome]} ({R7_PROTOCOL_LEVELS[biome]}) | {f1(statistics.mean(commons))} s | "
                     f"{f1(min(bosses))}–{f1(max(bosses))} s (média {f1(statistics.mean(bosses))}) |")
    k_common = statistics.mean(R7_MEASURED["common"]) / statistics.mean(common_avgs)
    k_boss = statistics.mean(R7_MEASURED["boss"]) / statistics.mean(boss_avgs)
    lines.append("")
    lines.append(f"Medido no r7: comuns {f1(R7_MEASURED['common'][0])}–{f1(R7_MEASURED['common'][1])} s, chefes "
                 f"{f1(R7_MEASURED['boss'][0])}–{f1(R7_MEASURED['boss'][1])} s. Fator jogo/modelo: comuns ×{f2(k_common)}, "
                 f"chefes ×{f2(k_boss)} (modelo r7: comuns {f1(min(common_avgs))}–{f1(max(common_avgs))} s, chefes "
                 f"{f1(min(boss_avgs))}–{f1(max(boss_avgs))} s).")
    return k_common, k_boss, "\n".join(lines)


def r7_reference(data, k_common, k_boss):
    model = bm.Model(without_stun(apply_candidate(data, {"enemies": R7_ENEMIES})), stun_mode="reset")
    lines = ["| Bioma (nível) | 2 comuns (pior–melhor) | Chefe + lacaio (pior–melhor) | Estimativa no jogo: comuns / chefe |", "|---|---|---|---|"]
    for biome in rd.BIOME_ORDER:
        rows = hero_fights(model, biome, R7_EXPECTED_LEVELS[biome], bm.Gear(), attributes=False, skill_points=False)
        c = [r["common"]["time"] for r in rows.values()]
        b = [r["boss"]["time"] for r in rows.values()]
        lines.append(f"| {NAMES[biome]} ({R7_EXPECTED_LEVELS[biome]}) | {f1(min(c))}–{f1(max(c))} s | {f1(min(b))}–{f1(max(b))} s | "
                     f"{f1(statistics.mean(c) * k_common)} s / {f1(statistics.mean(b) * k_boss)} s |")
    return "\n".join(lines)


def level_of(model, xp):
    return model.level_for_xp(xp)


def routes_table(model):
    lines = ["| Rota | Região | Lutas | XP de abates | XP de missões | XP chefe (+lacaio) | Nível na entrada | Nível no chefe | Nível na saída |",
             "|---|---|---|---|---|---|---|---|---|"]
    routes = {}
    for route in ("rush", "quests"):
        visits = bm.route_xp(model, route)
        routes[route] = visits
        for v in visits:
            lines.append(f"| {'só chefes' if route == 'rush' else 'missões regionais'} | {NAMES[v.biome]} | {f1(v.fights)} | {v.kill_xp:.0f} | "
                         f"{v.quest_xp:.0f} | {v.boss_xp:.0f} | {f1(level_of(model, v.entry_xp))} | {f1(level_of(model, v.boss_xp_before))} | "
                         f"{f1(level_of(model, v.exit_xp))} |")
    return routes, "\n".join(lines)


def time_to_level_table(model, routes, k_common):
    lines = ["| Região | XP por luta comum (1,5 inimigo) | Nível típico | XP para o próximo | Lutas por nível (só abates) | Minutos por nível (só abates) | Minutos por nível (com missões) |",
             "|---|---|---|---|---|---|---|"]
    for v in routes["quests"]:
        commons = model.biome_commons(v.biome)
        tw = sum(w for _, w in commons)
        xp_fight = sum(e.xp_reward * w for e, w in commons) / tw * bm.COMMONS_PER_ENCOUNTER
        lvl = int(level_of(model, (v.entry_xp + v.boss_xp_before) / 2))
        need = model.row(lvl)["xp_to_next"]
        fight_time = statistics.mean(
            model.fight(bm.Build(h, lvl), model.common_fight(v.biome))["time"] for h in bm.HEROES) * k_common * 0.75 + MAP_OVERHEAD_SECONDS
        fights = need / xp_fight
        with_quests = need / ((v.kill_xp + v.quest_xp) / v.fights)
        lines.append(f"| {NAMES[v.biome]} | {xp_fight:.0f} | {lvl} | {need:.0f} | {f1(fights)} | {f1(fights * fight_time / 60)} | {f1(with_quests * fight_time / 60)} |")
    return "\n".join(lines)


def gear_table(gear):
    lines = ["| Região | Momento | Dano plano | Dano % | Vel. de ataque | Crítico | Espinhos | Defesa plana | Vida | Arma com afixo |",
             "|---|---|---|---|---|---|---|---|---|---|"]
    for biome in rd.BIOME_ORDER:
        for phase in ("entry", "boss"):
            g = gear[(biome, phase)]
            share = gear["weapon_with_affix_share"][biome] if phase == "entry" else None
            lines.append(f"| {NAMES[biome]} | {'entrada' if phase == 'entry' else 'antes do chefe'} | +{f1(g.ad_flat)} | +{pct(g.ad_pct)} | "
                         f"+{pct(g.attack_speed)} | +{g.crit * 100:.1f}% | {pct(g.thorns)} | +{f1(g.defense)} | +{g.max_hp:.0f} | "
                         f"{pct(share) if share is not None else ''} |")
    return "\n".join(lines)


def ttk_tables(model, routes, gear, k_common, k_boss, route="quests"):
    visits = {v.biome: v for v in routes[route]}
    out = []
    summary = {}
    head = ("| Bioma (nível comuns / chefe) | Herói | 2 comuns (modelo → jogo) | Chefe + lacaio (modelo → jogo) | "
            "Golpes do chefe perdidos p/ stun | Vida mín. no chefe | Só ataque básico: mata o chefe em / morre em |")
    out.append(head)
    out.append("|---|---|---|---|---|---|---|")
    for biome in rd.BIOME_ORDER:
        v = visits[biome]
        lvl_common = int(level_of(model, (v.entry_xp + v.boss_xp_before) / 2))
        lvl_boss = int(level_of(model, v.boss_xp_before))
        g_common = gear[(biome, "entry")].plus(gear[(biome, "boss")]).scaled(0.5)
        g_boss = gear[(biome, "boss")]
        commons, bosses = [], []
        for hero in bm.HEROES:
            c = model.fight(bm.Build(hero, lvl_common, g_common), model.common_fight(biome))
            b = model.fight(bm.Build(hero, lvl_boss, g_boss), model.boss_fight(biome))
            nostun_model = bm.Model(without_stun(model.data), model.stun_mode)
            b0 = nostun_model.fight(bm.Build(hero, lvl_boss, g_boss), model.boss_fight(biome))
            basic = model.fight(bm.Build(hero, lvl_boss, g_boss), [model.biome_boss(biome)], use_skills=False)
            lost = b0["swings"] - b["swings"] if b["stuns"] else 0
            commons.append(c["time"])
            bosses.append(b["time"])
            out.append(f"| {NAMES[biome]} ({lvl_common} / {lvl_boss}) | {hero} | {f1(c['time'])} → {f1(c['time'] * k_common)} s | "
                       f"{f1(b['time'])} → {f1(b['time'] * k_boss)} s | {lost:+d} ({b['stuns']} stuns) | {pct(max(b['min_hp'], 0))}"
                       f"{' (morre)' if b['death_time'] is not None else ''} | {f1(basic['time'])} s / {f1(basic['death_time'])} s |")
        summary[biome] = {
            "levels": (lvl_common, lvl_boss),
            "common": (min(commons) * k_common, statistics.mean(commons) * k_common, max(commons) * k_common),
            "boss": (min(bosses) * k_boss, statistics.mean(bosses) * k_boss, max(bosses) * k_boss),
        }
    return summary, "\n".join(out)


def lucky_gear_table(model, routes, gear, k_boss):
    """Boss fight with the 90th-percentile flat AttackDamage (a lucky weapon roll) instead of the mean."""
    lines = ["| Bioma | Dano plano médio → p90 | Chefe no jogo, média dos 5: equipamento médio → p90 |", "|---|---|---|"]
    visits = {v.biome: v for v in routes["quests"]}
    for biome in rd.BIOME_ORDER:
        lvl = int(model.level_for_xp(visits[biome].boss_xp_before))
        mean_gear = gear[(biome, "boss")]
        lucky = bm.Gear(**{k: getattr(mean_gear, k) for k in mean_gear.__dataclass_fields__})
        lucky.ad_flat = gear["flat_p90"][biome]
        t_mean = statistics.mean(model.fight(bm.Build(h, lvl, mean_gear), model.boss_fight(biome))["time"] for h in bm.HEROES)
        t_lucky = statistics.mean(model.fight(bm.Build(h, lvl, lucky), model.boss_fight(biome))["time"] for h in bm.HEROES)
        lines.append(f"| {NAMES[biome]} ({lvl}) | +{f1(mean_gear.ad_flat)} → +{f1(lucky.ad_flat)} | {f1(t_mean * k_boss)} → {f1(t_lucky * k_boss)} s |")
    return "\n".join(lines)


def summary_table(summaries):
    lines = ["| Bioma | Rota | Níveis (comuns / chefe) | 2 comuns no jogo (pior / média / melhor) | Chefe no jogo (pior / média / melhor) | Meta |",
             "|---|---|---|---|---|---|"]
    for biome in rd.BIOME_ORDER:
        for route, summary in summaries.items():
            s = summary[biome]
            c, b = s["common"], s["boss"]
            ok_c = TARGET["common"][0] <= c[1] <= TARGET["common"][1]
            ok_b = TARGET["boss"][0] <= b[1] <= TARGET["boss"][1]
            lines.append(f"| {NAMES[biome]} | {route} | {s['levels'][0]} / {s['levels'][1]} | {f1(c[2])} / **{f1(c[1])}** / {f1(c[0])} s | "
                         f"{f1(b[2])} / **{f1(b[1])}** / {f1(b[0])} s | {'ok' if ok_c else 'FORA'} / {'ok' if ok_b else 'FORA'} |")
    return "\n".join(lines)


def power_table(model, routes, gear):
    lines = ["| Rota | Região | Nível | Vida | Ataque básico (DPS) | Kit Maithe / Joaquim (DPS) | Chefe da região mata o herói parado em |",
             "|---|---|---|---|---|---|---|"]
    for route, visits in routes.items():
        for v in visits:
            lvl = int(level_of(model, v.entry_xp))
            g = gear[route][(v.biome, "entry")]
            p = model.player(bm.Build("Maithe", lvl, g))
            kits = []
            for hero in ("Maithe", "Joaquim"):
                ph = model.player(bm.Build(hero, lvl, g))
                kits.append(sum((ph.hit(s.damage(sl)) + s.dot_total) / s.cycle for s, sl in ph.skills))
            boss = model.biome_boss(v.biome)
            lines.append(f"| {'só chefes' if route == 'rush' else 'missões regionais'} | {NAMES[v.biome]} | {lvl} | {p.hp:.0f} | {f1(p.basic_dps)} | "
                         f"{f1(kits[0])} / {f1(kits[1])} | {f1(model.time_to_kill_player(bm.Build('Maithe', lvl, g), boss))} s |")
    return "\n".join(lines)


def stun_table(model_reset, model_pause, routes, gear):
    lines = ["| Bioma | Herói | Skill | Stun (comum / chefe) | Golpes do chefe numa luta: sem stun → stun r7 (reset) → stun r8 (pausa) |", "|---|---|---|---|---|"]
    visits = {v.biome: v for v in routes["quests"]}
    stunners = {"Lavine": "skill_lavine_correntes_das_trevas", "Joaquim": "skill_joaquim_terremoto", "Icaro": "skill_icaro_granada_eletrica"}
    for biome in rd.BIOME_ORDER:
        lvl = int(level_of(model_pause, visits[biome].boss_xp_before))
        g = gear[(biome, "boss")]
        for hero, sid in stunners.items():
            skill = next(s for s in model_pause.data.skills[hero] if s.id == sid)
            nostun = bm.Model(without_stun(model_pause.data), "pause").fight(bm.Build(hero, lvl, g), [model_pause.biome_boss(biome)])
            r = model_reset.fight(bm.Build(hero, lvl, g), [model_reset.biome_boss(biome)])
            p = model_pause.fight(bm.Build(hero, lvl, g), [model_pause.biome_boss(biome)])
            lines.append(f"| {NAMES[biome]} | {hero} | {sid.split('_', 2)[2]} | {f1(skill.stun)} / {f1(skill.stun * bm.BOSS_STUN_MULTIPLIER)} s | "
                         f"{nostun['swings']} em {f1(nostun['time'])} s → {r['swings']} em {f1(r['time'])} s → {p['swings']} em {f1(p['time'])} s |")
    return "\n".join(lines)


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser()
    parser.add_argument("--stun", default="pause", choices=["reset", "pause"])
    parser.add_argument("--candidate")
    parser.add_argument("--runs", type=int, default=3000)
    parser.add_argument("--sections", default="all")
    args = parser.parse_args()

    data = rd.load_all()
    k_common, k_boss, cal = calibrate(data)  # calibration always against the shipped r7 numbers
    if args.candidate:
        with open(args.candidate, encoding="utf-8") as handle:
            data = apply_candidate(data, json.load(handle))
    model = bm.Model(data, args.stun)
    print("## Calibração (protocolo r7)\n")
    print(cal)
    print("\n## Referência r7 (níveis esperados 2/4/6/8, sem equipamento nem missões)\n")
    print(r7_reference(data, k_common, k_boss))
    routes, rt = routes_table(model)
    print("\n## Rotas de XP\n")
    print(rt)
    print("\n## Tempo para subir de nível (rota com missões)\n")
    print(time_to_level_table(model, routes, k_common))
    gear = {route: bm.gear_by_region(model, route, args.runs) for route in routes}
    print("\n## Equipamento esperado (rota com missões)\n")
    print(gear_table(gear["quests"]))
    print("\n## Poder na entrada de cada região\n")
    print(power_table(model, routes, gear))
    summaries = {}
    for route in ("rush", "quests"):
        summary, table = ttk_tables(model, routes, gear[route], k_common, k_boss, route)
        summaries["só chefes" if route == "rush" else "missões"] = summary
        print(f"\n## TTK por herói × bioma — rota {'só chefes' if route == 'rush' else 'com missões regionais'} (stun: {args.stun})\n")
        print(table)
    print("\n## Sorte no loot (p90 do dano plano, rota com missões)\n")
    print(lucky_gear_table(model, routes, gear["quests"], k_boss))
    print("\n## Resumo contra a meta\n")
    print(summary_table(summaries))
    print("\n## Stun nos chefes\n")
    print(stun_table(bm.Model(data, "reset"), bm.Model(data, "pause"), routes, gear["quests"]))


if __name__ == "__main__":
    main()
