using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The freed barque and Apep's fight (map 76, Docs/Mapas100.md: "defender a barca" and "Apep, a Serpente que Bebe o Rio"), pure. The barque the
    /// rowers pushed free sails the sandbank for the rest of the game day (<see cref="RioBarqueDefenseRules"/>, daily mark
    /// <see cref="FreedMarkKey"/>); while it sails, the river it floats on is no longer hers to drink:
    /// <code>
    /// barque freed today: "beber o rio" hits at 0.75× (1.1 → 0.825) and heals Apep nothing (else 2% of her health)
    /// </code>
    /// Decision (2026-11-16): the defence is the preparation for the world boss, not a second reward; it changes only her signature, the rest
    /// of the fight (phases, midnight) stays. Online the barque is the server's, so one group's work helps everyone that night.
    /// </summary>
    public static class ApepBarqueLinkRules
    {
        public const string FreedMarkKey = "rio.barque.freed";
        public const float FreedDrinkFactor = 0.75f;

        public static float DrinkDamageFactor(bool barqueFreed) => RegionBossRules.DrinkDamageFactor * (barqueFreed ? FreedDrinkFactor : 1f);

        public static float DrinkHealFraction(bool barqueFreed) => barqueFreed ? 0f : RegionBossRules.DrinkHealFraction;
    }

    /// <summary>
    /// Hahya and Ohya fighting beside the cousins against the Dream of Destruction (map 82, Docs/Mapas100.md: "o Hahya e o Ohya (já libertados)
    /// ajudam na luta"), pure. A giant is freed once the hero has beaten him (Hahya, <see cref="HahyaId"/>; Ohya, <see cref="OhyaId"/>):
    /// <code>
    /// each freed giant strikes the Dream every 15 s of fight (Hahya at 15, 30…; Ohya 7.5 s later), 2% of its health a blow
    /// the fight clock runs only while the hero is within the boss's ability range
    /// </code>
    /// Decision (2026-11-16): the giants are a reward for the old fights, ~8% a minute with both, never the whole fight; they do not tank or
    /// draw the Dream. The giants' shouts come from the sky over the ring (no model: they are the ones who dreamed it).
    /// </summary>
    public static class DreamGiantsRules
    {
        public const string HahyaId = "enemy_mountain_warlord";
        public const string OhyaId = RegionBossRules.OhyaId;
        public const float StrikeIntervalSeconds = 15f;
        public const float OhyaOffsetSeconds = 7.5f;
        public const float StrikeFraction = 0.02f;

        public static int Allies(bool hahyaFreed, bool ohyaFreed) => (hahyaFreed ? 1 : 0) + (ohyaFreed ? 1 : 0);

        /// <summary>How many blows a giant whose turn is shifted by <paramref name="offset"/> has struck after <paramref name="fightSeconds"/> of fight.</summary>
        public static int BlowsBy(float fightSeconds, float offset) => Mathf.Max(0, Mathf.FloorToInt((fightSeconds - offset) / StrikeIntervalSeconds));

        /// <summary>Does that giant strike between <paramref name="before"/> and <paramref name="after"/> s of fight?</summary>
        public static bool StrikesBetween(float before, float after, float offset) => BlowsBy(after, offset) > BlowsBy(before, offset);

        /// <summary>How much a blow takes off a Dream of <paramref name="maxHealth"/>.</summary>
        public static float BlowDamage(float maxHealth) => Mathf.Max(0f, maxHealth) * StrikeFraction;
    }

    /// <summary>
    /// The last Arca Resgatada +5 (map 96, Docs/Mapas100.md: "Vencida, não morre: devolve a arca e fica com vergonha (a última Arca Resgatada +5)"),
    /// pure. When Āz, a Capitã Cobiçosa, is beaten with the hero by the ring she gives back the chest of the invasions at its highest tier:
    /// <code>
    /// once per real day (UTC) · the same chest the Invasão de Āz pays at +5 (XP, gold, 2 Pedras da Bênção, 1 Pedra da Alma), Region 16's tier
    /// no house item and no horse: those stay with the invasion's bando
    /// </code>
    /// Decision (2026-11-16): she comes back in 5 min, so the chest is daily, not per win; the day claimed is a time mark written when it is paid.
    /// </summary>
    public static class AzRescuedChestRules
    {
        public const int Tier = InvasionRules.MaxTier;
        public const string ClaimKey = "ceu.az.rescued_chest_day";
        public const float WitnessRange = 60f;
        public const BiomeType Region = BiomeType.Mountain;

        public static bool CanClaim(long lastClaimDay, long today) => today > lastClaimDay;

        /// <summary>The seed of the chest of UTC day <paramref name="utcDay"/> (the same for every player that day, like the invasion's).</summary>
        public static int Seed(long utcDay) => unchecked((int)(utcDay * 2654435761L) ^ 0x41A2);

        public static CangacoChest Chest(long utcDay) => InvasionRules.ChestFor(Tier, Region, Seed(utcDay), true, _ => false);
    }
}
