using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of three small services of the race cities in the seamless world (<see cref="CityServiceRules"/>): Puzur's rune pillar in
    /// the Salões de Bronze (one free rune a day), the Lira do Touro in Aratta (the song of the day: light feet for 5 min) and the war drum of
    /// the Tambor Rubro (the Têmpera do Tambor while the drums play). Added to the overworld root; each point exists only when the layout holds
    /// its region. The song is a short StaminaRegen-style effect refreshed while it applies, so it ends by itself (on a mount, in a fight, or if
    /// the driver stops); the Têmpera is one 10-minute effect. The only saves are the two daily marks. The hero keeps the controls.
    /// </summary>
    public sealed class CityServicesMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string SongSource = "city_lyre_song";
        private const string TemperSource = "city_drum_temper";
        private const float SongEffectSeconds = 1.2f;
        private const float SongRefresh = 0.5f;
        private const float HintCooldown = 6f;
        private const float FeedbackHeight = 2.4f;

        /// <summary>Metres round each point where the driver stays on.</summary>
        private const float AreaHalf = 40f;

        private enum ServiceKind
        {
            Rune,
            Lyre,
            Temper,
        }

        private struct ServicePoint
        {
            public ServiceKind Kind;
            public Vector2 World;
            public float Reach;
        }

        private readonly List<ServicePoint> _points = new List<ServicePoint>();
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private BuffController _buffs;
        private StatModifierDefinition _songEffect;
        private StatModifierDefinition _temperEffect;

        private float _songUntil = float.NegativeInfinity;
        private bool _songOn;
        private float _nextSongRefresh;
        private int _hintPoint = -1;
        private float _nextHint;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => Time.time < _songUntil;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            OverworldLayout layout = view != null ? view.Layout : null;
            AddPoint(layout, OverworldLayouts.KaggenAnchorZoneId, OverworldLayouts.SaloesOffset + CityTownRecipes.BronzeHallsRuneStone,
                ServiceKind.Rune, CityServiceRules.RuneReach);
            AddPoint(layout, OverworldLayouts.CostaAnchorZoneId, OverworldLayouts.ArattaOffset + CityTownRecipes.ArattaBullLyre,
                ServiceKind.Lyre, CityServiceRules.LyreReach);
            AddPoint(layout, OverworldLayouts.AreiasAnchorZoneId, OverworldLayouts.TamborRubroOffset + CityTownRecipes.TamborRubroTemperDrum,
                ServiceKind.Temper, CityServiceRules.TemperReach);
            if (_points.Count == 0)
            {
                enabled = false;
                return;
            }

            _areas = new Rect[_points.Count];
            for (int i = 0; i < _points.Count; i++)
            {
                _areas[i] = new Rect(_points[i].World - new Vector2(AreaHalf, AreaHalf), new Vector2(AreaHalf * 2f, AreaHalf * 2f));
            }
        }

        private void AddPoint(OverworldLayout layout, string anchorZoneId, Vector2 regionLocal, ServiceKind kind, float reach)
        {
            OverworldZone anchor = layout?.Zone(anchorZoneId);
            if (anchor != null)
            {
                _points.Add(new ServicePoint { Kind = kind, World = anchor.Origin + regionLocal, Reach = reach });
            }
        }

        private void OnDisable()
        {
            SetSong(false);
            _hintPoint = -1;
        }

        private void Update()
        {
            if (_areas == null || !FindPlayer())
            {
                return;
            }

            bool alive = _stats == null || _stats.IsAlive;
            bool inCombat = InCombat();
            TickSong(alive, inCombat);
            if (!alive || _controller.Input == null)
            {
                _hintPoint = -1;
                return;
            }

            Vector2 hero = new Vector2(_player.position.x, _player.position.z);
            int at = -1;
            for (int i = 0; i < _points.Count; i++)
            {
                if (CityServiceRules.InReach(_points[i].World, hero, _points[i].Reach))
                {
                    at = i;
                    break;
                }
            }

            if (at < 0)
            {
                _hintPoint = -1;
                return;
            }

            ServicePoint point = _points[at];
            if (_hintPoint != at || Time.unscaledTime >= _nextHint)
            {
                _hintPoint = at;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(Prompt(point.Kind));
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            long today = CityServiceRules.DayOf(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            switch (point.Kind)
            {
                case ServiceKind.Rune:
                    UseRune(today);
                    break;
                case ServiceKind.Lyre:
                    UseLyre(today, inCombat);
                    break;
                default:
                    UseTemper(today, inCombat);
                    break;
            }
        }

        private bool FindPlayer()
        {
            if (_player == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(PlayerTag);
                if (go == null)
                {
                    return false;
                }

                _player = go.transform;
                _controller = go.GetComponent<PlayerController>();
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
                _buffs = go.GetComponent<BuffController>();
            }

            return _controller != null;
        }

        private static bool InCombat()
        {
            MountController mount = MountController.Active;
            return mount != null && MountRules.IsInCombat(mount.SecondsSinceCombat);
        }

        private static bool Mounted => MountController.Active != null && MountController.Active.IsMounted;

        private static CharacterId? Hero() => ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

        private static string Prompt(ServiceKind kind)
        {
            switch (kind)
            {
                case ServiceKind.Rune:
                    return CityServiceTexts.RunePrompt;
                case ServiceKind.Lyre:
                    return CityServiceTexts.LyrePrompt;
                default:
                    return CityServiceTexts.TemperPrompt;
            }
        }

        // ------------------------------------------------------------------
        // Puzur's rune
        // ------------------------------------------------------------------

        private void UseRune(long today)
        {
            ServiceLocator.TryGet(out WorldMapProgress progress);
            long takenDay = progress != null ? progress.GetDailyMark(CityServiceRules.RuneDailyKey) : 0;
            if (CityServiceRules.CheckRune(takenDay, today) != CityServiceCheck.Ready)
            {
                Say(CityServiceTexts.RuneTaken);
                return;
            }

            ItemDefinition rune = ItemDatabase.GetById(CityServiceRules.RuneOf(today));
            if (rune == null || _bag == null || _bag.AddItem(rune, 1) > 0)
            {
                Say(CityServiceTexts.BagFull);
                return;
            }

            progress?.SetDailyMark(CityServiceRules.RuneDailyKey, (int)today);
            CharacterId? cousin = Hero();
            if (cousin.HasValue)
            {
                Say(CityServiceTexts.RuneLine(cousin.Value));
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            SayAbove(string.Format(CityServiceTexts.Get(localization, CityServiceTexts.RuneReceived), rune.DisplayName, 1));
        }

        // ------------------------------------------------------------------
        // The Lira do Touro
        // ------------------------------------------------------------------

        private void UseLyre(long today, bool inCombat)
        {
            if (CityServiceRules.CheckLyre(inCombat) != CityServiceCheck.Ready)
            {
                Say(CityServiceTexts.LyreCombat);
                return;
            }

            bool renewing = Time.time < _songUntil;
            _songUntil = Time.time + CityServiceRules.SongSeconds;
            ServiceLocator.TryGet(out LocalizationManager localization);
            string song = CityServiceTexts.Get(localization, CityServiceTexts.SongName(CityServiceRules.SongOf(today)));
            SayAbove(string.Format(CityServiceTexts.Get(localization, renewing ? CityServiceTexts.LyreAgain : CityServiceTexts.LyrePlaying), song));
            CharacterId? cousin = Hero();
            if (!renewing && cousin.HasValue)
            {
                Say(CityServiceTexts.SongLine(cousin.Value));
            }
        }

        private void TickSong(bool alive, bool inCombat)
        {
            float left = _songUntil - Time.time;
            if (left <= 0f && _songUntil > float.NegativeInfinity)
            {
                _songUntil = float.NegativeInfinity;
                SetSong(false);
                Say(CityServiceTexts.LyreEnd);
                return;
            }

            SetSong(CityServiceRules.SongApplies(left, inCombat, Mounted, alive));
        }

        private void SetSong(bool on)
        {
            if (_buffs == null)
            {
                return;
            }

            if (!on)
            {
                if (_songOn)
                {
                    _buffs.RemoveEffectsBySource(SongSource);
                    _songOn = false;
                }

                return;
            }

            if (_songOn && Time.time < _nextSongRefresh)
            {
                return;
            }

            _songEffect ??= new StatModifierDefinition
            {
                displayName = "Canção da Lira",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = CityServiceRules.SongSpeedBonus,
                durationSeconds = SongEffectSeconds,
                isDebuff = false,
            };
            _songOn = true;
            _nextSongRefresh = Time.time + SongRefresh;
            _buffs.ApplyEffect(_songEffect, SongSource);
        }

        // ------------------------------------------------------------------
        // The Têmpera do Tambor
        // ------------------------------------------------------------------

        private void UseTemper(long today, bool inCombat)
        {
            ServiceLocator.TryGet(out WorldMapProgress progress);
            long givenDay = progress != null ? progress.GetDailyMark(CityServiceRules.TemperDailyKey) : 0;
            double nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            CityServiceCheck check = CityServiceRules.CheckTemper(DrumRhythmRules.Playing(nowSeconds), givenDay, today, inCombat);
            if (check != CityServiceCheck.Ready)
            {
                Say(check == CityServiceCheck.TakenToday ? CityServiceTexts.TemperTaken
                    : check == CityServiceCheck.InCombat ? CityServiceTexts.TemperCombat : CityServiceTexts.TemperWait);
                return;
            }

            if (_buffs == null)
            {
                return;
            }

            _temperEffect ??= new StatModifierDefinition
            {
                displayName = "Têmpera do Tambor",
                affectedStat = StatType.AttackDamage,
                application = ModifierApplication.PercentAdditive,
                value = CityServiceRules.TemperDamageBonus,
                durationSeconds = CityServiceRules.TemperSeconds,
                isDebuff = false,
            };
            _buffs.ApplyEffect(_temperEffect, TemperSource);
            progress?.SetDailyMark(CityServiceRules.TemperDailyKey, (int)today);
            Say(CityServiceTexts.TemperDone);
            CharacterId? cousin = Hero();
            if (cousin.HasValue)
            {
                SayAbove(CityServiceTexts.Get(ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null,
                    CityServiceTexts.TemperLine(cousin.Value)));
            }
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void Say(string key)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager manager);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, CityServiceTexts.Get(manager, key));
        }

        private void SayAbove(string text)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), text);
            }
        }
    }
}
