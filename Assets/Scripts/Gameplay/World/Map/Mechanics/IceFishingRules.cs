using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Fishing;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A fishing hole in the ice (<c>OverworldLayouts.IceFishingHoles</c>, region coordinates).</summary>
    public readonly struct IceHole
    {
        public readonly Vector2 Center;
        public readonly float Radius;
        public readonly bool Great;

        public IceHole(Vector2 center, float radius, bool great)
        {
            Center = center;
            Radius = radius;
            Great = great;
        }
    }

    public enum IceFishingState
    {
        /// <summary>The line is down: pressing now scares the fish.</summary>
        Waiting = 0,
        /// <summary>The fish pulls: press within the window.</summary>
        Bite = 1,
        Caught = 2,
        ScaredOff = 3,
        Lost = 4,
    }

    /// <summary>
    /// Ice fishing on Old Kul's lake (Docs/Mapas100.md, map 64: "pesca no gelo; peixes raros do lago"), a one-button catch that
    /// fits the walking world (the full reeling game of the world map's panel stays there). Pure: the same seed gives the same
    /// fishing, so the online server can run it. The hero keeps every control: walking away simply ends the session.
    /// <code>
    /// bite after 5–11 s · 1.4 s to press · each hole rests 30 s after a cast
    /// odds: common 70 · uncommon 24 · rare 5.5 · Peixe-Dourado 0.5 (relative weights)
    /// night: rare × 2 · the Buraco Grande: rare × 1.5 and legendary × 3
    /// reach: 4.5 m past the rim of a hole
    /// </code>
    /// </summary>
    public sealed class IceFishingSession
    {
        public const float MinBiteDelay = 5f;
        public const float MaxBiteDelay = 11f;
        public const float BiteWindow = 1.4f;

        private readonly System.Random _random;
        private readonly bool _night;
        private readonly bool _greatHole;
        private float _timer;

        public IceFishingSession(int seed, bool night, bool greatHole)
        {
            _random = new System.Random(seed);
            _night = night;
            _greatHole = greatHole;
            _timer = MinBiteDelay + (float)_random.NextDouble() * (MaxBiteDelay - MinBiteDelay);
        }

        public IceFishingState State { get; private set; } = IceFishingState.Waiting;

        /// <summary>The fish that was landed (<see cref="IceFishingState.Caught"/> only).</summary>
        public FishSpec Fish { get; private set; }

        public int Grams { get; private set; }

        public bool IsOver => State == IceFishingState.Caught || State == IceFishingState.ScaredOff || State == IceFishingState.Lost;

        public void Tick(float deltaTime)
        {
            if (IsOver)
            {
                return;
            }

            _timer -= deltaTime;
            if (_timer > 0f)
            {
                return;
            }

            if (State == IceFishingState.Waiting)
            {
                State = IceFishingState.Bite;
                _timer = BiteWindow;
            }
            else
            {
                State = IceFishingState.Lost;
            }
        }

        public void Press()
        {
            if (State == IceFishingState.Waiting)
            {
                State = IceFishingState.ScaredOff;
            }
            else if (State == IceFishingState.Bite)
            {
                Fish = IceFishingRules.Roll(_random.NextDouble(), _night, _greatHole);
                float kg = Fish.MinKg + (float)_random.NextDouble() * (Fish.MaxKg - Fish.MinKg);
                Grams = Mathf.RoundToInt(kg * 1000f);
                State = IceFishingState.Caught;
            }
        }
    }

    /// <summary>The table and the places of <see cref="IceFishingSession"/>. Cold-water species of the existing catalogue, so the Coleção and the backpack just work.</summary>
    public static class IceFishingRules
    {
        public const float HoleCooldownSeconds = 30f;
        public const float Reach = 4.5f;
        public const float CommonWeight = 70f;
        public const float UncommonWeight = 24f;
        public const float RareWeight = 5.5f;
        public const float LegendaryWeight = 0.5f;
        public const float NightRareFactor = 2f;
        public const float GreatHoleRareFactor = 1.5f;
        public const float GreatHoleLegendaryFactor = 3f;

        private static readonly string[] Common = { FishCatalog.Lambari, FishCatalog.Cascudo };
        private static readonly string[] Uncommon = { FishCatalog.Piau, FishCatalog.Mandi };
        private const string Rare = FishCatalog.Surubim;
        private const string Legendary = FishCatalog.PeixeDourado;

        /// <summary>The nearest hole whose rim is within <see cref="Reach"/> of <paramref name="p"/>, or −1.</summary>
        public static int HoleAt(IReadOnlyList<IceHole> holes, Vector2 p)
        {
            int found = -1;
            float best = float.MaxValue;
            for (int i = 0; i < holes.Count; i++)
            {
                float d = Vector2.Distance(p, holes[i].Center);
                if (d <= holes[i].Radius + Reach && d < best)
                {
                    best = d;
                    found = i;
                }
            }

            return found;
        }

        /// <summary>The species for a roll in [0, 1).</summary>
        public static FishSpec Roll(double roll, bool night, bool greatHole)
        {
            float rare = RareWeight * (night ? NightRareFactor : 1f) * (greatHole ? GreatHoleRareFactor : 1f);
            float legendary = LegendaryWeight * (greatHole ? GreatHoleLegendaryFactor : 1f);
            float total = CommonWeight + UncommonWeight + rare + legendary;
            float at = (float)Math.Max(0d, Math.Min(0.999999d, roll)) * total;
            if (at < CommonWeight)
            {
                return FishCatalog.Get(Common[at < CommonWeight * 0.5f ? 0 : 1]);
            }

            at -= CommonWeight;
            if (at < UncommonWeight)
            {
                return FishCatalog.Get(Uncommon[at < UncommonWeight * 0.5f ? 0 : 1]);
            }

            at -= UncommonWeight;
            return FishCatalog.Get(at < rare ? Rare : Legendary);
        }
    }
}
