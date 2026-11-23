using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The second round of the Margens da Mistura (2026-11-23) on the region's driver: the late shadows that rise behind the fallen beasts at night (78,
    /// <see cref="LateShadowEchoRules"/>), the Betume Vivo that the dark pools spit at night (79, <see cref="NightTarRules"/>) and the pitch smoke that a fire
    /// skill bursts (79, <see cref="PitchSmokeIgniteRules"/>). The summoned beasts live only while it is night and the hero is near; nothing of them is saved.
    /// The save only keeps, once each, that the cousin already spoke of each of the three (a time mark written on that event).
    /// </summary>
    public sealed partial class MisturaMechanicsDriver
    {
        private const string ShadowLineKey = "mistura.late_shadow.first";
        private const string NightTarLineKey = "mistura.night_tar.first";
        private const string SmokeLineKey = "mistura.pitch_smoke.burst";
        private const string DesertZoneId = "sombras_atrasadas";
        private const string PitchZoneId = "pocas_de_piche";
        private const float SummonProbeHeight = 6f;
        private const float SummonCorpseSeconds = 5f;
        private const float SmokeScanRange = 70f;
        private const int EchoRingKeys = 3000;
        private const float EchoRingRadius = 2f;

        private static readonly Color EchoColor = new Color(0.55f, 0.35f, 0.85f, 1f);

        private struct PendingShadow
        {
            public Vector2 Local;
            public float RiseAt;
            public int Level;
            public int Slot;
        }

        private struct LitSmoke
        {
            public EnemyController Enemy;
            public float BurstAt;
        }

        private readonly List<PendingShadow> _pendingShadows = new List<PendingShadow>();
        private readonly List<GameObject> _shadows = new List<GameObject>();
        private readonly List<GameObject> _nightTars = new List<GameObject>();
        private readonly List<KeyValuePair<GameObject, float>> _summonCorpses = new List<KeyValuePair<GameObject, float>>();
        private readonly List<LitSmoke> _litSmokes = new List<LitSmoke>();
        private readonly Dictionary<int, float> _smokeLastLit = new Dictionary<int, float>();
        private float[] _tarReadyAt;
        private GameObject _summonPrefab;
        private EnemyDefinition _shadowDefinition;
        private EnemyDefinition _tarDefinition;
        private bool _nightBeastsUp;
        private int _echoSlot;

        private void HookNight() => EventBus.Subscribe<EnemyDiedEvent>(OnNightBeastDied);

        private void UnhookNight() => EventBus.Unsubscribe<EnemyDiedEvent>(OnNightBeastDied);

        private static bool IsNightNow() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        /// <summary>Every frame in the region: shadows rise, dark pools spit, lit smokes burst, and dawn or distance sends the summoned home.</summary>
        private void TickNight(Vector2 local, bool night)
        {
            ReleaseSummonCorpses();
            TickPendingShadows(local, night);
            FadeSummoned(_shadows, local, ShadowFades, night, true);
            FadeSummoned(_nightTars, local, TarFades, night, false);
            TickNightPools(local, night);
            TickLitSmokes(local);
        }

        // ------------------------------------------------------------------
        // Late shadows (78)
        // ------------------------------------------------------------------

        private void OnNightBeastDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null || _player == null || !_inRegion)
            {
                return;
            }

            bool wasShadow = _shadows.Remove(evt.Enemy);
            bool wasTar = _nightTars.Remove(evt.Enemy);
            if (wasShadow || wasTar)
            {
                _summonCorpses.Add(new KeyValuePair<GameObject, float>(evt.Enemy, Time.time + SummonCorpseSeconds));
            }

            EnemyController enemy = evt.Enemy.GetComponent<EnemyController>();
            string id = enemy != null && enemy.Definition != null ? enemy.Definition.name : null;
            Vector2 at = new Vector2(evt.Enemy.transform.position.x, evt.Enemy.transform.position.z) - _origin;
            bool boss = evt.WasBoss || (enemy != null && enemy.IsBoss);
            if (!LateShadowEchoRules.LeavesShadow(IsNightNow(), at, id, boss, wasShadow, _shadows.Count + _pendingShadows.Count))
            {
                return;
            }

            MapMonsterInfo info = evt.Enemy.GetComponent<MapMonsterInfo>();
            _echoSlot = (_echoSlot + 1) % (LateShadowEchoRules.MaxAlive * 2);
            _pendingShadows.Add(new PendingShadow
            {
                Local = at,
                RiseAt = Time.time + LateShadowEchoRules.RiseDelaySeconds,
                Level = LateShadowEchoRules.ShadowLevel(info != null ? info.Level : 0),
                Slot = _echoSlot,
            });
            ShowRing(EchoRingKeys + _echoSlot, at, EchoRingRadius, EchoColor, true);
            SayNight(MisturaNightTexts.ShadowComing);
        }

        private void TickPendingShadows(Vector2 local, bool night)
        {
            for (int i = _pendingShadows.Count - 1; i >= 0; i--)
            {
                PendingShadow pending = _pendingShadows[i];
                if (LateShadowEchoRules.Fades(night, Vector2.Distance(local, pending.Local)))
                {
                    ShowRing(EchoRingKeys + pending.Slot, pending.Local, EchoRingRadius, EchoColor, false);
                    _pendingShadows.RemoveAt(i);
                    continue;
                }

                if (Time.time < pending.RiseAt)
                {
                    continue;
                }

                if (!EnsureSummoning(ref _shadowDefinition, LateShadowEchoRules.LateShadowId))
                {
                    continue;
                }

                GameObject shadow = Summon(_shadowDefinition, pending.Local, pending.Level, OverworldLayouts.DesertoRelogioMapId, DesertZoneId, "sombra_atrasada");
                if (shadow == null)
                {
                    continue;
                }

                ShowRing(EchoRingKeys + pending.Slot, pending.Local, EchoRingRadius, EchoColor, false);
                _pendingShadows.RemoveAt(i);
                _shadows.Add(shadow);
                SayFirstLine(ShadowLineKey, MisturaNightTexts.Moment.LateShadow);
            }
        }

        // ------------------------------------------------------------------
        // Betume Vivo of the night pools (79)
        // ------------------------------------------------------------------

        private void TickNightPools(Vector2 local, bool night)
        {
            MisturaTarPatch[] patches = OverworldLayouts.TarPatches;
            _tarReadyAt ??= new float[patches.Length];
            int here = MisturaTarRules.PatchAt(patches, local);
            if (here < 0 || !NightTarRules.Spits(night, true, _tar[here].Stage, Time.time, _tarReadyAt[here], _nightTars.Count))
            {
                return;
            }

            if (!EnsureSummoning(ref _tarDefinition, NightTarRules.LivingTarId))
            {
                return;
            }

            // It heaves up in the middle of the pool, so the hero has the patch's width to step out (or light it).
            GameObject tar = Summon(_tarDefinition, patches[here].Center, NightTarRules.Level, OverworldLayouts.PocosBetumeMapId, PitchZoneId, "betume_da_noite");
            if (tar == null)
            {
                return;
            }

            _tarReadyAt[here] = NightTarRules.NextReady(Time.time);
            _nightTars.Add(tar);
            SayNight(MisturaNightTexts.TarSpits);
            SayFirstLine(NightTarLineKey, MisturaNightTexts.Moment.NightTar);
        }

        /// <summary>The burn of a burning patch on one beast (the main tick calls it): the Betume Vivo takes three times the bite.</summary>
        private float NightBurnTick(EnemyController enemy)
        {
            bool livingTar = enemy.Definition != null && NightTarRules.IsLivingTar(enemy.Definition.name);
            return NightTarRules.BurnTick(enemy.Health.MaxHealth, livingTar);
        }

        // ------------------------------------------------------------------
        // The pitch smoke bursts (79)
        // ------------------------------------------------------------------

        /// <summary>A fire skill of the hero (from <c>OnSkillCast</c>): lights the pitch smokes it reaches.</summary>
        private void OnFireForSmoke(SkillCastEvent evt)
        {
            Vector2 landing = new Vector2(evt.TargetPoint.x, evt.TargetPoint.z) - _origin;
            Vector2 caster = new Vector2(_player.position.x, _player.position.z) - _origin;
            int lit = 0;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (lit >= PitchSmokeIgniteRules.MaxPerCast)
                {
                    break;
                }

                if (enemy == null || enemy.Health == null || enemy.Health.IsDead || enemy.Definition == null || !PitchSmokeIgniteRules.IsPitchSmoke(enemy.Definition.name))
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _origin;
                if (Vector2.Distance(at, caster) > SmokeScanRange || !PitchSmokeIgniteRules.Reaches(at, landing, evt.Skill.AreaRadius, caster))
                {
                    continue;
                }

                int key = enemy.GetInstanceID();
                _smokeLastLit.TryGetValue(key, out float last);
                if (!PitchSmokeIgniteRules.CanLight(Time.time, last))
                {
                    continue;
                }

                _smokeLastLit[key] = Time.time;
                _litSmokes.Add(new LitSmoke { Enemy = enemy, BurstAt = Time.time + PitchSmokeIgniteRules.FuseSeconds });
                lit++;
            }

            if (lit > 0)
            {
                SayNight(MisturaNightTexts.SmokeLit);
            }
        }

        private void TickLitSmokes(Vector2 local)
        {
            for (int i = _litSmokes.Count - 1; i >= 0; i--)
            {
                LitSmoke smoke = _litSmokes[i];
                if (smoke.Enemy == null || smoke.Enemy.Health == null || smoke.Enemy.Health.IsDead)
                {
                    _litSmokes.RemoveAt(i);
                    continue;
                }

                if (Time.time < smoke.BurstAt)
                {
                    continue;
                }

                _litSmokes.RemoveAt(i);
                BurstSmoke(smoke.Enemy, local);
            }
        }

        private void BurstSmoke(EnemyController smoke, Vector2 local)
        {
            Vector2 at = new Vector2(smoke.transform.position.x, smoke.transform.position.z) - _origin;
            int hit = 1;
            smoke.Health.ApplyDamage(PitchSmokeIgniteRules.SelfBurn(smoke.Health.MaxHealth), ElementType.Fire, _player.gameObject);
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy == smoke || enemy.Health == null || enemy.Health.IsDead)
                {
                    continue;
                }

                Vector2 p = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _origin;
                if (!PitchSmokeIgniteRules.InBurst(at, p))
                {
                    continue;
                }

                float bite = PitchSmokeIgniteRules.BeastBurn(enemy.Health.MaxHealth, enemy.IsBoss);
                if (bite > 0f)
                {
                    enemy.Health.ApplyDamage(bite, ElementType.Fire, _player.gameObject);
                    hit++;
                }
            }

            // The chain: a burst beside a pool lights the pool, with its usual fuse.
            MisturaTarPatch[] patches = OverworldLayouts.TarPatches;
            bool poolLit = false;
            for (int i = 0; i < patches.Length; i++)
            {
                if (_tar[i].Stage == MisturaTarStage.Sticky && PitchSmokeIgniteRules.LightsPatch(patches[i], at))
                {
                    _tar[i] = MisturaTarRules.Ignite(_tar[i]);
                    _fuseWarned[i] = false;
                    _burnWarned[i] = false;
                    poolLit = true;
                }
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(MisturaNightTexts.Format(localization, MisturaNightTexts.SmokeBurst, hit));
            if (poolLit)
            {
                Say(MisturaMechanicsTexts.TarLit);
            }

            if (_stats != null && _stats.IsAlive)
            {
                float singe = PitchSmokeIgniteRules.HeroBurn(at, local, _stats.CurrentHealth, _stats.MaxHealth);
                if (singe > 0f)
                {
                    _stats.ApplyHazardDamage(singe);
                    SayNight(MisturaNightTexts.SmokeSinged);
                }
            }

            SayFirstLine(SmokeLineKey, MisturaNightTexts.Moment.SmokeBurst);
        }

        // ------------------------------------------------------------------
        // Summoning and clean-up
        // ------------------------------------------------------------------

        private bool EnsureSummoning(ref EnemyDefinition definition, string enemyId)
        {
            if (_view == null || !RegionBossDirector.SpawningEnabled)
            {
                return false;
            }

            if (_summonPrefab == null)
            {
                EncounterArenaController encounter = FindAnyObjectByType<EncounterArenaController>(FindObjectsInactive.Include);
                _summonPrefab = encounter != null ? encounter.EnemyPrefab : null;
            }

            if (definition == null)
            {
                definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{enemyId}");
            }

            return _summonPrefab != null && definition != null;
        }

        /// <summary>Raises one beast at a region point; null (try again next frame) while the chunk under it is still building.</summary>
        private GameObject Summon(EnemyDefinition definition, Vector2 regionPoint, int level, string mapId, string zoneId, string tag)
        {
            Vector2 world = _origin + regionPoint;
            float groundY = _view.GroundHeight(world);
            if (!Physics.Raycast(new Vector3(world.x, groundY + SummonProbeHeight, world.y), Vector3.down, SummonProbeHeight * 2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            float footHeight = EnemyVisualSwapper.GetFootHeight(_summonPrefab.transform) * _summonPrefab.transform.localScale.y;
            Vector3 position = EncounterSpawnLayout.StandOnGround(new Vector3(world.x, groundY, world.y), footHeight);
            GameObject instance = Instantiate(_summonPrefab, position, Quaternion.identity);
            instance.name = $"{_summonPrefab.name}_{definition.name}_{tag}";
            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                // One waypoint: the hero; the summoned come for whoever woke them.
                controller.ResetForSpawn(definition, position, new[] { _player });
            }

            EnemyVisualSwapper.Apply(instance, definition);
            EnemyLevelScaler.Attach(instance, level);
            instance.AddComponent<MapMonsterInfo>().Configure(level, mapId, zoneId, null);
            EventBus.Publish(new EnemySpawnedEvent(instance));
            return instance;
        }

        private delegate bool FadeRule(bool night, float heroDistance);

        // Cached: a method group passed every frame would allocate a new delegate each time (C# 9 does not cache them).
        private static readonly FadeRule ShadowFades = LateShadowEchoRules.Fades;
        private static readonly FadeRule TarFades = NightTarRules.Fades;

        private void FadeSummoned(List<GameObject> summoned, Vector2 local, FadeRule fades, bool night, bool shadows)
        {
            bool faded = false;
            for (int i = summoned.Count - 1; i >= 0; i--)
            {
                GameObject beast = summoned[i];
                if (beast == null)
                {
                    summoned.RemoveAt(i);
                    continue;
                }

                Vector2 at = new Vector2(beast.transform.position.x, beast.transform.position.z) - _origin;
                if (!fades(night, Vector2.Distance(at, local)))
                {
                    continue;
                }

                summoned.RemoveAt(i);
                Destroy(beast);
                faded |= !night;
            }

            if (faded && shadows)
            {
                SayNight(MisturaNightTexts.ShadowsGone);
            }
        }

        private void ReleaseSummonCorpses()
        {
            for (int i = _summonCorpses.Count - 1; i >= 0; i--)
            {
                if (_summonCorpses[i].Key == null || Time.time >= _summonCorpses[i].Value)
                {
                    if (_summonCorpses[i].Key != null)
                    {
                        Destroy(_summonCorpses[i].Key);
                    }

                    _summonCorpses.RemoveAt(i);
                }
            }
        }

        /// <summary>The hero left the region (or the driver stopped): every summoned beast goes, every warning ring goes out.</summary>
        private void ClearNight()
        {
            foreach (PendingShadow pending in _pendingShadows)
            {
                ShowRing(EchoRingKeys + pending.Slot, pending.Local, EchoRingRadius, EchoColor, false);
            }

            _pendingShadows.Clear();
            _litSmokes.Clear();
            _smokeLastLit.Clear();
            DestroyAll(_shadows);
            DestroyAll(_nightTars);
            foreach (KeyValuePair<GameObject, float> corpse in _summonCorpses)
            {
                if (corpse.Key != null)
                {
                    Destroy(corpse.Key);
                }
            }

            _summonCorpses.Clear();
        }

        private static void DestroyAll(List<GameObject> list)
        {
            foreach (GameObject go in list)
            {
                if (go != null)
                {
                    Destroy(go);
                }
            }

            list.Clear();
        }

        private void SayNight(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(MisturaNightTexts.Get(localization, key));
        }

        /// <summary>The cousin's line the first time ever (one time mark each, written only here).</summary>
        private void SayFirstLine(string markKey, MisturaNightTexts.Moment moment)
        {
            if (!ServiceLocator.TryGet(out WorldMapProgress progress) || progress == null || progress.GetTimeMark(markKey) > 0L)
            {
                return;
            }

            progress.SetTimeMark(markKey, 1L);
            string line = MisturaNightTexts.LineFor(moment, ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null);
            if (line != null)
            {
                SayNight(line);
            }
        }
    }
}
