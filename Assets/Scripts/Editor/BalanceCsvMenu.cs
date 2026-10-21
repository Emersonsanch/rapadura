using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Designer round-trip between the balance spreadsheets in <c>Docs/Balance/</c> and the game's
    /// ScriptableObjects (see <c>Docs/Balance/README.md</c>):
    /// <list type="bullet">
    /// <item><b>Export/Import Progression CSV</b> — <c>progression.csv</c> is the single source of truth for the
    /// player level curve; importing writes it into <c>Assets/Resources/Balance/ProgressionCurve.asset</c>.</item>
    /// <item><b>Export Enemy &amp; Skill CSVs</b> — read-only review sheets (<c>enemies.csv</c>, <c>skills.csv</c>)
    /// regenerated from <c>Resources/Enemies</c> and <c>Resources/Skills/**</c>. They are not imported back;
    /// edit those assets (or their seeders) directly.</item>
    /// </list>
    /// </summary>
    public static class BalanceCsvMenu
    {
        private const string CurveAssetPath = "Assets/Resources/Balance/ProgressionCurve.asset";
        private const string EnemyFolder = "Assets/Resources/Enemies";
        private const string SkillFolder = "Assets/Resources/Skills";
        private const string BiomeFolder = "Assets/Resources/Biomes";

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        private static string BalanceDocsFolder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Docs", "Balance");

        private static string ProgressionCsvPath => Path.Combine(BalanceDocsFolder, "progression.csv");

        [MenuItem("Rapadura/Balance/Export Progression CSV")]
        public static void ExportProgressionCsv()
        {
            ProgressionCurve curve = LoadOrCreateCurve();
            if (curve.TableMaxLevel == 0)
            {
                curve.RegenerateFromFormula();
                EditorUtility.SetDirty(curve);
                AssetDatabase.SaveAssets();
            }

            WriteFile(ProgressionCsvPath, ProgressionCsv.Export(curve, Mathf.Max(ProgressionCurve.DefaultMaxLevel, curve.TableMaxLevel)));
            Debug.Log($"[Balance] Exported progression curve to {ProgressionCsvPath}");
        }

        [MenuItem("Rapadura/Balance/Import Progression CSV")]
        public static void ImportProgressionCsv()
        {
            if (!File.Exists(ProgressionCsvPath))
            {
                EditorUtility.DisplayDialog("Import Progression CSV", $"File not found:\n{ProgressionCsvPath}\n\nRun Export Progression CSV first.", "OK");
                return;
            }

            ProgressionCurve curve = LoadOrCreateCurve();
            try
            {
                List<ProgressionLevelRow> rows = ProgressionCsv.Import(File.ReadAllText(ProgressionCsvPath, Encoding.UTF8));
                Undo.RecordObject(curve, "Import Progression CSV");
                curve.SetRows(rows);
            }
            catch (Exception exception) when (exception is FormatException || exception is ArgumentException)
            {
                EditorUtility.DisplayDialog("Import Progression CSV", "The spreadsheet was NOT imported:\n\n" + exception.Message, "OK");
                return;
            }

            EditorUtility.SetDirty(curve);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Balance] Imported {curve.TableMaxLevel} levels from {ProgressionCsvPath} into {CurveAssetPath}");
        }

        [MenuItem("Rapadura/Balance/Regenerate Progression Table From Formula")]
        public static void RegenerateFromFormula()
        {
            ProgressionCurve curve = LoadOrCreateCurve();
            Undo.RecordObject(curve, "Regenerate Progression Table");
            curve.RegenerateFromFormula();
            EditorUtility.SetDirty(curve);
            AssetDatabase.SaveAssets();
            Debug.Log("[Balance] Progression table regenerated from the formula parameters. Export the CSV to update the spreadsheet.");
        }

        [MenuItem("Rapadura/Balance/Export Enemy and Skill CSVs")]
        public static void ExportEnemyAndSkillCsvs()
        {
            WriteFile(Path.Combine(BalanceDocsFolder, "enemies.csv"), BuildEnemiesCsv());
            WriteFile(Path.Combine(BalanceDocsFolder, "skills.csv"), BuildSkillsCsv());
            Debug.Log($"[Balance] Exported enemies.csv and skills.csv to {BalanceDocsFolder}");
        }

        private static ProgressionCurve LoadOrCreateCurve()
        {
            var curve = AssetDatabase.LoadAssetAtPath<ProgressionCurve>(CurveAssetPath);
            if (curve != null)
            {
                return curve;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(CurveAssetPath));
            curve = ScriptableObject.CreateInstance<ProgressionCurve>();
            curve.RegenerateFromFormula();
            AssetDatabase.CreateAsset(curve, CurveAssetPath);
            AssetDatabase.SaveAssets();
            return curve;
        }

        private static void WriteFile(string path, string contents)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, contents, new UTF8Encoding(false));
        }

        private static List<T> LoadAll<T>(string folder) where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
                .Select(path => AssetDatabase.LoadAssetAtPath<T>(path))
                .Where(asset => asset != null)
                .ToList();
        }

        private static string F(float value) => ProgressionCsv.FormatFloat(value);

        private static string R2(double value) => Math.Round(value, 2).ToString("0.##", Invariant);

        private static string Row(params string[] cells) => string.Join(",", cells.Select(CsvUtility.Escape)) + "\n";

        private static string BuildEnemiesCsv()
        {
            var biomeByEnemy = new Dictionary<EnemyDefinition, (string biome, int weight)>();
            foreach (BiomeDefinition biome in LoadAll<BiomeDefinition>(BiomeFolder))
            {
                foreach (BiomeEnemySpawnEntry entry in biome.Enemies)
                {
                    if (entry?.Enemy != null)
                    {
                        biomeByEnemy[entry.Enemy] = (biome.name, entry.Weight);
                    }
                }
            }

            var sb = new StringBuilder();
            sb.Append(Row("id", "name", "biome", "spawn_weight", "is_boss", "max_health", "contact_damage", "attack_damage",
                "attack_cooldown", "attack_dps", "attack_range", "experience_reward", "boss_xp_multiplier",
                "chase_speed", "detection_radius", "loot_drop_chance"));

            foreach (EnemyDefinition enemy in LoadAll<EnemyDefinition>(EnemyFolder))
            {
                biomeByEnemy.TryGetValue(enemy, out (string biome, int weight) spawn);
                double dps = enemy.AttackCooldown > 0f ? enemy.AttackDamage / (double)enemy.AttackCooldown : 0d;
                sb.Append(Row(enemy.name, enemy.EnemyName, spawn.biome ?? string.Empty, spawn.weight.ToString(Invariant),
                    enemy.IsBoss ? "1" : "0", F(enemy.MaxHealth), F(enemy.ContactDamage), F(enemy.AttackDamage),
                    F(enemy.AttackCooldown), R2(dps), F(enemy.AttackRange), enemy.ExperienceReward.ToString(Invariant),
                    F(enemy.BossExperienceMultiplier), F(enemy.ChaseSpeed), F(enemy.DetectionRadius), F(enemy.LootDropChance)));
            }

            return sb.ToString();
        }

        private static float SumTicks(IEnumerable<StatModifierDefinition> effects)
        {
            float total = 0f;
            foreach (StatModifierDefinition effect in effects)
            {
                if (effect != null && effect.ticksOverTime && effect.tickIntervalSeconds > 0f)
                {
                    total += Mathf.Floor(effect.durationSeconds / effect.tickIntervalSeconds) * effect.tickDamageOrHeal;
                }
            }

            return total;
        }

        private static string HeroOf(SkillDefinition skill)
        {
            string hero = skill.Requirement?.requiredClassOrSpecialization;
            if (!string.IsNullOrEmpty(hero))
            {
                return hero;
            }

            string path = AssetDatabase.GetAssetPath(skill).Replace('\\', '/');
            const string marker = "/Characters/";
            int index = path.IndexOf(marker, StringComparison.Ordinal);
            return index < 0 ? string.Empty : path.Substring(index + marker.Length).Split('/')[0];
        }

        private static string BuildSkillsCsv()
        {
            var sb = new StringBuilder();
            sb.Append(Row("id", "name", "hero", "type", "category", "element", "min_player_level", "cooldown", "mana_cost",
                "energy_cost", "cast_time", "recovery", "base_damage", "damage_per_level", "dot_damage",
                "base_healing", "healing_per_level", "hot_healing", "range", "area_radius", "max_level", "buffs",
                "debuffs", "dps_lv1", "damage_per_mana_lv1", "dps_max", "damage_per_mana_max"));

            IEnumerable<SkillDefinition> skills = LoadAll<SkillDefinition>(SkillFolder)
                .OrderBy(skill => HeroOf(skill).Length == 0)
                .ThenBy(skill => HeroOf(skill), StringComparer.Ordinal)
                .ThenBy(skill => skill.SkillId, StringComparer.Ordinal);

            foreach (SkillDefinition skill in skills)
            {
                float dot = SumTicks(skill.Debuffs);
                float hot = SumTicks(skill.Buffs);
                double hit = skill.BaseDamage + dot;
                double maxHit = skill.GetDamageForLevel(skill.MaxLevel) + dot;
                double cycle = Math.Max(skill.CooldownSeconds, skill.CastTimeSeconds + skill.RecoverySeconds);

                sb.Append(Row(skill.SkillId, skill.DisplayName, HeroOf(skill), skill.SkillType.ToString(), skill.Category.ToString(),
                    skill.Element.ToString(), (skill.Requirement?.minimumPlayerLevel ?? 1).ToString(Invariant),
                    F(skill.CooldownSeconds), F(skill.ManaCost), F(skill.EnergyCost), F(skill.CastTimeSeconds), F(skill.RecoverySeconds),
                    F(skill.BaseDamage), F(skill.GetDamageForLevel(2) - skill.GetDamageForLevel(1)), F(dot),
                    F(skill.BaseHealing), F(skill.GetHealingForLevel(2) - skill.GetHealingForLevel(1)), F(hot),
                    F(skill.Range), F(skill.AreaRadius), skill.MaxLevel.ToString(Invariant),
                    skill.Buffs.Length.ToString(Invariant), skill.Debuffs.Length.ToString(Invariant),
                    R2(cycle > 0 ? hit / cycle : 0), R2(skill.ManaCost > 0 ? hit / skill.ManaCost : 0),
                    R2(cycle > 0 ? maxHit / cycle : 0), R2(skill.ManaCost > 0 ? maxHit / skill.ManaCost : 0)));
            }

            return sb.ToString();
        }
    }
}
