using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Tutorial;

namespace Rapadura.UI.Tutorial
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the first-time hints (<see cref="TutorialSteps"/>), the hint
    /// bubble (<see cref="TutorialHintController"/>) and the "Mostrar dicas"/"Reiniciar tutorial"
    /// settings (<see cref="TutorialSettingsSection"/>). Same self-contained pattern as
    /// <c>ReturnToTitleTexts</c>: merged into the live <see cref="LocalizationManager"/> on first use
    /// (keys it already has win), pt-BR fallback when no manager is registered. Written for young
    /// players: short sentences, "toque" first (touch-first game).
    /// </summary>
    public static class TutorialTexts
    {
        public const string Header = "ui.tutorial.header";
        public const string GotIt = "ui.tutorial.got_it";
        public const string SettingsShowHints = "ui.tutorial.settings.show_hints";
        public const string SettingsReset = "ui.tutorial.settings.reset";
        public const string SettingsResetDone = "ui.tutorial.settings.reset_done";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Header, "Tip", "Dica"),
            new LocalizationEntry(GotIt, "Got it!", "Entendi!"),
            new LocalizationEntry(SettingsShowHints, "Show tips", "Mostrar dicas"),
            new LocalizationEntry(SettingsReset, "Restart tutorial", "Reiniciar tutorial"),
            new LocalizationEntry(SettingsResetDone, "The tips will show up again.", "As dicas vão aparecer de novo."),

            Title(TutorialSteps.WorldMap, "Time to travel!", "Hora de viajar!"),
            Body(TutorialSteps.WorldMap,
                "Tap a region connected to yours to walk there. Enemies may show up on the way!",
                "Toque numa região ligada à sua para caminhar até lá. Inimigos podem aparecer no caminho!"),

            Title(TutorialSteps.Encounter, "Get ready to fight!", "Prepare-se para lutar!"),
            Body(TutorialSteps.Encounter,
                "Tap the ground to walk and tap an enemy to attack. On a keyboard, use WASD to move.",
                "Toque no chão para andar e toque num inimigo para atacar. No teclado, use WASD para andar."),

            Title(TutorialSteps.Skills, "Special powers", "Poderes especiais"),
            Body(TutorialSteps.Skills,
                "Your powers are on the bar at the bottom. Tap one to use it when it is ready.",
                "Seus poderes ficam na barra de baixo. Toque num deles para usar quando estiver pronto."),

            Title(TutorialSteps.Loot, "You found something!", "Você achou algo!"),
            Body(TutorialSteps.Loot,
                "Everything you pick up goes into your Backpack. Open it to see and equip your items.",
                "Tudo o que você pega vai para a Mochila. Abra para ver e equipar seus itens."),

            Title(TutorialSteps.Quest, "New quest!", "Nova missão!"),
            Body(TutorialSteps.Quest,
                "Check your quests and their rewards with the Quests button.",
                "Veja suas missões e as recompensas no botão Missões."),

            Title(TutorialSteps.LevelUp, "Level up!", "Você subiu de nível!"),
            Body(TutorialSteps.LevelUp,
                "You earned attribute and skill points to get stronger. Open Skills to use them.",
                "Você ganhou pontos de atributo e de skill para ficar mais forte. Abra Skills para usá-los."),

            Title(TutorialSteps.Death, "Don't give up!", "Não desista!"),
            Body(TutorialSteps.Death,
                "Every hero falls sometimes. Continue from the last safe spot or load a saved game.",
                "Todo herói cai às vezes. Continue do último ponto seguro ou carregue um jogo salvo."),

            Title(TutorialSteps.Night, "Night has fallen", "A noite chegou"),
            Body(TutorialSteps.Night,
                "At night the sertão gets dark. Watch your step: the sun will be back soon!",
                "À noite o sertão fica escuro. Cuidado com o caminho: logo o sol volta!"),

            Title(TutorialSteps.Crafting, "You can craft something!", "Dá para criar algo!"),
            Body(TutorialSteps.Crafting,
                "You have the materials for a recipe. Open Crafting in the menu or walk up to a workbench.",
                "Você tem os materiais de uma receita. Abra Criação no menu ou chegue perto de uma bancada."),
        };

        private static LocalizationEntry Title(string stepId, string en, string ptBR) => new LocalizationEntry(TutorialSteps.TitleKey(stepId), en, ptBR);
        private static LocalizationEntry Body(string stepId, string en, string ptBR) => new LocalizationEntry(TutorialSteps.BodyKey(stepId), en, ptBR);

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(GotIt)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
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

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
