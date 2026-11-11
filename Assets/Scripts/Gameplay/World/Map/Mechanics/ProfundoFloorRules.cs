using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The moment of the glowing floor's cycle (<see cref="ProfundoFloorRules.PhaseAt"/>).</summary>
    public enum ProfundoFloorPhase
    {
        Quiet = 0,
        Warning = 1,
        Burning = 2,
    }

    /// <summary>
    /// The Floor That Lights (Docs/Mapas100.md, map 85: "o padrão do chão avisa 1,5 s antes; ficar nas pedras escuras"), pure. The floor is a
    /// grid of tiles; each cycle picks one of four patterns, and in all of them every lit tile has four dark neighbours, so a step or two always
    /// reaches a dark stone within the warning.
    /// <code>
    /// tile 8 m · cycle 9 s: quiet 5 s · warning 1.5 s · burning 2.5 s
    /// patterns in turn: checker, the other checker, diagonal (x+y) mod 3, diagonal (x−y) mod 3
    /// burn: 1 tick per 0.75 s on a lit tile, 4 % of max life, down to 25 % of max life at most
    /// </code>
    /// </summary>
    public static class ProfundoFloorRules
    {
        public const float TileSize = 8f;
        public const float QuietSeconds = 5f;
        public const float WarningSeconds = 1.5f;
        public const float BurnSeconds = 2.5f;
        public const float CycleSeconds = QuietSeconds + WarningSeconds + BurnSeconds;
        public const int PatternCount = 4;

        public const float BiteTickSeconds = 0.75f;
        public const float BiteFraction = 0.04f;
        public const float BiteFloorFraction = 0.25f;

        public static ProfundoFloorPhase PhaseAt(float time)
        {
            float t = Mathf.Repeat(time, CycleSeconds);
            return t < QuietSeconds ? ProfundoFloorPhase.Quiet : t < QuietSeconds + WarningSeconds ? ProfundoFloorPhase.Warning : ProfundoFloorPhase.Burning;
        }

        /// <summary>The pattern of the cycle running at <paramref name="time"/>.</summary>
        public static int PatternAt(float time) => Mod(Mathf.FloorToInt(time / CycleSeconds), PatternCount);

        public static Vector2Int Cell(Vector2 p) => new Vector2Int(Mathf.FloorToInt(p.x / TileSize), Mathf.FloorToInt(p.y / TileSize));

        /// <summary>The centre of a tile.</summary>
        public static Vector2 CellCenter(Vector2Int cell) => new Vector2((cell.x + 0.5f) * TileSize, (cell.y + 0.5f) * TileSize);

        /// <summary>Whether a tile lights in a pattern; the coordinates may be negative (the area is south of the plaza).</summary>
        public static bool IsLit(int pattern, Vector2Int cell)
        {
            switch (Mod(pattern, PatternCount))
            {
                case 0: return Mod(cell.x + cell.y, 2) == 0;
                case 1: return Mod(cell.x + cell.y, 2) == 1;
                case 2: return Mod(cell.x + cell.y, 3) == 0;
                default: return Mod(cell.x - cell.y, 3) == 0;
            }
        }

        public static float Bite(float currentHealth, float maxHealth) => ProfundoHazardRules.Bite(currentHealth, maxHealth, BiteFraction, BiteFloorFraction);

        private static int Mod(int value, int divisor)
        {
            int m = value % divisor;
            return m < 0 ? m + divisor : m;
        }
    }
}
