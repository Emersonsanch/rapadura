using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// "O painel do aviso" of the Ruínas dos Artífices (map 08, Docs/Mapas100.md: "ler as 3 inscrições dá pistas do selo e o bônus da missão do
    /// Ícaro"), pure and server-runnable: the three stelae along the avenue of ushabtis each carry one line of the Artificers' warning about the
    /// seal of the Grande Ushabti. [E] by a stela reads it; each is read once for good (a time mark in the save, so a new game reads them again).
    /// <code>
    /// 3 stelae · reach 3.5 m · each stela: +15 XP the first time · the third one read: +60 XP and the clue of the seal
    /// Ícaro (the inventor, who reads the Artificers' script best): every XP of the panel ×1.5
    /// </code>
    /// Decision (2026-11-16): "o bônus da missão do Ícaro" is the ×1.5 for Ícaro; no quest depends on the panel.
    /// </summary>
    public static class ArtificesInscriptionRules
    {
        public const float ReachRadius = 3.5f;
        public const int ReadExperience = 15;
        public const int PanelExperience = 60;
        public const float IcaroFactor = 1.5f;
        public const string MarkPrefix = "artifices_inscription_";

        /// <summary>The save key of stela <paramref name="index"/>.</summary>
        public static string MarkKey(int index) => MarkPrefix + index;

        /// <summary>Index of the stela within reach of <paramref name="local"/> (the closest), or -1.</summary>
        public static int StelaAt(Vector2 local, IReadOnlyList<Vector2> stelae)
        {
            int best = -1;
            float bestSqr = ReachRadius * ReachRadius;
            for (int i = 0; stelae != null && i < stelae.Count; i++)
            {
                float sqr = (stelae[i] - local).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>How many of <paramref name="read"/> are true.</summary>
        public static int ReadCount(IReadOnlyList<bool> read)
        {
            int count = 0;
            for (int i = 0; read != null && i < read.Count; i++)
            {
                count += read[i] ? 1 : 0;
            }

            return count;
        }

        /// <summary>
        /// The XP of reading a stela for the first time, when <paramref name="readBefore"/> of the <paramref name="total"/> were already read:
        /// <see cref="ReadExperience"/>, plus <see cref="PanelExperience"/> if this one completes the panel; ×<see cref="IcaroFactor"/> for Ícaro.
        /// 0 when every stela was already read.
        /// </summary>
        public static int ExperienceFor(int readBefore, int total, bool icaro)
        {
            if (total <= 0 || readBefore >= total)
            {
                return 0;
            }

            int xp = ReadExperience + (readBefore + 1 == total ? PanelExperience : 0);
            return icaro ? Mathf.RoundToInt(xp * IcaroFactor) : xp;
        }

        /// <summary>Whether reading one more stela, with <paramref name="readBefore"/> read, completes the panel (the clue of the seal).</summary>
        public static bool Completes(int readBefore, int total) => total > 0 && readBefore + 1 == total;
    }
}
