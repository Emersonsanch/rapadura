using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Fishing
{
    /// <summary>How hard a fish is to find (drives the catch table, the hook window and the cousin's reaction).</summary>
    public enum FishRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Legendary = 3,
    }

    /// <summary>The three water spots of "Pesca no remanso", one per region that has water.</summary>
    public enum FishingSpotId
    {
        /// <summary>Vô Jorge's Remanso de Faro, in the Floresta (next to the village).</summary>
        Acude = 0,
        /// <summary>The cold riacho coming down the Montanha.</summary>
        Riacho = 1,
        /// <summary>The underground lake of the Cavernas.</summary>
        LagoSubterraneo = 2,
    }

    /// <summary>Phases of the day as flags (<see cref="TimeOfDayPhase"/> → <see cref="FishingRules.ToMask"/>).</summary>
    [Flags]
    public enum PhaseMask
    {
        None = 0,
        Dawn = 1,
        Day = 2,
        Dusk = 4,
        Night = 8,
        All = Dawn | Day | Dusk | Night,
    }

    /// <summary>A water spot: its region and how its water behaves.</summary>
    public sealed class FishingSpotSpec
    {
        public readonly FishingSpotId Id;
        public readonly BiomeType Region;
        /// <summary>Under rock: the weather outside does not reach the water (treated as clear).</summary>
        public readonly bool Sheltered;
        /// <summary>Multiplier on the wait before a bite (fast water bites sooner, the dark lake later).</summary>
        public readonly float BiteDelayScale;

        public FishingSpotSpec(FishingSpotId id, BiomeType region, bool sheltered, float biteDelayScale)
        {
            Id = id;
            Region = region;
            Sheltered = sheltered;
            BiteDelayScale = biteDelayScale;
        }

        /// <summary>"fishing.spot.acude" (text keys: <c>.name</c>, <c>.intro</c>).</summary>
        public string KeyBase => "fishing.spot." + Id.ToString().ToLowerInvariant();
        public string NameKey => KeyBase + ".name";
        public string IntroKey => KeyBase + ".intro";
    }

    /// <summary>
    /// One species, as plain data. Its id is the id of its inventory item (<c>Resources/Items/item_fish_*.asset</c>),
    /// so the collection, the records, the achievement targets and the cooking ingredients all share one key.
    /// </summary>
    public sealed class FishSpec
    {
        public readonly string Id;
        public readonly FishRarity Rarity;
        public readonly IReadOnlyList<FishingSpotId> Spots;
        /// <summary>Phases when it bites at all.</summary>
        public readonly PhaseMask AllowedPhases;
        /// <summary>Phases when it bites more (<see cref="FishingRules.PreferredPhaseMultiplier"/>).</summary>
        public readonly PhaseMask PreferredPhases;
        /// <summary>Weather that makes it bite more (<see cref="FishingRules.PreferredWeatherMultiplier"/>).</summary>
        public readonly IReadOnlyList<WeatherType> PreferredWeather;
        public readonly float MinKg;
        public readonly float MaxKg;
        /// <summary>0..1: how hard it pulls (surge strength, line taken back).</summary>
        public readonly float Strength;
        /// <summary>Seconds of uninterrupted reeling it takes to land it (more = longer fight).</summary>
        public readonly float Stamina;

        public FishSpec(string id, FishRarity rarity, FishingSpotId[] spots, PhaseMask allowedPhases, PhaseMask preferredPhases,
            WeatherType[] preferredWeather, float minKg, float maxKg, float strength, float stamina)
        {
            Id = id;
            Rarity = rarity;
            Spots = spots ?? Array.Empty<FishingSpotId>();
            AllowedPhases = allowedPhases;
            PreferredPhases = preferredPhases;
            PreferredWeather = preferredWeather ?? Array.Empty<WeatherType>();
            MinKg = minKg;
            MaxKg = maxKg;
            Strength = strength;
            Stamina = stamina;
        }

        public bool LivesIn(FishingSpotId spot)
        {
            foreach (FishingSpotId candidate in Spots)
            {
                if (candidate == spot)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The matching <see cref="ItemRarity"/> (inventory colour coding).</summary>
        public ItemRarity ItemRarity => Rarity switch
        {
            FishRarity.Uncommon => ItemRarity.Uncommon,
            FishRarity.Rare => ItemRarity.Rare,
            FishRarity.Legendary => ItemRarity.Legendary,
            _ => ItemRarity.Common,
        };
    }

    /// <summary>
    /// "Pesca no remanso": the 14 species and the 3 spots, defined in code like <c>MealCatalog</c>. Each species is an
    /// inventory item (<c>Resources/Items/item_fish_*.asset</c>, weight = a typical fish of its kind) and some are
    /// cooking ingredients (<c>MealCatalog.TilapiaNaBrasa</c>, <c>MealCatalog.CaldoDePiaba</c>).
    /// <list type="bullet">
    /// <item>Açude (Floresta): tilápia, piaba, traíra (dusk/night), pacu (day), curimatã (rain), tucunaré (clear
    /// daylight) and the Peixe-Dourado do Açude (dawn/dusk only, more in the rain).</item>
    /// <item>Riacho (Montanha): piaba, lambari, piau, mandi (night), cascudo and the surubim (night, rain).</item>
    /// <item>Lago subterrâneo (Cavernas, sheltered from the weather): piaba, cascudo, mandi, the bagre-cego (the
    /// cave blind fish), the piaba-lanterna (glows at night) and the surubim.</item>
    /// </list>
    /// </summary>
    public static class FishCatalog
    {
        public const string Tilapia = "item_fish_tilapia";
        public const string Piaba = "item_fish_piaba";
        public const string Traira = "item_fish_traira";
        public const string Pacu = "item_fish_pacu";
        public const string Curimata = "item_fish_curimata";
        public const string Tucunare = "item_fish_tucunare";
        public const string PeixeDourado = "item_fish_peixe_dourado";
        public const string Lambari = "item_fish_lambari";
        public const string Piau = "item_fish_piau";
        public const string Mandi = "item_fish_mandi";
        public const string Cascudo = "item_fish_cascudo";
        public const string Surubim = "item_fish_surubim";
        public const string BagreCego = "item_fish_bagre_cego";
        public const string PiabaLanterna = "item_fish_piaba_lanterna";

        private static readonly WeatherType[] NoWeather = Array.Empty<WeatherType>();
        private static readonly WeatherType[] Rain = { WeatherType.Rain };
        private static readonly WeatherType[] Clear = { WeatherType.Clear };
        private static readonly WeatherType[] Fog = { WeatherType.Fog };

        private static readonly FishingSpotId[] AcudeOnly = { FishingSpotId.Acude };
        private static readonly FishingSpotId[] RiachoOnly = { FishingSpotId.Riacho };
        private static readonly FishingSpotId[] LagoOnly = { FishingSpotId.LagoSubterraneo };

        public static readonly IReadOnlyList<FishingSpotSpec> Spots = new[]
        {
            new FishingSpotSpec(FishingSpotId.Acude, BiomeType.Forest, sheltered: false, biteDelayScale: 1f),
            new FishingSpotSpec(FishingSpotId.Riacho, BiomeType.Mountain, sheltered: false, biteDelayScale: 0.85f),
            new FishingSpotSpec(FishingSpotId.LagoSubterraneo, BiomeType.Caves, sheltered: true, biteDelayScale: 1.15f),
        };

        public static readonly IReadOnlyList<FishSpec> Fish = new[]
        {
            // -- Açude (Floresta) --
            new FishSpec(Tilapia, FishRarity.Common, AcudeOnly, PhaseMask.All, PhaseMask.Day, NoWeather, 0.3f, 2.2f, 0.3f, 3f),
            new FishSpec(Piaba, FishRarity.Common, new[] { FishingSpotId.Acude, FishingSpotId.Riacho, FishingSpotId.LagoSubterraneo }, PhaseMask.All, PhaseMask.None, NoWeather, 0.02f, 0.12f, 0.1f, 2f),
            new FishSpec(Traira, FishRarity.Uncommon, AcudeOnly, PhaseMask.All, PhaseMask.Dusk | PhaseMask.Night, NoWeather, 0.5f, 3.5f, 0.6f, 4f),
            new FishSpec(Pacu, FishRarity.Uncommon, AcudeOnly, PhaseMask.Dawn | PhaseMask.Day | PhaseMask.Dusk, PhaseMask.Day, NoWeather, 1f, 6f, 0.55f, 4.5f),
            new FishSpec(Curimata, FishRarity.Uncommon, AcudeOnly, PhaseMask.All, PhaseMask.None, Rain, 0.4f, 2.5f, 0.4f, 3.5f),
            new FishSpec(Tucunare, FishRarity.Rare, AcudeOnly, PhaseMask.Dawn | PhaseMask.Day | PhaseMask.Dusk, PhaseMask.Day, Clear, 1f, 7f, 0.8f, 5f),
            new FishSpec(PeixeDourado, FishRarity.Legendary, AcudeOnly, PhaseMask.Dawn | PhaseMask.Dusk, PhaseMask.Dawn, Rain, 3f, 9f, 1f, 6f),

            // -- Riacho (Montanha) --
            new FishSpec(Lambari, FishRarity.Common, RiachoOnly, PhaseMask.All, PhaseMask.Day, NoWeather, 0.03f, 0.2f, 0.15f, 2f),
            new FishSpec(Piau, FishRarity.Uncommon, RiachoOnly, PhaseMask.All, PhaseMask.Dawn, Fog, 0.3f, 2f, 0.45f, 3.5f),
            new FishSpec(Mandi, FishRarity.Uncommon, new[] { FishingSpotId.Riacho, FishingSpotId.LagoSubterraneo }, PhaseMask.All, PhaseMask.Night, NoWeather, 0.1f, 1f, 0.35f, 3f),
            new FishSpec(Cascudo, FishRarity.Common, new[] { FishingSpotId.Riacho, FishingSpotId.LagoSubterraneo }, PhaseMask.All, PhaseMask.Night, NoWeather, 0.1f, 1.2f, 0.25f, 3f),
            new FishSpec(Surubim, FishRarity.Rare, new[] { FishingSpotId.Riacho, FishingSpotId.LagoSubterraneo }, PhaseMask.All, PhaseMask.Night, Rain, 4f, 25f, 0.9f, 6f),

            // -- Lago subterrâneo (Cavernas) --
            new FishSpec(BagreCego, FishRarity.Uncommon, LagoOnly, PhaseMask.All, PhaseMask.None, NoWeather, 0.2f, 1.5f, 0.4f, 3.5f),
            new FishSpec(PiabaLanterna, FishRarity.Rare, LagoOnly, PhaseMask.All, PhaseMask.Night, NoWeather, 0.05f, 0.3f, 0.5f, 3f),
        };

        private static Dictionary<string, FishSpec> _byId;

        public static FishSpec Get(string fishId)
        {
            if (string.IsNullOrEmpty(fishId))
            {
                return null;
            }

            if (_byId == null)
            {
                var table = new Dictionary<string, FishSpec>(StringComparer.Ordinal);
                foreach (FishSpec fish in Fish)
                {
                    table[fish.Id] = fish;
                }

                _byId = table;
            }

            return _byId.TryGetValue(fishId, out FishSpec spec) ? spec : null;
        }

        public static FishingSpotSpec GetSpot(FishingSpotId id)
        {
            foreach (FishingSpotSpec spot in Spots)
            {
                if (spot.Id == id)
                {
                    return spot;
                }
            }

            return null;
        }

        /// <summary>The water spot of <paramref name="region"/>, or null (the desert has no water to fish in).</summary>
        public static FishingSpotSpec SpotFor(BiomeType? region)
        {
            if (!region.HasValue)
            {
                return null;
            }

            foreach (FishingSpotSpec spot in Spots)
            {
                if (spot.Region == region.Value)
                {
                    return spot;
                }
            }

            return null;
        }

        /// <summary>Every species id, in catalog order (achievement targets, the collection page).</summary>
        public static string[] AllIds()
        {
            var ids = new string[Fish.Count];
            for (int i = 0; i < Fish.Count; i++)
            {
                ids[i] = Fish[i].Id;
            }

            return ids;
        }
    }
}
