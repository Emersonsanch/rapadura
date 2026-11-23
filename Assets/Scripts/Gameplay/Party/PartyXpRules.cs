using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Gameplay.Party
{
    /// <summary>Where a party member stands at a kill: the server fills one per member.</summary>
    public readonly struct PartyKillMember
    {
        public readonly string PlayerId;
        public readonly int Level;
        public readonly bool Alive;
        public readonly Vector3 Position;
        public readonly CharacterId? Cousin;

        /// <summary>Offline, on another map or in another instance: never eligible, whatever the position says.</summary>
        public readonly bool Away;

        public PartyKillMember(string playerId, int level, bool alive, Vector3 position, CharacterId? cousin = null, bool away = false)
        {
            PlayerId = playerId;
            Level = level;
            Alive = alive;
            Position = position;
            Cousin = cousin;
            Away = away;
        }
    }

    /// <summary>Why a member got no share of a kill (texts: <see cref="PartyTexts.XpCutKey"/>). Never renumber.</summary>
    public enum PartyXpCut
    {
        None = 0,
        Dead = 1,

        /// <summary>Beyond <see cref="PartyXpRules.ShareRadius"/>, or <see cref="PartyKillMember.Away"/>.</summary>
        TooFar = 2,

        /// <summary>More than <see cref="PartyXpRules.LevelBand"/> levels above the lowest present member (anti power-leveling).</summary>
        OutOfLevelBand = 3,
    }

    /// <summary>One member's part of a kill.</summary>
    public readonly struct PartyXpShare
    {
        public readonly string PlayerId;
        public readonly int Experience;
        public readonly PartyXpCut Cut;
        public readonly bool FamilyComplete;
        public readonly bool SiblingBond;

        public PartyXpShare(string playerId, int experience, PartyXpCut cut, bool familyComplete, bool siblingBond)
        {
            PlayerId = playerId;
            Experience = experience;
            Cut = cut;
            FamilyComplete = familyComplete;
            SiblingBond = siblingBond;
        }
    }

    /// <summary>
    /// Kill XP of a party (TODO.md 12.3), MU Online style, as a new pure function on top of the existing kill XP
    /// (<see cref="EnemyLevelScaling.LeveledKillExperience(int,int,int,int)"/>; the single-player path,
    /// <c>EnemyRewardGranter</c>, is untouched). For one kill:
    /// <code>
    /// presentes = vivos, não "Away", a até ShareRadius (40 m) do abate
    /// faixa     = presentes com nível ≤ menor nível presente + LevelBand (20); os acima ficam "fora da faixa"
    /// base      = XP do abate no nível do bicho (sem o corte por diferença de nível)
    /// pote      = base × PoolMultiplier(nº de presentes na faixa)          // 1,00 / 1,20 / 1,50 / 1,80 / 2,20
    /// parte_i   = pote × nível_i / Σ nível de TODOS os presentes            // proporcional ao nível, como no MU
    ///           × LevelDifferenceXpMultiplier(nível_i, nível do bicho)    // o corte de sempre (§1.2), por membro
    ///           × família (PartyFamilyRules: ×1,10 Família Completa, ×1,05 Laço de Irmãos)
    /// fora da faixa: parte 0, mas o nível continua na soma (a XP dele queima)
    /// </code>
    /// Why the band burns instead of redistributing: a level-80 carrying a level-10 neither earns anything from the kill nor
    /// hands its share to the lowbie, who keeps only its own small proportional part (10/90 of the pool), so
    /// "power-leveling" pays nobody. Each member's result is before its own session multipliers (fôlego, descansado,
    /// NG+, mestre), which the server applies per player exactly as <c>EnemyRewardGranter</c> does.
    /// A party of one gets exactly <see cref="EnemyLevelScaling.LeveledKillExperience(int,int,int,int)"/>.
    /// </summary>
    public static class PartyXpRules
    {
        /// <summary>Metres from the kill within which a living member shares it (about a screen and a half of the iso camera).</summary>
        public const float ShareRadius = 40f;

        /// <summary>Levels above the lowest present member a member may be and still share (anti power-leveling).</summary>
        public const int LevelBand = 20;

        /// <summary>Total XP of a kill as a multiple of the solo XP, by present (in-band) members: 1 … 5.</summary>
        public static readonly float[] PoolMultipliers = { 1.00f, 1.20f, 1.50f, 1.80f, 2.20f };

        /// <summary>Pool multiplier for <paramref name="presentInBand"/> members (clamped 1 … <see cref="PartyRules.MaxMembers"/>).</summary>
        public static float PoolMultiplier(int presentInBand)
        {
            int index = Math.Max(1, Math.Min(PoolMultipliers.Length, presentInBand)) - 1;
            return PoolMultipliers[index];
        }

        public static bool IsWithinRange(Vector3 memberPosition, Vector3 killPosition) =>
            (memberPosition - killPosition).sqrMagnitude <= ShareRadius * ShareRadius;

        /// <summary>Alive, here (not <see cref="PartyKillMember.Away"/>) and within <see cref="ShareRadius"/> of the kill. Also gates group loot.</summary>
        public static bool IsEligible(PartyKillMember member, Vector3 killPosition) =>
            member.Alive && !member.Away && !string.IsNullOrEmpty(member.PlayerId) && IsWithinRange(member.Position, killPosition);

        public static PartyXpCut CutOf(PartyKillMember member, Vector3 killPosition)
        {
            if (!member.Alive)
            {
                return PartyXpCut.Dead;
            }

            return IsEligible(member, killPosition) ? PartyXpCut.None : PartyXpCut.TooFar;
        }

        /// <summary>True when <paramref name="level"/> is within <see cref="LevelBand"/> of <paramref name="lowestPresentLevel"/>.</summary>
        public static bool IsInLevelBand(int level, int lowestPresentLevel) =>
            Math.Max(1, level) <= Math.Max(1, lowestPresentLevel) + LevelBand;

        /// <summary>
        /// The kill's XP before any level-difference cut: the asset XP (boss bonus included) for a creature without a
        /// level (<paramref name="creatureLevel"/> ≤ 0, campaign encounters), else what
        /// <see cref="EnemyLevelScaling.LeveledKillExperience(int,int,int,int)"/> gives a hero of the creature's own level.
        /// </summary>
        public static int BaseKillExperience(int assetExperience, int creatureLevel, int rankMultiplier)
        {
            if (creatureLevel <= 0)
            {
                return Math.Max(0, assetExperience);
            }

            return EnemyLevelScaling.LeveledKillExperience(assetExperience, creatureLevel, rankMultiplier, creatureLevel);
        }

        /// <summary>
        /// Splits one kill among <paramref name="members"/> (same order back). <paramref name="assetExperience"/> /
        /// <paramref name="rankMultiplier"/> are what <c>EnemyRewardGranter</c> reads (<c>BossBonusRules.GetExperienceReward</c>,
        /// <c>RegionBossRules.KillXpRankMultiplier</c>); <paramref name="creatureLevel"/> ≤ 0 = a creature without its own level.
        /// </summary>
        public static List<PartyXpShare> Split(int assetExperience, int creatureLevel, int rankMultiplier,
            IReadOnlyList<PartyKillMember> members, Vector3 killPosition)
        {
            var shares = new List<PartyXpShare>();
            if (members == null || members.Count == 0)
            {
                return shares;
            }

            // Who is present, and the band reference (the lowest present level).
            int lowest = int.MaxValue;
            long levelSum = 0;
            var presentCousins = new List<CharacterId?>();
            foreach (PartyKillMember member in members)
            {
                if (IsEligible(member, killPosition))
                {
                    int level = Math.Max(1, member.Level);
                    lowest = Math.Min(lowest, level);
                    levelSum += level;
                    presentCousins.Add(member.Cousin);
                }
            }

            int inBand = 0;
            foreach (PartyKillMember member in members)
            {
                if (IsEligible(member, killPosition) && IsInLevelBand(member.Level, lowest))
                {
                    inBand++;
                }
            }

            int baseExperience = BaseKillExperience(assetExperience, creatureLevel, rankMultiplier);
            double pool = baseExperience * (double)PoolMultiplier(inBand);
            bool familyComplete = PartyFamilyRules.IsFamilyComplete(presentCousins);

            foreach (PartyKillMember member in members)
            {
                PartyXpCut cut = CutOf(member, killPosition);
                if (cut == PartyXpCut.None && !IsInLevelBand(member.Level, lowest))
                {
                    cut = PartyXpCut.OutOfLevelBand;
                }

                if (cut != PartyXpCut.None || baseExperience <= 0 || levelSum <= 0)
                {
                    shares.Add(new PartyXpShare(member.PlayerId, 0, cut, false, false));
                    continue;
                }

                int level = Math.Max(1, member.Level);
                bool sibling = PartyFamilyRules.HasSiblingBond(member.Cousin, presentCousins);
                double part = pool * level / levelSum;
                if (creatureLevel > 0)
                {
                    part *= EnemyLevelScaling.LevelDifferenceXpMultiplier(level, EnemyLevelScaling.ClampLevel(creatureLevel));
                }

                part *= PartyFamilyRules.XpMultiplier(familyComplete, sibling);
                int experience = Math.Max(1, (int)Math.Floor(part + 0.5));
                shares.Add(new PartyXpShare(member.PlayerId, experience, PartyXpCut.None, familyComplete, sibling));
            }

            return shares;
        }
    }
}
