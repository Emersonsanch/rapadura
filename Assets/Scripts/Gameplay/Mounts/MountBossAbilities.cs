using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Skills.Vfx;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>Numbers and geometry of the abilities of <see cref="MountBossCatalog"/>, pure so the server can run them too. None deals damage.</summary>
    public static class MountBossRules
    {
        /// <summary>Rabo Solto: never pulls a critter closer than this to the dropped tail.</summary>
        public const float TailStopDistance = 0.5f;

        /// <summary>Nuvem de Fumaça: how often the cloud refreshes its hold on the critters inside.</summary>
        public const float CloudTickSeconds = 0.25f;

        /// <summary>Nuvem de Fumaça: each tick holds the critters inside for this long, so leaving the cloud frees them soon after.</summary>
        public const float CloudHoldSeconds = 0.6f;

        /// <summary>Mergulho Tímido: half-angle of "looking at her" (the 60° each side of the map boss's own gaze, Docs/Mapas100.md 34).</summary>
        public const float GazeHalfAngle = 60f;

        /// <summary>Mergulho Tímido: seconds she and the cousin stay under the water, out of sight.</summary>
        public const float DiveSeconds = 4f;

        /// <summary>Mergulho Tímido: detection range left while under (the Qilin's Passo Sem Pegada keeps 20% too).</summary>
        public const float DiveDetectionScale = 0.2f;

        /// <summary>Forja Acesa: range of the light of the boar's embers.</summary>
        public const float ForgeLampRadius = 10f;

        /// <summary>Nuvem de Fumaça (visual): seconds the smoke takes to billow out when the horse leaves it.</summary>
        public const float SmokeBillowSeconds = 0.4f;

        /// <summary>Nuvem de Fumaça (visual): the last seconds of the cloud, while it thins away.</summary>
        public const float SmokeThinSeconds = 1.5f;

        /// <summary>Nuvem de Fumaça (visual): smoke puffs per second for each square metre of the cloud at full density.</summary>
        public const float SmokePuffsPerSquareMetre = 0.5f;

        /// <summary>
        /// Nuvem de Fumaça (visual): how thick the cloud is <paramref name="elapsed"/> seconds after it was left (0–1): it billows out in
        /// <see cref="SmokeBillowSeconds"/>, holds, and thins in the last <see cref="SmokeThinSeconds"/>; 0 before and after.
        /// </summary>
        public static float SmokeDensity(float elapsed, float duration)
        {
            if (duration <= 0f || elapsed < 0f || elapsed >= duration)
            {
                return 0f;
            }

            float grow = Mathf.Clamp01(elapsed / SmokeBillowSeconds);
            float thin = Mathf.Clamp01((duration - elapsed) / Mathf.Min(SmokeThinSeconds, duration));
            return Mathf.Min(grow, thin);
        }

        /// <summary>Nuvem de Fumaça (visual): puffs a second for a cloud of <paramref name="radius"/> at <paramref name="density"/> (capped at 120).</summary>
        public static float SmokePuffsPerSecond(float radius, float density) =>
            Mathf.Min(120f, Mathf.PI * Mathf.Max(0f, radius) * Mathf.Max(0f, radius) * SmokePuffsPerSquareMetre * Mathf.Clamp01(density));

        /// <summary>
        /// Búfalo-d'Água de Jhilpur (Docs/Mapas100.md 31, "nada em lama e água"): whether a hero <paramref name="mounted"/> on
        /// <paramref name="mountId"/> wades a ford the monsoon flooded, where the current throws everyone else back (JhilpurMonsoonRules).
        /// The heavy paddy buffalo alone: the other swimmers are light enough to be carried.
        /// </summary>
        public static bool BuffaloWadesFlood(bool mounted, string mountId) => mounted && mountId == MountBossCatalog.JhilpurBuffalo;

        /// <summary>The line of the scene when a boss frees <paramref name="mountId"/> (its drop, <see cref="MountBossCatalog.BossSources"/>), or null.</summary>
        public static string FreedLineKey(string mountId)
        {
            foreach ((string _, string id) in MountBossCatalog.BossSources)
            {
                if (id == mountId)
                {
                    return "ui.mount.boss.freed." + mountId;
                }
            }

            foreach ((string _, string legendId) in MountLegendCatalog.BossSources)
            {
                if (legendId == mountId)
                {
                    return "ui.mount.boss.freed." + mountId;
                }
            }

            return null;
        }

        /// <summary>Rabo Solto: where the tail drops, just behind the lizard.</summary>
        public static Vector3 TailPoint(Vector3 from, Vector3 forward)
        {
            Vector3 flat = new Vector3(forward.x, 0f, forward.z);
            flat = flat.sqrMagnitude < 1e-6f ? Vector3.forward : flat.normalized;
            return from - flat * 1.2f;
        }

        /// <summary>Rabo Solto: how far a critter at <paramref name="critter"/> is drawn toward the tail (at most <paramref name="maxPull"/>, never onto it).</summary>
        public static float TailPull(Vector3 critter, Vector3 tail, float maxPull)
        {
            Vector3 d = tail - critter;
            d.y = 0f;
            return Mathf.Clamp(d.magnitude - TailStopDistance, 0f, Mathf.Max(0f, maxPull));
        }

        /// <summary>Rabo Solto, Nuvem de Fumaça: seconds a critter forgets the cousin (0 on bosses when <paramref name="bossMultiplier"/> is 0).</summary>
        public static float ForgetSeconds(float seconds, bool isBoss, float bossMultiplier) => MountRules.EffectDurationOn(seconds, isBoss, bossMultiplier);

        /// <summary>Nuvem de Fumaça: whether a critter at <paramref name="critter"/> is inside the cloud.</summary>
        public static bool InCloud(Vector3 cloud, Vector3 critter, float radius) => MountWaveAbilityRules.InRadius(cloud, critter, radius);

        /// <summary>
        /// Mergulho Tímido: whether a critter at <paramref name="critter"/>, facing <paramref name="critterForward"/>, is looking at
        /// the dragoness at <paramref name="mount"/> (flat, within <see cref="GazeHalfAngle"/> each side, inside <paramref name="radius"/>).
        /// </summary>
        public static bool LooksAt(Vector3 critter, Vector3 critterForward, Vector3 mount, float radius)
        {
            Vector3 to = mount - critter;
            to.y = 0f;
            Vector3 f = new Vector3(critterForward.x, 0f, critterForward.z);
            if (to.sqrMagnitude > radius * radius || f.sqrMagnitude < 1e-6f)
            {
                return false;
            }

            if (to.sqrMagnitude < 1e-6f)
            {
                return true;
            }

            return Vector3.Angle(f, to) <= GazeHalfAngle;
        }

        /// <summary>Mergulho Tímido: seconds of the slow on a critter that looked (bosses get <paramref name="bossMultiplier"/> of it).</summary>
        public static float GazeSlowSeconds(float seconds, bool isBoss, float bossMultiplier) => MountRules.EffectDurationOn(seconds, isBoss, bossMultiplier);
    }

    /// <summary>
    /// What the abilities of <see cref="MountBossCatalog"/> do in the arena; called by <see cref="MountSheetAbilities.Execute"/> for the
    /// kinds it does not handle. The numbers come from the ability definition and <see cref="MountBossRules"/>. Returns the toast key, or null.
    /// </summary>
    public static class MountBossAbilities
    {
        public static string Execute(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects)
        {
            Vector3 at = ctx.Hero.transform.position;
            switch (ability.Kind)
            {
                case MountAbilityKind.TailDecoy:
                    return Tail(ability, ctx, at);

                case MountAbilityKind.MudWallow:
                    return GroupBuff(ability, at, "banho_de_lama", StatType.CooldownReduction, MountAudio.Bray, MountBossTexts.ToastMud);

                case MountAbilityKind.LedgeWatch:
                    return GroupBuff(ability, at, "vigia_da_escarpa", StatType.CriticalChance, MountAudio.HoofStep, MountBossTexts.ToastWatch);

                case MountAbilityKind.SmokeCloud:
                    MountSmokeCloud.Spawn(at, ability);
                    MountAudio.Play(MountAudio.Sniff, at);
                    return MountBossTexts.ToastSmoke;

                case MountAbilityKind.ShyDive:
                    return Dive(ability, ctx, effects, at);

                case MountAbilityKind.EmberForge:
                    effects.StartLamp(ability.DurationSeconds, MountBossRules.ForgeLampRadius, new Color(1f, 0.6f, 0.25f));
                    return GroupBuff(ability, at, "forja_acesa", StatType.CriticalDamage, MountAudio.Cowbell, MountBossTexts.ToastForge);
            }

            return MountLegendAbilities.Execute(ability, ctx, effects);
        }

        private static string Tail(MountAbilityDefinition ability, MountAbilityContext ctx, Vector3 at)
        {
            // Rabo Solto: the lizard drops its tail, still wriggling; the commons around forget the cousin and go after it.
            Vector3 tail = MountBossRules.TailPoint(at, ctx.Hero.transform.forward);
            int fooled = 0;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                float seconds = MountBossRules.ForgetSeconds(ability.DurationSeconds, enemy.IsBoss, ability.BossMultiplier);
                if (seconds <= 0f || !MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                {
                    continue;
                }

                enemy.HoldAggro(seconds, forceAggroWhenHoldEnds: true);
                float pull = MountBossRules.TailPull(enemy.transform.position, tail, ability.Distance);
                if (pull > 0f)
                {
                    Vector3 toward = tail - enemy.transform.position;
                    toward.y = 0f;
                    MountPushDriver.Apply(enemy, toward.normalized, pull, MountWaveAbilityRules.KnockbackPushSeconds);
                }

                fooled++;
            }

            MountAudio.Play(MountAudio.Dash, at);
            return fooled > 0 ? MountBossTexts.ToastTail : MountBossTexts.ToastTailNone;
        }

        private static string GroupBuff(MountAbilityDefinition ability, Vector3 at, string name, StatType stat, string sound, string toast)
        {
            // Banho de Lama, Vigia da Escarpa, Forja Acesa: the cousin and the group around get the same short bonus.
            string source = MountWaveAbilities.BuffSourcePrefix + ability.AbilityId;
            foreach (PlayerStats ally in MountWaveAbilities.Allies(at, ability.Radius))
            {
                if (ally.TryGetComponent(out BuffController buffs))
                {
                    buffs.ApplyEffect(MountWaveAbilities.Modifier(name, stat, ability.Magnitude, ability.DurationSeconds, false), source, ally.gameObject);
                }
            }

            MountAudio.Play(sound, at);
            return toast;
        }

        private static string Dive(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects, Vector3 at)
        {
            // Mergulho Tímido: she shines and dives: whoever was looking at her is slowed, the commons lose the cousin, and both go
            // under the water out of sight for a moment.
            string source = MountWaveAbilities.BuffSourcePrefix + ability.AbilityId;
            int looked = 0;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                Transform t = enemy.transform;
                if (!MountBossRules.LooksAt(t.position, t.forward, at, ability.Radius))
                {
                    continue;
                }

                float seconds = MountBossRules.GazeSlowSeconds(ability.DurationSeconds, enemy.IsBoss, ability.BossMultiplier);
                if (seconds > 0f)
                {
                    enemy.Buffs?.ApplyEffect(MountWaveAbilities.Modifier("mergulho_timido", StatType.MoveSpeed, -ability.Magnitude, seconds, debuff: true),
                        source, ctx.Hero);
                    looked++;
                }

                if (!enemy.IsBoss)
                {
                    enemy.HoldAggro(MountBossRules.DiveSeconds, forceAggroWhenHoldEnds: true);
                }
            }

            effects.StartStealth(MountBossRules.DiveSeconds, MountBossRules.DiveDetectionScale);
            MountAudio.Play(MountAudio.Sniff, at);
            return looked > 0 ? MountBossTexts.ToastDiveSeen : MountBossTexts.ToastDive;
        }
    }

    /// <summary>
    /// Nuvem de Fumaça (the Cavalo de Fumaça Liberto): the last of the horse's smoke, left where it stood. While it lasts, the common
    /// critters inside forget every target (a short <see cref="EnemyController.HoldAggro"/> refreshed each tick); bosses walk through it.
    /// The smoke itself is a procedural particle cloud as wide as the effect (<see cref="MountBossRules.SmokeDensity"/>): it billows out,
    /// holds and thins away, and its last puffs fade on their own after the effect is over.
    /// </summary>
    public class MountSmokeCloud : MonoBehaviour
    {
        private static readonly Color SmokeColor = new Color(0.62f, 0.62f, 0.66f, 0.55f);

        private float _duration;
        private float _elapsed;
        private float _radius;
        private float _bossMultiplier;
        private float _tick;
        private ParticleSystem _smoke;

        public static MountSmokeCloud Spawn(Vector3 at, MountAbilityDefinition ability)
        {
            var go = new GameObject("MountSmokeCloud");
            go.transform.position = at;
            MountSmokeCloud cloud = go.AddComponent<MountSmokeCloud>();
            cloud._duration = ability.DurationSeconds;
            cloud._radius = ability.Radius;
            cloud._bossMultiplier = ability.BossMultiplier;
            cloud._smoke = NewSmoke(go.transform, ability.Radius);
            return cloud;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _elapsed += dt;
            if (_elapsed >= _duration)
            {
                ReleaseSmoke();
                Destroy(gameObject);
                return;
            }

            if (_smoke != null)
            {
                ParticleSystem.EmissionModule emission = _smoke.emission;
                emission.rateOverTime = MountBossRules.SmokePuffsPerSecond(_radius, MountBossRules.SmokeDensity(_elapsed, _duration));
            }

            _tick -= dt;
            if (_tick > 0f)
            {
                return;
            }

            _tick = MountBossRules.CloudTickSeconds;
            Vector3 at = transform.position;
            List<EnemyController> live = MountWaveAbilities.LiveEnemies();
            foreach (EnemyController enemy in live)
            {
                float seconds = MountBossRules.ForgetSeconds(MountBossRules.CloudHoldSeconds, enemy.IsBoss, _bossMultiplier);
                if (seconds > 0f && MountBossRules.InCloud(at, enemy.transform.position, _radius))
                {
                    enemy.HoldAggro(seconds, forceAggroWhenHoldEnds: true);
                }
            }
        }

        private void OnDestroy() => ReleaseSmoke();

        /// <summary>Lets the puffs already out finish fading on their own (the effect is over; nothing new is emitted).</summary>
        private void ReleaseSmoke()
        {
            if (_smoke == null)
            {
                return;
            }

            ParticleSystem smoke = _smoke;
            _smoke = null;
            smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            smoke.transform.SetParent(null, true);
            Destroy(smoke.gameObject, smoke.main.startLifetime.constantMax + 0.2f);
        }

        /// <summary>The cloud: big soft grey puffs over a disc of <paramref name="radius"/>, drifting up and swelling as they fade.</summary>
        private static ParticleSystem NewSmoke(Transform parent, float radius)
        {
            var go = new GameObject("Smoke");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.up * 0.3f;
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 400;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.4f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = SmokeColor;
            main.gravityModifier = -0.02f;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = Mathf.Max(0.5f, radius * 0.85f);

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(1f, 1.5f)));

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.85f, 0.85f, 0.9f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0.7f, 0.7f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = SkillVfxManager.GetSharedParticleMaterial(false);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            ps.Play();
            return ps;
        }
    }

    /// <summary>
    /// The little scene of a boss freeing its creature (<see cref="MountBossCatalog.BossSources"/>): when the boss falls and its mount drops,
    /// a soft column of light where it fell and the freed line over it (<see cref="MountBossRules.FreedLineKey"/>). Only looks: it takes no
    /// control, waits for nothing, and the mount is already in the collection (the HUD still says the drop as before).
    /// </summary>
    public static class MountBossFreeing
    {
        private const float LineHeight = 3.2f;

        /// <summary>Plays the scene for every boss mount among <paramref name="granted"/>, at <paramref name="at"/> (where the boss fell).</summary>
        public static void Play(Vector3 at, IReadOnlyList<OwnedMount> granted)
        {
            if (!Application.isPlaying || granted == null)
            {
                return;
            }

            foreach (OwnedMount mount in granted)
            {
                string key = mount != null ? MountBossRules.FreedLineKey(mount.mountId) : null;
                if (key == null)
                {
                    continue;
                }

                Burst(at, ColorOf(mount.mountId));
                ServiceLocator.TryGet(out LocalizationManager localization);
                string name = MountTexts.MountName(localization, MountCatalog.Get(mount.mountId));
                CombatFxManager.SpawnLootText(at + Vector3.up * LineHeight, MountTexts.Format(localization, key, name));
            }
        }

        private static Color ColorOf(string mountId)
        {
            switch (mountId)
            {
                case MountBossCatalog.FreedSmokeHorse: return new Color(0.8f, 0.8f, 0.85f, 0.9f);
                case MountBossCatalog.TameNinkiNanka: return new Color(0.45f, 0.9f, 0.8f, 0.9f);
                case MountBossCatalog.LivingEmberBoar: return new Color(1f, 0.6f, 0.25f, 0.9f);
                case MountLegendCatalog.LightSkiff: return new Color(1f, 0.95f, 0.7f, 0.95f);
                default: return new Color(1f, 0.92f, 0.65f, 0.9f);
            }
        }

        /// <summary>A soft column of light rising where the boss fell, gone in about two seconds.</summary>
        private static void Burst(Vector3 at, Color color)
        {
            var go = new GameObject("MountFreed_FX");
            go.transform.position = at + Vector3.up * 0.4f;
            go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 1.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
            main.startColor = color;
            main.gravityModifier = -0.05f;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40), new ParticleSystem.Burst(0.3f, 20) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 1.2f;

            ParticleSystem.ColorOverLifetimeModule fade = ps.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = SkillVfxManager.GetSharedParticleMaterial(true);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            ps.Play();
            Object.Destroy(go, main.duration + 2f);
        }
    }
}
