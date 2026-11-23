using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// PT/EN texts of the second round of the Margens da Mistura (2026-11-23): the late shadows at night (78, <see cref="LateShadowEchoRules"/>), the
    /// Betume Vivo of the night pools and the pitch smoke that catches fire (79, <see cref="NightTarRules"/>, <see cref="PitchSmokeIgniteRules"/>).
    /// Loaded on first use like the other mechanics' texts.
    /// </summary>
    public static class MisturaNightTexts
    {
        public enum Moment
        {
            /// <summary>The first late shadow ever risen behind a fallen beast.</summary>
            LateShadow = 0,
            /// <summary>The first Betume Vivo ever spat by a night pool.</summary>
            NightTar = 1,
            /// <summary>The first pitch smoke ever burst.</summary>
            SmokeBurst = 2,
        }

        public const string ShadowComing = "ui.mistura_night.shadow_coming";
        public const string ShadowsGone = "ui.mistura_night.shadows_gone";
        public const string TarSpits = "ui.mistura_night.tar_spits";
        public const string TarCatches = "ui.mistura_night.tar_catches";
        public const string SmokeLit = "ui.mistura_night.smoke_lit";
        public const string SmokeBurst = "ui.mistura_night.smoke_burst";
        public const string SmokeSinged = "ui.mistura_night.smoke_singed";

        private static readonly CharacterId[] Order = { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine };

        private static readonly (string En, string Pt)[][] Lines =
        {
            new[]
            {
                ("Maithe: The beast fell, but its shadow was late. Time runs crooked here at night.", "Maithe: O bicho caiu, mas a sombra dele atrasou. O tempo anda torto aqui de noite."),
                ("Ícaro: It's a delay! Like thunder after lightning. Count to two and it comes.", "Ícaro: É atraso! Igual trovão depois do raio. Conta até dois e ela chega."),
                ("Joaquim: Stay behind me. If the shadow comes late, I'll be early.", "Joaquim: Fica atrás de mim. Se a sombra chega atrasada, eu chego adiantado."),
                ("Maria: Vó Edna always says the shadow follows the body. Not here: here it chases it!", "Maria: A Vó Edna sempre diz que a sombra segue o corpo. Aqui não: aqui ela corre atrás!"),
                ("Lavine: Vô Jorge's clock was always late too. He said it was waiting for someone.", "Lavine: O relógio do Vô Jorge também vivia atrasado. Ele dizia que estava esperando alguém."),
            },
            new[]
            {
                ("Maithe: The pitch is breathing. At night it wakes and crawls out on its own.", "Maithe: O piche está respirando. De noite ele acorda e sai sozinho."),
                ("Ícaro: Fire! Light the pool and it'll stop spitting. Simple chemistry.", "Ícaro: Fogo! Acende a poça que ela para de cuspir. Química simples."),
                ("Joaquim: It sticks to the shield... Vô Jorge sealed his boat with this. Ugh.", "Joaquim: Gruda no escudo... O Vô Jorge calafetava o barco com isso. Eca."),
                ("Maria: Out of the pool, quick! Sticky like Vó Edna's sugar syrup, but angry.", "Maria: Sai da poça, rápido! Grudento feito a calda da Vó Edna, só que bravo."),
                ("Lavine: The light pools show the way. The dark ones spit. Let's keep to the lit ones.", "Lavine: As poças acesas mostram o caminho. As escuras cospem. Vamos pelas acesas."),
            },
            new[]
            {
                ("Maithe: The smoke was full of pitch. A little fire and it all came down at once.", "Maithe: A fumaça estava cheia de piche. Um pouco de fogo e caiu tudo de uma vez."),
                ("Ícaro: Boom! Did you see? Light one near a pool and the pool goes too!", "Ícaro: Bum! Viu só? Acende uma perto da poça que a poça vai junto!"),
                ("Joaquim: Too close, too close! Next time I'll hold the shield first.", "Joaquim: Perto demais, perto demais! Da próxima eu levanto o escudo antes."),
                ("Maria: Vó Edna would scold us for playing with fire. But she'd laugh after.", "Maria: A Vó Edna ia brigar com a gente por brincar com fogo. Mas depois ia rir."),
                ("Lavine: Vô Jorge said smoke that drips pitch is a cloud that forgot how to rain.", "Lavine: O Vô Jorge dizia que fumaça que pinga piche é nuvem que esqueceu como chover."),
            },
        };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var list = new List<LocalizationEntry>
            {
                new LocalizationEntry(ShadowComing, "Its late shadow is coming: step out of the ring!", "A sombra atrasada dele vem vindo: saia do anel!"),
                new LocalizationEntry(ShadowsGone, "Day came: the late shadows caught up with their beasts", "Amanheceu: as sombras atrasadas alcançaram os bichos delas"),
                new LocalizationEntry(TarSpits, "The dark pool spits a Living Bitumen! Fire would quiet it", "A poça escura cospe um Betume Vivo! Fogo acalmaria isto"),
                new LocalizationEntry(TarCatches, "The Living Bitumen catches fire like the pitch it is", "O Betume Vivo pega fogo feito o piche que ele é"),
                new LocalizationEntry(SmokeLit, "The pitch smoke caught: it bursts in a moment!", "A fumaça de piche pegou: estoura num instante!"),
                new LocalizationEntry(SmokeBurst, "The smoke bursts in flames: {0} hit", "A fumaça estoura em chamas: {0} atingidos"),
                new LocalizationEntry(SmokeSinged, "Too close to the burst: it singes you", "Perto demais do estouro: ele chamusca você"),
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

        private static string LineKey(Moment moment, int cousin) => $"ui.mistura_night.line_{(int)moment}_{cousin}";

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
                if (!manager.HasKey(ShadowComing))
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
