using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the route mechanics of <see cref="RouteMechanicsDriver"/>: Ammo's caravan in the storm (07), the monsoon and the
    /// boats of Jhilpur (31), the lifts and chutes of Sangkent (36), the drum's barges (41) and the locks of the Canal das Lanternas (43). Same
    /// self-contained pattern as <see cref="ProfundoPathsTexts"/>.
    /// </summary>
    public static class RouteMechanicsTexts
    {
        public const string CaravanPrompt = "ui.route_mech.caravan_prompt";
        public const string CaravanPromptPaid = "ui.route_mech.caravan_prompt_paid";
        public const string CaravanWaitsSand = "ui.route_mech.caravan_waits_sand";
        public const string CaravanStart = "ui.route_mech.caravan_start";
        public const string CaravanWaits = "ui.route_mech.caravan_waits";
        public const string CaravanWarning = "ui.route_mech.caravan_warning";
        public const string CaravanKneels = "ui.route_mech.caravan_kneels";
        public const string CaravanShelter = "ui.route_mech.caravan_shelter";
        public const string CaravanAmbush = "ui.route_mech.caravan_ambush";
        public const string CaravanArrived = "ui.route_mech.caravan_arrived";
        public const string CaravanArrivedPaid = "ui.route_mech.caravan_arrived_paid";
        public const string CaravanLost = "ui.route_mech.caravan_lost";
        public const string BagFull = "ui.route_mech.bag_full";

        public const string MonsoonStarts = "ui.route_mech.monsoon_starts";
        public const string MonsoonEnds = "ui.route_mech.monsoon_ends";
        public const string FordFlooded = "ui.route_mech.ford_flooded";
        public const string FordFloodedNoBoat = "ui.route_mech.ford_flooded_no_boat";
        public const string MooringRow = "ui.route_mech.mooring_row";
        public const string MooringFerry = "ui.route_mech.mooring_ferry";
        public const string MooringShallow = "ui.route_mech.mooring_shallow";
        public const string FerryComing = "ui.route_mech.ferry_coming";
        public const string FerryLeft = "ui.route_mech.ferry_left";
        public const string Crossed = "ui.route_mech.crossed";
        public const string SchoolLearn = "ui.route_mech.school_learn";
        public const string SchoolLesson = "ui.route_mech.school_lesson";
        public const string SchoolLeft = "ui.route_mech.school_left";
        public const string SchoolLearned = "ui.route_mech.school_learned";
        public const string RunPrompt = "ui.route_mech.run_prompt";
        public const string RunBackPrompt = "ui.route_mech.run_back_prompt";
        public const string RunNotMonsoon = "ui.route_mech.run_not_monsoon";
        public const string RunNeedsLesson = "ui.route_mech.run_needs_lesson";
        public const string RunGo = "ui.route_mech.run_go";

        public const string LiftPrompt = "ui.route_mech.lift_prompt";
        public const string LiftQueued = "ui.route_mech.lift_queued";
        public const string LiftLeftQueue = "ui.route_mech.lift_left_queue";
        public const string LiftUp = "ui.route_mech.lift_up";
        public const string ChutePrompt = "ui.route_mech.chute_prompt";
        public const string ChuteNightPrompt = "ui.route_mech.chute_night_prompt";
        public const string ChuteDown = "ui.route_mech.chute_down";
        public const string TerraceAvalanche = "ui.route_mech.terrace_avalanche";
        public const string TerraceArrived = "ui.route_mech.terrace_arrived";

        public const string BargePrompt = "ui.route_mech.barge_prompt";
        public const string BargeNext = "ui.route_mech.barge_next";
        public const string BargeNight = "ui.route_mech.barge_night";
        public const string BargeBoarded = "ui.route_mech.barge_boarded";
        public const string BargeLeft = "ui.route_mech.barge_left";
        public const string BargeGo = "ui.route_mech.barge_go";
        public const string BargeArrived = "ui.route_mech.barge_arrived";
        public const string LockTurn = "ui.route_mech.lock_turn";
        public const string LockCross = "ui.route_mech.lock_cross";
        public const string LockShutNow = "ui.route_mech.lock_shut_now";
        public const string LockBarge = "ui.route_mech.lock_barge";
        public const string LockBell = "ui.route_mech.lock_bell";

        public const string ZoneBlocked = "ui.route_mech.zone_blocked";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(CaravanPrompt, "[E] Escort Ammo's caravan to Qocho: the sand is coming", "[E] Escoltar a caravana do Ammo até Qocho: a areia vem aí"),
            new LocalizationEntry(CaravanPromptPaid, "[E] Escort Ammo's caravan to Qocho (he has paid today's escort already)",
                "[E] Escoltar a caravana do Ammo até Qocho (ele já pagou a escolta de hoje)"),
            new LocalizationEntry(CaravanWaitsSand, "Ammo waits for the sand: the caravan sets out in {0} min", "O Ammo espera a areia: a caravana sai em {0} min"),
            new LocalizationEntry(CaravanStart, "Ammo: Keep close. When the sand blows, walk right beside the camels.",
                "Ammo: Fique perto. Quando a areia soprar, ande colado nos camelos."),
            new LocalizationEntry(CaravanWaits, "The caravan waits for you", "A caravana espera você"),
            new LocalizationEntry(CaravanWarning, "Ammo: Here comes the sand! Stay by the camels", "Ammo: Lá vem a areia! Fique junto dos camelos"),
            new LocalizationEntry(CaravanKneels, "The sand blows and the camels kneel: walk beside them and the caravan goes on",
                "A areia sopra e os camelos ajoelham: ande ao lado deles e a caravana segue"),
            new LocalizationEntry(CaravanShelter, "The camels break the wind: the sand does not wear you here", "Os camelos cortam o vento: a areia não gasta você aqui"),
            new LocalizationEntry(CaravanAmbush, "The storm brought the beasts: they go for the caravan!", "A tempestade trouxe os bichos: eles vêm para cima da caravana!"),
            new LocalizationEntry(CaravanArrived, "The caravan reaches Qocho! Ammo pays {0} gold nuggets", "A caravana chegou a Qocho! O Ammo paga {0} pepitas de ouro"),
            new LocalizationEntry(CaravanArrivedPaid, "The caravan reaches Qocho. Ammo thanks you (today's pay is already given)",
                "A caravana chegou a Qocho. O Ammo agradece (a paga de hoje já saiu)"),
            new LocalizationEntry(CaravanLost, "You left the caravan behind: it turns back to its camp", "Você deixou a caravana para trás: ela volta para o acampamento"),
            new LocalizationEntry(BagFull, "The bag is full", "A bolsa está cheia"),

            new LocalizationEntry(MonsoonStarts, "The monsoon is here: the water rises and the fords are gone", "Chegou a monção: a água sobe e os vaus somem"),
            new LocalizationEntry(MonsoonEnds, "The monsoon is over: the fords are shallow again", "A monção passou: os vaus voltam a dar pé"),
            new LocalizationEntry(FordFlooded, "The monsoon covered the ford: the current pushes you back. Take a boat at the mooring on the road",
                "A monção cobriu o vau: a correnteza empurra você de volta. Pegue um barco no cais da estrada"),
            new LocalizationEntry(FordFloodedNoBoat, "The monsoon covered this ford: take the boats of the road fords or the root bridge",
                "A monção cobriu este vau: use os barcos dos vaus da estrada ou a ponte de raiz"),
            new LocalizationEntry(MooringRow, "[E] Row over to the other bank", "[E] Remar até a outra margem"),
            new LocalizationEntry(MooringFerry, "[E] Call Nuhzadag the ferryman (he comes over and rows slowly). Abiesous teaches rowing in Jhilpur",
                "[E] Chamar o Nuhzadag, o barqueiro (ele vem e rema devagar). O Abiesous ensina a remar em Jhilpur"),
            new LocalizationEntry(MooringShallow, "Out of the monsoon the ford is shallow: wade it (the monsoon comes in {0} day(s))",
                "Fora da monção o vau dá pé: atravesse a pé (a monção chega em {0} dia(s))"),
            new LocalizationEntry(FerryComing, "Nuhzadag is coming over for you... stay on the mooring", "O Nuhzadag vem buscar você... fique no cais"),
            new LocalizationEntry(FerryLeft, "You left the mooring: Nuhzadag rows back", "Você saiu do cais: o Nuhzadag volta"),
            new LocalizationEntry(Crossed, "On the other side!", "Do outro lado!"),
            new LocalizationEntry(SchoolLearn, "[E] Take Abiesous's rowing lesson ({0} s)", "[E] Ter a aula de remo do Abiesous ({0} s)"),
            new LocalizationEntry(SchoolLesson, "Abiesous: Pull with your back, not your arms... again...", "Abiesous: Puxe com as costas, não com os braços... de novo..."),
            new LocalizationEntry(SchoolLeft, "You left the lesson before the end", "Você saiu da aula antes do fim"),
            new LocalizationEntry(SchoolLearned, "Now you row alone: in the monsoon the fords and the canal are yours",
                "Agora você rema sozinho: na monção, os vaus e o canal são seus"),
            new LocalizationEntry(RunPrompt, "[E] Row down the canal to the Tarchon ford: the monsoon lifts the water over the banks",
                "[E] Descer o canal de barco até o vau de Tarchon: a monção levanta a água por cima dos bancos"),
            new LocalizationEntry(RunBackPrompt, "[E] Row back up the canal to Jhilpur", "[E] Subir o canal de barco de volta a Jhilpur"),
            new LocalizationEntry(RunNotMonsoon, "Out of the monsoon the canal is too shallow for a boat (the monsoon comes in {0} day(s))",
                "Fora da monção o canal é raso demais para o barco (a monção chega em {0} dia(s))"),
            new LocalizationEntry(RunNeedsLesson, "Only a rower takes the canal: have Abiesous's lesson at his school by the root bridge",
                "Só desce o canal quem sabe remar: tenha a aula do Abiesous na escola junto da ponte de raiz"),
            new LocalizationEntry(RunGo, "The high water takes the boat down the canal", "A água alta leva o barco canal abaixo"),

            new LocalizationEntry(LiftPrompt, "[E] Queue for the rope lift (the basket goes up in {0} s)", "[E] Entrar na fila do elevador de corda (o cesto sobe em {0} s)"),
            new LocalizationEntry(LiftQueued, "In the queue: the basket goes up in {0} s. Stay here", "Na fila: o cesto sobe em {0} s. Fique aqui"),
            new LocalizationEntry(LiftLeftQueue, "You left the lift's queue", "Você saiu da fila do elevador"),
            new LocalizationEntry(LiftUp, "The basket climbs the terraces", "O cesto sobe pelos terraços"),
            new LocalizationEntry(ChutePrompt, "[E] Slide down the stone chute to the 1st terrace", "[E] Descer pela calha de pedra até o 1º terraço"),
            new LocalizationEntry(ChuteNightPrompt, "[E] Slide down the frozen chute to the 1st terrace (faster at night)",
                "[E] Descer pela calha gelada até o 1º terraço (mais rápida à noite)"),
            new LocalizationEntry(ChuteDown, "Down the chute you go!", "Lá vai você calha abaixo!"),
            new LocalizationEntry(TerraceAvalanche, "Kustai's bell rings for the avalanche: lifts and chutes stand still",
                "O sino do Kustai toca a avalanche: elevadores e calhas parados"),
            new LocalizationEntry(TerraceArrived, "Terrace {0}", "{0}º terraço"),

            new LocalizationEntry(BargePrompt, "[E] Board the drum's barge (it leaves at {0}:00)", "[E] Embarcar na barca do tambor (sai às {0}h)"),
            new LocalizationEntry(BargeNext, "Next barge of the drum at {0}:00", "Próxima barca do tambor às {0}h"),
            new LocalizationEntry(BargeNight, "The bell has rung: no barge until the drum at 6:00. At night the locks stay shut: cross over their gates",
                "O sino tocou: nenhuma barca até o tambor das 6h. À noite as eclusas ficam fechadas: atravesse por cima das comportas"),
            new LocalizationEntry(BargeBoarded, "On board: the barge leaves at {0}:00 with the drum. Stay on the landing",
                "A bordo: a barca sai às {0}h com o tambor. Fique no cais"),
            new LocalizationEntry(BargeLeft, "You stepped off the barge before it left", "Você desceu da barca antes de ela sair"),
            new LocalizationEntry(BargeGo, "The drum beats and the barge pulls away", "O tambor bate e a barca sai"),
            new LocalizationEntry(BargeArrived, "The barge comes alongside the landing", "A barca encosta no cais"),
            new LocalizationEntry(LockTurn, "[E] Turn Gabryab's wheel and shut the lock (shut for {0} s)", "[E] Girar a roda do Gabryab e fechar a eclusa (fica fechada {0} s)"),
            new LocalizationEntry(LockCross, "[E] Cross over the shut gate", "[E] Atravessar por cima da comporta fechada"),
            new LocalizationEntry(LockShutNow, "The gate swings shut and the water rises behind it: walk over on top", "A comporta fecha e a água sobe atrás dela: dá para passar por cima"),
            new LocalizationEntry(LockBarge, "A barge is in the lock: Gabryab keeps the gate open", "Uma barca está na eclusa: o Gabryab deixa a comporta aberta"),
            new LocalizationEntry(LockBell, "The bell shut the gates: at night every lock is a way across", "O sino fechou as comportas: à noite toda eclusa é passagem"),

            new LocalizationEntry(ZoneBlocked, "The other end is still closed to you", "O outro lado ainda está fechado para você"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(CaravanPrompt))
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

        public static string Format(LocalizationManager manager, string key, params object[] arguments) => string.Format(CultureInfo.InvariantCulture, Get(manager, key), arguments);
    }
}
