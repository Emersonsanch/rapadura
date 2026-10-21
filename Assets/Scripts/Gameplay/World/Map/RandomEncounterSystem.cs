using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>What counts as one "step" for <see cref="RandomEncounterSystem"/>.</summary>
    public enum EncounterStepMode
    {
        /// <summary>One step per <see cref="RandomEncounterSettings.StepDistance"/> world units travelled (classic FF).</summary>
        Distance = 0,

        /// <summary>One step per <see cref="RandomEncounterSettings.SecondsPerStep"/> spent moving.</summary>
        Time = 1,
    }

    /// <summary>Inspector-tunable knobs for <see cref="RandomEncounterSystem"/>.</summary>
    [Serializable]
    public class RandomEncounterSettings
    {
        [SerializeField] private EncounterStepMode _stepMode = EncounterStepMode.Distance;
        [Tooltip("World units per step when Step Mode is Distance.")]
        [SerializeField] [Min(0.01f)] private float _stepDistance = 1f;
        [Tooltip("Seconds of movement per step when Step Mode is Time.")]
        [SerializeField] [Min(0.01f)] private float _secondsPerStep = 0.5f;

        [Header("Chance")]
        [Tooltip("Steps after an encounter (or after entering the map) during which no encounter can trigger.")]
        [SerializeField] [Min(0)] private int _graceSteps = 8;
        [Tooltip("Chance (0..1) on the first step after the grace period ends.")]
        [SerializeField] [Range(0f, 1f)] private float _baseChancePerStep = 0.04f;
        [Tooltip("Added to the chance for every further step without an encounter, so long droughts get less likely.")]
        [SerializeField] [Range(0f, 1f)] private float _chanceRampPerStep = 0.01f;
        [SerializeField] [Range(0f, 1f)] private float _maxChancePerStep = 0.3f;

        [Header("Group size")]
        [SerializeField] [Min(1)] private int _minEnemies = 1;
        [SerializeField] [Min(1)] private int _maxEnemies = 2;

        public EncounterStepMode StepMode => _stepMode;
        public float StepDistance => _stepDistance;
        public float SecondsPerStep => _secondsPerStep;
        public int GraceSteps => _graceSteps;
        public float BaseChancePerStep => _baseChancePerStep;
        public float ChanceRampPerStep => _chanceRampPerStep;
        public float MaxChancePerStep => _maxChancePerStep;
        public int MinEnemies => _minEnemies;
        public int MaxEnemies => Mathf.Max(_minEnemies, _maxEnemies);

        public RandomEncounterSettings()
        {
        }

        /// <summary>Plain-C# construction path for EditMode tests.</summary>
        public RandomEncounterSettings(
            EncounterStepMode stepMode,
            float stepDistance,
            float secondsPerStep,
            int graceSteps,
            float baseChancePerStep,
            float chanceRampPerStep,
            float maxChancePerStep,
            int minEnemies = 1,
            int maxEnemies = 1)
        {
            _stepMode = stepMode;
            _stepDistance = Mathf.Max(0.01f, stepDistance);
            _secondsPerStep = Mathf.Max(0.01f, secondsPerStep);
            _graceSteps = Mathf.Max(0, graceSteps);
            _baseChancePerStep = Mathf.Clamp01(baseChancePerStep);
            _chanceRampPerStep = Mathf.Clamp01(chanceRampPerStep);
            _maxChancePerStep = Mathf.Clamp01(maxChancePerStep);
            _minEnemies = Mathf.Max(1, minEnemies);
            _maxEnemies = Mathf.Max(_minEnemies, maxEnemies);
        }
    }

    /// <summary>
    /// Plain C# random-encounter roller for the world map (no MonoBehaviour, so it is unit tested
    /// directly). The owner feeds it movement through <see cref="Advance"/>; movement is converted
    /// into discrete steps (by distance or by time, see <see cref="EncounterStepMode"/>) and each
    /// step after the grace period rolls against a chance that ramps up the longer the player goes
    /// without a fight. When a roll hits, the enemy group is picked with
    /// <see cref="BiomeDefinition.RollEnemy"/> from the biome the player is currently in.
    ///
    /// All randomness flows through a single injected <see cref="System.Random"/>, so a fixed seed
    /// reproduces the exact same encounter sequence (tests, replays, debugging). A fresh instance
    /// starts inside the grace period, which is how "no encounter right after returning from
    /// combat" works: WorldMapController builds a new one every time the WorldMap scene loads.
    /// </summary>
    public class RandomEncounterSystem
    {
        private readonly RandomEncounterSettings _settings;
        private readonly System.Random _random;
        private float _accumulator;

        public int StepsSinceLastEncounter { get; private set; }
        public bool Enabled { get; set; } = true;
        public RandomEncounterSettings Settings => _settings;

        public bool IsInGracePeriod => StepsSinceLastEncounter < _settings.GraceSteps;

        public RandomEncounterSystem(RandomEncounterSettings settings, int seed)
            : this(settings, new System.Random(seed))
        {
        }

        public RandomEncounterSystem(RandomEncounterSettings settings, System.Random random)
        {
            _settings = settings ?? new RandomEncounterSettings();
            _random = random ?? new System.Random();
        }

        /// <summary>
        /// Chance (0..1) that the step numbered <paramref name="stepNumber"/> (1-based, counted since
        /// the last encounter) triggers one: 0 inside the grace period, then
        /// base + ramp × (steps past grace − 1), capped at the configured max.
        /// </summary>
        public float GetChanceForStep(int stepNumber)
        {
            if (stepNumber <= _settings.GraceSteps)
            {
                return 0f;
            }

            int stepsPastGrace = stepNumber - _settings.GraceSteps - 1;
            float chance = _settings.BaseChancePerStep + _settings.ChanceRampPerStep * stepsPastGrace;
            return Mathf.Min(_settings.MaxChancePerStep, chance);
        }

        /// <summary>
        /// Feeds movement since the last call. Only the quantity matching the configured
        /// <see cref="EncounterStepMode"/> is used (callers should pass 0 time while standing still
        /// so idling never triggers fights). Returns true, with the rolled payload, on the first step
        /// that triggers an encounter; any leftover movement in that call is discarded.
        /// </summary>
        public bool Advance(float distanceMoved, float deltaTime, BiomeDefinition biome, out EncounterPayload payload)
        {
            payload = null;

            if (!Enabled || biome == null)
            {
                return false;
            }

            bool byDistance = _settings.StepMode == EncounterStepMode.Distance;
            float amount = byDistance ? distanceMoved : deltaTime;
            float perStep = byDistance ? _settings.StepDistance : _settings.SecondsPerStep;

            if (amount <= 0f)
            {
                return false;
            }

            _accumulator += amount;

            while (_accumulator >= perStep)
            {
                _accumulator -= perStep;

                if (RollStep(biome, out payload))
                {
                    _accumulator = 0f;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Advances exactly one step and rolls it. A biome with an empty/zero-weight spawn table can
        /// never produce an encounter (the step still counts, but nothing triggers and the drought
        /// counter is not reset).
        /// </summary>
        public bool RollStep(BiomeDefinition biome, out EncounterPayload payload)
        {
            payload = null;

            if (!Enabled || biome == null)
            {
                return false;
            }

            StepsSinceLastEncounter++;

            float chance = GetChanceForStep(StepsSinceLastEncounter);
            if (chance <= 0f || _random.NextDouble() >= chance)
            {
                return false;
            }

            payload = BuildPayload(biome);
            if (payload == null)
            {
                return false;
            }

            ResetGrace();
            return true;
        }

        /// <summary>Rolls an enemy group for <paramref name="biome"/> without touching the step counter. Null if the biome has no rollable enemies.</summary>
        public EncounterPayload BuildPayload(BiomeDefinition biome)
        {
            if (biome == null)
            {
                return null;
            }

            int count = _random.Next(_settings.MinEnemies, _settings.MaxEnemies + 1);
            var enemies = new List<EnemyDefinition>(count);

            for (int i = 0; i < count; i++)
            {
                EnemyDefinition enemy = biome.RollEnemy(_random);
                if (enemy != null)
                {
                    enemies.Add(enemy);
                }
            }

            if (enemies.Count == 0)
            {
                return null;
            }

            return EncounterPayload.Create(biome.Type, enemies, _random.Next());
        }

        /// <summary>Restarts the grace period (called automatically after every triggered encounter).</summary>
        public void ResetGrace()
        {
            StepsSinceLastEncounter = 0;
            _accumulator = 0f;
        }
    }
}
