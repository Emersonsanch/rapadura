using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.ClassEvolution
{
    /// <summary>Where a cousin's next graduation mission stands.</summary>
    public enum GraduationMissionStatus
    {
        /// <summary>Already at <see cref="ClassEvolutionCatalog.MaxTier"/>.</summary>
        AllDone = 0,
        /// <summary>The next mission exists but the hero's level is too low.</summary>
        LevelTooLow = 1,
        /// <summary>The mentor can hand it out now.</summary>
        Available = 2,
        /// <summary>Accepted, kills still missing.</summary>
        InProgress = 3,
        /// <summary>Kills done: talk to the mentor to graduate.</summary>
        ReadyToTurnIn = 4
    }

    /// <summary>Per-cousin graduation state (also the save record). Plain and JSON-friendly.</summary>
    [Serializable]
    public class GraduationRecord
    {
        public string characterId;
        /// <summary>Graduations completed, 0..3.</summary>
        public int tier;
        /// <summary>Tier of the accepted mission (tier + 1), 0 = none accepted.</summary>
        public int activeMissionTier;
        public int missionKills;
        public int masterLevel;
        public long masterExperience;
        public List<MasterNodeRank> masterNodes = new List<MasterNodeRank>();
    }

    /// <summary>Ranks spent in one master-tree node.</summary>
    [Serializable]
    public class MasterNodeRank
    {
        public string nodeId;
        public int rank;
    }

    /// <summary>
    /// Pure rules of the class evolution (Docs/ClassEvolution.md §1): which mission is next, accepting it, counting kills in
    /// the mission's region and graduating. No Unity, no IO: the same calls run on the future server, which owns the record.
    /// </summary>
    public static class ClassEvolutionRules
    {
        public static int ClampTier(int tier) => Math.Max(0, Math.Min(ClassEvolutionCatalog.MaxTier, tier));

        /// <summary>The graduation the cousin would take next, or null at the max tier.</summary>
        public static GraduationSpec NextGraduation(CharacterId character, int tier) =>
            tier >= ClassEvolutionCatalog.MaxTier ? null : ClassEvolutionCatalog.GetGraduation(character, ClampTier(tier) + 1);

        public static GraduationMissionStatus Status(CharacterId character, GraduationRecord record, int heroLevel)
        {
            int tier = record != null ? ClampTier(record.tier) : 0;
            GraduationSpec next = NextGraduation(character, tier);
            if (next == null)
            {
                return GraduationMissionStatus.AllDone;
            }

            if (record != null && record.activeMissionTier == next.Tier)
            {
                return record.missionKills >= next.MissionKills ? GraduationMissionStatus.ReadyToTurnIn : GraduationMissionStatus.InProgress;
            }

            return heroLevel >= next.MinLevel ? GraduationMissionStatus.Available : GraduationMissionStatus.LevelTooLow;
        }

        /// <summary>Accepts the next mission when it is <see cref="GraduationMissionStatus.Available"/>. True when accepted.</summary>
        public static bool Accept(CharacterId character, GraduationRecord record, int heroLevel)
        {
            if (record == null || Status(character, record, heroLevel) != GraduationMissionStatus.Available)
            {
                return false;
            }

            record.activeMissionTier = ClampTier(record.tier) + 1;
            record.missionKills = 0;
            return true;
        }

        /// <summary>
        /// Counts one kill for the accepted mission when it happened in the mission's region (bosses count once, like any
        /// creature). Never goes past the target. True when the count moved.
        /// </summary>
        public static bool RecordKill(CharacterId character, GraduationRecord record, BiomeType? region)
        {
            if (record == null || record.activeMissionTier <= 0 || region == null)
            {
                return false;
            }

            GraduationSpec mission = ClassEvolutionCatalog.GetGraduation(character, record.activeMissionTier);
            if (mission == null || mission.MissionRegion != region.Value || record.missionKills >= mission.MissionKills)
            {
                return false;
            }

            record.missionKills++;
            return true;
        }

        /// <summary>Talking to the mentor with the kills done: graduates (tier + 1) and clears the mission. Returns the new graduation or null.</summary>
        public static GraduationSpec TurnIn(CharacterId character, GraduationRecord record, int heroLevel)
        {
            if (record == null || Status(character, record, heroLevel) != GraduationMissionStatus.ReadyToTurnIn)
            {
                return null;
            }

            GraduationSpec done = ClassEvolutionCatalog.GetGraduation(character, record.activeMissionTier);
            record.tier = done.Tier;
            record.activeMissionTier = 0;
            record.missionKills = 0;
            return done;
        }

        /// <summary>Repairs a record read from a save (bad tiers, mission of another tier, negative counts).</summary>
        public static void Sanitize(CharacterId character, GraduationRecord record)
        {
            if (record == null)
            {
                return;
            }

            record.characterId = character.ToString();
            record.tier = ClampTier(record.tier);
            if (record.activeMissionTier != record.tier + 1 || record.tier >= ClassEvolutionCatalog.MaxTier)
            {
                record.activeMissionTier = 0;
            }

            GraduationSpec mission = record.activeMissionTier > 0 ? ClassEvolutionCatalog.GetGraduation(character, record.activeMissionTier) : null;
            record.missionKills = mission == null ? 0 : Math.Max(0, Math.Min(mission.MissionKills, record.missionKills));
            MasterLevelRules.Sanitize(record);
        }
    }

    /// <summary>
    /// Pure class-combo tracker (Docs/ClassEvolution.md §3): one per caster, fed with skill ids and an explicit clock, so the
    /// server can run it as-is. Same window rule as <c>ComboTracker</c>: each cast within <see cref="ClassComboSpec.WindowSeconds"/>
    /// of the previous; a wrong cast resets, and restarts the sequence if it is the first skill.
    /// </summary>
    public sealed class ClassComboTracker
    {
        private int _step;
        private double _lastCast;

        public ClassComboSpec Combo { get; }

        public ClassComboTracker(ClassComboSpec combo)
        {
            Combo = combo;
        }

        public int Step => _step;

        /// <summary>Feeds one cast. True when it completed the combo (then the tracker starts over).</summary>
        public bool OnCast(string skillId, double now)
        {
            if (Combo == null || Combo.SkillIds.Count == 0 || string.IsNullOrEmpty(skillId))
            {
                return false;
            }

            bool inWindow = _step == 0 || now - _lastCast <= Combo.WindowSeconds;
            if (inWindow && skillId == Combo.SkillIds[_step])
            {
                return Advance(now);
            }

            _step = 0;
            return skillId == Combo.SkillIds[0] && Advance(now);
        }

        public void Reset() => _step = 0;

        private bool Advance(double now)
        {
            _step++;
            _lastCast = now;
            if (_step < Combo.SkillIds.Count)
            {
                return false;
            }

            _step = 0;
            return true;
        }
    }

    /// <summary>One candidate ally for a group buff: an id and a flat position (x/z, no UnityEngine type).</summary>
    public readonly struct SupportCandidate
    {
        public readonly string Id;
        public readonly float X;
        public readonly float Z;
        public readonly bool Alive;

        public SupportCandidate(string id, float x, float z, bool alive = true)
        {
            Id = id;
            X = x;
            Z = z;
            Alive = alive;
        }
    }

    /// <summary>
    /// Who a group support buff reaches (Docs/ClassEvolution.md §2): the caster always, plus up to <see cref="MaxAllies"/> living
    /// allies within the skill radius, closest first. Today the hero plays alone (the duo partner is a silhouette, not a
    /// fighter), so only the caster gets it; the party of FASE 12 reuses this on the server.
    /// </summary>
    public static class PartySupportRules
    {
        /// <summary>MU-style party of 5: caster + 4.</summary>
        public const int MaxAllies = 4;

        public static List<string> SelectAllies(float casterX, float casterZ, float radius, IEnumerable<SupportCandidate> candidates)
        {
            var inRange = new List<(string Id, float Distance)>();
            if (candidates != null && radius > 0f)
            {
                float r2 = radius * radius;
                foreach (SupportCandidate c in candidates)
                {
                    if (!c.Alive || string.IsNullOrEmpty(c.Id))
                    {
                        continue;
                    }

                    float dx = c.X - casterX, dz = c.Z - casterZ;
                    float d2 = dx * dx + dz * dz;
                    if (d2 <= r2)
                    {
                        inRange.Add((c.Id, d2));
                    }
                }
            }

            inRange.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            var result = new List<string>();
            foreach ((string id, float _) in inRange)
            {
                if (result.Count >= MaxAllies)
                {
                    break;
                }

                if (!result.Contains(id))
                {
                    result.Add(id);
                }
            }

            return result;
        }
    }
}
