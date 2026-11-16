using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The tunnels that change of the Galerias dos Mamutes (62, <see cref="MammothGalleryRules"/>): a pillar at each of the six mouths on the
    /// three roads out of the Boca do Gelo, amber when this week's tunnel runs from it, grey when the mammoth caved it in. At an open mouth [E]
    /// goes down the tunnel and out at the mouth it leads to on another road; at a caved-in one the line says when the mammoth digs again. The
    /// week (and so the tunnels) comes from the real clock alone: nothing is saved. The roads themselves always join the three mouths' roads.
    /// </summary>
    public sealed partial class LateRouteMechanicsDriver
    {
        private const int GalleryLineKey = 200;
        private const int GallerySpot = 200;
        private const float TunnelHover = 0.2f;

        /// <summary>Where the gallery part runs (region coordinates of the Mar Gelado): the Galerias dos Mamutes round the Boca do Gelo.</summary>
        public static readonly Rect GalleryBounds = Rect.MinMaxRect(820f, 380f, 1180f, 840f);

        private static readonly Color MouthOpenColor = new Color(1f, 0.72f, 0.3f, 1f);
        private static readonly Color MouthCavedColor = new Color(0.45f, 0.4f, 0.36f, 1f);

        private long _galleryWeek = long.MinValue;
        private int[] _tunnels;

        /// <summary>True while the hero stands at a tunnel mouth.</summary>
        private bool TickGallery(Vector2 local, double now)
        {
            if (!GalleryBounds.Contains(local))
            {
                return false;
            }

            long week = MammothGalleryRules.WeekAt(now);
            if (_tunnels == null || week != _galleryWeek)
            {
                _galleryWeek = week;
                _tunnels = MammothGalleryRules.TunnelsFor(week);
            }

            Vector2[] mouths = OverworldLayouts.MammothGalleryMouths;
            for (int i = 0; i < mouths.Length; i++)
            {
                bool open = _tunnels[i] >= 0;
                Pillar(GalleryLineKey + i, mouths[i] + _geladoOrigin, open ? 5f : 2.5f, open ? 0.6f : 1.2f, open ? MouthOpenColor : MouthCavedColor);
            }

            if (!CanInteract)
            {
                return false;
            }

            int mouth = MammothGalleryRules.MouthAt(mouths, local);
            if (mouth < 0)
            {
                return false;
            }

            int target = _tunnels[mouth];
            Vector2[] course = target >= 0 ? new[] { mouths[mouth], mouths[target] } : null;
            GalleryMouthCheck check = MammothGalleryRules.Check(_tunnels, mouth, course != null && CourseOpen(_geladoOrigin, course));
            int spot = GallerySpot + mouth;
            if (check == GalleryMouthCheck.CavedIn)
            {
                double left = MammothGalleryRules.SecondsToNextWeek(now);
                Hint(spot, Format(LateRouteMechanicsTexts.TunnelCaved, (int)(left / 86400d), (int)(left % 86400d / 3600d)));
                return true;
            }

            if (check == GalleryMouthCheck.ZoneBlocked)
            {
                HintKey(spot, LateRouteMechanicsTexts.ZoneBlocked);
                return true;
            }

            string road = Text(RoadKey(MammothGalleryRules.RoadOf(target)));
            Hint(spot, Format(LateRouteMechanicsTexts.TunnelPrompt, road));
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Pressed && BeginRide(_geladoOrigin, course, false, MammothGalleryRules.TunnelSpeed, 0f, TunnelHover, LateRouteMechanicsTexts.TunnelOut, road))
            {
                Say(LateRouteMechanicsTexts.TunnelDown);
            }

            return true;
        }

        /// <summary>The text key of road <paramref name="road"/> (0 south, 1 east, 2 north).</summary>
        public static string RoadKey(int road)
        {
            switch (road)
            {
                case 0:
                    return LateRouteMechanicsTexts.RoadSouth;
                case 1:
                    return LateRouteMechanicsTexts.RoadEast;
                default:
                    return LateRouteMechanicsTexts.RoadNorth;
            }
        }
    }
}
