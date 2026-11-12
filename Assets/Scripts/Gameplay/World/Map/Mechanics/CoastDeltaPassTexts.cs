using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the sun shadows of Meroë (27), the flooded houses of Kilwa (29), the dhows of Liyongo (30), the root bridges
    /// (32) and the escort of Ammo's caravan (40). Same self-contained pattern as <c>OverworldTexts</c>.
    /// </summary>
    public static class CoastDeltaPassTexts
    {
        public const string ShadowDoor = "ui.route4.shadow_door";
        public const string ShadowHold = "ui.route4.shadow_hold";
        public const string ShadowOpened = "ui.route4.shadow_opened";
        public const string ShadowTaken = "ui.route4.shadow_taken";
        public const string HouseLintel = "ui.route4.house_lintel";
        public const string HouseNoBubble = "ui.route4.house_no_bubble";
        public const string HouseStone = "ui.route4.house_stone";
        public const string HouseRight = "ui.route4.house_right";
        public const string HouseWrong = "ui.route4.house_wrong";
        public const string HouseOpened = "ui.route4.house_opened";
        public const string HouseTaken = "ui.route4.house_taken";
        public const string DhowPrompt = "ui.route4.dhow_prompt";
        public const string DhowDry = "ui.route4.dhow_dry";
        public const string DhowNoGold = "ui.route4.dhow_no_gold";
        public const string DhowSailing = "ui.route4.dhow_sailing";
        public const string DhowLanded = "ui.route4.dhow_landed";
        public const string BridgePrompt = "ui.route4.bridge_prompt";
        public const string BridgeNeedsWater = "ui.route4.bridge_needs_water";
        public const string BridgeGrowing = "ui.route4.bridge_growing";
        public const string BridgeGrown = "ui.route4.bridge_grown";
        public const string BridgeChest = "ui.route4.bridge_chest";
        public const string CaravanPrompt = "ui.route4.caravan_prompt";
        public const string CaravanNight = "ui.route4.caravan_night";
        public const string CaravanPaid = "ui.route4.caravan_paid";
        public const string CaravanStart = "ui.route4.caravan_start";
        public const string CaravanWaits = "ui.route4.caravan_waits";
        public const string CaravanAmbush = "ui.route4.caravan_ambush";
        public const string CaravanAmbushOver = "ui.route4.caravan_ambush_over";
        public const string CaravanAvalanche = "ui.route4.caravan_avalanche";
        public const string CaravanArrived = "ui.route4.caravan_arrived";
        public const string CaravanLost = "ui.route4.caravan_lost";
        public const string Found = "ui.route4.found";
        public const string BagFull = "ui.route4.bag_full";

        /// <summary>The four carved stones: fish, star, moon, shell.</summary>
        public static readonly string[] StoneKeys = { "ui.route4.stone_fish", "ui.route4.stone_star", "ui.route4.stone_moon", "ui.route4.stone_shell" };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(ShadowDoor, "A chapel's door: the pyramid's shadow points here at its hour", "A porta de uma capela: a sombra da pirâmide aponta para cá na hora dela"),
            new LocalizationEntry(ShadowHold, "You stand on the shadow's tip: hold still…", "Você está na ponta da sombra: fique parado…"),
            new LocalizationEntry(ShadowOpened, "The shadow opens the chapel!", "A sombra abre a capela!"),
            new LocalizationEntry(ShadowTaken, "The chapel is empty until tomorrow", "A capela está vazia até amanhã"),
            new LocalizationEntry(HouseLintel, "The lintel shows: {0}, {1}, {2}", "O lintel mostra: {0}, {1}, {2}"),
            new LocalizationEntry(HouseNoBubble, "The house is under water: you need a Bottled Air Bubble", "A casa está debaixo d'água: precisa de uma Bolha de Ar Engarrafada"),
            new LocalizationEntry(HouseStone, "[E] Press the {0} stone", "[E] Apertar a pedra {0}"),
            new LocalizationEntry(HouseRight, "The stone sinks: {0} of 3", "A pedra afunda: {0} de 3"),
            new LocalizationEntry(HouseWrong, "The door won't give: start again", "A porta não cede: comece de novo"),
            new LocalizationEntry(HouseOpened, "The carved door opens!", "A porta entalhada abre!"),
            new LocalizationEntry(HouseTaken, "This house was emptied today", "Esta casa já foi esvaziada hoje"),
            new LocalizationEntry(DhowPrompt, "[E] Sail the canal by dhow (2 gold)", "[E] Descer o canal de dhow (2 de ouro)"),
            new LocalizationEntry(DhowDry, "Low tide: the canal is dry, the dhows wait", "Maré baixa: o canal secou, os dhows esperam"),
            new LocalizationEntry(DhowNoGold, "The boatman wants 2 gold nuggets", "O barqueiro quer 2 pepitas de ouro"),
            new LocalizationEntry(DhowSailing, "The dhow slides down the canal", "O dhow desliza pelo canal"),
            new LocalizationEntry(DhowLanded, "Landed", "Desembarcou"),
            new LocalizationEntry(BridgePrompt, "[E] Water the forgotten bridge", "[E] Regar a ponte esquecida"),
            new LocalizationEntry(BridgeNeedsWater, "The roots are dry: Joaquim's Living Water or a full waterskin would wake them", "As raízes estão secas: a Água Viva do Joaquim ou um odre cheio as acordariam"),
            new LocalizationEntry(BridgeGrowing, "The roots drink and grow: stay close", "As raízes bebem e crescem: fique perto"),
            new LocalizationEntry(BridgeGrown, "The bridge remembers: it grows across the gorge!", "A ponte se lembra: cresce de um lado ao outro da garganta!"),
            new LocalizationEntry(BridgeChest, "[E] Open the root chest", "[E] Abrir o baú da raiz"),
            new LocalizationEntry(CaravanPrompt, "[E] Escort Ammo's caravan through the pass", "[E] Escoltar a caravana do Ammo pelo passo"),
            new LocalizationEntry(CaravanNight, "Ammo: \"Not at night: the pass is closed to caravans\"", "Ammo: \"De noite não: o passo fecha para caravanas\""),
            new LocalizationEntry(CaravanPaid, "Ammo: \"Enough for today, friend. Tomorrow again!\"", "Ammo: \"Por hoje chega, amigo. Amanhã de novo!\""),
            new LocalizationEntry(CaravanStart, "The caravan sets off: keep close to it", "A caravana parte: fique perto dela"),
            new LocalizationEntry(CaravanWaits, "The caravan waits for you", "A caravana espera por você"),
            new LocalizationEntry(CaravanAmbush, "Ambush! Defend the caravan", "Emboscada! Defenda a caravana"),
            new LocalizationEntry(CaravanAmbushOver, "The road is clear: on we go", "O caminho está livre: seguimos"),
            new LocalizationEntry(CaravanAvalanche, "Avalanche! Get away from the head of the caravan", "Avalanche! Saia de perto da frente da caravana"),
            new LocalizationEntry(CaravanArrived, "The caravan crossed the pass! Ammo pays {0} gold nuggets", "A caravana atravessou o passo! O Ammo paga {0} pepitas de ouro"),
            new LocalizationEntry(CaravanLost, "The caravan turns back: the escort is over", "A caravana volta: a escolta acabou"),
            new LocalizationEntry(Found, "You found: {0}", "Você achou: {0}"),
            new LocalizationEntry(BagFull, "Your bag is full", "Sua bolsa está cheia"),
            new LocalizationEntry(StoneKeys[0], "fish", "peixe"),
            new LocalizationEntry(StoneKeys[1], "star", "estrela"),
            new LocalizationEntry(StoneKeys[2], "moon", "lua"),
            new LocalizationEntry(StoneKeys[3], "shell", "concha"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(ShadowDoor))
                {
                    manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
                }

                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            _fallback ??= Entries.ToDictionary(entry => entry.Key, entry => entry.PtBR);
            return key != null && _fallback.TryGetValue(key, out string text) ? text : key;
        }
    }
}
