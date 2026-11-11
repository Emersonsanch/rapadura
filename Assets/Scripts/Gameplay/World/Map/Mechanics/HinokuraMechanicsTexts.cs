using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Arquipélago de Hinokura mechanics. Same self-contained pattern as <see cref="RioMechanicsTexts"/>.</summary>
    public static class HinokuraMechanicsTexts
    {
        public const string SluiceOpen = "ui.hinokura.sluice_open";
        public const string SluiceLit = "ui.hinokura.sluice_lit";
        public const string SluiceJammed = "ui.hinokura.sluice_jammed";
        public const string ForgeLit = "ui.hinokura.forge_lit";
        public const string TremorWarn = "ui.hinokura.tremor_warn";
        public const string TremorHit = "ui.hinokura.tremor_hit";
        public const string HeatWarm = "ui.hinokura.heat_warm";
        public const string HeatDanger = "ui.hinokura.heat_danger";
        public const string HeatScorching = "ui.hinokura.heat_scorching";
        public const string HeatGauge = "ui.hinokura.heat_gauge";
        public const string HeatCooled = "ui.hinokura.heat_cooled";
        public const string EruptionHorn = "ui.hinokura.eruption_horn";
        public const string Erupting = "ui.hinokura.erupting";
        public const string EruptionOver = "ui.hinokura.eruption_over";
        public const string GluttonRecoils = "ui.hinokura.glutton_recoils";
        public const string GluttonFed = "ui.hinokura.glutton_fed";

        /// <summary>The forges fed by the sluices, indexed like <c>OverworldLayouts.ForgeSluices</c>.</summary>
        public static readonly string[] ForgeNames =
        {
            "ui.hinokura.forge.east", "ui.hinokura.forge.west", "ui.hinokura.forge.castle",
        };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(SluiceOpen, "[E] Open the sluice: {0}", "[E] Abrir a comporta: {0}"),
            new LocalizationEntry(SluiceLit, "{0} is burning ({1} min left)", "{0} está acesa (faltam {1} min)"),
            new LocalizationEntry(SluiceJammed, "The tremor jammed the sluice: {0} s", "O tremor emperrou a comporta: {0} s"),
            new LocalizationEntry(ForgeLit, "Lava reaches the forge: {0} of 3 lit", "A lava chega à forja: {0} de 3 acesas"),
            new LocalizationEntry(TremorWarn, "The volcano rumbles: a tremor in {0} s", "O vulcão ronca: tremor em {0} s"),
            new LocalizationEntry(TremorHit, "Tremor! The channels shift and {0} goes out", "Tremor! Os canais mudam e {0} se apaga"),
            new LocalizationEntry(HeatWarm, "The air burns: find a cold stone", "O ar queima: procure uma pedra fria"),
            new LocalizationEntry(HeatDanger, "Dangerous heat! Get to a cold stone or away from the lava", "Calor perigoso! Vá a uma pedra fria ou saia de perto da lava"),
            new LocalizationEntry(HeatScorching, "You are scorching: the heat is hurting you", "Você está assando: o calor machuca"),
            new LocalizationEntry(HeatGauge, "Heat: {0}%", "Calor: {0}%"),
            new LocalizationEntry(HeatCooled, "The cold stone drinks the heat", "A pedra fria bebe o calor"),
            new LocalizationEntry(EruptionHorn, "The shepherds' horn! Lava down the marked route in {0} s", "A buzina dos pastores! Lava na rota marcada em {0} s"),
            new LocalizationEntry(Erupting, "The crater spits! Leave the marked route", "A cratera cospe! Saia da rota marcada"),
            new LocalizationEntry(EruptionOver, "The eruption dies down", "A erupção se acalma"),
            new LocalizationEntry(GluttonRecoils, "The forges burn: the Glutão recoils ({0} of 3)", "As forjas ardem: o Glutão recua ({0} de 3)"),
            new LocalizationEntry(GluttonFed, "The forges are out and the Glutão feeds: open the sluices", "As forjas estão apagadas e o Glutão se farta: abra as comportas"),
            new LocalizationEntry(ForgeNames[0], "the East Channel Forge", "a Forja do Canal Leste"),
            new LocalizationEntry(ForgeNames[1], "the West Channel Forge", "a Forja do Canal Oeste"),
            new LocalizationEntry(ForgeNames[2], "the Court Furnace", "a Fornalha do Pátio"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(SluiceOpen))
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
