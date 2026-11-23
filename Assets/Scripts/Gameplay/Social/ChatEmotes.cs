using System;
using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Social
{
    /// <summary>One emote of the quick wheel: id, name and the line shown in the chat, plain and in each cousin's voice.</summary>
    public sealed class ChatEmote
    {
        public readonly string Id;
        public readonly string NameEn;
        public readonly string NamePt;

        /// <summary>The line of an own character (no cousin). {0} = the player's name.</summary>
        public readonly string LineEn;
        public readonly string LinePt;

        private readonly (CharacterId Cousin, string En, string Pt)[] _cousinLines;

        internal ChatEmote(string id, string nameEn, string namePt, string lineEn, string linePt,
            params (CharacterId Cousin, string En, string Pt)[] cousinLines)
        {
            Id = id;
            NameEn = nameEn;
            NamePt = namePt;
            LineEn = lineEn;
            LinePt = linePt;
            _cousinLines = cousinLines;
        }

        public IReadOnlyList<(CharacterId Cousin, string En, string Pt)> CousinLines => _cousinLines;

        public bool HasCousinLine(CharacterId cousin)
        {
            foreach (var line in _cousinLines)
            {
                if (line.Cousin == cousin)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// The emotes of the chat (TODO.md 12.3, "emotes para o mobile"): eight, one quick wheel on a phone, so nobody has to
    /// type in the middle of a fight. An emote goes through <see cref="ChatRules.TrySendEmote"/> like a message (same channel
    /// rules, same anti-flood, the repetition counts "the same emote") but carries only its id; each client shows the line in
    /// its own language and, when the sender plays a cousin, in that cousin's voice: Maithe the eager huntress, Ícaro the
    /// inventor who wants to impress her, Joaquim the stubborn protector, Maria the healer who looks after everyone, and
    /// Lavine, the youngest, serious and wanting to be taken seriously. {0} = the sender's name in every line.
    /// </summary>
    public static class ChatEmotes
    {
        public const string Wave = "wave";
        public const string Thanks = "thanks";
        public const string Laugh = "laugh";
        public const string Cheer = "cheer";
        public const string Help = "help";
        public const string Follow = "follow";
        public const string Wait = "wait";

        /// <summary>The goodbye of the hearers: "Que a Luz te guarde".</summary>
        public const string Light = "light";

        private static readonly ChatEmote[] AllEmotes =
        {
            new ChatEmote(Wave, "Wave", "Acenar", "{0} waves.", "{0} acena.",
                (CharacterId.Maithe, "{0} waves the bow overhead: \"Over here!\"", "{0} acena com o arco no alto: \"Aqui!\""),
                (CharacterId.Icaro, "{0} waves a little gadget that blinks back: \"Hi! It says hi too.\"", "{0} acena com um engenho que pisca junto: \"Oi! Ele também deu oi.\""),
                (CharacterId.Joaquim, "{0} raises the shield in greeting: \"All clear here.\"", "{0} ergue o escudo num cumprimento: \"Por aqui está tudo seguro.\""),
                (CharacterId.Maria, "{0} waves with a warm smile: \"Everyone all right?\"", "{0} acena com um sorriso: \"Todo mundo bem?\""),
                (CharacterId.Lavine, "{0} gives a small, serious nod.", "{0} acena com a cabeça, séria.")),
            new ChatEmote(Thanks, "Thanks", "Obrigado", "{0} says thanks.", "{0} agradece.",
                (CharacterId.Maithe, "{0}: \"Thanks! I owe you one.\"", "{0}: \"Valeu! Fico te devendo essa.\""),
                (CharacterId.Icaro, "{0}: \"Thanks! I'll write that down in my notebook.\"", "{0}: \"Obrigado! Vou anotar isso no meu caderno.\""),
                (CharacterId.Joaquim, "{0}: \"Thanks. Next time I'll cover you.\"", "{0}: \"Obrigado. Da próxima vez eu te protejo.\""),
                (CharacterId.Maria, "{0}: \"Thank you, really. Vó Edna would be proud.\"", "{0}: \"Obrigada, de verdade. A Vó Edna ia ficar orgulhosa.\""),
                (CharacterId.Lavine, "{0}: \"...Thanks. I could have done it, but thanks.\"", "{0}: \"...Obrigada. Eu dava conta, mas obrigada.\"")),
            new ChatEmote(Laugh, "Laugh", "Rir", "{0} laughs.", "{0} ri.",
                (CharacterId.Maithe, "{0} bursts out laughing.", "{0} cai na gargalhada."),
                (CharacterId.Icaro, "{0} laughs so hard a spring pops out of the gadget.", "{0} ri tanto que uma mola pula do engenho."),
                (CharacterId.Joaquim, "{0} tries to stay serious and fails.", "{0} tenta ficar sério e não consegue."),
                (CharacterId.Maria, "{0} laughs behind a hand.", "{0} ri tapando a boca com a mão."),
                (CharacterId.Lavine, "{0} almost smiles. Almost.", "{0} quase sorri. Quase.")),
            new ChatEmote(Cheer, "Cheer", "Comemorar", "{0} cheers!", "{0} comemora!",
                (CharacterId.Maithe, "{0}: \"Got it! Right on target!\"", "{0}: \"Acertei! Bem no alvo!\""),
                (CharacterId.Icaro, "{0}: \"It worked! I told you it would work!\"", "{0}: \"Funcionou! Eu falei que ia funcionar!\""),
                (CharacterId.Joaquim, "{0} bangs the shield: \"Nobody got hurt!\"", "{0} bate no escudo: \"Ninguém se machucou!\""),
                (CharacterId.Maria, "{0} claps: \"We did it, together!\"", "{0} bate palmas: \"Conseguimos, juntos!\""),
                (CharacterId.Lavine, "{0}: \"Told you I was ready.\"", "{0}: \"Eu disse que estava pronta.\"")),
            new ChatEmote(Help, "Help!", "Socorro!", "{0} calls for help!", "{0} pede ajuda!",
                (CharacterId.Maithe, "{0}: \"Too many of them! Help!\"", "{0}: \"São muitos! Ajuda aqui!\""),
                (CharacterId.Icaro, "{0}: \"Help! The gadget jammed!\"", "{0}: \"Socorro! O engenho travou!\""),
                (CharacterId.Joaquim, "{0}: \"Behind me! And someone help!\"", "{0}: \"Atrás de mim! E alguém ajuda!\""),
                (CharacterId.Maria, "{0}: \"I need a hand here, quick!\"", "{0}: \"Preciso de uma mão aqui, rápido!\""),
                (CharacterId.Lavine, "{0}: \"I... could use a little help.\"", "{0}: \"Eu... aceito uma ajudinha.\"")),
            new ChatEmote(Follow, "Follow me", "Siga-me", "{0} says: follow me!", "{0} diz: me sigam!",
                (CharacterId.Maithe, "{0}: \"I know the trail. Follow me!\"", "{0}: \"Eu conheço a trilha. Vem comigo!\""),
                (CharacterId.Icaro, "{0}: \"This way! My compass never fails. Almost never.\"", "{0}: \"Por aqui! Minha bússola nunca erra. Quase nunca.\""),
                (CharacterId.Joaquim, "{0}: \"Stay behind me and follow.\"", "{0}: \"Fiquem atrás de mim e me sigam.\""),
                (CharacterId.Maria, "{0}: \"Come on, everyone together.\"", "{0}: \"Vamos, todo mundo junto.\""),
                (CharacterId.Lavine, "{0}: \"Follow me. I'm not lost.\"", "{0}: \"Me sigam. Eu não estou perdida.\"")),
            new ChatEmote(Wait, "Wait", "Esperem", "{0} asks everyone to wait.", "{0} pede para esperarem.",
                (CharacterId.Maithe, "{0}: \"Wait! I heard something.\"", "{0}: \"Espera! Ouvi alguma coisa.\""),
                (CharacterId.Icaro, "{0}: \"Wait, I'm fixing a gear!\"", "{0}: \"Espera, estou consertando uma engrenagem!\""),
                (CharacterId.Joaquim, "{0}: \"Hold on. Nobody goes ahead of me.\"", "{0}: \"Calma. Ninguém passa na minha frente.\""),
                (CharacterId.Maria, "{0}: \"Wait, let me heal everyone first.\"", "{0}: \"Esperem, deixa eu curar todo mundo primeiro.\""),
                (CharacterId.Lavine, "{0}: \"Wait. The shadows are restless.\"", "{0}: \"Esperem. As sombras estão inquietas.\"")),
            new ChatEmote(Light, "Light be with you", "Que a Luz te guarde", "{0}: \"Light be with you.\"", "{0}: \"Que a Luz te guarde.\"",
                (CharacterId.Maithe, "{0}: \"Light be with you! See you on the trail.\"", "{0}: \"Que a Luz te guarde! A gente se vê na trilha.\""),
                (CharacterId.Icaro, "{0}: \"Light be with you! And may it never run out of charge.\"", "{0}: \"Que a Luz te guarde! E que ela nunca fique sem carga.\""),
                (CharacterId.Joaquim, "{0}: \"Light be with you. And watch your back.\"", "{0}: \"Que a Luz te guarde. E cuida das costas.\""),
                (CharacterId.Maria, "{0}: \"Light be with you, and come back safe.\"", "{0}: \"Que a Luz te guarde, e volta bem.\""),
                (CharacterId.Lavine, "{0}: \"Light be with you. The quiet kind.\"", "{0}: \"Que a Luz te guarde. A quietinha.\"")),
        };

        /// <summary>The wheel, in order.</summary>
        public static IReadOnlyList<ChatEmote> All => AllEmotes;

        public static bool Exists(string id) => Find(id) != null;

        public static ChatEmote Find(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (ChatEmote emote in AllEmotes)
            {
                if (string.Equals(emote.Id, id, StringComparison.Ordinal))
                {
                    return emote;
                }
            }

            return null;
        }

        public static string NameKey(string id) => "social.emote." + id + ".name";

        /// <summary>The line of the emote in the cousin's voice, or the plain one for an own character (or a missing line).</summary>
        public static string LineKey(string id, CharacterId? cousin)
        {
            ChatEmote emote = Find(id);
            if (emote != null && cousin.HasValue && emote.HasCousinLine(cousin.Value))
            {
                return CousinLineKey(id, cousin.Value);
            }

            return "social.emote." + id + ".line";
        }

        public static string CousinLineKey(string id, CharacterId cousin) =>
            "social.emote." + id + ".line." + cousin.ToString().ToLowerInvariant();

        internal static IEnumerable<LocalizationEntry> Entries()
        {
            foreach (ChatEmote emote in AllEmotes)
            {
                yield return new LocalizationEntry(NameKey(emote.Id), emote.NameEn, emote.NamePt);
                yield return new LocalizationEntry(LineKey(emote.Id, null), emote.LineEn, emote.LinePt);
                foreach (var line in emote.CousinLines)
                {
                    yield return new LocalizationEntry(CousinLineKey(emote.Id, line.Cousin), line.En, line.Pt);
                }
            }
        }
    }
}
