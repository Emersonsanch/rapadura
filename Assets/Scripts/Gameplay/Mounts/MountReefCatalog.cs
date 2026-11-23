using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// The aquatic mount of Kirta (Docs/Mapas100.md ficha 28, "Apito de Concha para montarias aquáticas"; Docs/Mounts.md §3.15): the
    /// <b>Hipocampo de Kirta</b>, the horse-fronted, fish-tailed sea horse of the povo-do-mar (the hippocamps of the Phoenician coins of the
    /// Levant coast, Ugarit's neighbours). Uncommon, so it is a whistle sold in the region's city (§5.1): the <b>Apito de Concha</b>, a reef
    /// conch carved to whistle, on Danel's diving-tools counter (<see cref="Rapadura.Gameplay.World.Cities.CityShopCatalog.MergulhoDanel"/>,
    /// fixed line, never rotates). Exactly on the Uncommon budget (12, §2.4), <see cref="MountType.Aquatico"/> like the frog and the turtle
    /// (it opens the boat, tide and bog gates). Merged into <see cref="MountCatalog.Specs"/> after <see cref="MountLegendCatalog"/>.
    /// </summary>
    public static class MountReefCatalog
    {
        public const string KirtaHippocamp = "mount_kirta_hippocamp";

        /// <summary>The Apito de Concha: the whistle of the Hipocampo (the item id keeps the whistle pattern, <see cref="MountWaveCatalog.WhistleOf"/>).</summary>
        public static readonly string ConchWhistle = MountWaveCatalog.WhistleOf(KirtaHippocamp);

        /// <summary>The Uncommon one (§5.1): sold for gold at Danel's diving tools, in Kirta (Region 5, map 28).</summary>
        public static readonly IReadOnlyList<string> ShopWhistles = new[] { ConchWhistle };

        public static readonly IReadOnlyList<MountSpec> Specs = new List<MountSpec>
        {
            // ---------------- Incomum (loja da cidade da região) ----------------
            // Região 5, Costa das Monções (Cidade-Recife de Kirta, 28). Budget 12: 1.45 (3) + 3% (3) + 150 health (2) + 140 stamina (4).
            // A sea horse with a horse's chest and a fish's tail: a little slow on land, sturdy, long-breathed.
            new MountSpec
            {
                MountId = KirtaHippocamp, Type = MountType.Aquatico, Rarity = ItemRarity.Uncommon, Region = BiomeType.Forest,
                Speed = 1.45f, Defense = 0.03f, Health = 150f, Stamina = 140f, Diet = MountDiet.Carnivora,
                WhistleItemId = ConchWhistle, ShopPrice = 145, Recipe = MountVisualRecipe.KirtaHippocamp, SeatHeight = 0.95f,
                AbilityId = MountReefRules.ReefSongId, AbilityKind = MountAbilityKind.ReefSong, Cooldown = 40f, Duration = 6f, Radius = 6f,
                Magnitude = 0.015f,
            },
        };
    }

    /// <summary>
    /// Numbers and lines of <see cref="MountReefCatalog"/>, pure so the server can run them too. Nothing deals damage (§2.2).
    /// <code>
    /// Canto do Recife   the conch sings: a coral light opens 10 m for 6 s, and whoever of the group stands within 6 m
    ///                   heals 1.5% of max life per second (9% in all). Cooldown 40 s.
    /// Arrival line      the first time the Hipocampo comes home, the cousin being played says their own line (Vó Edna / Vô Jorge).
    /// </code>
    /// Decision (2026-11-23): the Uncommon heal is three quarters of the Unicorn's aura (2%/s, Rare) and lights the reef's coral colour
    /// round it, the same coral that heals 2%/s out of combat on the map: the Hipocampo carries a little of the relit reef with it.
    /// </summary>
    public static class MountReefRules
    {
        public const string ReefSongId = "canto_do_recife";

        /// <summary>Canto do Recife: radius of the coral light (m).</summary>
        public const float SongLampRadius = 10f;

        /// <summary>The coral colour of the relit reef (Docs/Cities.md Kirta palette, coral #E8836B).</summary>
        public static readonly Color SongLampColor = new Color(0.91f, 0.51f, 0.42f);

        /// <summary>Canto do Recife: the share of max life healed in all (fraction per second times seconds).</summary>
        public static float TotalHeal(MountSpec spec) => spec != null ? spec.Magnitude * spec.Duration : 0f;

        /// <summary>The localization key of <paramref name="cousin"/>'s line when the Hipocampo first comes home.</summary>
        public static string ArrivalLineKey(CharacterId cousin) => "mount." + MountReefCatalog.KirtaHippocamp + ".arrival." + cousin.ToString().ToLowerInvariant();

        /// <summary>
        /// The line said when <paramref name="mountId"/> joins the collection with <paramref name="rider"/> playing: the cousin's own arrival
        /// line for the Hipocampo, null for every other mount or when nobody was chosen yet.
        /// </summary>
        public static string ArrivalLineFor(string mountId, CharacterId? rider) =>
            mountId == MountReefCatalog.KirtaHippocamp && rider.HasValue ? ArrivalLineKey(rider.Value) : null;
    }
}
