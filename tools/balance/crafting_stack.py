"""Advanced crafting layer of the power check (Docs/Balance/README.md "Crafting avançado"):
python -I tools/balance/crafting_stack.py [--runs N]

Two things of the advanced crafting reach combat (runes are consumables like the potions, outside the TTK model):

  * Vô Jorge's weapon enchantments (ImbueCatalog.cs): one whole-fight modifier that shares the road-event blessing's
    slot (a fight with a road blessing pauses the enchantment, ImbueRules.AppliesThisFight). So the model offers every
    enchantment as one more candidate of the "event" slot of power_stack.best_stack, for every region (an upper bound:
    in the game the deeper ones need their tier). The §14.3 stack budget (power_stack.violations) must still hold.
  * Crafted / fused gear (CraftTierRules.RarityCaps): rarity rolled by material quality and capped by the tier, the
    fusion forge one step up to the same cap. Upper bound: on arriving at each region the hero gets one piece at the
    tier's cap in every slot the recipes make (MainHand, Head, Chest, Boots) as if every fusion succeeded, on top of the
    r8 loot Monte Carlo (bm.gear_by_region). Budget: CRAFT_*_BUDGET of the plain r8 hero, the stack budget on the
    crafted hero, stack + crafted + refine never under FULL_BOSS_FLOOR, and the whole pile (stack + refine + wings +
    fruits + perfect excellent pieces + forged pieces on the slots left, everything_gear) within the global
    power_stack.TOTAL_BOSS_BUDGET / TOTAL_BOSS_FLOOR of the item options layer.
"""

from __future__ import annotations

import argparse
import os
import random
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import balance_model as bm  # noqa: E402
import power_stack as ps  # noqa: E402
import rapadura_data as rd  # noqa: E402
import run_r8  # noqa: E402

ADVANCED = os.path.join(ps.SCRIPTS, "Gameplay", "Crafting", "Advanced")
IMBUE_CATALOG = os.path.join(ADVANCED, "ImbueCatalog.cs")
TIER_RULES = os.path.join(ADVANCED, "CraftTierRules.cs")
RARITY = {"Common": 0, "Uncommon": 1, "Rare": 2, "Epic": 3, "Legendary": 4}
# One representative base item per slot the advanced recipes make (the affix pool depends only on the slot).
CRAFTED_SLOTS = {"MainHand": "item_iron_sword", "Head": "item_leather_helmet", "Chest": "item_leather_chest", "Boots": "item_leather_boots"}
CRAFT_BOSS_BUDGET = 0.92  # r8 hero + crafted/fused gear: boss TTK >= 92% of the plain r8 one (same as the refine alone)
CRAFT_COMMON_BUDGET = 0.80


def load_imbues() -> list:
    """(id, tier, Modifier) of every enchantment, parsed from ImbueCatalog.cs."""
    source = ps._read(IMBUE_CATALOG)
    ids = dict(re.findall(r'public const string (\w+) = "(imbue_\w+)"', source))
    out = []
    for const, tier, stat, application, value in re.findall(
            r"new ImbueSpec\((\w+),\s*(\d+),\s*CreatureParts\.\w+,\s*\d+,\s*\d+,\s*StatType\.(\w+),\s*ModifierApplication\.(\w+),\s*([-\d.]+)f\)", source):
        out.append((ids[const], int(tier), ps.Modifier(stat, application == "PercentAdditive", float(value))))
    return out


def load_caps() -> list:
    """CraftTierRules.RarityCaps as ints (index 0 = tier 1)."""
    body = re.search(r"RarityCaps\s*=\s*\{(.*?)\};", ps._read(TIER_RULES), re.S).group(1)
    return [RARITY[name] for name in re.findall(r"ItemRarity\.(\w+)", body)]


