using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Story;
using UnityEngine;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>
    /// "Soube das novidades da estrada?": the personal-story villagers (<see cref="VillageNpcCatalog"/>) comment on
    /// the latest interlude seen, through <see cref="DialogueConditionType.StoryBeatSeen"/> — no manual flags, no
    /// asset edit. <see cref="WithNews"/> takes the villager's seeded graph and returns a copy with:
    /// <list type="bullet">
    /// <item>a choice <see cref="ChoiceKey"/> right before every "goodbye" (a choice with no next node), shown once
    /// the Forest interlude was seen;</item>
    /// <item>a node <see cref="NewsNodeId"/> that redirects to the newest beat seen (ending → Montanha → Deserto →
    /// Floresta) — <c>dialogue.&lt;npc&gt;.story_news.&lt;beat id&gt;</c>, with a goodbye.</item>
    /// </list>
    /// A "Nova Jornada+" clears the beats seen, so the news start over with the new journey. Villagers without lines
    /// (the gathering point) are returned untouched. Copies are cached per source asset. Texts:
    /// <see cref="VillageStoryNewsTexts"/>; who-knows-what follows Docs/Narrative.md §5.
    /// </summary>
    public static class VillageStoryNews
    {
        public const string NewsNodeId = "story_news";
        public const string ChoiceKey = "dialogue.village_npc.choice_story_news";
        public const string GoodbyeKey = "dialogue.common.choice_goodbye";

        /// <summary>Beats the villagers comment on, newest first (the first one seen wins).</summary>
        public static readonly StoryBeat[] NewestFirst =
        {
            StoryBeat.Ending,
            StoryBeat.InterludeMountain,
            StoryBeat.InterludeDesert,
            StoryBeat.InterludeForest,
        };

        /// <summary>Villagers who have news lines (NpcKey of <see cref="VillageNpcSpec"/>).</summary>
        public static readonly IReadOnlyList<string> NpcKeys = new[]
        {
            "village_survivor", "temple_acolyte", "windtrail_scholar", "ruins_archivist", "hermit_mystic",
        };

        private static readonly Dictionary<DialogueDefinition, DialogueDefinition> Cache = new Dictionary<DialogueDefinition, DialogueDefinition>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Cache.Clear();

        /// <summary><c>dialogue.village_survivor.story_news.interlude_desert</c>.</summary>
        public static string LineKey(string npcKey, StoryBeat beat) => "dialogue." + npcKey + ".story_news." + StoryBeats.Id(beat);

        public static string BeatNodeId(StoryBeat beat) => NewsNodeId + "_" + StoryBeats.Id(beat);

        public static bool HasNews(string npcKey) => npcKey != null && NpcKeys.Contains(npcKey);

        /// <summary>The graph with the news choice and nodes (cached copy), or <paramref name="source"/> as is.</summary>
        public static DialogueDefinition WithNews(DialogueDefinition source, string npcKey)
        {
            if (source == null || !HasNews(npcKey) || source.GetNode(NewsNodeId) != null)
            {
                return source;
            }

            if (Cache.TryGetValue(source, out DialogueDefinition cached) && cached != null)
            {
                return cached;
            }

            var nodes = new List<DialogueNode>();
            string speakerId = null;
            string speakerNameKey = null;
            foreach (DialogueNode node in source.Nodes)
            {
                if (node == null)
                {
                    continue;
                }

                DialogueNode copy = Copy(node);
                speakerId ??= string.IsNullOrEmpty(node.SpeakerId) ? null : node.SpeakerId;
                speakerNameKey ??= string.IsNullOrEmpty(node.SpeakerNameKey) ? null : node.SpeakerNameKey;
                int goodbye = copy.Choices.FindLastIndex(c => c != null && string.IsNullOrEmpty(c.NextNodeId));
                if (goodbye >= 0)
                {
                    copy.Choices.Insert(goodbye, new DialogueChoice
                    {
                        TextKey = ChoiceKey,
                        NextNodeId = NewsNodeId,
                        Conditions = { Seen(StoryBeat.InterludeForest) },
                    });
                }

                nodes.Add(copy);
            }

            var hub = new DialogueNode { Id = NewsNodeId, TextKey = string.Empty, SpeakerId = speakerId, SpeakerNameKey = speakerNameKey };
            foreach (StoryBeat beat in NewestFirst)
            {
                hub.Redirects.Add(new DialogueRedirect { TargetNodeId = BeatNodeId(beat), Conditions = { Seen(beat) } });
            }

            hub.Redirects.Add(new DialogueRedirect { TargetNodeId = BeatNodeId(StoryBeat.InterludeForest) });
            nodes.Add(hub);

            foreach (StoryBeat beat in NewestFirst)
            {
                var line = new DialogueNode { Id = BeatNodeId(beat), TextKey = LineKey(npcKey, beat), SpeakerId = speakerId, SpeakerNameKey = speakerNameKey };
                line.Choices.Add(new DialogueChoice { TextKey = GoodbyeKey, NextNodeId = string.Empty });
                nodes.Add(line);
            }

            DialogueDefinition result = ScriptableObject.CreateInstance<DialogueDefinition>();
            result.SetDataForTests(source.DialogueId, source.StartNodeId, nodes);
            result.name = source.name;
            result.hideFlags = HideFlags.DontUnloadUnusedAsset;
            Cache[source] = result;
            return result;
        }

        private static DialogueCondition Seen(StoryBeat beat)
        {
            return new DialogueCondition { Type = DialogueConditionType.StoryBeatSeen, Key = StoryBeats.Id(beat) };
        }

        // The seeded nodes are shared with the asset: copy the choice list so the asset itself is never changed.
        private static DialogueNode Copy(DialogueNode node)
        {
            return new DialogueNode
            {
                Id = node.Id,
                TextKey = node.TextKey,
                SpeakerId = node.SpeakerId,
                SpeakerNameKey = node.SpeakerNameKey,
                Choices = new List<DialogueChoice>(node.Choices ?? new List<DialogueChoice>()),
                AutoAdvanceToNodeId = node.AutoAdvanceToNodeId,
                Redirects = node.Redirects,
                OnEnterActions = node.OnEnterActions,
            };
        }
    }

    /// <summary>Texts (en + pt-BR) of <see cref="VillageStoryNews"/>: the choice and each villager's line per beat.</summary>
    public static class VillageStoryNewsTexts
    {
        public static readonly List<LocalizationEntry> All = new List<LocalizationEntry>
        {
            new LocalizationEntry(VillageStoryNews.ChoiceKey, "Heard the news from the road?", "Soube das novidades da estrada?"),

            // Sobrevivente da vila velha (Joaquim's old village).
            new LocalizationEntry("dialogue.village_survivor.story_news.interlude_forest",
                "They say the Alpha Wolf lay down gentle in the middle of the Forest. Back in the old village we thought a darkened beast never lit up again... good to be wrong.",
                "Dizem que o Lobo Alfa deitou manso no meio da Floresta. Na vila velha a gente achava que bicho apagado não acendia mais... que bom estar errado."),
            new LocalizationEntry("dialogue.village_survivor.story_news.interlude_desert",
                "Seu Bento came by telling of an ancients' panel out in the Desert. So it was a cracking seal that brought the shadow to the old village? At least now it has a name.",
                "O Seu Bento passou por aqui contando de um painel dos antigos lá no Deserto. Então foi um selo rachando que trouxe a sombra pra vila velha? Pelo menos agora tem nome."),
            new LocalizationEntry("dialogue.village_survivor.story_news.interlude_mountain",
                "I heard the mine bell ringing from the mountain, I swear I did! And Joaquim? Tell him a protector who knows when to step aside is still a protector.",
                "Ouvi o sino da mina tocando lá da serra, juro que ouvi! E o Joaquim? Diz pra ele que quem sabe a hora de sair da frente também protege."),
            new LocalizationEntry("dialogue.village_survivor.story_news.ending",
                "The Crystal is lit! I'm thinking of going back to the old village and planting again. With light like this, even the ruins feel like home.",
                "O Cristal acendeu! Tô pensando em voltar pra vila velha e plantar de novo. Com uma luz dessas, até as ruínas parecem casa."),

            // Acólita do Templo (Maria).
            new LocalizationEntry("dialogue.temple_acolyte.story_news.interlude_forest",
                "Maria was right: the Alpha Wolf wasn't bad, only darkened. The temple light trembled with joy when he calmed down.",
                "A Maria tinha razão: o Lobo Alfa não era mau, só estava apagado. A luz do templo tremeu de alegria quando ele sossegou."),
            new LocalizationEntry("dialogue.temple_acolyte.story_news.interlude_desert",
                "Guardians built to watch over a seal... The elder priestesses always spoke of an old warning. Now we know it was real.",
                "Guardiões construídos pra vigiar um selo... As sacerdotisas mais velhas sempre falaram de um aviso antigo. Agora a gente sabe que era de verdade."),
            new LocalizationEntry("dialogue.temple_acolyte.story_news.interlude_mountain",
                "A creature afraid of the dark... Whoever steals light must be very cold inside. I pray for it too, you know?",
                "Uma criatura com medo do escuro... Quem rouba luz deve estar com muito frio por dentro. Rezo por ela também, viu?"),
            new LocalizationEntry("dialogue.temple_acolyte.story_news.ending",
                "The Crystal shines again, and Maria's little sun medal shines with it. At the temple nobody talks about anything else.",
                "O Cristal voltou a brilhar, e a medalhinha de sol da Maria brilha junto. No templo ninguém fala de outra coisa."),

            // Estudioso Errante (Maithe, the marked golems).
            new LocalizationEntry("dialogue.windtrail_scholar.story_news.interlude_forest",
                "A dark little stone on the Wolf's collar? Interesting... the mountain golems bear marks of the same shade. Write that down — I already did.",
                "Uma pedrinha escura na coleira do Lobo? Interessante... os golens da serra têm marcas do mesmo tom. Anote isso, que eu já anotei."),
            new LocalizationEntry("dialogue.windtrail_scholar.story_news.interlude_desert",
                "The Desert panel confirms what I suspected: the golems were built — they're guardians. Maithe had the right eye for it.",
                "O painel do Deserto confirma o que eu desconfiava: os golens foram construídos, são guardiões. A Maithe teve o olho certo pra isso."),
            new LocalizationEntry("dialogue.windtrail_scholar.story_news.interlude_mountain",
                "So the Warlord came back to himself and said thank you? That's what a good guardian is: when it remembers who it is, it remembers kindness too.",
                "Então o Senhor da Guerra voltou a si e agradeceu? Guardião bom é assim: quando lembra quem é, lembra também de ser gentil."),
            new LocalizationEntry("dialogue.windtrail_scholar.story_news.ending",
                "I'm rewriting my whole book: 'The Dweller wasn't a monster, only lonely.' First sentence. It's a good one, isn't it?",
                "Vou reescrever meu livro inteiro: 'O Habitante não era monstro, só estava sozinho.' Primeira frase. Ficou bonita, não ficou?"),

            // Arquivista das Ruínas (Ícaro).
            new LocalizationEntry("dialogue.ruins_archivist.story_news.interlude_forest",
                "News from the Forest: the Alpha Wolf calmed down and there was golden sand on the trail. Golden sand... that comes from my ruins!",
                "Chegou notícia da Floresta: o Lobo Alfa sossegou e tinha areia dourada no caminho. Areia dourada... isso vem das minhas ruínas!"),
            new LocalizationEntry("dialogue.ruins_archivist.story_news.interlude_desert",
                "I heard Ícaro read the ancients' panel in one go. It took me twenty years to decipher half of it! Tell him I'm jealous. The good kind.",
                "Soube que o Ícaro leu o painel dos antigos de uma vez só. Eu levei vinte anos pra decifrar metade! Diz pra ele que eu tô com inveja. Da boa."),
            new LocalizationEntry("dialogue.ruins_archivist.story_news.interlude_mountain",
                "The ancients locked away a creature afraid of the dark... and built everything out of cold stone. Sometimes people who build a lot forget to light a lamp.",
                "Os antigos prenderam uma criatura com medo do escuro... e fizeram tudo de pedra fria. Às vezes quem constrói muito esquece de acender uma lamparina."),
            new LocalizationEntry("dialogue.ruins_archivist.story_news.ending",
                "I added a new note at the end of the ancients' warning: 'Solved by five cousins and a dog.' That's science too.",
                "Escrevi uma nota nova no fim do aviso dos antigos: 'Resolvido por cinco primos e um cachorro.' Ciência também é isso."),

            // Eremita Místico (Lavine, the Noite Rubra).
            new LocalizationEntry("dialogue.hermit_mystic.story_news.interlude_forest",
                "The Forest took a deep breath tonight. I felt it. The shadow that lived in the Wolf is gone... but the cold came from the Caves, and it still does.",
                "A Floresta respirou fundo esta noite. Eu senti. A sombra que morava no Lobo foi embora... mas o frio veio das Cavernas, e ainda vem."),
            new LocalizationEntry("dialogue.hermit_mystic.story_news.interlude_desert",
                "A seal cracking in the Caves... That's why Lavine's shadow has been restless. She's braver than she thinks.",
                "Um selo rachando nas Cavernas... Por isso a sombra da Lavine anda inquieta. Ela é mais corajosa do que pensa."),
            new LocalizationEntry("dialogue.hermit_mystic.story_news.interlude_mountain",
                "It's afraid of the dark... Who understands that better than Lavine? Her shadow may be the gentlest thing that creature has ever seen.",
                "Ele tem medo do escuro... Quem entende isso melhor que a Lavine? A sombra dela pode ser a coisa mais gentil que aquela criatura já viu."),
            new LocalizationEntry("dialogue.hermit_mystic.story_news.ending",
                "The night is soft again. Lavine showed that a shadow can hug too. For the first time in years, I sleep without a candle.",
                "A noite ficou macia de novo. A Lavine mostrou que a sombra também abraça. Pela primeira vez em anos, eu durmo sem vela."),
        };

        /// <summary>Merges <see cref="All"/> into <paramref name="localization"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureLoaded(LocalizationManager localization)
        {
            if (localization == null)
            {
                return;
            }

            localization.LoadEntries(All.Where(entry => !localization.HasKey(entry.Key)).ToList());
        }
    }
}
