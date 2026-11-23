using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>
    /// Which real beast plays each event role in each of the 7 rooms of the Vigília da Coluna and the Cerco da Treva
    /// (<see cref="TimedEventCatalog.SevenRooms"/>), decided 2026-11-23 (Docs/Network.md §14). The wave rules stay asset-free
    /// (<see cref="TimedWaveRules"/> spends its budget in <see cref="TimedEnemyRole"/>s); the server looks the id up here and
    /// spawns that <c>EnemyDefinition</c> (Resources/Enemies) at the wave's level. Each room uses the beasts of the region whose
    /// monster band holds the room's middle level (the wave level is the middle − 6, +2 per wave), preferring the region's own
    /// beasts over borrowed stand-ins and the "Treva" ones (shadows, smoke, dimmed) where the region has them:
    /// <list type="table">
    /// <item><term>1 (15–79)</term><description>Costa das Monções / Grutas do Limiar (maps 22–30).</description></item>
    /// <item><term>2 (80–149)</term><description>Planície de Mingcheng / Estepe dos Nartes (41–52).</description></item>
    /// <item><term>3 (150–219)</term><description>Picos de Farnkesh / Mar Gelado (53–64).</description></item>
    /// <item><term>4 (220–289)</term><description>Arquipélago de Hinokura (65–70).</description></item>
    /// <item><term>5 (290–359)</term><description>Margens da Mistura (77–82).</description></item>
    /// <item><term>6 (360–429)</term><description>Limiar Profundo (83–90).</description></item>
    /// <item><term>7 (430–500)</term><description>Reinos do Céu (91–100).</description></item>
    /// </list>
    /// Casters are ranged beasts; Brutes and Archons are heavy ones (EditMode-tested). No region boss is used: the Archon is
    /// a heavy beast the server buffs as the event's closer, so the world bosses keep their own fights.
    /// </summary>
    public static class TimedEventEnemyRoster
    {
        public const int RoomCount = 7;

        // Order of each row: Shade, Runner, Caster, Brute, Elite, Archon (TimedEnemyRole 0–5).
        private static readonly string[][] Rows =
        {
            // 1 — Costa das Monções / Grutas do Limiar
            new[]
            {
                "enemy_cave_ooze", "enemy_desert_black_sand_jackal", "enemy_cave_kongamato",
                "enemy_mountain_dimmed_stone_ram", "enemy_desert_young_scorpion_king", "enemy_mountain_kushite_ushabti",
            },
            // 2 — Planície de Mingcheng / Estepe dos Nartes
            new[]
            {
                "enemy_forest_terracotta_soldier", "enemy_mountain_steppe_wolf", "enemy_cave_smoke_messenger",
                "enemy_mountain_sleepwalker_giant", "enemy_forest_taotie_servant", "enemy_forest_lime_guardian",
            },
            // 3 — Picos de Farnkesh / Mar Gelado
            new[]
            {
                "enemy_forest_dark_bat", "enemy_forest_little_shadow", "enemy_mountain_shadow_kite",
                "enemy_mountain_smoke_walrus", "enemy_mountain_smoke_harpy", "enemy_mountain_rope_ushabti",
            },
            // 4 — Arquipélago de Hinokura
            new[]
            {
                "enemy_cave_living_ash", "enemy_mountain_devouring_salamander", "enemy_mountain_ember_gull",
                "enemy_mountain_furnace_ogre", "enemy_mountain_lacquer_ushabti", "enemy_mountain_slag_giant",
            },
            // 5 — Margens da Mistura
            new[]
            {
                "enemy_forest_late_shadow", "enemy_mountain_pitch_smoke", "enemy_forest_tar_heron",
                "enemy_forest_living_tar", "enemy_mountain_clock_ushabti", "enemy_forest_ullikummi_son",
            },
            // 6 — Limiar Profundo
            new[]
            {
                "enemy_forest_copy_shadow", "enemy_forest_ember_wolf", "enemy_forest_smoke_archer",
                "enemy_forest_smoke_giant", "enemy_forest_lesser_shadow_dragon", "enemy_forest_lesser_archon",
            },
            // 7 — Reinos do Céu
            new[]
            {
                "enemy_forest_hold_shadow", "enemy_forest_dark_stowaway", "enemy_forest_cracked_jar",
                "enemy_cave_dark_drop", "enemy_forest_fled_archon", "enemy_forest_chained_archon",
            },
        };

        public static readonly TimedEnemyRole[] Roles =
        {
            TimedEnemyRole.Shade, TimedEnemyRole.Runner, TimedEnemyRole.Caster,
            TimedEnemyRole.Brute, TimedEnemyRole.Elite, TimedEnemyRole.Archon,
        };

        /// <summary>The enemy id of a role in a room (1-based, clamped to 1–7).</summary>
        public static string EnemyFor(TimedEnemyRole role, int roomIndex)
        {
            int room = Math.Max(1, Math.Min(RoomCount, roomIndex));
            int column = (int)role;
            string[] row = Rows[room - 1];
            return column >= 0 && column < row.Length ? row[column] : row[0];
        }

        /// <summary>The enemy id of a role for a level (the room whose band holds it; below 15 → room 1, above 500 → room 7).</summary>
        public static string EnemyForLevel(TimedEnemyRole role, int level)
        {
            TimedEventRoomBand[] bands = TimedEventCatalog.SevenRooms;
            int room = level < bands[0].MinLevel ? 1 : RoomCount;
            foreach (TimedEventRoomBand band in bands)
            {
                if (band.Contains(level))
                {
                    room = band.Index;
                    break;
                }
            }

            return EnemyFor(role, room);
        }

        /// <summary>Every (room, role, id) of the table, room by room.</summary>
        public static IEnumerable<(int Room, TimedEnemyRole Role, string EnemyId)> All()
        {
            for (int room = 1; room <= RoomCount; room++)
            {
                foreach (TimedEnemyRole role in Roles)
                {
                    yield return (room, role, EnemyFor(role, room));
                }
            }
        }

        /// <summary>The ids a wave spawns, group by group (the server spawns each at <see cref="TimedWave"/>'s level).</summary>
        public static List<(string EnemyId, int Count, int Gate)> Resolve(TimedWave wave, int roomIndex)
        {
            var list = new List<(string, int, int)>();
            if (wave?.Groups == null)
            {
                return list;
            }

            foreach (TimedWaveGroup group in wave.Groups)
            {
                list.Add((EnemyFor(group.Role, roomIndex), group.Count, group.Gate));
            }

            return list;
        }
    }
}
