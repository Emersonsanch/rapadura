using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The floes of the Mar das Auroras (63, <see cref="AuroraFloeRules"/>): the floes drawn as short white bars drifting across the line of the
    /// chain (green-lilac and in line while the aurora dances, with the aurora's band over them; still and in line all night). At either end of
    /// the chain [E] hops across, floe to floe (one short arc a floe); once the hop has begun the hero lands on each floe wherever it drifts, so
    /// a dance that ends mid-way never drops anyone in the sea. While the floes drift the line says when the aurora dances next. The way round by
    /// the sea fork is always open.
    /// </summary>
    public sealed partial class LateRouteMechanicsDriver
    {
        private const int FloeLineKey = 300;
        private const int FloeSpot = 300;
        private const float FloeHover = 0.3f;
        private const float FloeHalfLength = 1.6f;
        private const float AuroraHeight = 28f;

        /// <summary>Where the floe part runs (region coordinates of the Mar Gelado): the chain from the north road to the lighthouse road.</summary>
        public static readonly Rect FloeBounds = Rect.MinMaxRect(0f, 1030f, 400f, 1230f);

        private static readonly Color FloeColor = new Color(0.88f, 0.94f, 1f, 1f);
        private static readonly Color FloeLitColor = new Color(0.5f, 0.88f, 0.69f, 1f);
        private static readonly Color AuroraColor = new Color(0.73f, 0.55f, 0.88f, 0.55f);
        private static readonly Color FloeEndColor = new Color(0.5f, 0.88f, 0.69f, 1f);

        private int _hopLeg = -1;
        private bool _hopReverse;
        private bool _floeDanceSaid;

        /// <summary>True while the hero stands at an end of the chain.</summary>
        private bool TickFloes(Vector2 local, double now)
        {
            if (!FloeBounds.Contains(local))
            {
                return false;
            }

            bool night = IsNight();
            Vector2[] chain = OverworldLayouts.AuroraFloeChain;
            ShowFloes(chain, now, night);
            bool dancing = AuroraFloeRules.Dancing(now);
            if (!night && dancing && !_floeDanceSaid)
            {
                Say(LateRouteMechanicsTexts.FloeDance);
            }

            _floeDanceSaid = dancing;
            if (!CanInteract)
            {
                return false;
            }

            int end = AuroraFloeRules.EndAt(chain, local);
            if (end < 0)
            {
                return false;
            }

            FloeCheck check = AuroraFloeRules.Check(now, night, CourseOpen(_geladoOrigin, chain));
            int spot = FloeSpot + end;
            if (check == FloeCheck.Drifting)
            {
                int wait = Mathf.CeilToInt(AuroraFloeRules.SecondsToDance(now));
                Hint(spot, Format(LateRouteMechanicsTexts.FloeDrifting, wait / 60, wait % 60));
                return true;
            }

            if (check == FloeCheck.ZoneBlocked)
            {
                HintKey(spot, LateRouteMechanicsTexts.ZoneBlocked);
                return true;
            }

            if (night)
            {
                HintKey(spot, LateRouteMechanicsTexts.FloePromptNight);
            }
            else
            {
                Hint(spot, Format(LateRouteMechanicsTexts.FloePrompt, Mathf.CeilToInt(AuroraFloeRules.DanceLeft(now))));
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Pressed)
            {
                _hopReverse = end == 1;
                _hopLeg = -1;
                NextHop(true);
                if (_hopLeg >= 0)
                {
                    Say(LateRouteMechanicsTexts.FloeHop);
                }
            }

            return true;
        }

        /// <summary>After a leg: on to the next floe, or done (arrived across, or the ride was broken off).</summary>
        private void NextHop(bool arrived)
        {
            Vector2[] chain = OverworldLayouts.AuroraFloeChain;
            int legs = chain.Length - 1;
            int next = _hopLeg + 1;
            _hopLeg = -1;
            if (!arrived || next >= legs)
            {
                return;
            }

            Vector2[] leg = { chain[HopIndex(next, chain.Length)], chain[HopIndex(next + 1, chain.Length)] };
            if (BeginRide(_geladoOrigin, leg, false, AuroraFloeRules.HopSpeed, AuroraFloeRules.HopArc, FloeHover,
                next + 1 == legs ? LateRouteMechanicsTexts.FloeArrived : null))
            {
                _hopLeg = next;
            }
        }

        /// <summary>Lands the hero on the floes where they drift now: the ride's offset slides from the floe left to the floe ahead.</summary>
        private void SteerHop(double now)
        {
            int n = OverworldLayouts.AuroraFloeChain.Length;
            bool night = IsNight();
            float from = FloeDrift(HopIndex(_hopLeg, n), n, now, night);
            float to = FloeDrift(HopIndex(_hopLeg + 1, n), n, now, night);
            float lateral = Mathf.Lerp(from, to, _ride.Progress);
            _ride.Lateral = _hopReverse ? -lateral : lateral;
        }

        /// <summary>The chain index of the <paramref name="step"/>-th point of the hop (from the end the hero set out from).</summary>
        private int HopIndex(int step, int count) => _hopReverse ? count - 1 - step : step;

        /// <summary>The drift of chain point <paramref name="index"/>: the two ends are the roads, which do not drift.</summary>
        private static float FloeDrift(int index, int count, double now, bool night) =>
            index <= 0 || index >= count - 1 ? 0f : AuroraFloeRules.Drift(index, now, night);

        private void ShowFloes(Vector2[] chain, double now, bool night)
        {
            bool inLine = AuroraFloeRules.InLine(now, night);
            Vector2 along = (chain[chain.Length - 1] - chain[0]).normalized;
            Vector2 right = MechanicsRide.Right(along);
            int key = FloeLineKey;
            Pillar(key++, chain[0] + _geladoOrigin, 5f, 0.5f, FloeEndColor);
            Pillar(key++, chain[chain.Length - 1] + _geladoOrigin, 5f, 0.5f, FloeEndColor);
            for (int i = 1; i < chain.Length - 1; i++)
            {
                Vector2 c = chain[i] + right * FloeDrift(i, chain.Length, now, night) + _geladoOrigin;
                GroundSegment(key++, c - along * FloeHalfLength, c + along * FloeHalfLength, 0.15f, 1.8f, inLine ? FloeLitColor : FloeColor);
            }

            if (AuroraFloeRules.Dancing(now) || night)
            {
                Vector2 a = chain[0] + _geladoOrigin;
                Vector2 b = chain[chain.Length - 1] + _geladoOrigin;
                Segment(key, new Vector3(a.x, WaterLineY(a) + AuroraHeight, a.y), new Vector3(b.x, WaterLineY(b) + AuroraHeight, b.y), 4f, AuroraColor);
            }
        }
    }
}
