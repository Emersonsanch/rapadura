using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the Ossuário dos Artífices: the world-map button, the in-dungeon HUD and banners
    /// (<c>ui.dungeon.*</c>, <c>dungeon.*</c>), one line per cousin when going in and when the Colosso rests
    /// (<c>dungeon.line.*</c>), the enemies' names (<c>enemy.&lt;id&gt;.name</c>) and the dungeon items
    /// (<c>item.&lt;id&gt;.*</c>; pt-BR = the asset text). Same self-contained pattern as <c>EventRoomTexts</c>: merged into
    /// the live <see cref="LocalizationManager"/> by <see cref="DungeonProgress.Initialize"/> (keys it already has win),
    /// pt-BR fallback without one.
    /// </summary>
    public static class DungeonTexts
    {
        public const string DungeonName = "ui.dungeon.ossuario.name";

        /// <summary>{0} = dungeon.</summary>
        public const string MapEnter = "ui.dungeon.map.enter";
        /// <summary>{0} = dungeon, {1} = level.</summary>
        public const string MapNeedLevel = "ui.dungeon.map.need_level";

        /// <summary>{0} = keys held.</summary>
        public const string HudKeys = "ui.dungeon.hud.keys";
        public const string GoalVestibule = "ui.dungeon.hud.goal.vestibule";
        public const string GoalGate = "ui.dungeon.hud.goal.gate";
        public const string GoalGallery = "ui.dungeon.hud.goal.gallery";
        public const string GoalBoss = "ui.dungeon.hud.goal.boss";
        public const string GoalCleared = "ui.dungeon.hud.goal.cleared";

        public const string RoomVestibule = "dungeon.room.vestibule";
        public const string RoomGallery = "dungeon.room.gallery";
        public const string RoomColossus = "dungeon.room.colossus";

        /// <summary>{0} = key name.</summary>
        public const string GateNeedKey = "dungeon.gate.need_key";
        public const string GateOpened = "dungeon.gate.opened";
        public const string SarcophagusLoot = "dungeon.sarcophagus.loot";
        public const string SarcophagusAmbush = "dungeon.sarcophagus.ambush";
        public const string WallHint = "dungeon.wall.hint";
        public const string WallBroken = "dungeon.wall.broken";
        public const string ChestOpened = "dungeon.chest.opened";
        public const string KeyFound = "dungeon.key.found";
        public const string SorcererRaise = "dungeon.sorcerer.raise";
        public const string ColossusAwakes = "dungeon.colossus.awakes";
        public const string ColossusSkulls = "dungeon.colossus.skulls";
        public const string ColossusCage = "dungeon.colossus.cage";
        public const string CageBroken = "dungeon.colossus.cage_broken";
        public const string CageHit = "dungeon.colossus.cage_hit";
        public const string Cleared = "dungeon.cleared";
        public const string SpectralTouch = "dungeon.set.spectral_touch";
        public const string GuardUp = "dungeon.warrior.guard";

        public const string SetName = "ui.dungeon.set.name";
        public const string SetBonus = "ui.dungeon.set.bonus";

        // Items (pt-BR = the asset text, kept in sync by the dungeon tests).
        public const string KeyNamePt = "Chave de Osso Rúnica";
        public const string KeyDescriptionPt = "Chave de marfim gravada com runas dos Artífices. Abre um portão gradeado do Ossuário (e some na fechadura).";
        public const string ChestNamePt = "Armadura Peitoral de Costelas";
        public const string ChestDescriptionPt = "Peitoral de marfim dos Artífices, leve como cesto. Conjunto Marfim dos Artífices (2 peças): Toque Espectral.";
        public const string HelmNamePt = "Elmo Crânio de Cobre";
        public const string HelmDescriptionPt = "Elmo de cobre em forma de crânio sorridente, forrado de lã. Conjunto Marfim dos Artífices (2 peças): Toque Espectral.";
        public const string FragmentNamePt = "Fragmento de Osso Antigo";
        public const string FragmentDescriptionPt = "Lasca de marfim que ainda brilha de leve. Os artesãos pagam bem por ela.";
        public const string CoinNamePt = "Moeda Furada de Qocho";
        public const string CoinDescriptionPt = "Moeda antiga com um furo no meio, para levar num cordão. Vale umas pratas na cidade.";
        public const string SwordNamePt = "Espada Enferrujada do Vigia";
        public const string SwordDescriptionPt = "Espada de cobre de um vigia do Ossuário. Velha e manchada, mas ainda corta.";

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static string ItemNameKey(string itemId) => "item." + itemId + ".name";
        public static string ItemDescriptionKey(string itemId) => "item." + itemId + ".description";
        public static string EnemyNameKey(string enemyId) => "enemy." + enemyId + ".name";

        public static string RoomNameKey(int room) => room <= 0 ? RoomVestibule : room == 1 ? RoomGallery : RoomColossus;

        public static string LineKey(bool cleared, CharacterId hero) => $"dungeon.line.{(cleared ? "win" : "start")}.{hero.ToString().ToLowerInvariant()}";

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(DungeonName, "Ossuary of the Artificers", "Ossuário dos Artífices"),
                new LocalizationEntry(MapEnter, "Enter: {0}", "Entrar: {0}"),
                new LocalizationEntry(MapNeedLevel, "{0} opens to heroes of level {1}.", "{0} abre para heróis de nível {1}."),

                new LocalizationEntry(HudKeys, "Bone keys: {0}", "Chaves de osso: {0}"),
                new LocalizationEntry(GoalVestibule, "Calm the guardians of the Vestibule", "Acalme os guardiões do Vestíbulo"),
                new LocalizationEntry(GoalGate, "Open the barred gate with a Runic Bone Key", "Abra o portão gradeado com uma Chave de Osso Rúnica"),
                new LocalizationEntry(GoalGallery, "Cross the Archers' Gallery", "Atravesse a Galeria dos Arqueiros"),
                new LocalizationEntry(GoalBoss, "Face the Skull Colossus!", "Enfrente o Colosso de Crânios!"),
                new LocalizationEntry(GoalCleared, "The ossuary sleeps peacefully again. Well done!", "O Ossuário voltou a dormir em paz. Muito bem!"),

                new LocalizationEntry(RoomVestibule, "Vestibule", "Vestíbulo"),
                new LocalizationEntry(RoomGallery, "Archers' Gallery", "Galeria dos Arqueiros"),
                new LocalizationEntry(RoomColossus, "Colossus Chamber", "Câmara do Colosso"),

                new LocalizationEntry(GateNeedKey, "Locked! You need: {0}", "Trancado! Precisa de: {0}"),
                new LocalizationEntry(GateOpened, "Creak! The bars go up.", "Nhéééc! As grades sobem."),
                new LocalizationEntry(SarcophagusLoot, "The lid slides open... treasure!", "A tampa desliza... um tesouro!"),
                new LocalizationEntry(SarcophagusAmbush, "Boo! Sleepy skulls woke up!", "Buu! Caveiras sonolentas acordaram!"),
                new LocalizationEntry(WallHint, "A cracked bone wall... something heavy could knock it down.", "Uma parede de ossos rachada... algo pesado pode derrubá-la."),
                new LocalizationEntry(WallBroken, "The bone wall crumbles! A secret niche!", "A parede de ossos desmorona! Um nicho secreto!"),
                new LocalizationEntry(ChestOpened, "The Artificers' legendary chest!", "O baú lendário dos Artífices!"),
                new LocalizationEntry(KeyFound, "+ Runic Bone Key", "+ Chave de Osso Rúnica"),
                new LocalizationEntry(SorcererRaise, "The Sorcerer relights the fallen guardians!", "O Feiticeiro reacende os guardiões caídos!"),
                new LocalizationEntry(ColossusAwakes, "The Skull Colossus stands up!", "O Colosso de Crânios se levanta!"),
                new LocalizationEntry(ColossusSkulls, "Bouncing skulls! Watch the walls!", "Crânios saltitantes! Cuidado com as paredes!"),
                new LocalizationEntry(ColossusCage, "Rib cage! Hit it 3 times to break free!", "Prisão de Costelas! Bata 3 vezes para escapar!"),
                new LocalizationEntry(CageBroken, "Free! The charged blow missed you.", "Livre! O golpe carregado errou você."),
                new LocalizationEntry(CageHit, "Crack!", "Crec!"),
                new LocalizationEntry(Cleared, "Ossuary cleared! The guardians rest.", "Ossuário vencido! Os guardiões descansam."),
                new LocalizationEntry(SpectralTouch, "Spectral Touch!", "Toque Espectral!"),
                new LocalizationEntry(GuardUp, "Shield up!", "Escudo erguido!"),

                new LocalizationEntry(SetName, "Ivory Armour of the Artificers", "Armadura de Marfim dos Artífices"),
                new LocalizationEntry(SetBonus, "2 pieces: Spectral Touch — critical hits have a 15% chance to throw a bone shard at nearby enemies.",
                    "2 peças: Toque Espectral — golpes críticos têm 15% de chance de lançar um estilhaço de osso nos inimigos próximos."),

                new LocalizationEntry(EnemyNameKey(DungeonRules.FloatingSkullId), "Floating Skull", "Caveira Flutuante"),
                new LocalizationEntry(EnemyNameKey(DungeonRules.SkeletonWarriorId), "Copper Skeleton Warrior", "Guerreiro Esqueleto de Cobre"),
                new LocalizationEntry(EnemyNameKey(DungeonRules.SkeletonArcherId), "Skeleton Archer of the Pit", "Arqueiro Esqueleto do Fosso"),
                new LocalizationEntry(EnemyNameKey(DungeonRules.BoneSorcererId), "Bone Sorcerer", "Feiticeiro das Ossadas"),
                new LocalizationEntry(EnemyNameKey(DungeonRules.BoneColossusId), "The Skull Colossus", "O Colosso de Crânios"),

                new LocalizationEntry(ItemNameKey(DungeonRules.BoneKeyItemId), "Runic Bone Key", KeyNamePt),
                new LocalizationEntry(ItemDescriptionKey(DungeonRules.BoneKeyItemId),
                    "Ivory key carved with the Artificers' runes. Opens one barred gate of the Ossuary (and stays in the lock).", KeyDescriptionPt),
                new LocalizationEntry(ItemNameKey(DungeonRules.BoneChestItemId), "Rib Breastplate", ChestNamePt),
                new LocalizationEntry(ItemDescriptionKey(DungeonRules.BoneChestItemId),
                    "The Artificers' ivory breastplate, light as a basket. Ivory of the Artificers set (2 pieces): Spectral Touch.", ChestDescriptionPt),
                new LocalizationEntry(ItemNameKey(DungeonRules.BoneHelmItemId), "Copper Skull Helm", HelmNamePt),
                new LocalizationEntry(ItemDescriptionKey(DungeonRules.BoneHelmItemId),
                    "A copper helm shaped like a grinning skull, lined with wool. Ivory of the Artificers set (2 pieces): Spectral Touch.", HelmDescriptionPt),
                new LocalizationEntry(ItemNameKey(DungeonRules.BoneFragmentItemId), "Ancient Bone Fragment", FragmentNamePt),
                new LocalizationEntry(ItemDescriptionKey(DungeonRules.BoneFragmentItemId), "A sliver of ivory that still glows a little. Crafters pay well for it.", FragmentDescriptionPt),
                new LocalizationEntry(ItemNameKey(DungeonRules.HoledCoinItemId), "Holed Coin of Qocho", CoinNamePt),
                new LocalizationEntry(ItemDescriptionKey(DungeonRules.HoledCoinItemId), "An old coin with a hole in the middle, to wear on a string. Worth a few silvers in town.", CoinDescriptionPt),
                new LocalizationEntry(ItemNameKey(DungeonRules.RustySwordItemId), "Watchman's Rusty Sword", SwordNamePt),
                new LocalizationEntry(ItemDescriptionKey(DungeonRules.RustySwordItemId), "A copper sword of an Ossuary watchman. Old and stained, but it still cuts.", SwordDescriptionPt),
            };

            AddLines(entries, false, new[]
            {
                (CharacterId.Maithe, "Ícaro, stay behind me. And no touching anything that glows... I mean it!", "Ícaro, fica atrás de mim. E nada de mexer no que brilha... é sério!"),
                (CharacterId.Maria, "Poor guardians, they lost their light. Let's help them rest, Joaquim.", "Coitados dos guardiões, perderam a luz. Vamos ajudar eles a descansar, Joaquim."),
                (CharacterId.Joaquim, "Cracked walls? My Tide can open any of them. Stand back, Maria!", "Paredes rachadas? Meu Maremoto abre qualquer uma. Afasta, Maria!"),
                (CharacterId.Icaro, "These are the Artificers' machines of bone! I bet a grenade opens that wall...", "São as máquinas de osso dos Artífices! Aposto que uma granada abre aquela parede..."),
                (CharacterId.Lavine, "Vó Edna told me about this place! The skulls only look scary, right? ...Right?", "A Vó Edna me contou deste lugar! As caveiras só parecem assustadoras, né? ...Né?"),
            });
            AddLines(entries, true, new[]
            {
                (CharacterId.Maithe, "Done. Now they can sleep — and Ícaro owes me one.", "Pronto. Agora eles podem dormir — e o Ícaro me deve uma."),
                (CharacterId.Maria, "Sleep well, Colossus. The Light is gentle with those who rest.", "Dorme bem, Colosso. A Luz é gentil com quem descansa."),
                (CharacterId.Joaquim, "Big one, huh? Wait till I tell Vô Jorge!", "Grandão, hein? Espera eu contar pro Vô Jorge!"),
                (CharacterId.Icaro, "I'm keeping a rib for my next invention. For science!", "Vou guardar uma costela pra minha próxima invenção. Pela ciência!"),
                (CharacterId.Lavine, "We did it! I wasn't scared. Not even a little. Okay, a little.", "Conseguimos! Eu não tive medo. Nem um pouquinho. Tá, um pouquinho."),
            });

            return entries;
        }

        private static void AddLines(List<LocalizationEntry> entries, bool cleared, (CharacterId hero, string en, string pt)[] lines)
        {
            foreach ((CharacterId hero, string en, string pt) in lines)
            {
                entries.Add(new LocalizationEntry(LineKey(cleared, hero), en, pt));
            }
        }

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(DungeonName)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
