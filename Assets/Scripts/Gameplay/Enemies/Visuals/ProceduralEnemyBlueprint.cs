using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The enemies built from code (see <see cref="ProceduralEnemyModels"/>): the five that never had a
    /// Meshy model, plus jointed low-poly animals replacing the static Meshy wolf, spider and scorpion so
    /// their legs can step (<see cref="Rapadura.Gameplay.Locomotion.ProceduralLegLocomotion"/>).
    /// </summary>
    public enum ProceduralEnemyKind
    {
        None = 0,
        BatSwarm = 1,
        StoneGolem = 2,
        Warlord = 3,
        RuinGuardian = 4,
        AncientDweller = 5,
        Wolf = 6,
        AlphaWolf = 7,
        Spider = 8,
        Scorpion = 9,

        // Night-only species (Docs/DayNight.md), each with its own model since the night-models pass.
        /// <summary>Lobisomem-do-sertão: shaggy quadruped that rears up on its hind legs to strike.</summary>
        Werewolf = 10,
        /// <summary>Morcego-vampiro gigante: one big bat with a wide wingspan.</summary>
        VampireBat = 11,
        /// <summary>Coruja-rasga-mortalha: barn owl with a pale heart-shaped face.</summary>
        ShroudOwl = 12,
        /// <summary>Enxame de vaga-lumes-de-fogo: a cloud of glowing fireflies that moves as one enemy.</summary>
        FireflySwarm = 13,
        /// <summary>Porco-do-mato da Caipora (queixada): bristly peccary with a pale collar, comes in herds.</summary>
        Peccary = 14,
        /// <summary>Mãe-da-lua (urutau): potoo with huge yellow eyes and a gaping mouth, carrying its branch.</summary>
        Potoo = 15,

        // [distância] (ranged) commons of the playable maps 07/08/13/14/19/20 (Docs/Mapas100.md), shoot via EnemyRangedAttack.
        /// <summary>Abutre de Brasa: bald-headed vulture with ember-tipped wings that spits embers.</summary>
        EmberVulture = 16,
        /// <summary>Lanterna Quebrada dos Artífices: a floating cracked lantern engine that shoots bolts of trapped light.</summary>
        BrokenLantern = 17,
        /// <summary>Gavião-de-Penacho: crested hawk that throws hard flight feathers like darts.</summary>
        CrestedHawk = 18,
        /// <summary>Peixe-Lanterna Apagado: a dimmed anglerfish swimming in the cave air, flicking globs from its lure.</summary>
        LanternFish = 19,

        // Guardians of the Ossuário dos Artífices (Docs/Dungeons.md), spawned by the DungeonDirector (OssuaryEnemyRecipes).
        /// <summary>Caveira Flutuante: an ivory skull with soul-fire sockets and a trail of green wisps.</summary>
        FloatingSkull = 20,
        /// <summary>Guerreiro Esqueleto de Cobre: copper-boned skeleton with a crested helm, bone shield and copper sword.</summary>
        SkeletonWarrior = 21,
        /// <summary>Arqueiro Esqueleto do Fosso: hooded ivory skeleton with a recurved longbow and rune arrows.</summary>
        SkeletonArcher = 22,
        /// <summary>Feiticeiro das Ossadas: hunched skeleton in a burial robe, bone staff with a violet orb.</summary>
        BoneSorcerer = 23,
        /// <summary>O Colosso de Crânios (boss): a giant of stacked skulls round a soul-fire core, with a femur club.</summary>
        BoneColossus = 24,

        // The beasts the map sheets of the continuous worlds (Docs/Mapas100.md 07–24) and Taa Pwani (25) asked for, which
        // used to be played by stand-ins (OverworldBeastRecipes, OverworldOddityRecipes).
        /// <summary>Chacal Apagado (Areias): lean sandy jackal with tall ears and a dark saddle, ember eyes.</summary>
        Jackal = 25,
        /// <summary>Cobra-de-Areia Apagada (Areias): horned sand viper, forepart raised, body lying in an S.</summary>
        SandSnake = 26,
        /// <summary>Orc Coroado de Āz (Areias, Tambor Rubro): a big orc wearing one of Āz's tin crowns, with a club.</summary>
        CrownedOrc = 27,
        /// <summary>Pedra que Rola (Kaggen): a ushabti boulder that rolls at you, molten cracks and runes.</summary>
        RollingStone = 28,
        /// <summary>Neblina do Vento Ruim (Kaggen): a knot of cold fog with two pale eyes and puffs circling it.</summary>
        IllWindMist = 29,
        /// <summary>Babuíno Apagado (Kaggen): grey-maned baboon, long pink muzzle, arched tail.</summary>
        Baboon = 30,
        /// <summary>Lodo de Gruta (Limiar): a mossy dome of cave mud with swallowed crystals poking out.</summary>
        CaveOoze = 31,
        /// <summary>Corvo-da-Treva (Limiar): black raven with violet-tipped feathers, throws them like darts.</summary>
        DarkCrow = 32,
        /// <summary>Gaivota Ladra (Costa das Monções): white-and-grey gull with a stolen coin in its beak.</summary>
        ThiefGull = 33,
        /// <summary>Caranguejo-dos-Coqueiros Gigante (Costa das Monções): red giant crab, one big pincer, eyes on stalks.</summary>
        CoconutCrab = 34,

        // The beasts of the late continuous worlds (Docs/Mapas100.md 25–40: Costa das Monções, Delta de Jhilpur, Teto do
        // Mundo) that the overworld layouts played with stand-ins (CoastDeltaBeastRecipes, PamirBeastRecipes).
        /// <summary>Crocodilo do Mangue Apagado (Costa, Delta): long armoured crocodile, back scutes, jaw that gapes.</summary>
        MangroveCrocodile = 35,
        /// <summary>Leão do Capim Apagado (Costa, 26): tawny lion with a dark mane and a tufted tail.</summary>
        GrassLion = 36,
        /// <summary>Rinoceronte-de-Pó (Costa, 26): dust-grey rhino with two horns, lowers its head to charge.</summary>
        DustRhino = 37,
        /// <summary>Macaco Ladrão (Delta, 32): grey langur with a black face, long curled tail, a stolen bangle.</summary>
        ThiefMonkey = 38,
        /// <summary>Lontra Ladra (Delta, 31): long low otter, head up, a stolen glowing fish in its mouth.</summary>
        ThiefOtter = 39,
        /// <summary>Píton Apagada (Delta, 32): big blotched python, forepart raised, body in a wide S (slithers like the sand snake).</summary>
        Python = 40,
        /// <summary>Trol Apagado (Delta, 33): mossy swamp troll with long arms and a caged will-o'-wisp on its back.</summary>
        DimmedTroll = 41,
        /// <summary>Urso-Pardo do Pamir Apagado (Teto, 36): brown bear with a shoulder hump and a jaw that gapes.</summary>
        BrownBear = 42,
        /// <summary>Iaque Selvagem Apagado (Teto, 38): shaggy black yak with a skirt of long hair and wide horns.</summary>
        WildYak = 43,
        /// <summary>Irbis Apagado (Teto, 40): pale snow leopard with dark rosettes and a long thick tail.</summary>
        Irbis = 44,
        /// <summary>Almas (Teto, 38): stooped, hairy wild man of the glacier holding a snowball.</summary>
        Almas = 45,
        /// <summary>Marmota Gigante (Teto, 36–37): chubby golden marmot with big incisors and a bushy tail.</summary>
        GiantMarmot = 46,

        // The beasts of the Planície de Mingcheng and the Estepe dos Nartes (Docs/Mapas100.md 41–52) that the overworld
        // layouts played with stand-ins (MingchengBeastRecipes, NartesBeastRecipes).
        /// <summary>Leão de Pedra Apagado (Mingcheng, 41–42): grey guardian lion statue, curly mane, a jaw that gapes.</summary>
        StoneLion = 47,
        /// <summary>Raposa de Cauda Lanterna (Mingcheng, 41): red fox whose bushy tail ends in a glowing paper lantern.</summary>
        LanternFox = 48,
        /// <summary>Soldado de Terracota Apagado (Mingcheng, 41, 45): clay soldier in lamellar armour with a halberd.</summary>
        TerracottaSoldier = 49,
        /// <summary>Tartaruga de Pedra (Mingcheng, 43): stone turtle carrying a stele on its shell, a jaw that snaps.</summary>
        StoneTurtle = 50,
        /// <summary>Búfalo de Lama (Mingcheng, 44): grey water buffalo with swept-back horns, lowers its head to charge.</summary>
        MudBuffalo = 51,
        /// <summary>Filhote de Nian (Mingcheng, 45): red-and-gold lion-dog cub with one horn and a big jaw.</summary>
        NianCub = 52,
        /// <summary>Grifo Jovem (Nartes, 47–48): lion body, eagle head and folded wings in Scythian gold; wings spread to strike.</summary>
        YoungGriffin = 53,
        /// <summary>Saiga Assustada (Nartes, 47): pale steppe antelope with the drooping nose and amber horns; lowers its head to butt.</summary>
        Saiga = 54,
        /// <summary>Lobo de Bronze (Nartes, 48): Scythian bronze wolf from the kurgans, verdigris spirals, a jaw that gapes.</summary>
        BronzeWolf = 55,
        /// <summary>Gigante Sonâmbulo (Nartes, 49): huge hill giant walking in his sleep, eyes shut, arms out; slams with both.</summary>
        SleepwalkerGiant = 56,
        /// <summary>Besouro-Casca (Nartes, 50): six-legged beetle with bark-ridged wing cases and pincer mandibles.</summary>
        BarkBeetle = 57,
        /// <summary>Ochokochi (Nartes, 52): shaggy Georgian forest man with the axe-blade on his chest; arms up to hug and cut.</summary>
        Ochokochi = 58,

        // The beasts of the Picos de Farnkesh (Docs/Mapas100.md 53–58) that the overworld layout played with stand-ins
        // (FarnkeshBeastRecipes).
        /// <summary>Bode de Fumaça (Vale, 53): goat whose fleece is smoke with embers, black curled horns; lowers its head to butt.</summary>
        SmokeGoat = 59,
        /// <summary>Abelha Dourada (Vale, 53): dog-sized bee turned to gold by Midas's touch; buzzing wings, the sting swings under.</summary>
        GoldenBee = 60,
        /// <summary>Pássaro-Flauta Apagado (Vale, 53): long-tailed songbird with a reed-pipe beak; [distância], the beak opens to shoot.</summary>
        FluteBird = 61,
        /// <summary>Raposa da Vinha (Vale, 53): red fox with a grape-stained muzzle and vines in its brush; a jaw that gapes.</summary>
        VineFox = 62,
        /// <summary>Gralha-Alpina (Farnkesh, 54): black chough with yellow beak and red legs; the beak opens on the windup.</summary>
        AlpineChough = 63,
        /// <summary>Takin Apagado (Farnkesh, 54): golden takin with the Roman nose, greyed by the shadow; lowers its head to charge.</summary>
        Takin = 64,
        /// <summary>Cabra-Azul (Farnkesh, 54): the bharal, slate blue with the black stripe, horns out; lowers its head to butt.</summary>
        BlueSheep = 65,
        /// <summary>Morcego-Vela (Pontes, 55): bat with a lit candle on its head; the flame flares before the bite.</summary>
        CandleBat = 66,
        /// <summary>Ushabti de Corda (Pontes, 55): servant figure of rope and clay with its hoe; both arms go up to strike.</summary>
        RopeUshabti = 67,
        /// <summary>Gato-de-Pallas Apagado (Ermidas, 56): stocky long-furred manul, flat ears, ringed tail; a jaw that gapes.</summary>
        PallasCat = 68,
        /// <summary>Águia da Tempestade (Garganta, 57): dark grey eagle with lightning in its wing tips; [distância].</summary>
        StormEagle = 69,
        /// <summary>Harpia de Fumaça (Ninho, 58): smoky bird of prey with a pale masked face and long talons; the beak opens to strike.</summary>
        SmokeHarpy = 70,
    }

    /// <summary>
    /// A procedural enemy model as data: a flat list of named parts (index of the parent, local
    /// pose, optional mesh). Meshes can be shared by several parts (the bats of the swarm). Turned
    /// into a GameObject hierarchy by <see cref="ProceduralEnemyModels.Instantiate(ProceduralEnemyBlueprint, Material, Transform)"/>
    /// or baked to assets by the editor menu. Owns its meshes: <see cref="DestroyMeshes"/> them when
    /// the blueprint is not cached.
    /// </summary>
    public sealed class ProceduralEnemyBlueprint
    {
        public struct Part
        {
            public string Name;
            /// <summary>Index of the parent part, or -1 for a child of the model root.</summary>
            public int Parent;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
            /// <summary>Null for a pivot-only node.</summary>
            public Mesh Mesh;
        }

        private readonly List<Part> _parts = new List<Part>();
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly Dictionary<string, Mesh> _meshByKey = new Dictionary<string, Mesh>(StringComparer.Ordinal);
        private readonly EnemyMeshBuilder _builder = new EnemyMeshBuilder();

        private readonly string _meshNamePrefix;

        /// <param name="meshNamePrefix">Prefix of the mesh names; default "ProcEnemy_&lt;kind&gt;_" (the pet model passes its own).</param>
        public ProceduralEnemyBlueprint(ProceduralEnemyKind kind, bool isBoss, string meshNamePrefix = null)
        {
            Kind = kind;
            IsBoss = isBoss;
            _meshNamePrefix = meshNamePrefix ?? "ProcEnemy_" + kind + "_";
        }

        public ProceduralEnemyKind Kind { get; }
        public bool IsBoss { get; }
        public IReadOnlyList<Part> Parts => _parts;

        /// <summary>Distinct meshes (each listed once even when several parts share it).</summary>
        public IReadOnlyList<Mesh> Meshes => _meshes;

        /// <summary>Triangles drawn by the whole model (a shared mesh counts once per part using it).</summary>
        public int TriangleCount
        {
            get
            {
                int total = 0;
                foreach (Part part in _parts)
                {
                    if (part.Mesh != null)
                    {
                        total += part.Mesh.triangles.Length / 3;
                    }
                }

                return total;
            }
        }

        public int IndexOf(string partName)
        {
            for (int i = 0; i < _parts.Count; i++)
            {
                if (_parts[i].Name == partName)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Adds a pivot-only node; returns its index (pass as <c>parent</c> of its children).</summary>
        public int Node(string name, int parent, Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
        {
            return AddPart(name, parent, position, rotation, scale, null);
        }

        /// <summary>
        /// Adds a part whose mesh is drawn by <paramref name="draw"/> in the part's local space. Parts
        /// passing the same <paramref name="meshKey"/> share one mesh (drawn once, by the first).
        /// </summary>
        public int Mesh(string name, int parent, Vector3 position, Action<EnemyMeshBuilder> draw, Quaternion? rotation = null,
            Vector3? scale = null, string meshKey = null)
        {
            string key = meshKey ?? name;
            if (!_meshByKey.TryGetValue(key, out Mesh mesh))
            {
                _builder.Clear();
                draw(_builder);
                mesh = _builder.ToMesh(_meshNamePrefix + key);
                _meshByKey[key] = mesh;
                _meshes.Add(mesh);
            }

            return AddPart(name, parent, position, rotation, scale, mesh);
        }

        private int AddPart(string name, int parent, Vector3 position, Quaternion? rotation, Vector3? scale, Mesh mesh)
        {
            if (parent >= _parts.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(parent), "A part's parent must be added before it.");
            }

            _parts.Add(new Part
            {
                Name = name,
                Parent = parent,
                Position = position,
                Rotation = rotation ?? Quaternion.identity,
                Scale = scale ?? Vector3.one,
                Mesh = mesh,
            });
            return _parts.Count - 1;
        }

        /// <summary>Destroys the meshes (only for blueprints that are not cached/baked).</summary>
        public void DestroyMeshes()
        {
            foreach (Mesh mesh in _meshes)
            {
                if (mesh != null)
                {
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
            }

            _meshes.Clear();
            _meshByKey.Clear();
        }
    }
}
