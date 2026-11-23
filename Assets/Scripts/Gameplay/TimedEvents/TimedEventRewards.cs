using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>What one player takes home from one room.</summary>
    public readonly struct TimedEventReward
    {
        public readonly TimedRewardBand Band;

        /// <summary>XP in kill-equivalents of a common enemy of the room's level (the server multiplies by its XP table, like <c>WorldBossRules</c>).</summary>
        public readonly float XpCommons;

        public readonly int Gold;

        /// <summary>Pedras-Faísca and Pedras da Alma Viva (the same stones every other activity pays).</summary>
        public readonly int BlessingStones;
        public readonly int SoulStones;

        /// <summary>Event material (Cinza da Vigília, Óleo da Lâmpada, Fragmento de Ohyah): trades and crafts, never stats on its own.</summary>
        public readonly string MaterialId;
        public readonly int MaterialCount;

        /// <summary>Rare cosmetic (title, aura, skin) or null. Cosmetic only: no stats.</summary>
        public readonly string CosmeticId;

        /// <summary>Bema oferendas (0 outside the Bema).</summary>
        public readonly int Offerings;

        public TimedEventReward(TimedRewardBand band, float xpCommons, int gold, int blessingStones, int soulStones,
            string materialId, int materialCount, string cosmeticId, int offerings)
        {
            Band = band;
            XpCommons = xpCommons;
            Gold = gold;
            BlessingStones = blessingStones;
            SoulStones = soulStones;
            MaterialId = materialId;
            MaterialCount = materialCount;
            CosmeticId = cosmeticId;
            Offerings = offerings;
        }

        public bool IsEmpty => Band == TimedRewardBand.None;
    }

    /// <summary>
    /// Rewards by band (Docs/Network.md §14). No pay-to-win: everything here is earned by playing (tickets drop and cost
    /// in-game gold, never real money), the items are the stones and materials every other activity already pays, and the
    /// rare prizes are cosmetics without stats. A lost room pays half the XP and gold, no stones, no cosmetic.
    /// Rolls are seeded per player (<see cref="TimedEventAgenda.Seed"/> with the player id), so the server can redo them.
    /// </summary>
    public static class TimedEventRewards
    {
        public const float BaseXpCommons = 40f;
        public const int BaseGoldPerRoom = 60;
        public const float DefeatMultiplier = 0.5f;

        public const string VigiliaMaterial = "mat_cinza_da_vigilia";
        public const string CercoMaterial = "mat_oleo_da_lampada";
        public const string CacadaMaterial = "mat_fragmento_de_ohyah";

        /// <summary>The Caçada's extra item (one per winner, <see cref="TimedRaidShare.WonItem"/>): a chest of the room's tier.</summary>
        public const string OhyahChest = "item_bau_de_ohyah";

        /// <summary>"Marca de Ohyah": the top damage / healing / tank mark (a trophy for the house, no stats).</summary>
        public const string OhyahMark = "item_marca_de_ohyah";

        /// <summary>Cosmetic tables (titles, auras, skins: no stats).</summary>
        public static readonly IReadOnlyDictionary<TimedEventKind, string[]> Cosmetics = new Dictionary<TimedEventKind, string[]>
        {
            { TimedEventKind.VigiliaDaColuna, new[] { "cosm_titulo_guardiao_da_vigilia", "cosm_aura_coluna_da_gloria", "cosm_lanterna_da_vigilia" } },
            { TimedEventKind.CercoDaTreva, new[] { "cosm_titulo_defensor_da_lampada", "cosm_manto_do_portao", "cosm_brasao_lampada_acesa" } },
            { TimedEventKind.CacadaAOhyah, new[] { "cosm_titulo_cacador_de_gigantes", "cosm_elmo_de_ohyah", "cosm_sombra_do_gigante" } },
        };

        public static float BandMultiplier(TimedRewardBand band)
        {
            switch (band)
            {
                case TimedRewardBand.Gold: return 1f;
                case TimedRewardBand.Silver: return 0.75f;
                case TimedRewardBand.Bronze: return 0.5f;
                case TimedRewardBand.Participation: return 0.25f;
                default: return 0f;
            }
        }

        public static int Stones(TimedRewardBand band)
        {
            switch (band)
            {
                case TimedRewardBand.Gold: return 3;
                case TimedRewardBand.Silver: return 2;
                case TimedRewardBand.Bronze: return 1;
                default: return 0;
            }
        }

        public static float CosmeticChance(TimedRewardBand band)
        {
            switch (band)
            {
                case TimedRewardBand.Gold: return 0.03f;
                case TimedRewardBand.Silver: return 0.01f;
                case TimedRewardBand.Bronze: return 0.003f;
                default: return 0f;
            }
        }

        public static string MaterialOf(TimedEventKind kind)
        {
            switch (kind)
            {
                case TimedEventKind.VigiliaDaColuna: return VigiliaMaterial;
                case TimedEventKind.CercoDaTreva: return CercoMaterial;
                case TimedEventKind.CacadaAOhyah: return CacadaMaterial;
                default: return null;
            }
        }

        /// <summary>The reward of one band in one room. <paramref name="seed"/> = the player's seed of that occurrence.</summary>
        public static TimedEventReward For(TimedEventKind kind, TimedRewardBand band, int roomIndex, TimedEventOutcome outcome,
            int seed, TimedBemaModifiers bema)
        {
            if (band == TimedRewardBand.None || kind == TimedEventKind.None)
            {
                return default;
            }

            int room = Math.Max(1, roomIndex);
            bool won = outcome == TimedEventOutcome.Victory;
            float scale = BandMultiplier(band) * (won ? 1f : DefeatMultiplier);
            float xp = BaseXpCommons * scale * bema.XpMultiplier;
            int gold = (int)Math.Round(BaseGoldPerRoom * room * scale);
            int stones = won ? Stones(band) : 0;
            int soul = won && band == TimedRewardBand.Gold && room >= 3 ? 1 : 0;
            int material = band == TimedRewardBand.Participation ? 1 : 1 + (int)band - (int)TimedRewardBand.Participation;
            if (!won)
            {
                material = Math.Max(1, material / 2);
            }

            string cosmetic = null;
            if (won)
            {
                double chance = CosmeticChance(band) * bema.CosmeticChanceMultiplier;
                if (TimedEventAgenda.Roll(seed, (int)kind, room, 501) < chance)
                {
                    var table = new List<string>(Cosmetics.TryGetValue(kind, out string[] list) ? list : Array.Empty<string>());
                    if (bema.Active)
                    {
                        table.Add(TimedBemaRules.BemaCosmeticId);
                    }

                    if (table.Count > 0)
                    {
                        int pick = (int)(TimedEventAgenda.Roll(seed, (int)kind, room, 502) * table.Count) % table.Count;
                        cosmetic = table[pick];
                    }
                }
            }

            return new TimedEventReward(band, xp, gold, stones, soul, MaterialOf(kind), material, cosmetic,
                TimedBemaRules.Offerings(band, bema.Active));
        }

        /// <summary>True for any id this module can hand out as a "rare" prize: all cosmetics (no stats), by contract.</summary>
        public static bool IsCosmetic(string id) => id != null && id.StartsWith("cosm_", StringComparison.Ordinal);
    }
}
