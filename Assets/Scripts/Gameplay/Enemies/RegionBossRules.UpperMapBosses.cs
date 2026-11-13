using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Six more map bosses of Regions 10–16, one in each of 10, 11, 12, 13, 15 and 16 (Docs/Mapas100.md 57, 63, 67, 74, 86, 97): the numbers and the
    /// pure rules of their abilities (<see cref="RegionBossSignature"/> 60–65). Their spawns are in <see cref="MapSpawns"/> with the others; the fight in
    /// <see cref="RegionBossAbilities"/>.
    /// </summary>
    public static partial class RegionBossRules
    {
        // ---- O Rei dos Ays (57): the gust down the gorge ----

        public const float AysIntervalSeconds = 10f;
        public const float AysWindupSeconds = 2.2f;
        public const float AysDamageFactor = 1.35f;

        /// <summary>How far downwind of the king the gust runs, and how wide it is either side of its line.</summary>
        public const float AysGustLength = 16f;

        public const float AysGustHalfWidth = 2.5f;

        /// <summary>The axis of the Garganta dos Ays (the ficha's wind, 0,8/0,6 like the map's own gusts): up the gorge on even gusts, down it on odd ones.</summary>
        public static readonly Vector2 AysGorgeAxis = new Vector2(0.8f, 0.6f);

        // ---- A Aurora Presa (63): stay on the lit route ----

        public const float AuroraIntervalSeconds = 11f;
        public const float AuroraWindupSeconds = 2.4f;
        public const float AuroraDamageFactor = 1.3f;

        /// <summary>How far round it the ice cracks; the route it lights runs this far from it too, this wide either side.</summary>
        public const float AuroraReach = 12f;

        public const float AuroraRouteHalfWidth = 2f;

        /// <summary>Each dance the route turns this many degrees on (clockwise from north): three routes, then the first again.</summary>
        public const float AuroraTurnDegrees = 120f;

        public const int AuroraDances = 3;

        // ---- Guayota, o que Prendeu o Sol (67): the squares of the slope ----

        public const float CraterIntervalSeconds = 10f;
        public const float CraterWindupSeconds = 2.2f;
        public const float CraterDamageFactor = 1.4f;

        /// <summary>The side of a square of the slope; the lava falls on every other one, within this reach of where he stood.</summary>
        public const float CraterSquare = 4f;

        public const float CraterReach = 12f;

        // ---- O Nó Errado (74): the two loops beside it ----

        public const float KnotIntervalSeconds = 10f;
        public const float KnotWindupSeconds = 2.2f;
        public const float KnotDamageFactor = 1.35f;

        /// <summary>The two loops lie this far either side of the knot, across the line to the hero, this wide.</summary>
        public const float KnotLoopOffset = 4f;

        public const float KnotLoopRadius = 3f;

        // ---- A Águia de Asas de Treva (86): the two beats of the wings ----

        public const float WingIntervalSeconds = 10f;
        public const float WingWindupSeconds = 2f;
        public const float WingDamageFactor = 1.4f;

        /// <summary>The shadow of the wings: a bar this deep along her flight and this long across it, on the hero's spot and again a beat further on.</summary>
        public const float WingBarHalfDepth = 1.6f;

        public const float WingSpanHalf = 7f;

        public const float WingBeatGap = 5f;

        // ---- A Sombra da Vela (97): the boom swings across ----

        public const float BoomIntervalSeconds = 10f;
        public const float BoomWindupSeconds = 2f;
        public const float BoomDamageFactor = 1.35f;

        /// <summary>Under the mast is safe; the boom reaches this far; it swings to starboard (east) on even swings, to port (west) on odd ones.</summary>
        public const float BoomInner = 2.5f;

        public const float BoomOuter = 9f;

        /// <summary>The flat direction the gust of O Rei dos Ays blows on gust number <paramref name="gust"/> (0 first: up the gorge).</summary>
        public static Vector2 AysGustDirection(int gust) => Mathf.Abs(gust) % 2 == 0 ? AysGorgeAxis.normalized : -AysGorgeAxis.normalized;

        /// <summary>Does gust number <paramref name="gust"/> of O Rei dos Ays (at <paramref name="king"/>) catch a hero at <paramref name="hero"/>? Downwind of him, on its line; upwind and aside are safe.</summary>
        public static bool AysGustHits(int gust, Vector2 king, Vector2 hero)
        {
            Vector2 along = AysGustDirection(gust);
            Vector2 offset = hero - king;
            float ahead = Vector2.Dot(offset, along);
            if (ahead < 0f || ahead > AysGustLength)
            {
                return false;
            }

            return Mathf.Abs(along.x * offset.y - along.y * offset.x) <= AysGustHalfWidth;
        }

        /// <summary>Which of the three routes A Aurora Presa lights on dance number <paramref name="dance"/>.</summary>
        public static int AuroraDanceOf(int dance) => Mathf.Abs(dance) % AuroraDances;

        /// <summary>The flat direction of the route A Aurora Presa lights on dance number <paramref name="dance"/> (0 first: north, then 120° on, clockwise).</summary>
        public static Vector2 AuroraRouteDirection(int dance)
        {
            float a = AuroraDanceOf(dance) * AuroraTurnDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(a), Mathf.Cos(a));
        }

        /// <summary>Is a hero at <paramref name="hero"/> on the route A Aurora Presa (at <paramref name="aurora"/>) lights on dance number <paramref name="dance"/>?</summary>
        public static bool OnAuroraRoute(int dance, Vector2 aurora, Vector2 hero)
        {
            Vector2 along = AuroraRouteDirection(dance);
            Vector2 offset = hero - aurora;
            float ahead = Vector2.Dot(offset, along);
            return ahead >= 0f && ahead <= AuroraReach && Mathf.Abs(along.x * offset.y - along.y * offset.x) <= AuroraRouteHalfWidth;
        }

        /// <summary>Does the ice round A Aurora Presa crack under a hero at <paramref name="hero"/>? Within her reach, everywhere but the lit route.</summary>
        public static bool AuroraRouteHits(int dance, Vector2 aurora, Vector2 hero) =>
            (hero - aurora).sqrMagnitude <= AuroraReach * AuroraReach && !OnAuroraRoute(dance, aurora, hero);

        /// <summary>Is the square of the slope under <paramref name="hero"/> one the lava falls on in eruption number <paramref name="eruption"/>, the squares laid out from <paramref name="crater"/>?</summary>
        public static bool CraterSquareIsHot(int eruption, Vector2 crater, Vector2 hero)
        {
            int i = Mathf.FloorToInt((hero.x - crater.x) / CraterSquare);
            int j = Mathf.FloorToInt((hero.y - crater.y) / CraterSquare);
            return ((i + j + Mathf.Abs(eruption)) & 1) == 0;
        }

        /// <summary>Does eruption number <paramref name="eruption"/> of Guayota (who stood at <paramref name="crater"/>) burn a hero at <paramref name="hero"/>? Within his reach, on a hot square.</summary>
        public static bool CraterSpitHits(int eruption, Vector2 crater, Vector2 hero) =>
            (hero - crater).sqrMagnitude <= CraterReach * CraterReach && CraterSquareIsHot(eruption, crater, hero);

        /// <summary>The centres of the two loops of O Nó Errado (at <paramref name="knot"/>), across the line to the hero's spot <paramref name="mark"/>.</summary>
        public static Vector2[] KnotLoopCentres(Vector2 knot, Vector2 mark)
        {
            Vector2 line = mark - knot;
            Vector2 along = line.sqrMagnitude > 0.01f ? line.normalized : Vector2.up;
            Vector2 across = new Vector2(along.y, -along.x);
            return new[] { knot + across * KnotLoopOffset, knot - across * KnotLoopOffset };
        }

        /// <summary>Does O Nó Errado (at <paramref name="knot"/>, the hero's spot at the warning <paramref name="mark"/>) catch a hero at <paramref name="hero"/> in one of its loops?</summary>
        public static bool KnotLoopsHit(Vector2 knot, Vector2 mark, Vector2 hero)
        {
            foreach (Vector2 loop in KnotLoopCentres(knot, mark))
            {
                if ((hero - loop).sqrMagnitude <= KnotLoopRadius * KnotLoopRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the shadow of the wings of A Águia de Asas de Treva, flying from <paramref name="eagle"/> through the hero's spot <paramref name="mark"/>, fall on a
        /// hero at <paramref name="hero"/>? Two bars across her flight, on the mark and <see cref="WingBeatGap"/> m past it; under her, short of the mark, is safe.
        /// </summary>
        public static bool WingSpanHits(Vector2 eagle, Vector2 mark, Vector2 hero)
        {
            Vector2 line = mark - eagle;
            float reach = line.magnitude;
            Vector2 along = reach > 0.1f ? line / reach : Vector2.up;
            Vector2 offset = hero - eagle;
            if (Mathf.Abs(along.x * offset.y - along.y * offset.x) > WingSpanHalf)
            {
                return false;
            }

            float ahead = Vector2.Dot(offset, along);
            return Mathf.Abs(ahead - reach) <= WingBarHalfDepth || Mathf.Abs(ahead - reach - WingBeatGap) <= WingBarHalfDepth;
        }

        /// <summary>Does swing number <paramref name="swing"/> of A Sombra da Vela swing the boom to starboard (east, even swings) or to port (west)?</summary>
        public static bool BoomSwingsStarboard(int swing) => Mathf.Abs(swing) % 2 == 0;

        /// <summary>Does the boom of A Sombra da Vela (at <paramref name="mast"/>) catch a hero at <paramref name="hero"/>? On the side it swings to, between the mast and its end.</summary>
        public static bool BoomSwingHits(bool starboard, Vector2 mast, Vector2 hero)
        {
            Vector2 offset = hero - mast;
            float distance = offset.magnitude;
            if (distance <= BoomInner || distance > BoomOuter)
            {
                return false;
            }

            return starboard ? offset.x > 0f : offset.x < 0f;
        }

        private static float UpperMapBossInterval(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.GorgeGust: return AysIntervalSeconds;
                case RegionBossSignature.AuroraRoute: return AuroraIntervalSeconds;
                case RegionBossSignature.CraterSpit: return CraterIntervalSeconds;
                case RegionBossSignature.CoilLoops: return KnotIntervalSeconds;
                case RegionBossSignature.WingSpan: return WingIntervalSeconds;
                default: return BoomIntervalSeconds;
            }
        }

        private static float UpperMapBossWindup(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.GorgeGust: return AysWindupSeconds;
                case RegionBossSignature.AuroraRoute: return AuroraWindupSeconds;
                case RegionBossSignature.CraterSpit: return CraterWindupSeconds;
                case RegionBossSignature.CoilLoops: return KnotWindupSeconds;
                case RegionBossSignature.WingSpan: return WingWindupSeconds;
                default: return BoomWindupSeconds;
            }
        }
    }
}
