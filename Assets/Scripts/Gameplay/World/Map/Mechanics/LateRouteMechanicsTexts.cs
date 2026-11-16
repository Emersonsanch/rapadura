using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the late route mechanics of <see cref="LateRouteMechanicsDriver"/>: the walks along the bottom of Kirta's reef
    /// (28), the tunnels of the Galerias dos Mamutes (62), the floes of the Mar das Auroras (63), the sledge of the Areal Escondido (73), the dark
    /// of the Cova da Treva (88) and the lanterns between the barques (97). Same self-contained pattern as <see cref="RouteMechanicsTexts"/>.
    /// </summary>
    public static class LateRouteMechanicsTexts
    {
        public const string DivePromptNorth = "ui.late_route.dive_prompt_north";
        public const string DivePromptSouth = "ui.late_route.dive_prompt_south";
        public const string DivePromptBack = "ui.late_route.dive_prompt_back";
        public const string DiveNoAir = "ui.late_route.dive_no_air";
        public const string DiveSharks = "ui.late_route.dive_sharks";
        public const string DiveDown = "ui.late_route.dive_down";
        public const string DiveBreath = "ui.late_route.dive_breath";
        public const string DiveUp = "ui.late_route.dive_up";

        public const string TunnelPrompt = "ui.late_route.tunnel_prompt";
        public const string TunnelCaved = "ui.late_route.tunnel_caved";
        public const string TunnelDown = "ui.late_route.tunnel_down";
        public const string TunnelOut = "ui.late_route.tunnel_out";
        public const string RoadSouth = "ui.late_route.road_south";
        public const string RoadEast = "ui.late_route.road_east";
        public const string RoadNorth = "ui.late_route.road_north";

        public const string FloePrompt = "ui.late_route.floe_prompt";
        public const string FloePromptNight = "ui.late_route.floe_prompt_night";
        public const string FloeDrifting = "ui.late_route.floe_drifting";
        public const string FloeDance = "ui.late_route.floe_dance";
        public const string FloeHop = "ui.late_route.floe_hop";
        public const string FloeArrived = "ui.late_route.floe_arrived";

        public const string SledgePrompt = "ui.late_route.sledge_prompt";
        public const string SledgeBackPrompt = "ui.late_route.sledge_back_prompt";
        public const string SledgeSerpentPrompt = "ui.late_route.sledge_serpent_prompt";
        public const string SledgeHaul = "ui.late_route.sledge_haul";
        public const string SledgeSerpent = "ui.late_route.sledge_serpent";
        public const string SledgeDoor = "ui.late_route.sledge_door";
        public const string SledgeArrived = "ui.late_route.sledge_arrived";
        public const string SledgePaid = "ui.late_route.sledge_paid";

        public const string DarkEnter = "ui.late_route.dark_enter";
        public const string DarkMaria = "ui.late_route.dark_maria";
        public const string DarkHit = "ui.late_route.dark_hit";
        public const string StandPrompt = "ui.late_route.stand_prompt";
        public const string StandBurning = "ui.late_route.stand_burning";
        public const string StandLit = "ui.late_route.stand_lit";

        public const string RackPrompt = "ui.late_route.rack_prompt";
        public const string RackDone = "ui.late_route.rack_done";
        public const string LanternTaken = "ui.late_route.lantern_taken";
        public const string LanternFlame = "ui.late_route.lantern_flame";
        public const string LanternOut = "ui.late_route.lantern_out";
        public const string LanternCarried = "ui.late_route.lantern_carried";
        public const string SunStandPrompt = "ui.late_route.sun_stand_prompt";
        public const string SunStandEmpty = "ui.late_route.sun_stand_empty";
        public const string LanternSet = "ui.late_route.lantern_set";
        public const string VoyageDone = "ui.late_route.voyage_done";

        public const string ZoneBlocked = "ui.late_route.zone_blocked";
        public const string BagFull = "ui.late_route.bag_full";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(DivePromptNorth, "[E] Go down and walk the bottom of the lagoon to the coral islets' ford",
                "[E] Descer e andar pelo fundo da lagoa até o vau das ilhotas de coral"),
            new LocalizationEntry(DivePromptSouth, "[E] Go down and walk the bottom over the abyss to the great lagoon's trail",
                "[E] Descer e andar pelo fundo sobre o abismo até a trilha da lagoa grande"),
            new LocalizationEntry(DivePromptBack, "[E] Go down and walk the bottom back to Kirta", "[E] Descer e andar pelo fundo de volta a Kirta"),
            new LocalizationEntry(DiveNoAir, "Too short a breath: the longest stretch between two bubbles takes {0} s and you hold {1} s (Pughat's Breathing Shell and Danel's Air Bubble help)",
                "Fôlego curto: o trecho mais longo entre duas bolhas pede {0} s e você aguenta {1} s (a Concha Respirável do Pughat e a Bolha de Ar do Danel ajudam)"),
            new LocalizationEntry(DiveSharks, "At night the mud sharks circle the abyss: the south bottom is closed until day",
                "À noite os tubarões-de-lodo rondam o abismo: o fundo do sul fica fechado até o dia"),
            new LocalizationEntry(DiveDown, "You sink to the coral and walk along the bottom", "Você afunda até o coral e anda pelo fundo"),
            new LocalizationEntry(DiveBreath, "An air bubble: a full breath", "Uma bolha de ar: fôlego cheio"),
            new LocalizationEntry(DiveUp, "You come up to the surface", "Você sobe à tona"),

            new LocalizationEntry(TunnelPrompt, "[E] Go down the mammoth's tunnel: this week it comes out on the {0}",
                "[E] Descer no túnel do mamute: esta semana ele sai na {0}"),
            new LocalizationEntry(TunnelCaved, "The mammoth dug through here: this mouth caved in. The tunnels change in {0} d {1} h",
                "O mamute cavou por aqui: esta boca desabou. Os túneis mudam em {0} d {1} h"),
            new LocalizationEntry(TunnelDown, "Into the dark of the gallery, along the mammoth's tunnel", "Para o escuro da galeria, pelo túnel do mamute"),
            new LocalizationEntry(TunnelOut, "You come out on the {0}", "Você sai na {0}"),
            new LocalizationEntry(RoadSouth, "south road", "estrada do sul"),
            new LocalizationEntry(RoadEast, "east road", "estrada do leste"),
            new LocalizationEntry(RoadNorth, "north road", "estrada do norte"),

            new LocalizationEntry(FloePrompt, "[E] Hop the floes to the other side (the aurora holds the way for {0} s)",
                "[E] Pular as placas até o outro lado (a aurora segura a rota por {0} s)"),
            new LocalizationEntry(FloePromptNight, "[E] Hop the floes to the other side: at night they stand still in line",
                "[E] Pular as placas até o outro lado: à noite elas param em linha"),
            new LocalizationEntry(FloeDrifting, "The floes drift apart: the aurora dances in {0}:{1:00}", "As placas se afastam: a aurora dança em {0}:{1:00}"),
            new LocalizationEntry(FloeDance, "The aurora dances: the floes swing into line", "A aurora dança: as placas se alinham"),
            new LocalizationEntry(FloeHop, "You hop from floe to floe", "Você pula de placa em placa"),
            new LocalizationEntry(FloeArrived, "Across, on firm ice", "Do outro lado, no gelo firme"),

            new LocalizationEntry(SledgePrompt, "[E] Take the sledge's rope and haul it down through the Closed Gates",
                "[E] Pegar a corda do trenó e puxá-lo pelas Portas Fechadas"),
            new LocalizationEntry(SledgeBackPrompt, "[E] Take the sledge's rope and haul it back to the Sand Sled", "[E] Pegar a corda do trenó e puxá-lo de volta ao Trenó de Areia"),
            new LocalizationEntry(SledgeSerpentPrompt, "[E] Board the sledge: the Great Serpent pulls it", "[E] Subir no trenó: a Grande Serpente puxa"),
            new LocalizationEntry(SledgeHaul, "Tap [E] to heave on the rope: alone the sledge crawls", "Toque [E] para puxar a corda: sozinho o trenó anda devagar"),
            new LocalizationEntry(SledgeSerpent, "The Great Serpent remembers she pulls the barque and sets off", "A Grande Serpente lembra que puxava a barca e arranca"),
            new LocalizationEntry(SledgeDoor, "The stone door glows as the barque goes through", "A porta de pedra brilha quando a barca passa"),
            new LocalizationEntry(SledgeArrived, "The sledge stops at the end of the road", "O trenó para no fim da estrada"),
            new LocalizationEntry(SledgePaid, "The sledge-masters pay the run: {0} gold", "Os mestres do trenó pagam a viagem: {0} de ouro"),

            new LocalizationEntry(DarkEnter, "The Dark: out of the light the beasts take 30% less damage. Fight by a lamp",
                "Treva: fora da luz os bichos levam 30% menos dano. Lute perto de uma lâmpada"),
            new LocalizationEntry(DarkMaria, "Maria's light opens the dark around her", "A luz da Maria abre a treva em volta dela"),
            new LocalizationEntry(DarkHit, "The blow is lost in the dark: light the beast (a lamp, a light skill)",
                "O golpe se perde na treva: ilumine o bicho (uma lâmpada, uma skill de luz)"),
            new LocalizationEntry(StandPrompt, "[E] Light the lamp (it lights 16 m for 10 min)", "[E] Acender a lâmpada (ilumina 16 m por 10 min)"),
            new LocalizationEntry(StandBurning, "The lamp burns for {0} min more", "A lâmpada queima por mais {0} min"),
            new LocalizationEntry(StandLit, "The lamp catches: the dark draws back", "A lâmpada acende: a treva recua"),

            new LocalizationEntry(RackPrompt, "[E] Take a moon lantern to the Sun Ship ({0}/5 today)", "[E] Pegar uma lanterna da lua para a Barca do Sol ({0}/5 hoje)"),
            new LocalizationEntry(RackDone, "Today's voyage of the light is complete: come back tomorrow", "A viagem da luz de hoje está completa: volte amanhã"),
            new LocalizationEntry(LanternTaken, "The lantern is lit: walk it to the Stern Cabin (the flame lasts {0} s; blows shake it, rides put it out)",
                "A lanterna acende: leve-a andando até a Cabine da Popa (a chama dura {0} s; golpes a sacodem, caronas a apagam)"),
            new LocalizationEntry(LanternFlame, "The lantern's flame: {0} s", "A chama da lanterna: {0} s"),
            new LocalizationEntry(LanternOut, "The lantern went out: take another at the Moon Ship", "A lanterna apagou: pegue outra na Barca da Lua"),
            new LocalizationEntry(LanternCarried, "The light does not travel by lift: the lantern went out", "A luz não viaja de carona: a lanterna apagou"),
            new LocalizationEntry(SunStandPrompt, "[E] Set the lantern on the Sun Ship's stand", "[E] Pôr a lanterna no suporte da Barca do Sol"),
            new LocalizationEntry(SunStandEmpty, "The stand waits for a lantern from the Moon Ship", "O suporte espera uma lanterna da Barca da Lua"),
            new LocalizationEntry(LanternSet, "The lantern is on the Sun's deck: {0} gold ({1}/5 today)", "A lanterna está no convés do Sol: {0} de ouro ({1}/5 hoje)"),
            new LocalizationEntry(VoyageDone, "Five lanterns: today's voyage of the light is complete and the sail takes the right wind",
                "Cinco lanternas: a viagem da luz de hoje se completa e a vela pega o vento certo"),

            new LocalizationEntry(ZoneBlocked, "The other end is still closed to you", "O outro lado ainda está fechado para você"),
            new LocalizationEntry(BagFull, "Your bag is full", "Sua bolsa está cheia"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(DivePromptNorth))
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
