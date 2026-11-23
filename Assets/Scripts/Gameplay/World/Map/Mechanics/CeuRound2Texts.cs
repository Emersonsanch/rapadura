using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// PT/EN texts of the second round of the Reinos do Céu (2026-11-23): the stubborn sparks of the hold and the moon that grows (93,
    /// <see cref="MoonDeckSparkRules"/>), the cracks by night (99, <see cref="CeuSkyHoldRules"/>) and the porch that keeps the day's story (100,
    /// <see cref="CeuPorchRules"/>). Loaded on first use like the other mechanics' texts.
    /// </summary>
    public static class CeuRound2Texts
    {
        public enum Moment
        {
            /// <summary>The deck's first full moon ever.</summary>
            Feast = 0,
            /// <summary>The fifth story heard to the end.</summary>
            AllStories = 1,
        }

        public const string SparkPrompt = "ui.ceu2.spark_prompt";
        public const string SparkListening = "ui.ceu2.spark_listening";
        public const string SparkSulks = "ui.ceu2.spark_sulks";
        public const string SparkWalkedOff = "ui.ceu2.spark_walked_off";
        public const string SparkFollows = "ui.ceu2.spark_follows";
        public const string SparkDropped = "ui.ceu2.spark_dropped";
        public const string SparkLit = "ui.ceu2.spark_lit";
        public const string SparkLavine = "ui.ceu2.spark_lavine";
        public const string FeastFull = "ui.ceu2.feast_full";
        public const string FeastRadio = "ui.ceu2.feast_radio";
        public const string FeastBagFull = "ui.ceu2.feast_bag_full";
        public const string CrackNight = "ui.ceu2.crack_night";
        public const string SealNight = "ui.ceu2.seal_night";
        public const string PorchResume = "ui.ceu2.porch_resume";
        public const string PorchKept = "ui.ceu2.porch_kept";
        public const string PorchKeptStop = "ui.ceu2.porch_kept_stop";
        public const string AllStoriesMemory = "ui.ceu2.all_stories_memory";
        public const string AllStoriesRadio = "ui.ceu2.all_stories_radio";

        private static readonly CharacterId[] Order = { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine };

        private static readonly (string En, string Pt)[][] Lines =
        {
            new[]
            {
                ("Maithe: Five sparks, five lanterns. The light didn't want to go home alone.", "Maithe: Cinco faíscas, cinco lanternas. A luz não queria ir para casa sozinha."),
                ("Ícaro: The moon grew! It's a light battery, I knew it. A huge one.", "Ícaro: A lua cresceu! É uma bateria de luz, eu sabia. Uma enorme."),
                ("Joaquim: Nobody had to be hit. Just asked. Vô Jorge would like that.", "Joaquim: Ninguém precisou apanhar. Só pedir. O Vô Jorge ia gostar disso."),
                ("Maria: A party on the moon! Vó Edna has to hear about this.", "Maria: Festa na lua! A Vó Edna tem que saber disso."),
                ("Lavine: The moon is a shoe again, full of light. The barque will carry it to the Sun.", "Lavine: A lua virou sapato de novo, cheio de luz. A barca vai levar para o Sol."),
            },
            new[]
            {
                ("Maithe: All five. I'll tell them to the little ones the way he told us.", "Maithe: As cinco. Vou contar para os pequenos do jeito que ele contava para nós."),
                ("Ícaro: I wrote them all down. In case the chair forgets.", "Ícaro: Anotei todas. Caso a cadeira esqueça."),
                ("Joaquim: He's not in the chair. But the stories hold it, like roots.", "Joaquim: Ele não está na cadeira. Mas as histórias seguram ela, feito raiz."),
                ("Maria: I cried at the third one again. Don't tell Vó Edna.", "Maria: Chorei na terceira de novo. Não conta para a Vó Edna."),
                ("Lavine: The spark went up and stayed a little longer today. I think he heard all five too.", "Lavine: A faísca subiu e ficou um pouco mais hoje. Acho que ele também ouviu as cinco."),
            },
        };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var list = new List<LocalizationEntry>
            {
                new LocalizationEntry(SparkPrompt, "A stubborn spark hides here. [E] to talk it round (no striking!)", "Uma faísca teimosa se esconde aqui. [E] para convencer (sem bater!)"),
                new LocalizationEntry(SparkListening, "It listens... stay close and keep your hands still", "Ela escuta... fique perto e de mãos quietas"),
                new LocalizationEntry(SparkSulks, "You struck: the spark sulks and hides for a while", "Você bateu: a faísca ficou de birra e se escondeu um tempo"),
                new LocalizationEntry(SparkWalkedOff, "You walked off: the spark goes back to its corner", "Você se afastou: a faísca volta para o cantinho dela"),
                new LocalizationEntry(SparkFollows, "The spark agreed! Take it to the lantern of the Prow Cabin", "A faísca topou! Leve até a lanterna da Cabine da Proa"),
                new LocalizationEntry(SparkDropped, "The spark slipped back to the hold", "A faísca escapou de volta para o porão"),
                new LocalizationEntry(SparkLit, "A lantern lights on the deck and the moon grows: {0}/5", "Uma lanterna acende no convés e a lua cresce: {0}/5"),
                new LocalizationEntry(SparkLavine, "Lavine: See? It only wanted company on the way.", "Lavine: Viu? Ela só queria companhia no caminho."),
                new LocalizationEntry(FeastFull, "Full moon! The deck holds its feast: +1 Rune of Clarity", "Lua cheia! O convés vira festa: +1 Runa da Clareza"),
                new LocalizationEntry(FeastRadio, "Vó Edna (radio): I can see the moon full from here, my dears. Now come home, the plov is getting cold. Over.",
                    "Vó Edna (rádio): Daqui eu vejo a lua cheia, meus amores. Agora venham, que o plov está esfriando. Câmbio."),
                new LocalizationEntry(FeastBagFull, "Full moon! The rune waits by the lantern: make room in the bag",
                    "Lua cheia! A runa espera junto da lanterna: abra espaço na mochila"),
                new LocalizationEntry(CrackNight, "By night the shadow runs better: hold longer, and mind the faster drips", "De noite a sombra escorre melhor: segure mais tempo e cuidado com os pingos mais rápidos"),
                new LocalizationEntry(SealNight, "Sealed by night: the light bites harder, but the crack opens again sooner", "Selada de noite: a luz morde mais, mas a rachadura reabre mais cedo"),
                new LocalizationEntry(PorchResume, "The story picks up where it stopped", "A história continua de onde parou"),
                new LocalizationEntry(PorchKept, "Story kept: {0}/5", "História guardada: {0}/5"),
                new LocalizationEntry(PorchKeptStop, "The story waits for you, at the same line, until the day ends", "A história espera você, na mesma fala, até o fim do dia"),
                new LocalizationEntry(AllStoriesMemory, "Vô Jorge (memory): Five stories, one family. Tell them again, and I'm still on the porch.",
                    "Vô Jorge (lembrança): Cinco histórias, uma família. Conta de novo, que eu continuo na varanda."),
                new LocalizationEntry(AllStoriesRadio, "Vó Edna (radio): You heard them all... He'd be rocking that chair and laughing. Over.",
                    "Vó Edna (rádio): Vocês ouviram todas... Ele ia estar balançando essa cadeira e rindo. Câmbio."),
            };

            for (int m = 0; m < Lines.Length; m++)
            {
                for (int c = 0; c < Lines[m].Length; c++)
                {
                    list.Add(new LocalizationEntry(LineKey((Moment)m, c), Lines[m][c].En, Lines[m][c].Pt));
                }
            }

            return list;
        }

        private static string LineKey(Moment moment, int cousin) => $"ui.ceu2.line_{(int)moment}_{cousin}";

        /// <summary>The cousin's line for the moment (null when no cousin is known).</summary>
        public static string LineFor(Moment moment, CharacterId? hero)
        {
            if (hero == null)
            {
                return null;
            }

            int index = System.Array.IndexOf(Order, hero.Value);
            return index >= 0 ? LineKey(moment, index) : null;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(SparkPrompt))
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
