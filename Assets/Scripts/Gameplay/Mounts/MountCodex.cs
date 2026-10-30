using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>One line of the "Caderno de Montarias": a species of the full catalog of 30 (§3), with where it shows up.</summary>
    public sealed class MountCodexEntry
    {
        public readonly string MountId;
        public readonly ItemRarity Rarity;
        public readonly BiomeType Region;

        public MountCodexEntry(string mountId, ItemRarity rarity, BiomeType region)
        {
            MountId = mountId;
            Rarity = rarity;
            Region = region;
        }

        public string NameKey => "mount." + MountId + ".name";

        /// <summary>"Aparece perto do Lobo Alfa…" — the hint shown under the silhouette.</summary>
        public string HintKey => "mount." + MountId + ".source";

        /// <summary>Playable today (in <see cref="MountCatalog"/>); the others are only listed.</summary>
        public bool IsAvailable => MountCatalog.FindSpec(MountId) != null;
    }

    /// <summary>
    /// The "Caderno de Montarias" (Docs/Mounts.md §7, phase M2): all 30 species of §3, in catalog order. Owned ones show
    /// their name and stats, seen-but-not-owned ones their name, the rest a "???" silhouette with the hint of where they
    /// appear. The species in <see cref="MountCatalog"/> (3 starters + M3 wave 1) are playable, the rest only listed; their names and
    /// hints live in <see cref="MountTexts"/>, the other 27 here.
    /// </summary>
    public static class MountCodex
    {
        // id, rarity, region, en name, pt name, en hint, pt hint
        private static readonly (string Id, ItemRarity Rarity, BiomeType Region, string NameEn, string NamePt, string HintEn, string HintPt)[] Rows =
        {
            ("mount_pony", ItemRarity.Common, BiomeType.Forest, null, null, null, null),
            ("mount_big_dog", ItemRarity.Common, BiomeType.Forest, null, null, null, null),
            ("mount_capybara", ItemRarity.Common, BiomeType.Forest, "Giant Capybara", "Capivara Gigante", "Haras da Vila.", "Haras da Vila."),
            ("mount_mountain_goat", ItemRarity.Common, BiomeType.Mountain, "Leaping Goat", "Bode Saltador", "A corral up on the Pico.", "Um curral lá no Pico."),
            ("mount_pack_donkey", ItemRarity.Common, BiomeType.Desert, null, null, null, null),
            ("mount_rhea", ItemRarity.Common, BiomeType.Desert, "Running Rhea", "Ema Corredora", "A critter fair at the oasis.", "Uma feira de bichos no oásis."),
            ("mount_pinto_horse", ItemRarity.Uncommon, BiomeType.Forest, "Cowboy's Pinto Horse", "Cavalo Pampa do Vaqueiro", "Haras da Vila, for seasoned cousins.", "Haras da Vila, para primos experientes."),
            ("mount_enchanted_donkey", ItemRarity.Uncommon, BiomeType.Desert, "Enchanted Sertão Donkey", "Jumento Encantado do Sertão", "A thirsty donkey who knows the way.", "Um jumento com sede que sabe o caminho."),
            ("mount_giant_armadillo", ItemRarity.Uncommon, BiomeType.Caves, "Giant Ball Armadillo", "Tatu-Bola Gigante", "Rolls around where the bats fly.", "Rola por onde os morcegos voam."),
            ("mount_maned_wolf", ItemRarity.Uncommon, BiomeType.Forest, "Enchanted Maned Wolf", "Lobo-Guará Encantado", "Walks with the forest wolves, more at night.", "Anda com os lobos da mata, mais à noite."),
            ("mount_caipora_peccary", ItemRarity.Uncommon, BiomeType.Forest, "Caipora's Peccary", "Caititu da Caipora", "A friend of whoever finds the forest secrets.", "Amigo de quem acha os segredos da Floresta."),
            ("mount_river_seahorse", ItemRarity.Uncommon, BiomeType.Forest, "Enchanted River Seahorse", "Cavalo-Marinho Encantado do Rio", "Something shines at the bottom of the açude.", "Algo brilha no fundo do açude."),
            ("mount_giant_anteater", ItemRarity.Uncommon, BiomeType.Caves, "Giant Anteater", "Tamanduá-Bandeira Gigante", "A keeper's den in the caves.", "A toca de um tratador nas Cavernas."),
            ("mount_golden_scarab", ItemRarity.Uncommon, BiomeType.Desert, "Golden Scarab", "Escaravelho Dourado", "Digs near the dune scorpions.", "Cava perto dos escorpiões das dunas."),
            ("mount_snow_tiger", ItemRarity.Rare, BiomeType.Mountain, "Snow Tiger", "Tigre-das-Neves", "Follows the mountain warlord.", "Segue o Senhor da Guerra da Montanha."),
            ("mount_woolly_mammoth", ItemRarity.Rare, BiomeType.Mountain, "Woolly Mammoth", "Mamute Lanoso", "Heavy steps near the stone golems.", "Passos pesados perto dos golens."),
            ("mount_unicorn", ItemRarity.Rare, BiomeType.Forest, "Forest Unicorn", "Unicórnio da Mata", "Seen near the Lobo Alfa.", "Visto perto do Lobo Alfa."),
            ("mount_hippogriff", ItemRarity.Rare, BiomeType.Mountain, "Gale Hippogriff", "Hipogrifo Ventania", "A chick fell from its nest on the Pico.", "Um filhote caiu do ninho no Pico."),
            ("mount_griffin", ItemRarity.Rare, BiomeType.Desert, "Ruins Griffin", "Grifo das Ruínas", "Nests by the Ruin Guardian.", "Faz ninho junto ao Guardião da Ruína."),
            ("mount_light_jaguar", ItemRarity.Rare, BiomeType.Forest, "Jaguar of Light", "Onça-Pintada de Luz", "Hunts with spiders and wolves at night.", "Caça com aranhas e lobos à noite."),
            ("mount_carbuncle", ItemRarity.Rare, BiomeType.Caves, "Cave Carbuncle", "Carbúnculo das Grutas", "A jewel glows near the Ancient Dweller.", "Uma joia brilha perto do Habitante Ancestral."),
            ("mount_fire_rhea", ItemRarity.Rare, BiomeType.Desert, "Fire Rhea", "Ema de Fogo", "Festa Junina prize.", "Prenda da Festa Junina."),
            ("mount_pegasus", ItemRarity.Epic, BiomeType.Mountain, "White-Cloud Pegasus", "Pégaso Nuvem-Branca", "Above the warlord's mountain.", "Acima da montanha do Senhor da Guerra."),
            ("mount_sleipnir", ItemRarity.Epic, BiomeType.Forest, "Sleipnir, Eight-Legged Steed", "Sleipnir, o Corcel de Oito Patas", "For travellers of the four corners.", "Para viajantes dos quatro cantos."),
            ("mount_kirin", ItemRarity.Epic, BiomeType.Caves, "Dew Kirin", "Kirin do Orvalho", "Walks without footprints in the caves.", "Anda sem pegadas nas Cavernas."),
            ("mount_boitata", ItemRarity.Epic, BiomeType.Forest, "Boitatá, Forest Guardian", "Boitatá, Guardião da Mata", "Thanks whoever puts out the abandoned fires.", "Agradece a quem apaga as fogueiras abandonadas."),
            ("mount_boi_bumba", ItemRarity.Epic, BiomeType.Forest, "Starry Boi-Bumbá", "Boi-Bumbá Estrelado", "Dances at the Festa Junina.", "Dança na Festa Junina."),
            ("mount_sertao_phoenix", ItemRarity.Legendary, BiomeType.Caves, "Sertão Phoenix", "Fênix do Sertão", "Ashes deep in the caves… very rare.", "Cinzas no fundo das Cavernas… muito rara."),
            ("mount_dragon_hatchling", ItemRarity.Legendary, BiomeType.Desert, "Brasinha the Dragon Hatchling", "Dragão Filhote Brasinha", "An egg, warm, near the Ruin Guardian.", "Um ovo, quentinho, perto do Guardião da Ruína."),
            ("mount_elder_tortoise", ItemRarity.Legendary, BiomeType.Forest, "Elder Tortoise", "Jabuti Ancião", "Patience: 100 days of daily quests.", "Paciência: 100 dias de missões diárias."),
        };

        public static readonly IReadOnlyList<MountCodexEntry> Entries = Rows.Select(r => new MountCodexEntry(r.Id, r.Rarity, r.Region)).ToList();

        /// <summary>Names and hints of the species not in <see cref="MountCatalog"/> yet (merged by <see cref="MountTexts"/>).</summary>
        public static IEnumerable<LocalizationEntry> TextEntries()
        {
            foreach (var r in Rows)
            {
                if (r.NameEn == null)
                {
                    continue;
                }

                yield return new LocalizationEntry("mount." + r.Id + ".name", r.NameEn, r.NamePt);
                yield return new LocalizationEntry("mount." + r.Id + ".source", r.HintEn, r.HintPt);
            }
        }

        public static int Count => Entries.Count;
    }
}
