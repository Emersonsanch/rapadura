using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What the mouth of the Grande Forja answers to [E] (<see cref="GreatForgeRules.Check"/>).</summary>
    public enum GreatForgeCheck
    {
        /// <summary>The next stage can be lit now.</summary>
        Ready,

        /// <summary>The next stage waits for a boss to fall.</summary>
        NeedBoss,

        /// <summary>The next stage waits for a city quest.</summary>
        NeedQuest,

        /// <summary>The next stage waits for the cousins to bring what it burns.</summary>
        NeedItems,

        /// <summary>All five stages are lit.</summary>
        AllLit,
    }

    /// <summary>One stage of the Grande Forja: what it waits for, what it burns, what it pays at once, and what it adds to the city for good.</summary>
    public sealed class GreatForgeStage
    {
        public readonly int Number;
        public readonly string Id;

        /// <summary>The boss that must have fallen at least once (null: none).</summary>
        public readonly string BossId;

        /// <summary>The city quest that must be done (null: none).</summary>
        public readonly string QuestId;

        public readonly (string ItemId, int Count)[] Delivery;
        public readonly (string ItemId, int Count)[] Reward;

        public GreatForgeStage(int number, string id, string bossId, string questId, (string, int)[] delivery, (string, int)[] reward)
        {
            Number = number;
            Id = id;
            BossId = bossId;
            QuestId = questId;
            Delivery = delivery;
            Reward = reward;
        }
    }

    /// <summary>
    /// The Grande Forja of the Salões de Bronze (Docs/Mapas100.md §3.3 and 17: "acende em 5 etapas pela comunidade (offline: pelas
    /// missões; online: pelo servidor inteiro entregando Faíscas da Forja). Cada etapa acesa dá à cidade um serviço novo"), pure and
    /// server-runnable; the scene side is the forge mouth of <c>CityServicesMechanicsDriver</c> (Napirasu at the forge). Offline, the
    /// cousins light it themselves: each stage waits for its boss or Untash/Shilhak's quest, burns what the halls lack, pays at once, and
    /// gives the city a service for good. Saved as the number of stages lit (<see cref="StageKey"/>).
    /// <code>
    /// 1 O Fole       · O Fole Apagado beaten (17)      · —                                  · 2 Pedra da Bênção, 80 gold   · refine +2 %
    /// 2 A Brasa      · Untash's "Escória na Escada"     · 10 iron ore, 10 wood               · 4 iron, 100 gold             · 1 Pedra da Bênção a day at the forge
    /// 3 A Bigorna    · Shilhak's "Morcegos no Veio"     · 6 iron, 10 stone                   · 1 Pedra da Alma, 120 gold    · Puzur engraves 2 runes a day
    /// 4 As Runas     · —                                · the 4 free bronze runes, 1 each    · 2 Pedra da Vida, 140 gold    · refine +4 % (in all)
    /// 5 O Cristal    · Hahya, the Giant Warrior (boss) · 3 Pedra da Bênção, 5 iron          · 2 Amuleto da Sorte, 200 gold · refine +5 %, 2 Pedras da Bênção a day
    /// </code>
    /// The refine bonus counts only on rolls that can fail and never passes the 95 % cap (<see cref="Refining.RefineRules.ChanceFor(int, bool, bool, float)"/>).
    /// </summary>
    public static class GreatForgeRules
    {
        public const int StageCount = 5;

        /// <summary>Metres from the forge mouth where [E] answers.</summary>
        public const float Reach = 2.6f;

        /// <summary>The number of stages lit (0–5), kept in the world map's mark store.</summary>
        public const string StageKey = "city.saloes_de_bronze.great_forge";

        /// <summary>The daily mark of the forge's gift (the UTC day it was last given).</summary>
        public const string GiftDailyKey = "city.saloes_de_bronze.forge_gift";

        public const string GoldItemId = "item_gold";
        public const string BlessingStone = "item_pedra_bencao";

        public static readonly IReadOnlyList<GreatForgeStage> Stages = new[]
        {
            new GreatForgeStage(1, "fole", RegionBossRules.DarkBellowsId, null,
                Array.Empty<(string, int)>(),
                new[] { (BlessingStone, 2), (GoldItemId, 80) }),
            new GreatForgeStage(2, "brasa", null, CityQuestCatalog.SaloesGolems,
                new[] { ("item_iron_ore", 10), ("item_wood", 10) },
                new[] { ("item_iron", 4), (GoldItemId, 100) }),
            new GreatForgeStage(3, "bigorna", null, CityQuestCatalog.SaloesBats,
                new[] { ("item_iron", 6), ("item_stone", 10) },
                new[] { ("item_pedra_alma", 1), (GoldItemId, 120) }),
            new GreatForgeStage(4, "runas", null, null,
                new[] { ("item_runa_clareza", 1), ("item_runa_couro", 1), ("item_runa_cura", 1), ("item_runa_vento", 1) },
                new[] { ("item_pedra_vida", 2), (GoldItemId, 140) }),
            new GreatForgeStage(5, "cristal", MountDropRules.MountainWarlord, null,
                new[] { (BlessingStone, 3), ("item_iron", 5) },
                new[] { ("item_amuleto_sorte", 2), (GoldItemId, 200) }),
        };

        public static int Clamp(int stagesLit) => Math.Max(0, Math.Min(StageCount, stagesLit));

        /// <summary>The stage to light next, or null when all five are lit.</summary>
        public static GreatForgeStage Next(int stagesLit) => Clamp(stagesLit) < StageCount ? Stages[Clamp(stagesLit)] : null;

        /// <summary>
        /// Whether the next stage can be lit: <paramref name="bossBeaten"/> and <paramref name="questDone"/> answer for an id,
        /// <paramref name="countInBag"/> for an item id (any may be null: then nothing counts).
        /// </summary>
        public static GreatForgeCheck Check(int stagesLit, Func<string, bool> bossBeaten, Func<string, bool> questDone, Func<string, int> countInBag)
        {
            GreatForgeStage stage = Next(stagesLit);
            if (stage == null)
            {
                return GreatForgeCheck.AllLit;
            }

            if (stage.BossId != null && (bossBeaten == null || !bossBeaten(stage.BossId)))
            {
                return GreatForgeCheck.NeedBoss;
            }

            if (stage.QuestId != null && (questDone == null || !questDone(stage.QuestId)))
            {
                return GreatForgeCheck.NeedQuest;
            }

            foreach ((string itemId, int count) in stage.Delivery)
            {
                if (countInBag == null || countInBag(itemId) < count)
                {
                    return GreatForgeCheck.NeedItems;
                }
            }

            return GreatForgeCheck.Ready;
        }

        /// <summary>Extra refine chance the lit forge gives every refine that can fail: +2 % from the Fole, +4 % from the Runas, +5 % with the Cristal.</summary>
        public static float RefineBonus(int stagesLit)
        {
            int lit = Clamp(stagesLit);
            if (lit >= 5)
            {
                return 0.05f;
            }

            if (lit >= 4)
            {
                return 0.04f;
            }

            return lit >= 1 ? 0.02f : 0f;
        }

        /// <summary>Pedras da Bênção the forge gives once a UTC day: 1 from the Brasa, 2 with the Cristal, none before.</summary>
        public static int DailyBlessings(int stagesLit)
        {
            int lit = Clamp(stagesLit);
            return lit >= 5 ? 2 : lit >= 2 ? 1 : 0;
        }

        /// <summary>Runes Puzur engraves free each day (<see cref="CityServiceRules.RuneOf"/>): 2 once the Bigorna is lit.</summary>
        public static int RunesPerDay(int stagesLit) => Clamp(stagesLit) >= 3 ? 2 : 1;

        /// <summary>The forge's gift: only once a stage that gives one is lit, once per UTC day.</summary>
        public static bool GiftReady(int stagesLit, long givenDay, long today) => DailyBlessings(stagesLit) > 0 && givenDay != today;

        /// <summary>The stages lit in a save (0 without one), as kept in the world map's mark store.</summary>
        public static int StagesLit(WorldMapProgress progress) => progress != null ? Clamp(progress.GetDailyMark(StageKey)) : 0;
    }
}
