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
            ("mount_capybara", ItemRarity.Common, BiomeType.Forest, "Taweret's Pygmy Hippo", "Hipopotinha de Taweret", "Srosh's Stable.", "Estrebaria do Srosh."),
            ("mount_mountain_goat", ItemRarity.Common, BiomeType.Mountain, "Walia Ibex", "Íbex-da-Walia", "The Litema corral, up in the Kaggen Range.", "O Curral de Litema, lá na Serra de Kaggen."),
            ("mount_pack_donkey", ItemRarity.Common, BiomeType.Desert, null, null, null, null),
            ("mount_rhea", ItemRarity.Common, BiomeType.Desert, "Running Ostrich", "Avestruz Corredora", "The caravanserai at Qocho Oasis.", "O caravançará do Oásis de Qocho."),
            ("mount_pinto_horse", ItemRarity.Uncommon, BiomeType.Forest, "Heavenly Horse of Ferghana", "Cavalo Celeste de Fergana", "Srosh's Stable, for seasoned cousins.", "Estrebaria do Srosh, para primos experientes."),
            ("mount_enchanted_donkey", ItemRarity.Uncommon, BiomeType.Desert, "Three-Legged Ass", "Asno de Três Patas", "A thirsty white ass who knows the way to every well.", "Um asno branco com sede que sabe o caminho de todo poço."),
            ("mount_giant_armadillo", ItemRarity.Uncommon, BiomeType.Caves, "Giant Pangolin", "Pangolim Gigante", "Rolls into a ball where the bats fly.", "Se enrola em bola por onde os morcegos voam."),
            ("mount_maned_wolf", ItemRarity.Uncommon, BiomeType.Forest, "Wepwawet's Jackal", "Chacal de Upuaut", "Walks with the Tugai wolves, more at night.", "Anda com os lobos do tugai, mais à noite."),
            ("mount_caipora_peccary", ItemRarity.Uncommon, BiomeType.Forest, "Trailblazer Warthog", "Facóquero Trilheiro", "A friend of whoever finds the secrets of the Zarafshan Woods.", "Amigo de quem acha os segredos do Bosque do Zarafshan."),
            ("mount_river_seahorse", ItemRarity.Uncommon, BiomeType.Forest, "Lake Van Water Horse", "Cavalo-d'Água do Lago Van", "Something shines at the bottom of the backwater.", "Algo brilha no fundo do remanso."),
            ("mount_giant_anteater", ItemRarity.Uncommon, BiomeType.Caves, "Giant Aardvark", "Porco-Formigueiro Gigante", "The Lampkeepers' Den, in the caves.", "A Toca das Lamparinas, nas Grutas."),
            ("mount_golden_scarab", ItemRarity.Uncommon, BiomeType.Desert, "Khepri's Scarab", "Escaravelho de Khepri", "Digs near the Flaming Sands scorpions.", "Cava perto dos escorpiões das Areias Flamejantes."),
            ("mount_snow_tiger", ItemRarity.Rare, BiomeType.Mountain, "Irbis the Snow Leopard", "Irbis, o Leopardo-das-Neves", "Follows Hahya the Giant Warrior.", "Segue o Hahya, o Gigante Guerreiro."),
            ("mount_woolly_mammoth", ItemRarity.Rare, BiomeType.Mountain, "Komi Mammoth", "Mamute de Komi", "Heavy steps near the stone ushabtis.", "Passos pesados perto dos ushabtis de pedra."),
            ("mount_unicorn", ItemRarity.Rare, BiomeType.Forest, "Singing Shadhavar", "Shadhavar Cantante", "Seen near Kök-Böri, the Sky Wolf.", "Visto perto do Kök-Böri, o Lobo Celeste."),
            ("mount_hippogriff", ItemRarity.Rare, BiomeType.Mountain, "Gale Senmurv", "Senmurv Ventania", "A chick fell from its nest in the Kaggen Range.", "Um filhote caiu do ninho na Serra de Kaggen."),
            ("mount_griffin", ItemRarity.Rare, BiomeType.Desert, "Scythian Gold Griffin", "Grifo de Ouro Cita", "Nests by the Great Ushabti.", "Faz ninho junto ao Grande Ushabti."),
            ("mount_light_jaguar", ItemRarity.Rare, BiomeType.Forest, "Apedemak's Light Lion", "Leão de Luz de Apedemak", "Hunts with spiders and wolves at night.", "Caça com aranhas e lobos à noite."),
            ("mount_carbuncle", ItemRarity.Rare, BiomeType.Caves, "Threshold Jewel Fox", "Raposa-Joia do Limiar", "A jewel glows near Tages, the Ancient Dweller.", "Uma joia brilha perto do Tages, o Habitante Ancestral."),
            ("mount_fire_rhea", ItemRarity.Rare, BiomeType.Desert, "Ember Ostrich", "Avestruz de Brasa", "Bema prize.", "Prêmio do Bema."),
            ("mount_pegasus", ItemRarity.Epic, BiomeType.Mountain, "Cloud-White Tulpar", "Tulpar Nuvem-Branca", "Above Hahya's mountain.", "Acima da serra do Hahya."),
            ("mount_sleipnir", ItemRarity.Epic, BiomeType.Forest, "Sleipnir, Eight-Legged Steed", "Sleipnir, o Corcel de Oito Patas", "For travellers of the four corners.", "Para viajantes dos quatro cantos."),
            ("mount_kirin", ItemRarity.Epic, BiomeType.Caves, "Dew Qilin", "Qilin do Orvalho", "Walks without footprints in the caves.", "Anda sem pegadas nas Grutas."),
            ("mount_boitata", ItemRarity.Epic, BiomeType.Forest, "Mehen the Guardian Coil", "Mehen, a Serpente Guardiã", "Thanks whoever puts out the abandoned fires.", "Agradece a quem apaga as fogueiras abandonadas."),
            ("mount_boi_bumba", ItemRarity.Epic, BiomeType.Forest, "Song Ox", "Boi das Canções", "Sings at the Bema.", "Canta no Bema."),
            ("mount_sertao_phoenix", ItemRarity.Legendary, BiomeType.Caves, "Bennu, Bird of Renewal", "Bennu, a Ave do Recomeço", "A feather of light deep in the caves… very rare.", "Uma pena de luz no fundo das Grutas… muito rara."),
            ("mount_dragon_hatchling", ItemRarity.Legendary, BiomeType.Desert, "Little Ember Vishap", "Vishap Filhote Brasinha", "An egg, warm, near the Great Ushabti.", "Um ovo, quentinho, perto do Grande Ushabti."),
            ("mount_elder_tortoise", ItemRarity.Legendary, BiomeType.Forest, "Ijapa the Elder Tortoise", "Ijapa, a Tartaruga Anciã", "Patience: 100 days of daily quests.", "Paciência: 100 dias de missões diárias."),
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
