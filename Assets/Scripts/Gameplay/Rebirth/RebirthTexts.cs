using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Rebirth
{
    /// <summary>
    /// Localization (en + pt-BR) of the "Volta da Luz" (reset) and the "Subida da Coluna" (grand reset): refusals, the
    /// kept/cleared parts, titles and cosmetics of each milestone, the summary/HUD lines, and the lines of Vó Edna (with the
    /// Vô Jorge remembered, bible §1.7) and of the five cousins. Tone of bible §0/§4: a story told on the porch, no
    /// religious words. Same self-contained pattern as <c>PvpTexts</c>: merged into the live <see cref="LocalizationManager"/>
    /// on first use (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class RebirthTexts
    {
        public const string ResetName = "rebirth.ui.reset_name";
        public const string GrandResetName = "rebirth.ui.grand_name";

        /// <summary>"Nível {0} → 1 · Ouro: {1} · Pontos de luz: +{2} (total {3})"</summary>
        public const string Summary = "rebirth.ui.summary";

        /// <summary>"Pede nível {0} e {1} de ouro, na {2}."</summary>
        public const string RequirementLine = "rebirth.ui.requirement";

        /// <summary>"A luz ainda está voltando: mais {0} h."</summary>
        public const string CooldownLine = "rebirth.ui.cooldown";

        /// <summary>"Eco da Luz: XP de caça ×{0} até o nível {1}."</summary>
        public const string EchoStatus = "rebirth.ui.echo";

        /// <summary>"Voltas da Luz: {0} · Degraus da Coluna: {1}"</summary>
        public const string CountLine = "rebirth.ui.count";

        public const string LineOffer = "rebirth.line.offer";
        public const string LineAfter = "rebirth.line.after";
        public const string LineEcho = "rebirth.line.echo";
        public const string LineGrandOffer = "rebirth.line.grand_offer";
        public const string LineGrandAfter = "rebirth.line.grand_after";
        public const string LineAllHome = "rebirth.line.all_home";

        public static string RefusalKey(RebirthRefusal reason) => "rebirth.refusal." + Snake(reason.ToString());

        public static string PartKey(RebirthPart part) => "rebirth.part." + Snake(part.ToString());

        public static string TitleKey(string titleId) => "rebirth.title." + titleId;

        public static string CosmeticKey(string cosmeticId) => "rebirth.cosmetic." + cosmeticId;

        public static string CousinResetLineKey(CharacterId cousin) => "rebirth.line.reset." + Snake(cousin.ToString());

        public static string CousinGrandLineKey(CharacterId cousin) => "rebirth.line.grand." + Snake(cousin.ToString());

        private static readonly (RebirthRefusal Reason, string En, string Pt)[] RefusalTexts =
        {
            (RebirthRefusal.None, "Done.", "Feito."),
            (RebirthRefusal.InvalidPlayer, "That character was not found.", "Personagem não encontrado."),
            (RebirthRefusal.LevelTooLow, "Your light is not full yet: reach the level asked first.", "Sua luz ainda não encheu: chegue ao nível pedido antes."),
            (RebirthRefusal.NotEnoughGold, "Not enough gold.", "Ouro insuficiente."),
            (RebirthRefusal.CycleFull, "Ten returns done: now climb the Column at the Cais da Coluna.", "Dez voltas feitas: agora é subir a Coluna, no Cais da Coluna."),
            (RebirthRefusal.CycleNotFull, "The Column asks for ten returns of the light first.", "A Coluna pede dez voltas da luz antes."),
            (RebirthRefusal.MaxReached, "All your light is already home. There is nothing more to return.", "Toda a sua luz já voltou para casa. Não há mais o que devolver."),
            (RebirthRefusal.TooSoon, "The light is still on its way back. Come back later.", "A luz ainda está voltando. Volte mais tarde."),
            (RebirthRefusal.WrongPlace, "This is done by the Cristal at Grandma Edna's Village (the Column climb, at the Cais da Coluna).",
                "Isso se faz junto do Cristal, na Vila da Vó Edna (a subida da Coluna, no Cais da Coluna)."),
            (RebirthRefusal.Stained, "The light only goes back clean: let your stains fade or pay the bail.", "A luz só volta limpa: deixe as manchas sumirem ou pague a fiança."),
            (RebirthRefusal.Busy, "Finish your trade, duel or arena first.", "Termine a troca, o duelo ou a arena antes."),
            (RebirthRefusal.Dead, "Get up first.", "Levante-se antes."),
            (RebirthRefusal.StaleRequest, "Something changed. Check the summary again.", "Algo mudou. Confira o resumo de novo."),
        };

        private static readonly (RebirthPart Part, string En, string Pt)[] PartTexts =
        {
            (RebirthPart.Level, "Level (back to 1)", "Nível (volta ao 1)"),
            (RebirthPart.Experience, "Experience", "Experiência"),
            (RebirthPart.AttributePoints, "Attribute points (they come back with the levels, plus the light bonus)", "Pontos de atributo (voltam com os níveis, mais o bônus de luz)"),
            (RebirthPart.SkillAndTalentPoints, "Skill and talent points (free respec)", "Pontos de skill e de talento (redistribuição grátis)"),
            (RebirthPart.Items, "Items, equipment and the Chest", "Itens, equipamento e o Baú"),
            (RebirthPart.Gold, "Gold (minus the price)", "Ouro (menos o preço)"),
            (RebirthPart.Quests, "Finished quests and the story", "Missões feitas e a história"),
            (RebirthPart.Caderno, "Grandma Edna's Notebook and Grandpa Jorge's keepsakes", "Caderno da Vó Edna e Lembranças do Vô Jorge"),
            (RebirthPart.Achievements, "Achievements", "Conquistas"),
            (RebirthPart.Mounts, "Mounts", "Montarias"),
            (RebirthPart.House, "House", "Casa"),
            (RebirthPart.Guild, "Guild", "Guilda"),
            (RebirthPart.Graduations, "Class graduations", "Graduações de classe"),
            (RebirthPart.MasterLevel, "Master level (active again at 500)", "Nível de Mestre (volta a valer no 500)"),
            (RebirthPart.GrowthFruits, "Growth fruits already eaten", "Frutas de crescimento já comidas"),
            (RebirthPart.PvpRecord, "Arena rating", "Rating da arena"),
        };

        private static readonly (string Id, string En, string Pt)[] TitleTexts =
        {
            ("title_rebirth_returned_spark", "Returned Spark", "Faísca que Voltou"),
            ("title_rebirth_lit_lamp", "Lit Lamp", "Lamparina Acesa"),
            ("title_rebirth_waxing_moon", "Waxing Moon", "Lua Crescente"),
            ("title_rebirth_full_moon", "Full Moon", "Lua Cheia"),
            ("title_rebirth_sun_boat", "Passenger of the Sun Boat", "Passageiro da Barca do Sol"),
            ("title_rebirth_twelve_jars", "Twelfth Jar", "Décima Segunda Jarra"),
            ("title_rebirth_way_home", "The Way Home", "O Caminho de Casa"),
            ("title_rebirth_column_step_1", "First Step of the Column", "Primeiro Degrau da Coluna"),
            ("title_rebirth_column_step_2", "Second Step of the Column", "Segundo Degrau da Coluna"),
            ("title_rebirth_column_step_3", "Third Step of the Column", "Terceiro Degrau da Coluna"),
            ("title_rebirth_column_step_4", "Fourth Step of the Column", "Quarto Degrau da Coluna"),
            ("title_rebirth_column_step_5", "Top of the Column", "Alto da Coluna"),
        };

        private static readonly (string Id, string En, string Pt)[] CosmeticTexts =
        {
            ("cosmetic_rebirth_spark_thread", "Thread of light above the head", "Fio de luz sobre a cabeça"),
            ("cosmetic_rebirth_floating_lamp", "Floating oil lamp", "Lamparina que flutua"),
            ("cosmetic_rebirth_moon_trail", "Crescent-moon footprints", "Pegadas de lua crescente"),
            ("cosmetic_rebirth_full_moon_cloak", "Full-moon felt cloak", "Manto de feltro de lua cheia"),
            ("cosmetic_rebirth_sun_boat_wake", "Golden wake of the Sun Boat", "Esteira dourada da Barca do Sol"),
            ("cosmetic_rebirth_jar_wheel", "Little wheel of twelve jars on the back", "Rodinha de doze jarras nas costas"),
            ("cosmetic_rebirth_way_home_glow", "Glow of the way home", "Brilho do caminho de casa"),
            ("cosmetic_rebirth_column_step_1", "Column aura (1 step)", "Aura da Coluna (1 degrau)"),
            ("cosmetic_rebirth_column_step_2", "Column aura (2 steps)", "Aura da Coluna (2 degraus)"),
            ("cosmetic_rebirth_column_step_3", "Column aura (3 steps)", "Aura da Coluna (3 degraus)"),
            ("cosmetic_rebirth_column_step_4", "Column aura (4 steps)", "Aura da Coluna (4 degraus)"),
            ("cosmetic_rebirth_column_step_5", "Column aura (top)", "Aura da Coluna (alto)"),
        };

        private static readonly (CharacterId Cousin, string En, string Pt)[] CousinResetLines =
        {
            (CharacterId.Maithe, "One, two, three, four, five... everybody's here. Then zero me out: the wind knows the way back.",
                "Um, dois, três, quatro, cinco... tá todo mundo aqui. Então pode zerar, que o vento sabe o caminho de volta."),
            (CharacterId.Icaro, "Level 1 again, but the points are stored. See? Science. It's an engine of light, just inside me.",
                "Nível 1 de novo, mas com os pontos guardados. Viu? Ciência. É um engenho de luz, só que dentro de mim."),
            (CharacterId.Joaquim, "Leave it to me! Starting over is like a second plate of plov: the second one is always better.",
                "Deixa comigo! Começar de novo é igual repetir o plov: o segundo prato é sempre melhor."),
            (CharacterId.Maria, "Breathe... feel it? The light went to the Moon and a little stayed with me. Go with light.",
                "Respira... sentiu? A luz foi para a Lua e um pouquinho ficou comigo. Vai com luz."),
            (CharacterId.Lavine, "The Moon Boat took my light and gave back more. Fair. And level 1 doesn't make me \"little\".",
                "A Barca da Lua levou minha luz e devolveu mais. Justo. E nível 1 não me faz \"pequena\"."),
        };

        private static readonly (CharacterId Cousin, string En, string Pt)[] CousinGrandLines =
        {
            (CharacterId.Maithe, "Up the Column, one step. From up here I can count all of you at once.",
                "Um degrau da Coluna. Daqui de cima dá para contar todo mundo de uma vez."),
            (CharacterId.Icaro, "Ten returns, one step. The Artificers built a ladder out of light. Grandpa would love the blueprint.",
                "Dez voltas, um degrau. Os Artífices fizeram uma escada de luz. O Vô ia adorar ver a planta."),
            (CharacterId.Joaquim, "A whole step! Leave it to me... but somebody save me a plate for when I get back down.",
                "Um degrau inteiro! Deixa comigo... mas alguém guarda um prato para quando eu descer."),
            (CharacterId.Maria, "Breathe. Grandpa used to say the light climbs slowly, step by step. I'm on mine.",
                "Respira. O Vô dizia que a luz sobe devagar, degrau por degrau. Eu tô no meu."),
            (CharacterId.Lavine, "Kaggen threw his shoe to the sky and it became the Moon. I just climbed a step. Same family of trick.",
                "O Kaggen jogou o sapato no céu e virou a Lua. Eu só subi um degrau. É a mesma família de truque."),
        };

        private static readonly LocalizationEntry[] UiEntries =
        {
            new LocalizationEntry(ResetName, "Return of the Light", "Volta da Luz"),
            new LocalizationEntry(GrandResetName, "Climb of the Column", "Subida da Coluna"),
            new LocalizationEntry(Summary, "Level {0} → 1 · Gold: {1} · Light points: +{2} (total {3})",
                "Nível {0} → 1 · Ouro: {1} · Pontos de luz: +{2} (total {3})"),
            new LocalizationEntry(RequirementLine, "Needs level {0} and {1} gold, at {2}.", "Pede nível {0} e {1} de ouro, em {2}."),
            new LocalizationEntry(CooldownLine, "The light is still on its way back: {0} h more.", "A luz ainda está voltando: mais {0} h."),
            new LocalizationEntry(EchoStatus, "Echo of the Light: hunting XP ×{0} up to level {1}.", "Eco da Luz: XP de caça ×{0} até o nível {1}."),
            new LocalizationEntry(CountLine, "Returns of the Light: {0} · Steps of the Column: {1}", "Voltas da Luz: {0} · Degraus da Coluna: {1}"),
            new LocalizationEntry(LineOffer,
                "Sit here, by the Cristal. Your grandpa Jorge used to say the Moon gathers the light we free along the road, and gives it all back brighter. Want to give yours back?",
                "Senta aqui, perto do Cristal. O Vô Jorge dizia que a Lua junta a luz que a gente solta pelo caminho e devolve tudo mais clarinho. Quer devolver a sua?"),
            new LocalizationEntry(LineAfter,
                "There. You're light as a feather again, but the light hasn't forgotten the way back.",
                "Pronto. Você está levinho de novo, mas a luz não esqueceu o caminho de volta."),
            new LocalizationEntry(LineEcho,
                "Until you reach where you were, the light remembers the road: whatever you hunt counts four times.",
                "Até você chegar onde estava, a luz lembra a estrada: o que você caçar rende quatro vezes."),
            new LocalizationEntry(LineGrandOffer,
                "(on the radio) Ten times already? Your grandpa said that at the top of the Column there's a step for every traveller who came back ten times. Go see yours.",
                "(pelo rádio) Dez vezes já? O Vô Jorge dizia que no alto da Coluna tem um degrau para cada viajante que voltou dez vezes. Vai lá ver o seu."),
            new LocalizationEntry(LineGrandAfter,
                "(on the radio) I can see a thread of light over the village from here. Your grandpa's chair is facing it. He'd have loved this.",
                "(pelo rádio) Daqui eu vejo um fio de luz em cima da vila. A cadeira do Vô Jorge tá virada para ele. Ele ia adorar isso."),
            new LocalizationEntry(LineAllHome,
                "All your light is home now. Come, the tea is warm. The rest of the story your grandpa told, we tell together.",
                "Toda a sua luz já voltou para casa. Vem, que o chá tá quentinho. O resto da história do Vô, a gente conta junto."),
        };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(UiEntries);
            foreach (var t in RefusalTexts)
            {
                entries.Add(new LocalizationEntry(RefusalKey(t.Reason), t.En, t.Pt));
            }

            foreach (var t in PartTexts)
            {
                entries.Add(new LocalizationEntry(PartKey(t.Part), t.En, t.Pt));
            }

            foreach (var t in TitleTexts)
            {
                entries.Add(new LocalizationEntry(TitleKey(t.Id), t.En, t.Pt));
            }

            foreach (var t in CosmeticTexts)
            {
                entries.Add(new LocalizationEntry(CosmeticKey(t.Id), t.En, t.Pt));
            }

            foreach (var t in CousinResetLines)
            {
                entries.Add(new LocalizationEntry(CousinResetLineKey(t.Cousin), t.En, t.Pt));
            }

            foreach (var t in CousinGrandLines)
            {
                entries.Add(new LocalizationEntry(CousinGrandLineKey(t.Cousin), t.En, t.Pt));
            }

            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(ResetName)))
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
            catch (FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string Refusal(LocalizationManager manager, RebirthRefusal reason) => Get(manager, RefusalKey(reason));

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                var map = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    map[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = map;
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>"StaleRequest" → "stale_request".</summary>
        public static string Snake(string pascal)
        {
            if (string.IsNullOrEmpty(pascal))
            {
                return pascal;
            }

            var builder = new System.Text.StringBuilder(pascal.Length + 8);
            for (int i = 0; i < pascal.Length; i++)
            {
                char c = pascal[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        builder.Append('_');
                    }

                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