def crafted_gear_by_region(model: bm.Model, caps: list, runs: int = 3000, seed: int = 8) -> dict:
    """bm.gear_by_region (quest route) where, on entering region i, every CRAFTED_SLOTS slot is also offered one roll at
    caps[i] (tier i + 1). Same RNG stream for the loot part would differ anyway, so it is its own Monte Carlo."""
    data = model.data
    rng = random.Random(seed)
    sums = {(b, phase): bm.Gear() for b in rd.BIOME_ORDER for phase in ("entry", "boss")}
    for _ in range(runs):
        equipped = {}

        def offer(slot, gear):
            if slot not in equipped or gear.score() > equipped[slot].score():
                equipped[slot] = gear

        def total():
            g = bm.Gear()
            for piece in equipped.values():
                g = g.plus(piece)
            return g

        for index, biome_name in enumerate(rd.BIOME_ORDER):
            biome = data.biomes[biome_name]
            for slot, item_id in CRAFTED_SLOTS.items():
                offer(*bm.roll_item(data, rng, item_id, caps[index]))
            sums[(biome_name, "entry")] = sums[(biome_name, "entry")].plus(total())
            fights = bm.region_fights(model, biome_name, "quests")
            whole = int(fights)
            for _ in range(whole + (1 if rng.random() < fights - whole else 0)):
                for _ in range(rng.randint(1, 2)):
                    for slot, gear in bm.roll_kill(data, rng, biome, False, None):
                        offer(slot, gear)
            sums[(biome_name, "boss")] = sums[(biome_name, "boss")].plus(total())
            for slot, gear in bm.roll_kill(data, rng, biome, True, model.biome_boss(biome_name)):
                offer(slot, gear)
            for _ in range(rng.randint(0, 2)):
                for slot, gear in bm.roll_kill(data, rng, biome, False, None):
                    offer(slot, gear)
    return {key: value.scaled(1.0 / runs) for key, value in sums.items()}


def everything_gear(base_gear: dict, crafted: dict, curve, wings, options, cycle: int) -> dict:
    """power_stack.optioned_gear (stack + refine + wings + fruits + perfect excellent pieces) plus the crafted gain on the
    slots that are NOT excellent: a crafted or fused piece never carries MU options (crafted ones are generated without
    them; FusionRules refuses pieces with options), so a slot is either excellent or forged. The gain per free slot is
    the average of the four crafted slots' (weapon included: conservative for the armour slots that are left)."""
    out = ps.optioned_gear(base_gear, curve, wings, options, cycle, "excellent")
    for biome in rd.BIOME_ORDER:
        pieces = 4 if cycle > 0 else ps.OPTION_PIECES_BY_REGION[biome]
        free = max(0, len(CRAFTED_SLOTS) - pieces) / len(CRAFTED_SLOTS)
        for phase in ("entry", "boss"):
            key = ("Caves", "boss") if cycle > 0 else (biome, phase)
            gain = crafted[key].plus(base_gear[key].scaled(-1.0)).scaled(free)
            out[(biome, phase)] = out[(biome, phase)].plus(gain)
    return out


def imbued_context(ctx: ps.Context, imbues: list) -> ps.Context:
    """ctx whose event slot also offers every enchantment (they share it in the game)."""
    return ps.Context(ctx.data, ctx.rules, ctx.meals, ctx.relics, ctx.owner_multiplier,
                      list(ctx.events) + [mod for _, _, mod in imbues if mod.stat in ps.COMBAT_STATS],
                      ctx.k_common, ctx.k_boss, ctx.hunting)


def crafted_violations(plain: dict, crafted: dict, crafted_refined: dict, everything: dict, cycle: int) -> list:
    out = [f"stack on crafted hero: {v}" for v in ps.violations(crafted, cycle) if "basic-only" not in v and "commons" not in v]
    for biome in plain:
        p, c, cr = ps.biome_summary(plain, biome), ps.biome_summary(crafted, biome), ps.biome_summary(crafted_refined, biome)
        e = ps.biome_summary(everything, biome)
        if e["stack_boss"] < ps.TOTAL_BOSS_BUDGET * p["r8_boss"]:
            out.append(f"{biome}: everything+crafted boss {e['stack_boss']:.1f} s < {ps.TOTAL_BOSS_BUDGET:.0%} of r8 {p['r8_boss']:.1f} s")
        if e["stack_boss"] < ps.TOTAL_BOSS_FLOOR:
            out.append(f"{biome}: everything+crafted boss {e['stack_boss']:.1f} s < {ps.TOTAL_BOSS_FLOOR:g} s")
        if c["r8_boss"] < CRAFT_BOSS_BUDGET * p["r8_boss"]:
            out.append(f"{biome}: crafted boss {c['r8_boss']:.1f} s < {CRAFT_BOSS_BUDGET:.0%} of r8 {p['r8_boss']:.1f} s")
        if c["r8_common"] < CRAFT_COMMON_BUDGET * p["r8_common"]:
            out.append(f"{biome}: crafted commons {c['r8_common']:.1f} s < {CRAFT_COMMON_BUDGET:.0%} of r8 {p['r8_common']:.1f} s")
        if cr["stack_boss"] < ps.FULL_BOSS_FLOOR:
            out.append(f"{biome}: stack+crafted+refine boss {cr['stack_boss']:.1f} s < {ps.FULL_BOSS_FLOOR:g} s")
    return out


