using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>Where a relic is found: a hidden spot on the world map, or a breakable secret in an encounter arena.</summary>
    public enum RelicSource
    {
        MapSecret = 0,
        ArenaSecret = 1,
    }

    /// <summary>
    /// One of the "Relíquias do Sertão": a unique keepsake of the family (Vô Chico, Vó Zefa and the five cousins)
    /// hidden around the island. Cosmetic/lore first, plus one small permanent passive applied to whichever hero
    /// is playing (<see cref="RelicPassiveApplier"/>) through the same <see cref="BuffController"/> modifiers the
    /// equipment uses. Texts live in <see cref="SecretTexts"/> under <c>relic.&lt;id&gt;.*</c>.
    /// </summary>
    public sealed class RelicDefinition
    {
        public readonly string Id;
        public readonly RelicSource Source;
        public readonly BiomeType Region;

        /// <summary>Hero the keepsake belongs to; null for the grandparents' / the whole family's.</summary>
        public readonly CharacterId? Hero;

        public readonly StatType Stat;
        public readonly ModifierApplication Application;
        public readonly float Value;

        public RelicDefinition(string id, RelicSource source, BiomeType region, CharacterId? hero, StatType stat, ModifierApplication application, float value)
        {
            Id = id;
            Source = source;
            Region = region;
            Hero = hero;
            Stat = stat;
            Application = application;
            Value = value;
        }

        public string NameKey => "relic." + Id + ".name";
        public string LoreKey => "relic." + Id + ".lore";
        public string PassiveKey => "relic." + Id + ".passive";

        /// <summary>Where-to-look hint shown for a relic still missing from the collection.</summary>
        public string ClueKey => "relic." + Id + ".clue";

        /// <summary>BuffController source key of this relic's passive (one per relic, so re-applying refreshes instead of stacking).</summary>
        public string SourceKey => RelicCatalog.PassiveSourcePrefix + Id;

        /// <summary>
        /// The passive's size for <paramref name="playing"/>: a hero's own keepsake works twice as well in their
        /// hands (<see cref="RelicCatalog.OwnerMultiplier"/>) — Maithe with her first bow, Ícaro with his first gear.
        /// </summary>
        public float ValueFor(CharacterId? playing)
        {
            return Hero.HasValue && playing.HasValue && Hero.Value == playing.Value ? Value * RelicCatalog.OwnerMultiplier : Value;
        }

        /// <summary>The passive as a permanent modifier (duration 0 = never expires), sized for <paramref name="playing"/>.</summary>
        public StatModifierDefinition CreateModifier(CharacterId? playing = null)
        {
            return new StatModifierDefinition
            {
                displayName = Id,
                affectedStat = Stat,
                application = Application,
                value = ValueFor(playing),
                durationSeconds = 0f,
                isDebuff = false,
            };
        }
    }

    /// <summary>
    /// The 12 "Relíquias do Sertão". Story glue: Vô Chico (the late grandfather, a tropeiro who later ran the
    /// casa de farinha) hid keepsakes of the family around the island for the grandchildren to find one day.
    /// Eight sit behind world-map secrets (<see cref="WorldSecretCatalog"/>, two per region), four inside
    /// breakable arena secrets (one per biome, <see cref="ArenaRelicFor"/>). Ids are persisted: never rename.
    /// Passive sizes stay below a single affix roll (see Resources/Affixes): a full collection is a nice edge,
    /// not a build.
    /// </summary>
    public static class RelicCatalog
    {
        public const string PassiveSourcePrefix = "relic/";

        /// <summary>A hero's own relic applies its passive this many times over while that hero plays.</summary>
        public const float OwnerMultiplier = 2f;

        public const string GaitaVoChico = "relic_gaita_vo_chico";
        public const string ArcoMaithe = "relic_arco_maithe";
        public const string CartasVoChico = "relic_cartas_vo_chico";
        public const string EscudinhoJoaquim = "relic_escudinho_joaquim";
        public const string MedalhaMaria = "relic_medalha_maria";
        public const string ChocalhoTeimosa = "relic_chocalho_teimosa";
        public const string EngrenagemIcaro = "relic_engrenagem_icaro";
        public const string FitaLavine = "relic_fita_lavine";
        public const string CaniveteVoChico = "relic_canivete_vo_chico";
        public const string ChapeuCouro = "relic_chapeu_couro";
        public const string PiaoIrmaos = "relic_piao_irmaos";
        public const string RetratoPrimos = "relic_retrato_primos";

        private static readonly RelicDefinition[] AllRelics =
        {
            // Map secrets (WorldSecretCatalog).
            new RelicDefinition(GaitaVoChico, RelicSource.MapSecret, BiomeType.Forest, null, StatType.StaminaRegen, ModifierApplication.PercentAdditive, 0.08f),
            new RelicDefinition(ArcoMaithe, RelicSource.MapSecret, BiomeType.Forest, CharacterId.Maithe, StatType.CriticalChance, ModifierApplication.Flat, 0.01f),
            new RelicDefinition(CartasVoChico, RelicSource.MapSecret, BiomeType.Desert, null, StatType.MaxHealth, ModifierApplication.Flat, 10f),
            new RelicDefinition(EscudinhoJoaquim, RelicSource.MapSecret, BiomeType.Desert, CharacterId.Joaquim, StatType.Defense, ModifierApplication.Flat, 2f),
            new RelicDefinition(MedalhaMaria, RelicSource.MapSecret, BiomeType.Mountain, CharacterId.Maria, StatType.HealthRegen, ModifierApplication.Flat, 0.4f),
            new RelicDefinition(ChocalhoTeimosa, RelicSource.MapSecret, BiomeType.Mountain, null, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0.03f),
            // CooldownReduction / ResourceCostReduction are 0-based fractions (GetAdditiveFractionTotal, Balance §7.6): authored Flat.
            new RelicDefinition(EngrenagemIcaro, RelicSource.MapSecret, BiomeType.Caves, CharacterId.Icaro, StatType.CooldownReduction, ModifierApplication.Flat, 0.03f),
            new RelicDefinition(FitaLavine, RelicSource.MapSecret, BiomeType.Caves, CharacterId.Lavine, StatType.MaxMana, ModifierApplication.Flat, 8f),

            // Arena secrets (ArenaSecretSpawner), one per biome.
            // +1, not +2 (Balance §14): on the 10-damage basic attack +2 was +20%, the biggest single piece of the power stack.
            new RelicDefinition(CaniveteVoChico, RelicSource.ArenaSecret, BiomeType.Forest, null, StatType.AttackDamage, ModifierApplication.Flat, 1f),
            new RelicDefinition(ChapeuCouro, RelicSource.ArenaSecret, BiomeType.Desert, null, StatType.MaxStamina, ModifierApplication.Flat, 8f),
            new RelicDefinition(PiaoIrmaos, RelicSource.ArenaSecret, BiomeType.Mountain, null, StatType.AttackSpeed, ModifierApplication.Flat, 0.03f),
            new RelicDefinition(RetratoPrimos, RelicSource.ArenaSecret, BiomeType.Caves, null, StatType.ResourceCostReduction, ModifierApplication.Flat, 0.03f),
        };

        private static Dictionary<string, RelicDefinition> _byId;

        /// <summary>Every relic, in display order (map secrets by region, then the arena ones).</summary>
        public static IReadOnlyList<RelicDefinition> All => AllRelics;

        public static int Count => AllRelics.Length;

        public static RelicDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (_byId == null)
            {
                var table = new Dictionary<string, RelicDefinition>();
                foreach (RelicDefinition relic in AllRelics)
                {
                    table[relic.Id] = relic;
                }

                _byId = table;
            }

            return _byId.TryGetValue(id, out RelicDefinition found) ? found : null;
        }

        /// <summary>The relic hidden in <paramref name="biome"/>'s arena secret.</summary>
        public static RelicDefinition ArenaRelicFor(BiomeType biome)
        {
            foreach (RelicDefinition relic in AllRelics)
            {
                if (relic.Source == RelicSource.ArenaSecret && relic.Region == biome)
                {
                    return relic;
                }
            }

            return null;
        }

        /// <summary>Every relic id (achievement target list, tests).</summary>
        public static string[] AllIds()
        {
            var ids = new string[AllRelics.Length];
            for (int i = 0; i < AllRelics.Length; i++)
            {
                ids[i] = AllRelics[i].Id;
            }

            return ids;
        }
    }
}
