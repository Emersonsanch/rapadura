using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>One web strung between two mulberry trees of the Bosque das Amoreiras (map 04), in the region's own coordinates.</summary>
    public readonly struct ZarafshanWeb
    {
        public readonly string Id;
        public readonly Vector2 A;
        public readonly Vector2 B;

        public ZarafshanWeb(string id, Vector2 a, Vector2 b)
        {
            Id = id;
            A = a;
            B = b;
        }
    }

    /// <summary>
    /// The webs of the Bosque das Amoreiras de Seda (Docs/Mapas100.md 04: "teias: fios brilhantes entre as árvores prendem por 1,5 s (StunRules);
    /// cortáveis com skill"), pure and server-runnable. Eight glowing webs are strung between neighbouring trees of the mulberry rows, across the
    /// alleys of the soft maze (never across a trail). Walking into a whole web holds the hero for 1.5 s (a stun on the BuffController) and tears
    /// it; any skill the hero casts that lands on a web, or cast with the hero beside it, cuts it with no hold. A torn or cut web is spun again
    /// 45 s later, so it can never hold twice in a row. The moment each web broke stays in the save (a time mark).
    /// <code>
    /// 8 webs of 8.5 m (two per block of rows) · caught within 0.7 m of the thread · hold 1.5 s · spun again 45 s after it broke
    /// cut by a skill landing within its area + 2 m of the thread, or cast from within 3 m of it
    /// </code>
    /// </summary>
    public static class ZarafshanWebRules
    {
        public const float CatchReach = 0.7f;
        public const float HoldSeconds = 1.5f;
        public const long RespinSeconds = 45L;
        public const float CutMargin = 2f;
        public const float CutFromReach = 3f;

        /// <summary>The save key of a web (a time mark: the unix second it was last torn or cut, 0 = never).</summary>
        public static string BrokenKey(string webId) => "zarafshan.web." + webId;

        /// <summary>Whether a web broken at <paramref name="brokenAt"/> is whole again at <paramref name="now"/> (unix seconds).</summary>
        public static bool Whole(long brokenAt, long now) => brokenAt <= 0L || now - brokenAt >= RespinSeconds || now < brokenAt;

        /// <summary>Distance (m) from <paramref name="p"/> to the web's thread.</summary>
        public static float Distance(ZarafshanWeb web, Vector2 p) => MapLayoutRules.DistanceToSegment(p, web.A, web.B);

        /// <summary>The whole web within <see cref="CatchReach"/> of <paramref name="p"/>, or −1.</summary>
        public static int CatchAt(IReadOnlyList<ZarafshanWeb> webs, IReadOnlyList<long> brokenMarks, Vector2 p, long now)
        {
            for (int i = 0; webs != null && i < webs.Count; i++)
            {
                long broken = brokenMarks != null && i < brokenMarks.Count ? brokenMarks[i] : 0L;
                if (Whole(broken, now) && Distance(webs[i], p) <= CatchReach)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Whether a skill landing at <paramref name="target"/> with <paramref name="areaRadius"/>, cast from <paramref name="caster"/>, cuts the web.</summary>
        public static bool Cuts(ZarafshanWeb web, Vector2 target, float areaRadius, Vector2 caster) =>
            Distance(web, target) <= Mathf.Max(0f, areaRadius) + CutMargin || Distance(web, caster) <= CutFromReach;
    }
}