def run(ctx: ps.Context, runs: int) -> dict:
    model = bm.Model(ctx.data)
    base_gear = bm.gear_by_region(model, "quests", runs)
    crafted = crafted_gear_by_region(model, load_caps(), runs)
    curve = ps.load_refine()
    imbues = load_imbues()
    ictx = imbued_context(ctx, imbues)
    wings, options = ps.load_wings(), ps.load_options()
    out = {"plain": {}, "imbued": {}, "crafted": {}, "crafted_refined": {}, "everything": {}}
    for c in (0, 1):
        out["everything"][c] = ps.evaluate(ictx, c, runs, everything_gear(base_gear, crafted, curve, wings, options, c))
        out["plain"][c] = ps.evaluate(ctx, c, runs, base_gear)
        out["imbued"][c] = ps.evaluate(ictx, c, runs, base_gear)
        out["crafted"][c] = ps.evaluate(ctx, c, runs, crafted)
        out["crafted_refined"][c] = ps.evaluate(ctx, c, runs, ps.refined_gear(crafted, curve))
    return out


def report(res: dict) -> str:
    lines = ["| Ciclo | Bioma | Chefe: r8 → pilha → pilha c/ benzedura | Chefe: r8 → r8 c/ peças feitas → pilha+feitas+refino | "
             "2 comuns: r8 → pilha c/ benzedura → r8 c/ feitas |", "|---|---|---|---|---|"]
    for c in (0, 1):
        for biome in rd.BIOME_ORDER:
            p = ps.biome_summary(res["plain"][c], biome)
            i = ps.biome_summary(res["imbued"][c], biome)
            k = ps.biome_summary(res["crafted"][c], biome)
            kr = ps.biome_summary(res["crafted_refined"][c], biome)
            f1 = ps.f1
            lines.append(f"| NG+{c} | {run_r8.NAMES[biome]} | {f1(p['r8_boss'])} → {f1(p['stack_boss'])} → **{f1(i['stack_boss'])}** s | "
                         f"{f1(p['r8_boss'])} → {f1(k['r8_boss'])} → **{f1(kr['stack_boss'])}** s | "
                         f"{f1(p['r8_common'])} → {f1(i['stack_common'])} → {f1(k['r8_common'])} s |")
    return "\n".join(lines)


def problems(res: dict) -> list:
    out = [f"NG+{c} enchant: {v}" for c in (0, 1) for v in ps.violations(res["imbued"][c], c)]
    out += [f"NG+{c} crafted: {v}" for c in (0, 1) for v in crafted_violations(res["plain"][c], res["crafted"][c], res["crafted_refined"][c], res["everything"][c], c)]
    return out


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser()
    parser.add_argument("--runs", type=int, default=3000)
    args = parser.parse_args()
    ctx = ps.load_context()
    print("## Benzeduras (lidas do ImbueCatalog.cs)\n")
    for imbue_id, tier, mod in load_imbues():
        print(f"- {imbue_id} (nível {tier}): {mod.stat} +{mod.value:g}{'%' if mod.percent else ''} — mesma vaga da bênção da estrada")
    print(f"\nTeto de raridade por nível de ofício: {', '.join(bm.RARITY_NAMES[c] for c in load_caps())}\n")
    res = run(ctx, args.runs)
    print(report(res))
    print("\n## Orçamento do crafting avançado\n")
    found = problems(res)
    print("\n".join(f"- {p}" for p in found) if found else "- dentro do orçamento")


if __name__ == "__main__":
    main()
