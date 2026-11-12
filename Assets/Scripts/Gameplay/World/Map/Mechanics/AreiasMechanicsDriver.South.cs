using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The far south of the Areias Flamejantes, on the same driver as the sandstorm (the hub keeps it on over the whole region; these parts idle
    /// outside their maps): the sunbeams of the Galerias do Karez (09, <see cref="KarezSunbeamRules"/>: short-lived ManaRegen effects on the
    /// hero, dazzle effects on the beasts in a beam, refreshed while they apply, so they end by themselves if the driver stops), the singing of
    /// the Dunas que Cantam (10, <see cref="DuneSongRules"/>: a slide down a dune stuns the beasts round the hero) and the blinding salt of the
    /// Trono de Namrael (12, <see cref="NamraelGlareRules"/>: at noon the renderers of the beasts past 10 m are switched off, and every one is
    /// switched back on when the glare ends, the hero leaves the salt or the driver stops). Never takes the controls and deals no damage.
    /// </summary>
    public sealed partial class AreiasMechanicsDriver
    {
        private const string SunManaSource = "karez_sun_mana";
        private const string DarkManaSource = "karez_dark_mana";
        private const string DazzleSource = "karez_sun_dazzle";
        private const float SouthEffectSeconds = 1.2f;
        private const float SouthEffectRefresh = 0.5f;
        private const float BeastPassSeconds = 0.5f;
        private const float GlarePassSeconds = 0.2f;
        private const float SouthBeastRange = 80f;
        private const float BeamViewRange = 110f;
        private const float BeamHeight = 26f;
        private const float BeamWidth = 2.6f;
        private const float DarkHintCooldown = 45f;
        private const float BeamHintCooldown = 30f;
        private const float DuneHintCooldown = 90f;
        private const float SongRingSeconds = 0.9f;
        private const int SongRingPoints = 32;
        private const int GlintCount = 6;

        private static readonly Color BeamColor = new Color(0.95f, 0.82f, 0.48f, 0.38f);
        private static readonly Color SongColor = new Color(1f, 0.85f, 0.5f, 1f);
        private static readonly Color GlintColor = new Color(1f, 1f, 0.97f, 0.8f);

        private readonly StaticGroundCache _southGround = new StaticGroundCache();
        private readonly DuneSlide _slide = new DuneSlide();
        private readonly Dictionary<EnemyController, Renderer[]> _glareHidden = new Dictionary<EnemyController, Renderer[]>();
        private readonly List<EnemyController> _glareSeen = new List<EnemyController>();
        private readonly List<EnemyController> _glareStale = new List<EnemyController>();
        private readonly List<Renderer> _rendererScratch = new List<Renderer>();
        private readonly LineRenderer[] _glints = new LineRenderer[GlintCount];

        private BuffController _heroBuffs;
        private StatModifierDefinition _sunMana;
        private StatModifierDefinition _darkMana;
        private StatModifierDefinition _dazzleDamage;
        private StatModifierDefinition _dazzleSpeed;
        private LineRenderer[] _beamLines;
        private LineRenderer _songRing;

        private string _manaSource;
        private float _nextManaRefresh;
        private float _nextSouthBeastPass;
        private float _nextDarkHint;
        private float _nextBeamHint;
        private bool _nightHinted;
        private bool _inGalleries;

        private bool _inDunes;
        private bool _hasLastHero;
        private Vector3 _lastHero;
        private float _lastSong = float.NegativeInfinity;
        private float _songAt = float.NegativeInfinity;
        private Vector3 _songCentre;
        private float _nextDuneHint;

        private bool _onSalt;
        private NamraelGlarePhase _glarePhase;
        private bool _blinded;
        private bool _shadedBefore;
        private float _nextGlarePass;

        private void StartSouth()
        {
            _sunMana = SouthEffect(StatType.ManaRegen, KarezSunbeamRules.BeamManaFactor);
            _darkMana = SouthEffect(StatType.ManaRegen, KarezSunbeamRules.DarkManaFactor);
            _dazzleDamage = SouthEffect(StatType.AttackDamage, KarezSunbeamRules.DazzleDamageFactor);
            _dazzleSpeed = SouthEffect(StatType.MoveSpeed, KarezSunbeamRules.DazzleSpeedFactor);
            _beamLines = new LineRenderer[OverworldLayouts.KarezSunbeams.Length];
        }

        private static StatModifierDefinition SouthEffect(StatType stat, float factor) => new StatModifierDefinition
        {
            displayName = "Karez",
            affectedStat = stat,
            application = ModifierApplication.PercentAdditive,
            value = factor - 1f,
            durationSeconds = SouthEffectSeconds,
            isDebuff = factor < 1f,
        };

        private void TickSouth(Vector2 world, Vector2 local)
        {
            if (_beamLines == null)
            {
                return;
            }

            if (_heroBuffs == null)
            {
                _heroBuffs = _player.GetComponent<BuffController>();
            }

            bool onPost = OverworldRules.SettlementAt(_layout, world) != null;
            if (OverworldLayouts.KarezGalleriesBounds.Contains(local))
            {
                _inGalleries = true;
                TickGalleries(local, onPost);
            }
            else
            {
                LeaveGalleries();
            }

            if (OverworldLayouts.SingingDunesBounds.Contains(local))
            {
                _inDunes = true;
                TickDunes(local, onPost);
            }
            else
            {
                LeaveDunes();
            }

            if (OverworldLayouts.NamraelSaltBounds.Contains(local))
            {
                _onSalt = true;
                TickSalt(local, onPost);
            }
            else
            {
                LeaveSalt();
            }
        }

        private void LeaveSouth()
        {
            LeaveGalleries();
            LeaveDunes();
            LeaveSalt();
        }

        // ------------------------------------------------------------------
        // 09 · The sunbeams of the karez
        // ------------------------------------------------------------------

        private static bool IsDay() => !ServiceLocator.TryGet(out TimeOfDayManager time) || time == null || KarezSunbeamRules.IsDay(time.DaylightFactor);

        private void TickGalleries(Vector2 local, bool onPost)
        {
            bool day = IsDay();
            Vector2[] beams = OverworldLayouts.KarezSunbeams;
            bool inBeam = KarezSunbeamRules.BeamAt(beams, local) >= 0;
            float factor = _stats.IsAlive ? KarezSunbeamRules.HeroManaFactor(true, onPost, day, inBeam) : 1f;
            SetHeroMana(factor > 1f ? SunManaSource : factor < 1f ? DarkManaSource : null);

            if (!day)
            {
                HideLines(_beamLines);
                if (!_nightHinted && !onPost)
                {
                    _nightHinted = true;
                    SaySouth(AreiasSouthTexts.KarezNight);
                }

                return;
            }

            _nightHinted = false;
            ShowBeams(local);
            if (factor < 1f && Time.unscaledTime >= _nextDarkHint)
            {
                _nextDarkHint = Time.unscaledTime + DarkHintCooldown;
                SaySouth(AreiasSouthTexts.KarezDark);
            }
            else if (factor > 1f && Time.unscaledTime >= _nextBeamHint)
            {
                _nextBeamHint = Time.unscaledTime + BeamHintCooldown;
                SaySouth(AreiasSouthTexts.KarezBeam);
            }

            TickDazzle(local, beams);
        }

        private void SetHeroMana(string source)
        {
            if (_heroBuffs == null)
            {
                return;
            }

            if (source != _manaSource)
            {
                if (_manaSource != null)
                {
                    _heroBuffs.RemoveEffectsBySource(_manaSource);
                }

                _manaSource = source;
                _nextManaRefresh = 0f;
            }

            if (source == null || Time.time < _nextManaRefresh)
            {
                return;
            }

            _nextManaRefresh = Time.time + SouthEffectRefresh;
            _heroBuffs.ApplyEffect(source == SunManaSource ? _sunMana : _darkMana, source);
        }

        /// <summary>The beasts in a beam are dazzled: softer and slower while they stay in it (bosses left alone).</summary>
        private void TickDazzle(Vector2 local, Vector2[] beams)
        {
            if (Time.time < _nextSouthBeastPass)
            {
                return;
            }

            _nextSouthBeastPass = Time.time + BeastPassSeconds;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.IsBoss || enemy.Buffs == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _origin;
                if ((at - local).sqrMagnitude > SouthBeastRange * SouthBeastRange || !OverworldLayouts.KarezGalleriesBounds.Contains(at))
                {
                    continue;
                }

                if (KarezSunbeamRules.Dazzled(true, KarezSunbeamRules.BeamAt(beams, at) >= 0, enemy.IsBoss))
                {
                    enemy.Buffs.ApplyEffect(_dazzleDamage, DazzleSource);
                    enemy.Buffs.ApplyEffect(_dazzleSpeed, DazzleSource);
                }
            }
        }

        private void ShowBeams(Vector2 local)
        {
            Vector2[] beams = OverworldLayouts.KarezSunbeams;
            for (int i = 0; i < beams.Length; i++)
            {
                bool show = (beams[i] - local).sqrMagnitude <= BeamViewRange * BeamViewRange;
                LineRenderer line = _beamLines[i];
                if (!show)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }

                    continue;
                }

                if (line == null)
                {
                    line = _beamLines[i] = NewLine("Ar_KarezSunbeam", BeamWidth, BeamColor);
                    Vector2 point = _origin + beams[i];
                    float y = _southGround.GroundAt(_layout, point);
                    line.SetPosition(0, new Vector3(point.x, y, point.y));
                    line.SetPosition(1, new Vector3(point.x, y + BeamHeight, point.y));
                }

                line.enabled = true;
            }
        }

        private void LeaveGalleries()
        {
            if (!_inGalleries)
            {
                return;
            }

            _inGalleries = false;
            SetHeroMana(null);
            HideLines(_beamLines);
            _nightHinted = false;
        }

        // ------------------------------------------------------------------
        // 10 · The singing dunes
        // ------------------------------------------------------------------

        private void TickDunes(Vector2 local, bool onPost)
        {
            Vector3 hero = _player.position;
            float dt = Time.deltaTime;
            bool sand = !onPost && _stats.IsAlive && !OverworldLayouts.InDunesCrescent(local);
            if (!_hasLastHero || dt <= 0f || !sand)
            {
                _hasLastHero = true;
                _lastHero = hero;
                _slide.Reset();
                ShowSongRing();
                return;
            }

            Vector3 delta = hero - _lastHero;
            _lastHero = hero;
            float speed = new Vector2(delta.x, delta.z).magnitude / dt;
            if (_slide.Feed(delta.y, speed) && DuneSongRules.CanSing(_lastSong, Time.time))
            {
                Sing(local);
            }

            if (!DuneSongRules.CanSing(_lastSong, Time.time) || Time.unscaledTime < _nextDuneHint)
            {
                ShowSongRing();
                return;
            }

            _nextDuneHint = Time.unscaledTime + DuneHintCooldown;
            SaySouth(AreiasSouthTexts.DuneHint);
            ShowSongRing();
        }

        /// <summary>The sand sings: every beast within reach, bosses excepted, is frozen with fright.</summary>
        private void Sing(Vector2 local)
        {
            _lastSong = Time.time;
            _songAt = Time.time;
            _songCentre = _player.position;
            _nextDuneHint = Time.unscaledTime + DuneHintCooldown;
            SaySouth(AreiasSouthTexts.DuneSings);
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Buffs == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _origin;
                if (DuneSongRules.Scares(enemy.IsBoss, Vector2.Distance(at, local)))
                {
                    enemy.Buffs.ApplyStun(DuneSongRules.ScareSeconds);
                }
            }
        }

        /// <summary>A golden ring that runs out over the sand from where the dune sang.</summary>
        private void ShowSongRing()
        {
            float t = (Time.time - _songAt) / SongRingSeconds;
            if (t < 0f || t > 1f)
            {
                if (_songRing != null)
                {
                    _songRing.enabled = false;
                }

                return;
            }

            if (_songRing == null)
            {
                _songRing = NewLine("Ar_DuneSong", 0.3f, SongColor);
                _songRing.positionCount = SongRingPoints + 1;
            }

            _songRing.enabled = true;
            float radius = Mathf.Lerp(2f, DuneSongRules.ScareRadius, t);
            _songRing.startColor = _songRing.endColor = new Color(SongColor.r, SongColor.g, SongColor.b, 1f - t * 0.8f);
            for (int i = 0; i <= SongRingPoints; i++)
            {
                float a = i * Mathf.PI * 2f / SongRingPoints;
                _songRing.SetPosition(i, _songCentre + new Vector3(Mathf.Cos(a) * radius, 0.4f, Mathf.Sin(a) * radius));
            }
        }

        private void LeaveDunes()
        {
            if (!_inDunes)
            {
                return;
            }

            _inDunes = false;
            _hasLastHero = false;
            _slide.Reset();
            if (_songRing != null)
            {
                _songRing.enabled = false;
            }
        }

        // ------------------------------------------------------------------
        // 12 · The blinding salt of Namrael
        // ------------------------------------------------------------------

        private void TickSalt(Vector2 local, bool onPost)
        {
            float hour = ServiceLocator.TryGet(out TimeOfDayManager time) && time != null ? time.CurrentHour : 0f;
            NamraelGlarePhase phase = NamraelGlareRules.PhaseAt(hour);
            bool shaded = NamraelGlareRules.InShade(OverworldLayouts.NamraelShadeTowers, local);
            bool blinded = _stats.IsAlive && NamraelGlareRules.Blinded(phase, true, shaded, onPost);

            if (phase != _glarePhase)
            {
                if (phase == NamraelGlarePhase.Rising)
                {
                    SaySouth(AreiasSouthTexts.GlareRising);
                }
                else if (phase == NamraelGlarePhase.None && _glarePhase == NamraelGlarePhase.Glare)
                {
                    SaySouth(AreiasSouthTexts.GlareOver);
                }

                _glarePhase = phase;
            }

            if (blinded && !_blinded)
            {
                SaySouth(AreiasSouthTexts.GlareOn);
            }
            else if (!blinded && _blinded && phase == NamraelGlarePhase.Glare && (shaded || onPost) && !_shadedBefore)
            {
                SaySouth(AreiasSouthTexts.GlareShade);
            }

            _shadedBefore = shaded || onPost;
            _blinded = blinded;
            ShowGlints(blinded);
            if (!blinded)
            {
                RevealAll();
                return;
            }

            if (Time.time < _nextGlarePass)
            {
                return;
            }

            _nextGlarePass = Time.time + GlarePassSeconds;
            _glareSeen.Clear();
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _origin;
                if (!NamraelGlareRules.Hidden(true, enemy.IsBoss, Vector2.Distance(at, local)))
                {
                    continue;
                }

                _glareSeen.Add(enemy);
                HideInGlare(enemy);
            }

            // Every beast hidden before and not lost in the glare now (came close, died, despawned) shows again.
            _glareStale.Clear();
            foreach (EnemyController enemy in _glareHidden.Keys)
            {
                if (enemy == null || !_glareSeen.Contains(enemy))
                {
                    _glareStale.Add(enemy);
                }
            }

            foreach (EnemyController enemy in _glareStale)
            {
                Reveal(enemy);
            }
        }

        private void HideInGlare(EnemyController enemy)
        {
            if (_glareHidden.ContainsKey(enemy))
            {
                return;
            }

            _rendererScratch.Clear();
            enemy.GetComponentsInChildren(false, _rendererScratch);
            var hidden = new List<Renderer>(_rendererScratch.Count);
            foreach (Renderer renderer in _rendererScratch)
            {
                if (renderer != null && renderer.enabled)
                {
                    renderer.enabled = false;
                    hidden.Add(renderer);
                }
            }

            _glareHidden[enemy] = hidden.ToArray();
        }

        private void Reveal(EnemyController enemy)
        {
            if (!_glareHidden.TryGetValue(enemy, out Renderer[] renderers))
            {
                return;
            }

            _glareHidden.Remove(enemy);
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }

        private void RevealAll()
        {
            if (_glareHidden.Count == 0)
            {
                return;
            }

            foreach (Renderer[] renderers in _glareHidden.Values)
            {
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                    }
                }
            }

            _glareHidden.Clear();
        }

        /// <summary>White glints of the salt round a blinded hero.</summary>
        private void ShowGlints(bool show)
        {
            for (int i = 0; i < GlintCount; i++)
            {
                LineRenderer glint = _glints[i];
                if (!show)
                {
                    if (glint != null)
                    {
                        glint.enabled = false;
                    }

                    continue;
                }

                glint = glint != null ? glint : (_glints[i] = NewLine("Ar_SaltGlint", 0.12f, GlintColor));
                glint.enabled = true;
                float a = (i / (float)GlintCount + Time.time * 0.05f) * Mathf.PI * 2f;
                float r = NamraelGlareRules.SightRange + 0.5f + Mathf.PingPong(Time.time * 0.7f + i * 0.31f, 1.5f);
                Vector3 foot = _player.position + new Vector3(Mathf.Cos(a) * r, 0.2f, Mathf.Sin(a) * r);
                glint.SetPosition(0, foot);
                glint.SetPosition(1, foot + Vector3.up * (1.2f + (i % 3) * 0.5f));
            }
        }

        private void LeaveSalt()
        {
            if (!_onSalt)
            {
                return;
            }

            _onSalt = false;
            _blinded = false;
            _shadedBefore = false;
            _glarePhase = NamraelGlarePhase.None;
            RevealAll();
            ShowGlints(false);
        }

        // ------------------------------------------------------------------

        private static void HideLines(LineRenderer[] lines)
        {
            if (lines == null)
            {
                return;
            }

            foreach (LineRenderer line in lines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private void SaySouth(string key)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, AreiasSouthTexts.Get(localization, key));
        }
    }
}
