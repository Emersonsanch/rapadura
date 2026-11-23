using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The stubborn sparks of the Porão da Lua and the moon that grows on the deck (93, <see cref="MoonDeckSparkRules"/>), on the Céu driver: a small light
    /// at each hiding place, [E] starts talking, any strike of the hero (basic attack or skill) makes the spark sulk; a convinced spark follows the hero
    /// to the lantern of the Cabine da Proa. Each lit lantern is a daily mark (written on delivery) and the feast is one more daily mark, written only when
    /// its rune went into the bag. The cousin's line of the first full moon is kept once (time mark).
    /// </summary>
    public sealed partial class CeuMechanicsDriver
    {
        private const int SpotSparkBase = 200;
        private const int SpotLantern = 210;
        private const float SparkViewRange = 90f;
        private const float LanternViewRange = 220f;
        private const float SparkBeamHeight = 2.4f;
        private const float LanternBeamLow = 3f;
        private const float LanternBeamPerSpark = 4f;
        private const string FeastLineKey = "ceu.moon_deck.first_feast";

        private static readonly Color SparkColor = new Color(1f, 0.86f, 0.45f, 1f);
        private static readonly Color SulkColor = new Color(0.45f, 0.42f, 0.5f, 0.8f);

        private readonly MoonDeckSparkSession _sparks = new MoonDeckSparkSession();
        private readonly int[] _sparkDays = new int[MoonDeckSparkRules.SparkCount];
        private readonly LineRenderer[] _sparkBeams = new LineRenderer[MoonDeckSparkRules.SparkCount];
        private LineRenderer _carriedBeam;
        private LineRenderer _lanternBeam;
        private int _feastDay;
        private bool _struck;
        private float _nextSparkRead;

        private void HookMoonDeck()
        {
            EventBus.Subscribe<SkillCastEvent>(OnHeroSkillForSparks);
            EventBus.Subscribe<PlayerAttackPerformedEvent>(OnHeroAttackForSparks);
        }

        private void UnhookMoonDeck()
        {
            EventBus.Unsubscribe<SkillCastEvent>(OnHeroSkillForSparks);
            EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnHeroAttackForSparks);
            _sparks.Cancel();
        }

        private bool IsHero(GameObject who) => _player != null && who != null && (who == _player.gameObject || who.transform.IsChildOf(_player));

        private void OnHeroSkillForSparks(SkillCastEvent evt) => _struck |= IsHero(evt.Caster);

        private void OnHeroAttackForSparks(PlayerAttackPerformedEvent evt) => _struck |= IsHero(evt.Attacker);

        private static int Today() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.Day : 1;

        private void ReadSparkMarks()
        {
            if (Time.unscaledTime < _nextSparkRead)
            {
                return;
            }

            _nextSparkRead = Time.unscaledTime + 0.5f;
            if (!ServiceLocator.TryGet(out WorldMapProgress progress) || progress == null)
            {
                return;
            }

            for (int i = 0; i < _sparkDays.Length; i++)
            {
                _sparkDays[i] = progress.GetDailyMark(MoonDeckSparkRules.MarkKey(i));
            }

            _feastDay = progress.GetDailyMark(MoonDeckSparkRules.FeastMarkKey);
        }

        private int TickMoonDeck(Vector2 local, int spot)
        {
            bool struck = _struck;
            _struck = false;
            int today = Today();
            ReadSparkMarks();
            ShowSparks(local, today);

            if (_sparks.Talking >= 0)
            {
                int talking = _sparks.Talking;
                bool inStay = (MoonDeckSparkRules.HidingPlaces[talking] - local).sqrMagnitude <= MoonDeckSparkRules.StayReach * MoonDeckSparkRules.StayReach;
                switch (_sparks.TickTalk(Time.deltaTime, inStay, struck, Time.time))
                {
                    case MoonSparkEvent.Sulked:
                        Say2(CeuRound2Texts.SparkSulks);
                        break;
                    case MoonSparkEvent.Cancelled:
                        Say2(CeuRound2Texts.SparkWalkedOff);
                        break;
                    case MoonSparkEvent.Convinced:
                        Say2(CeuRound2Texts.SparkFollows);
                        break;
                }

                return SpotSparkBase + talking;
            }

            if (_sparks.Carrying >= 0)
            {
                int carried = _sparks.Carrying;
                bool aboard = MoonDeckSparkRules.ShipArea.Contains(local);
                bool alive = _stats == null || _stats.IsAlive;
                bool atLantern = (MoonDeckSparkRules.Lantern - local).sqrMagnitude <= MoonDeckSparkRules.DeliverReach * MoonDeckSparkRules.DeliverReach;
                switch (_sparks.TickCarry(aboard, alive, atLantern))
                {
                    case MoonSparkEvent.Dropped:
                        Say2(CeuRound2Texts.SparkDropped);
                        break;
                    case MoonSparkEvent.Delivered:
                        DeliverSpark(carried, today);
                        break;
                }
            }

            TickFeast(local, today);
            if (_sparks.Busy)
            {
                return spot;
            }

            int near = MoonDeckSparkRules.SparkNear(MoonDeckSparkRules.HidingPlaces, local, _sparkDays, today, _sparks.SulkUntil, Time.time);
            if (near < 0)
            {
                return spot;
            }

            if (HintDue(SpotSparkBase + near))
            {
                Say2(CeuRound2Texts.SparkPrompt);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Interacted && _sparks.BeginTalk(near))
            {
                // A strike in this same frame already counts against it.
                _struck = false;
                Say2(CeuRound2Texts.SparkListening);
            }

            return SpotSparkBase + near;
        }

        private void DeliverSpark(int spark, int today)
        {
            _sparkDays[spark] = today;
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
            {
                progress.SetDailyMark(MoonDeckSparkRules.MarkKey(spark), today);
            }

            if (_stats != null && _stats.IsAlive)
            {
                _stats.Heal(MoonDeckSparkRules.DeliverHeal(_stats.MaxHealth));
            }

            int lit = MoonDeckSparkRules.LitCount(_sparkDays, today);
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(CeuRound2Texts.Format(localization, CeuRound2Texts.SparkLit, lit));
            if (Selected() == CharacterId.Lavine && !MoonDeckSparkRules.IsFullMoon(lit))
            {
                Say2(CeuRound2Texts.SparkLavine);
            }
        }

        /// <summary>The feast of the full moon: once a game day, its mark written only when the rune went into the bag (otherwise it waits by the lantern).</summary>
        private void TickFeast(Vector2 local, int today)
        {
            int lit = MoonDeckSparkRules.LitCount(_sparkDays, today);
            if (!MoonDeckSparkRules.FeastDue(lit, _feastDay, today)
                || (MoonDeckSparkRules.Lantern - local).sqrMagnitude > MoonDeckSparkRules.DeliverReach * MoonDeckSparkRules.DeliverReach
                || !HintDue(SpotLantern))
            {
                return;
            }

            if (!MechanicsBagGrant.Grant(_bag, (MoonDeckSparkRules.FeastItemId, 1)))
            {
                Say2(CeuRound2Texts.FeastBagFull);
                return;
            }

            _feastDay = today;
            bool known = ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null;
            if (known)
            {
                progress.SetDailyMark(MoonDeckSparkRules.FeastMarkKey, today);
            }

            if (_stats != null && _stats.IsAlive)
            {
                _stats.Heal(_stats.MaxHealth);
            }

            Say2(CeuRound2Texts.FeastFull);
            Say2(CeuRound2Texts.FeastRadio);
            if (known && progress.GetTimeMark(FeastLineKey) <= 0L)
            {
                progress.SetTimeMark(FeastLineKey, 1L);
                string line = CeuRound2Texts.LineFor(CeuRound2Texts.Moment.Feast, Selected());
                if (line != null)
                {
                    Say2(line);
                }
            }
        }

        private static CharacterId? Selected() => ServiceLocator.TryGet(out CharacterSelectionState selection) && selection != null ? selection.Selected : null;

        private void ShowSparks(Vector2 local, int today)
        {
            for (int i = 0; i < _sparkBeams.Length; i++)
            {
                Vector2 place = MoonDeckSparkRules.HidingPlaces[i];
                bool show = (place - local).sqrMagnitude <= SparkViewRange * SparkViewRange && !MoonDeckSparkRules.LitToday(_sparkDays[i], today) && _sparks.Carrying != i;
                if (show && _sparkBeams[i] == null)
                {
                    _sparkBeams[i] = NewLine("Ceu_StubbornSpark", 0.35f);
                }

                LineRenderer beam = _sparkBeams[i];
                if (beam == null)
                {
                    continue;
                }

                beam.enabled = show;
                if (!show)
                {
                    continue;
                }

                Vector3 foot = Ground(place);
                bool sulking = Time.time < _sparks.SulkUntil[i];
                float flicker = _sparks.Talking == i ? 0.3f + _sparks.TalkFraction : 0.3f;
                beam.startColor = beam.endColor = sulking ? SulkColor : SparkColor;
                beam.SetPosition(0, foot + Vector3.up * flicker);
                beam.SetPosition(1, foot + Vector3.up * (flicker + SparkBeamHeight * (sulking ? 0.3f : 1f)));
            }

            bool carrying = _sparks.Carrying >= 0;
            if (carrying && _carriedBeam == null)
            {
                _carriedBeam = NewLine("Ceu_CarriedSpark", 0.25f);
                _carriedBeam.startColor = _carriedBeam.endColor = SparkColor;
            }

            if (_carriedBeam != null)
            {
                _carriedBeam.enabled = carrying;
                if (carrying)
                {
                    Vector3 at = _player.position + _player.right * 0.8f + Vector3.up * (1.8f + Mathf.Sin(Time.time * 3f) * 0.15f);
                    _carriedBeam.SetPosition(0, at);
                    _carriedBeam.SetPosition(1, at + Vector3.up * 0.5f);
                }
            }

            bool nearLantern = (MoonDeckSparkRules.Lantern - local).sqrMagnitude <= LanternViewRange * LanternViewRange;
            if (nearLantern && _lanternBeam == null)
            {
                _lanternBeam = NewLine("Ceu_MoonLantern", 0.5f);
            }

            if (_lanternBeam != null)
            {
                _lanternBeam.enabled = nearLantern;
                if (nearLantern)
                {
                    int lit = MoonDeckSparkRules.LitCount(_sparkDays, today);
                    Vector3 foot = Ground(MoonDeckSparkRules.Lantern + new Vector2(6f, 0f));
                    _lanternBeam.startColor = _lanternBeam.endColor = MoonDeckSparkRules.IsFullMoon(lit) ? Color.white : Moon;
                    _lanternBeam.SetPosition(0, foot);
                    _lanternBeam.SetPosition(1, foot + Vector3.up * (LanternBeamLow + LanternBeamPerSpark * lit));
                }
            }
        }

        private void QuietMoonDeck()
        {
            _sparks.Cancel();
            foreach (LineRenderer beam in _sparkBeams)
            {
                Hide(beam);
            }

            Hide(_carriedBeam);
            Hide(_lanternBeam);
        }

        private void Say2(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(CeuRound2Texts.Get(localization, key));
        }
    }
}
