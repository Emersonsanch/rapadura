using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Tacho;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Item ids (<c>Resources/Items</c>) of the gear the special gates of the maps ask for (Docs/MetodologiaMapasERacas.md §2.1).
    /// Texts in <c>ContentTexts</c> (<c>item.&lt;id&gt;.*</c>), sold in the region's city (<c>CityShopCatalog</c>) or given by
    /// its quests (<c>RegionQuestCatalog</c>).
    /// </summary>
    public static class MapRequirementItems
    {
        // Regiões 1–4 (cities of this build).
        public const string LeatherBoots = "item_leather_boots";
        public const string ClimbingRope = "item_corda_escalada";
        public const string GardenKey = "item_chave_jardim";
        public const string Waterskin = "item_odre_agua";
        public const string Theriac = "item_teriaga";
        public const string WoolCloak = "item_manta_la";
        public const string Pickaxe = "item_pickaxe";
        public const string GingerTea = "item_cha_gengibre";
        public const string OilLamp = "item_lamparina_oleo";
        public const string RunicLamp = "item_lamparina_runica";
        public const string EternalFireOil = "item_oleo_fogo_eterno";

        /// <summary>
        /// Asas de Lona do Takhsich: worn in the "Costas/Asas" slot they give the double jump of the Ninho de Mahaway (24,
        /// <see cref="MapRequirementTags.WingsOrDoubleJump"/>); they glide, they do not fly (not <see cref="MapRequirementTags.WingsRequired"/>).
        /// Sold at the Feira da Miwnay (Mercado dos Túneis).
        /// </summary>
        public const string CanvasWings = "item_asas_lona_takhsich";

        // Regiões 5–8 (their cities are still planned: Docs/Mapas100.md §2–§3).
        public const string DhowPassage = "item_passagem_dhow";
        public const string ArtificerRelic = "item_reliquia_artifices";
        public const string BreathingShell = "item_concha_respiravel";
        public const string AirBubble = "item_bolha_ar";
        public const string RootBoots = "item_botas_raiz";
        public const string BalancePole = "item_vara_equilibrio";
        public const string ReedBoat = "item_barco_junco";
        public const string AntiVenomAmulet = "item_amuleto_antiveneno";
        public const string PurifyingFruit = "item_fruta_purificadora";
        public const string ThermalClothes = "item_roupa_termica";
        public const string ClimbingCrampons = "item_grampos_escalada";
        public const string StormCloak = "item_capa_tempestade";
        public const string SunSeal = "item_selo_sol";

        /// <summary>The items created for the gates (not the reused leather boots / pickaxe).</summary>
        public static readonly string[] Created =
        {
            ClimbingRope, GardenKey, Waterskin, Theriac, WoolCloak, GingerTea, OilLamp, RunicLamp, EternalFireOil, CanvasWings,
            DhowPassage, ArtificerRelic, BreathingShell, AirBubble, RootBoots, BalancePole, ReedBoat, AntiVenomAmulet,
            PurifyingFruit, ThermalClothes, ClimbingCrampons, StormCloak, SunSeal,
        };
    }

    /// <summary>
    /// What the gates look at in the hero (plain data, so the online server can fill it from its own records): the selected
    /// cousin, the item ids in the backpack, what is worn per slot and the kinds of mount the hero owns. Built in the
    /// game by <see cref="MapRequirementHeroState"/>; in tests by hand (<see cref="Carry"/>, <see cref="Wear"/>, <see cref="Own"/>).
    /// </summary>
    public sealed class MapRequirementState
    {
        public CharacterId? Hero;
        public readonly HashSet<string> Carried = new HashSet<string>(StringComparer.Ordinal);
        public readonly Dictionary<EquipmentSlot, string> Equipped = new Dictionary<EquipmentSlot, string>();
        public readonly HashSet<MountType> OwnedMountTypes = new HashSet<MountType>();

        public MapRequirementState(CharacterId? hero = null)
        {
            Hero = hero;
        }

        public MapRequirementState Carry(params string[] itemIds)
        {
            foreach (string id in itemIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    Carried.Add(id);
                }
            }

            return this;
        }

        public MapRequirementState Wear(EquipmentSlot slot, string itemId)
        {
            if (slot != EquipmentSlot.None && !string.IsNullOrEmpty(itemId))
            {
                Equipped[slot] = itemId;
            }

            return this;
        }

        public MapRequirementState Own(MountType type)
        {
            OwnedMountTypes.Add(type);
            return this;
        }

        /// <summary>In the backpack or worn.</summary>
        public bool Has(string itemId) => !string.IsNullOrEmpty(itemId) && (Carried.Contains(itemId) || Equipped.ContainsValue(itemId));

        public string WornIn(EquipmentSlot slot) => Equipped.TryGetValue(slot, out string id) ? id : null;
    }

    /// <summary>
    /// Where every special gate of the maps comes from (Docs/MetodologiaMapasERacas.md §2.1), pure and EditMode-tested
    /// (MapRequirementTests): an item in the backpack or worn, wings in the "Costas/Asas" slot of the Tacho Encantado, a
    /// winged/aquatic mount, or a cousin's own gift (§3: Joaquim does not sink in the mud, Lavine breathes the poison mists,
    /// Maithe walks the rope bridges). Plugged into <see cref="MapAccessRules.CheckWarp"/>/<see cref="MapAccessRules.CheckWalk"/>
    /// through <see cref="Lookup"/>. Unknown tags are never met; the empty tag always is.
    /// </summary>
    public static class MapRequirementRules
    {
        /// <summary>Is <paramref name="tag"/> met by the hero described in <paramref name="state"/>?</summary>
        public static bool IsMet(string tag, MapRequirementState state)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return true;
            }

            if (state == null)
            {
                return false;
            }

            switch (tag)
            {
                case MapRequirementTags.LeatherBoots:
                    return state.Hero == CharacterId.Joaquim || !string.IsNullOrEmpty(state.WornIn(EquipmentSlot.Boots));
                case MapRequirementTags.ElevatedPlatforms:
                    return state.Has(MapRequirementItems.ClimbingRope) || HasWings(state);
                case MapRequirementTags.GardenKey:
                    return state.Has(MapRequirementItems.GardenKey);
                case MapRequirementTags.FreshWater:
                    return state.Has(MapRequirementItems.Waterskin);
                case MapRequirementTags.PoisonProtection:
                    return state.Hero == CharacterId.Lavine || state.Has(MapRequirementItems.Theriac) || state.Has(MapRequirementItems.AntiVenomAmulet);
                case MapRequirementTags.WoolCloak:
                    return state.Has(MapRequirementItems.WoolCloak) || state.Has(MapRequirementItems.ThermalClothes) || WearsFeltCape(state);
                case MapRequirementTags.PickaxeTorch:
                    return state.Has(MapRequirementItems.Pickaxe) && HasLamp(state);
                case MapRequirementTags.BlizzardResistance:
                    return state.Has(MapRequirementItems.GingerTea) || state.Has(MapRequirementItems.ThermalClothes);
                case MapRequirementTags.TorchRequired:
                    return HasLamp(state);
                case MapRequirementTags.RunicLamp:
                    return state.Has(MapRequirementItems.RunicLamp);
                case MapRequirementTags.EternalFireOil:
                    return state.Has(MapRequirementItems.EternalFireOil);
                case MapRequirementTags.WingsOrDoubleJump:
                    // Real wings, the flying mount (the Grifo da Coluna) or the double jump of the canvas wings worn on the back.
                    return HasWings(state) || state.OwnedMountTypes.Contains(MountType.Voador)
                        || state.WornIn(EquipmentSlot.Wings) == MapRequirementItems.CanvasWings;
                case MapRequirementTags.WingsRequired:
                    return HasWings(state) || state.OwnedMountTypes.Contains(MountType.Voador);
                case MapRequirementTags.BoatOrDock:
                    return state.Has(MapRequirementItems.DhowPassage) || state.OwnedMountTypes.Contains(MountType.Aquatico);
                case MapRequirementTags.ArtificerRelic:
                    return state.Has(MapRequirementItems.ArtificerRelic);
                case MapRequirementTags.BreathingShell:
                    return state.Has(MapRequirementItems.BreathingShell);
                case MapRequirementTags.AirBubble:
                    return state.Has(MapRequirementItems.AirBubble) || state.Has(MapRequirementItems.BreathingShell);
                case MapRequirementTags.RootBoots:
                    // §2: "Botas Trançadas de Raiz Viva, Montaria Capivara" — a water mount floats over the mud too.
                    return state.Hero == CharacterId.Joaquim || state.WornIn(EquipmentSlot.Boots) == MapRequirementItems.RootBoots
                        || state.OwnedMountTypes.Contains(MountType.Aquatico);
                case MapRequirementTags.RootBridgeBalance:
                    return state.Hero == CharacterId.Maithe || state.Has(MapRequirementItems.BalancePole);
                case MapRequirementTags.SaddleRequired:
                    return state.Has(MapRequirementItems.ReedBoat) || state.OwnedMountTypes.Contains(MountType.Aquatico);
                case MapRequirementTags.AntiVenomAmulet:
                    return state.Has(MapRequirementItems.AntiVenomAmulet);
                case MapRequirementTags.PurifyingFruit:
                    return state.Has(MapRequirementItems.PurifyingFruit);
                case MapRequirementTags.ThermalClothes:
                    return state.WornIn(EquipmentSlot.Chest) == MapRequirementItems.ThermalClothes;
                case MapRequirementTags.ClimbingCrampons:
                    return state.Has(MapRequirementItems.ClimbingCrampons);
                case MapRequirementTags.StormProtection:
                    return state.Has(MapRequirementItems.StormCloak);
                case MapRequirementTags.SunSeal:
                    return state.Has(MapRequirementItems.SunSeal);
                default:
                    return false;
            }
        }

        /// <summary>The lookup <see cref="MapAccessRules"/> takes (<c>Func&lt;string, bool&gt;</c>); null state = nothing is met.</summary>
        public static Func<string, bool> Lookup(MapRequirementState state) => tag => IsMet(tag, state);

        /// <summary>A lit lamp: the oil lamp of the Vila das Lamparinas or the runic one.</summary>
        public static bool HasLamp(MapRequirementState state) =>
            state != null && (state.Has(MapRequirementItems.OilLamp) || state.Has(MapRequirementItems.RunicLamp));

        /// <summary>Real wings of the Tacho Encantado in the "Costas/Asas" slot (the felt cape does not fly).</summary>
        public static bool HasWings(MapRequirementState state)
        {
            string worn = state?.WornIn(EquipmentSlot.Wings);
            return WingCatalog.TryParse(worn, out WingStyle style, out _) && !(WingCatalog.Get(style)?.IsCape ?? true);
        }

        /// <summary>The Capa de Feltro dos Nartes (Tacho, Joaquim) worn on the back keeps the mountain wind out like a wool cloak.</summary>
        public static bool WearsFeltCape(MapRequirementState state)
        {
            string worn = state?.WornIn(EquipmentSlot.Wings);
            return WingCatalog.TryParse(worn, out WingStyle style, out _) && style == WingStyle.CapaVaqueiro;
        }

        /// <summary>
        /// Everything the hero has to carry/wear for <paramref name="tag"/> with the bought gear alone: the primary item, plus
        /// the lamp for "Picareta e Tocha". Empty when only wings or a mount do (Asas Obrigatórias, map 39).
        /// </summary>
        public static string[] KitOf(string tag)
        {
            string primary = PrimaryItemOf(tag);
            if (primary == null)
            {
                return Array.Empty<string>();
            }

            return tag == MapRequirementTags.PickaxeTorch ? new[] { primary, MapRequirementItems.OilLamp } : new[] { primary };
        }

        /// <summary>
        /// The item that answers <paramref name="tag"/> (the one sold/given in the return city), or null when only wings or
        /// a mount do (Asas Obrigatórias). Used by the docs table, the hints and the tests.
        /// </summary>
        public static string PrimaryItemOf(string tag)
        {
            switch (tag)
            {
                case MapRequirementTags.LeatherBoots: return MapRequirementItems.LeatherBoots;
                case MapRequirementTags.ElevatedPlatforms: return MapRequirementItems.ClimbingRope;
                case MapRequirementTags.GardenKey: return MapRequirementItems.GardenKey;
                case MapRequirementTags.FreshWater: return MapRequirementItems.Waterskin;
                case MapRequirementTags.PoisonProtection: return MapRequirementItems.Theriac;
                case MapRequirementTags.WoolCloak: return MapRequirementItems.WoolCloak;
                case MapRequirementTags.PickaxeTorch: return MapRequirementItems.Pickaxe;
                case MapRequirementTags.BlizzardResistance: return MapRequirementItems.GingerTea;
                case MapRequirementTags.TorchRequired: return MapRequirementItems.OilLamp;
                case MapRequirementTags.RunicLamp: return MapRequirementItems.RunicLamp;
                case MapRequirementTags.EternalFireOil: return MapRequirementItems.EternalFireOil;
                case MapRequirementTags.WingsOrDoubleJump: return MapRequirementItems.CanvasWings;
                case MapRequirementTags.BoatOrDock: return MapRequirementItems.DhowPassage;
                case MapRequirementTags.ArtificerRelic: return MapRequirementItems.ArtificerRelic;
                case MapRequirementTags.BreathingShell: return MapRequirementItems.BreathingShell;
                case MapRequirementTags.AirBubble: return MapRequirementItems.AirBubble;
                case MapRequirementTags.RootBoots: return MapRequirementItems.RootBoots;
                case MapRequirementTags.RootBridgeBalance: return MapRequirementItems.BalancePole;
                case MapRequirementTags.SaddleRequired: return MapRequirementItems.ReedBoat;
                case MapRequirementTags.AntiVenomAmulet: return MapRequirementItems.AntiVenomAmulet;
                case MapRequirementTags.PurifyingFruit: return MapRequirementItems.PurifyingFruit;
                case MapRequirementTags.ThermalClothes: return MapRequirementItems.ThermalClothes;
                case MapRequirementTags.ClimbingCrampons: return MapRequirementItems.ClimbingCrampons;
                case MapRequirementTags.StormProtection: return MapRequirementItems.StormCloak;
                case MapRequirementTags.SunSeal: return MapRequirementItems.SunSeal;
                default: return null;
            }
        }
    }
}
