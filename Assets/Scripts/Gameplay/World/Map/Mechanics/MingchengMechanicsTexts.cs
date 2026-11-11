using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Planície de Mingcheng mechanics. Same self-contained pattern as <see cref="MarGeladoMechanicsTexts"/>.</summary>
    public static class MingchengMechanicsTexts
    {
        public const string ClimbPrompt = "ui.mingcheng.climb_prompt";
        public const string LightPrompt = "ui.mingcheng.light_prompt";
        public const string DescendPrompt = "ui.mingcheng.descend_prompt";
        public const string BeaconLit = "ui.mingcheng.beacon_lit";
        public const string CorridorOn = "ui.mingcheng.corridor_on";
        public const string LookPrompt = "ui.mingcheng.look_prompt";
        public const string LookResting = "ui.mingcheng.look_resting";
        public const string VisionStart = "ui.mingcheng.vision_start";
        public const string VisionEnd = "ui.mingcheng.vision_end";
        public const string ChestPrompt = "ui.mingcheng.chest_prompt";
        public const string ChestFound = "ui.mingcheng.chest_found";
        public const string ChestBagFull = "ui.mingcheng.chest_bag_full";
        public const string ChestEmpty = "ui.mingcheng.chest_empty";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(ClimbPrompt, "[E] Climb the tower to the wall", "[E] Subir a torre até a muralha"),
            new LocalizationEntry(LightPrompt, "[E] Light the beacon", "[E] Acender o farol"),
            new LocalizationEntry(DescendPrompt, "[E] Take the stair down", "[E] Descer pela escada"),
            new LocalizationEntry(BeaconLit, "Beacons lit: {0}", "Faróis acesos: {0}"),
            new LocalizationEntry(CorridorOn, "A corridor of light runs between the beacons", "Um corredor de luz corre entre os faróis"),
            new LocalizationEntry(LookPrompt, "[E] Look into the water", "[E] Olhar a água"),
            new LocalizationEntry(LookResting, "The water is still rippling: wait a little", "A água ainda treme: espere um pouco"),
            new LocalizationEntry(VisionStart, "The reflection shows what the dry world hides", "O reflexo mostra o que o seco esconde"),
            new LocalizationEntry(VisionEnd, "The reflection fades", "O reflexo se apaga"),
            new LocalizationEntry(ChestPrompt, "[E] Open the reflected chest", "[E] Abrir o baú refletido"),
            new LocalizationEntry(ChestFound, "From the reflection: {0}", "Do reflexo: {0}"),
            new LocalizationEntry(ChestBagFull, "The bag is full: {0} sank back", "A mochila está cheia: {0} voltou para a água"),
            new LocalizationEntry(ChestEmpty, "This chest is empty until tomorrow", "Este baú está vazio até amanhã"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(ClimbPrompt))
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

        public static string Format(LocalizationManager manager, string key, object argument) => string.Format(CultureInfo.InvariantCulture, Get(manager, key), argument);
    }
}
