using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>The non-shop, non-drop ways a mount joins the cousins (Docs/Mounts.md §3.6).</summary>
    public enum MountSourceKind
    {
        Quest = 0,
        Achievement = 1,
        Secret = 2,
        Event = 3,
    }

    /// <summary>One mission / achievement / map secret / event that gives a mount.</summary>
    public sealed class MountSourceHook
    {
        public readonly string MountId;
        public readonly MountSourceKind Kind;

        /// <summary>The quest id, achievement id, world-secret id or event id the game reports.</summary>
        public readonly string HookId;

        /// <summary>Event mounts: their price in prendas (0 for the others).</summary>
        public readonly int Prendas;

        public MountSourceHook(string mountId, MountSourceKind kind, string hookId, int prendas = 0)
        {
            MountId = mountId;
            Kind = kind;
            HookId = hookId;
            Prendas = prendas;
        }

        public string SourceTag => MountSourceHooks.SourceTagOf(Kind);
    }

    /// <summary>
    /// Data hooks of the wave-1 mounts that come from content not written yet (§3.6, §8 stories): the quest "Água pro
    /// Jumento", the achievements "Amigo da Caipora" and "Paciência de Jabuti", the map secret "A Mata que Não Queima" and
    /// the Festa Junina event (prendas). <see cref="MountCollection"/> listens to <c>QuestCompletedEvent</c>,
    /// <c>AchievementUnlockedEvent</c> and <c>SecretClaimedEvent</c> and grants the matching mount once; whoever
    /// writes the quest/achievement/secret only has to use the id below. The event shop calls
    /// <see cref="MountCollection.BuyWithPrendas"/>. Pure data + rules, the server reuses them.
    /// </summary>
    public static class MountSourceHooks
    {
        public const string QuestAguaProJumento = "quest_agua_pro_jumento";
        public const string AchievementAmigoDaCaipora = "achievement_amigo_da_caipora";
        public const string AchievementPacienciaDeJabuti = "achievement_paciencia_de_jabuti";
        public const string SecretMataQueNaoQueima = "secret_mata_que_nao_queima";
        public const string EventFestaJunina = "event_festa_junina";

        public static readonly IReadOnlyList<MountSourceHook> All = new[]
        {
            new MountSourceHook(MountWaveCatalog.EnchantedDonkey, MountSourceKind.Quest, QuestAguaProJumento),
            new MountSourceHook(MountWaveCatalog.CaiporaPeccary, MountSourceKind.Achievement, AchievementAmigoDaCaipora),
            new MountSourceHook(MountWaveCatalog.ElderTortoise, MountSourceKind.Achievement, AchievementPacienciaDeJabuti),
            new MountSourceHook(MountWaveCatalog.Boitata, MountSourceKind.Secret, SecretMataQueNaoQueima),
            new MountSourceHook(MountWaveCatalog.FireRhea, MountSourceKind.Event, EventFestaJunina, 90),
            new MountSourceHook(MountWaveCatalog.BoiBumba, MountSourceKind.Event, EventFestaJunina, 250),
        };

        public static IEnumerable<MountSourceHook> Matching(MountSourceKind kind, string hookId)
        {
            return string.IsNullOrEmpty(hookId) ? Enumerable.Empty<MountSourceHook>() : All.Where(h => h.Kind == kind && h.HookId == hookId);
        }

        public static MountSourceHook For(string mountId) => All.FirstOrDefault(h => h.MountId == mountId);

        /// <summary>The <see cref="OwnedMount.source"/> tag of a hook kind.</summary>
        public static string SourceTagOf(MountSourceKind kind)
        {
            switch (kind)
            {
                case MountSourceKind.Quest: return MountCollection.SourceQuest;
                case MountSourceKind.Achievement: return MountCollection.SourceAchievement;
                case MountSourceKind.Secret: return MountCollection.SourceSecret;
                default: return MountCollection.SourceEvent;
            }
        }

        /// <summary>
        /// Rewards are given once: a quest/achievement/secret mount already owned is not given again. Event mounts are
        /// bought (every purchase is a new mount, like the shop).
        /// </summary>
        public static bool ShouldGrant(MountSourceHook hook, bool alreadyOwned)
        {
            return hook != null && (hook.Kind == MountSourceKind.Event || !alreadyOwned);
        }

        /// <summary>Pays an event mount: true and <paramref name="prendas"/> lowered when it is an event mount and affordable.</summary>
        public static bool TryPayPrendas(string mountId, ref int prendas)
        {
            MountSourceHook hook = For(mountId);
            if (hook == null || hook.Kind != MountSourceKind.Event || hook.Prendas <= 0 || prendas < hook.Prendas)
            {
                return false;
            }

            prendas -= hook.Prendas;
            return true;
        }
    }
}
