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

            Title(TutorialSteps.WorldMap, "Time for adventure!", "Hora da aventura!"),
            Body(TutorialSteps.WorldMap,
                "Tap Explore to look for enemies here. Win fights to open new regions, then tap a connected region to walk there.",
                "Toque em Explorar para procurar inimigos aqui. Vença lutas para abrir novas regiões e toque numa região ligada para ir até lá."),

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
                "You earned attribute and skill points to get stronger. Tap Sheet to spend your attribute points; skill points go in Skills.",
                "Você ganhou pontos de atributo e de skill para ficar mais forte. Toque em Ficha para distribuir os atributos; os pontos de skill ficam em Skills."),

            Title(TutorialSteps.Death, "Don't give up!", "Não desista!"),
            Body(TutorialSteps.Death,
                "Every hero falls sometimes. Continue from the last safe spot or load a saved game.",
                "Todo herói cai às vezes. Continue do último ponto seguro ou carregue um jogo salvo."),

            Title(TutorialSteps.Night, "Night has fallen", "A noite chegou"),
            Body(TutorialSteps.Night,
                "At night Dawn Isle gets dark and the critters get fiercer: more of them come, in bigger and faster waves, and some have glowing eyes. They are tougher, but give more XP and loot. Your lamp lights the way; the sun will be back soon!",
                "À noite a Ilha da Aurora fica escura e os bichos ficam mais bravos: vêm mais, em ondas maiores e mais rápidas, e alguns têm olhos brilhando. São mais fortes, mas dão mais XP e mais saque. Sua lamparina ilumina o caminho; logo o sol volta!"),

            Title(TutorialSteps.Crafting, "You can craft something!", "Dá para criar algo!"),
            Body(TutorialSteps.Crafting,
                "You have the materials for a recipe. Open Crafting in the menu or walk up to a workbench.",
                "Você tem os materiais de uma receita. Abra Criação no menu ou chegue perto de uma bancada."),

            Title(TutorialSteps.RegionNpc, "Someone to talk to", "Tem gente para conversar"),
            Body(TutorialSteps.RegionNpc,
                "Talk to the locals! Tap \"Talk to…\": they have quests, rewards and good advice about the region's boss.",
                "Converse com os moradores! Toque em \"Conversar com…\": eles têm missões, recompensas e bons conselhos sobre o chefe da região."),

            Title(TutorialSteps.Village, "Grandma Edna's village", "A vila da Vó Edna"),
            Body(TutorialSteps.Village,
                "Tap Village to visit the family: chat with the villagers, buy at the Shop and craft at the Workbench.",
                "Toque em Vila para visitar a família: converse com os moradores, compre na Loja e crie itens na Bancada."),

            Title(TutorialSteps.Materials, "Materials!", "Materiais!"),
            Body(TutorialSteps.Materials,
                "Wood, fiber, ore... each region drops its own. Keep them in your Backpack: they build items at the Village Workbench and finish gathering quests.",
                "Madeira, fibra, minério... cada região dá os seus. Guarde na Mochila: eles viram itens na Bancada da Vila e completam missões de coleta."),

            Title(TutorialSteps.Stun, "Stunned!", "Atordoado!"),
            Body(TutorialSteps.Stun,
                "A stunned enemy stops and loses its attack. Hit hard while it's dizzy! Bosses shake it off twice as fast.",
                "Inimigo atordoado para e perde o ataque. Aproveite para bater enquanto ele está tonto! Chefes se recuperam na metade do tempo."),
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
