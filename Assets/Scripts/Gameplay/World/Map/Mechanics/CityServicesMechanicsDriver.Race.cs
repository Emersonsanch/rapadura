using System;
using System.Globalization;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Corrida de Shulgi in Aratta (<see cref="ShulgiRaceRules"/>): [E] between the horn posts of the South street calls a
    /// <see cref="NartesRaceSession"/> on <see cref="OverworldLayouts.ShulgiRaceCourse"/>; a gold beacon and ring mark the next gate; the medal
    /// pays Fitas de Corrida into the mount collection's ribbon store, and Shulgi's ribbon post trades them for the Auroque de Aratta and the
    /// two Aratta cosmetics. The hero keeps every control; riding away, dying or leaving ends the race by itself.
    /// </summary>
    public sealed partial class CityServicesMechanicsDriver
    {
        private const float GateBeaconHeight = 24f;
        private const int RingSegments = 32;
        private static readonly Color RibbonBlue = new Color(0.25f, 0.49f, 0.75f, 1f);

        private NartesRaceSession _race;
        private int _raceCount = -1;
        private Vector2 _raceOrigin;
        private OverworldLayout _raceLayout;
        private LineRenderer _gateBeacon;
        private LineRenderer _gateRing;
        private int _gateDrawn = -1;
        private Material _lineMaterial;

        private void StartRace(bool inCombat)
        {
            if (_race != null)
            {
                return;
            }

            if (inCombat)
            {
                Say(CityServiceTexts.RaceCombat);
                return;
            }

            _race = new NartesRaceSession(OverworldLayouts.ShulgiRaceCourse);
            _raceCount = -1;
            _gateDrawn = -1;
            CharacterId? cousin = Hero();
            if (cousin.HasValue)
            {
                SayAbove(CityServiceTexts.Get(Localization(), CityServiceTexts.RaceLine(cousin.Value)));
            }
        }

        private void TickRace(bool alive)
        {
            if (_race == null)
            {
                return;
            }

            if (!alive)
            {
                AbandonRace();
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _raceOrigin;
            NartesRaceState before = _race.State;
            bool took = _race.Tick(Time.deltaTime, local);
            LocalizationManager localization = Localization();
            if (_race.State == NartesRaceState.Countdown && _race.CountdownLeft != _raceCount)
            {
                _raceCount = _race.CountdownLeft;
                SayAbove(string.Format(CityServiceTexts.Get(localization, CityServiceTexts.RaceCount), _raceCount));
            }

            if (before == NartesRaceState.Countdown && _race.State == NartesRaceState.Running)
            {
                Say(CityServiceTexts.RaceGo);
            }

            if (_race.IsOver)
            {
                FinishRace();
                return;
            }

            if (took)
            {
                SayAbove(string.Format(CityServiceTexts.Get(localization, CityServiceTexts.RaceGate), _race.NextGate, _race.GateCount,
                    Mathf.FloorToInt(_race.Elapsed)));
            }

            ShowGate();
        }

        private void FinishRace()
        {
            LocalizationManager localization = Localization();
            switch (_race.State)
            {
                case NartesRaceState.Finished:
                    string seconds = _race.Elapsed.ToString("0.0", CultureInfo.InvariantCulture);
                    NartesRaceMedal medal = ShulgiRaceRules.MedalFor(_race.Elapsed);
                    if (medal == NartesRaceMedal.None)
                    {
                        SayAbove(string.Format(CityServiceTexts.Get(localization, CityServiceTexts.RaceNoMedal), seconds));
                        break;
                    }

                    long today = CityServiceRules.DayOf(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    ServiceLocator.TryGet(out WorldMapProgress progress);
                    bool first = progress == null || progress.GetDailyMark(ShulgiRaceRules.DailyKey) != today;
                    int ribbons = ShulgiRaceRules.RibbonsFor(medal, first);
                    MountCollection collection = MountCollection.ResolveShared();
                    if (collection == null)
                    {
                        break;
                    }

                    collection.AddRibbons(ShulgiRaceRules.RibbonKey, ribbons);
                    if (first)
                    {
                        progress?.SetDailyMark(ShulgiRaceRules.DailyKey, (int)today);
                    }

                    SayAbove(string.Format(CityServiceTexts.Get(localization, CityServiceTexts.RaceMedal),
                        CityServiceTexts.Get(localization, CityServiceTexts.MedalName(medal)), seconds, ribbons,
                        collection.RibbonsOf(ShulgiRaceRules.RibbonKey)));
                    break;
                case NartesRaceState.TimedOut:
                    Say(CityServiceTexts.RaceTimeOut);
                    break;
                default:
                    Say(CityServiceTexts.RaceAbandon);
                    break;
            }

            _race = null;
            HideGate();
        }

        private void AbandonRace()
        {
            if (_race == null)
            {
                return;
            }

            _race.Abandon();
            if (_player != null)
            {
                Say(CityServiceTexts.RaceAbandon);
            }

            _race = null;
            HideGate();
        }

        // ------------------------------------------------------------------
        // Shulgi's ribbon post
        // ------------------------------------------------------------------

        private void UseRibbons()
        {
            MountCollection collection = MountCollection.ResolveShared();
            if (collection == null)
            {
                return;
            }

            int ribbons = collection.RibbonsOf(ShulgiRaceRules.RibbonKey);
            int next = ShulgiRaceRules.NextOffer(offer => offer.Kind == ShulgiOfferKind.Mount ? collection.Owns(offer.Id) : collection.OwnsCosmetic(offer.Id));
            LocalizationManager localization = Localization();
            switch (ShulgiRaceRules.CheckTrade(next, ribbons))
            {
                case ShulgiTradeCheck.AllOwned:
                    SayAbove(string.Format(CityServiceTexts.Get(localization, CityServiceTexts.RibbonAllOwned), ribbons));
                    return;
                case ShulgiTradeCheck.NeedRibbons:
                    ShulgiOffer wanted = ShulgiRaceRules.Offers[next];
                    SayAbove(string.Format(CityServiceTexts.Get(localization, CityServiceTexts.RibbonNeed), OfferName(localization, wanted), wanted.Cost, ribbons));
                    return;
            }

            ShulgiOffer offer = ShulgiRaceRules.Offers[next];
            bool done;
            if (offer.Kind == ShulgiOfferKind.Mount)
            {
                done = collection.RedeemRibbons(ShulgiRaceRules.RibbonKey, offer.Id) != null;
            }
            else
            {
                done = collection.GrantCosmetic(offer.Id) && collection.SpendRibbons(ShulgiRaceRules.RibbonKey, offer.Cost);
            }

            if (!done)
            {
                return;
            }

            SayAbove(string.Format(CityServiceTexts.Get(localization, CityServiceTexts.RibbonTraded), OfferName(localization, offer), offer.Cost,
                collection.RibbonsOf(ShulgiRaceRules.RibbonKey)));
            CharacterId? cousin = Hero();
            if (cousin.HasValue)
            {
                Say(CityServiceTexts.RibbonLine(cousin.Value));
            }
        }

        private static string OfferName(LocalizationManager localization, ShulgiOffer offer)
        {
            if (offer.Kind == ShulgiOfferKind.Mount)
            {
                return MountTexts.MountName(localization, MountCatalog.Get(offer.Id));
            }

            MountCosmeticSpec spec = MountCosmetics.Get(offer.Id);
            return spec != null ? MountTexts.Format(localization, spec.NameKey) : offer.Id;
        }

        // ------------------------------------------------------------------
        // The next gate
        // ------------------------------------------------------------------

        private void ShowGate()
        {
            _gateBeacon = _gateBeacon != null ? _gateBeacon : NewLine("Shulgi_RaceGate", 0.6f);
            _gateRing = _gateRing != null ? _gateRing : NewLine("Shulgi_RaceRing", 0.3f);
            _gateBeacon.enabled = _gateRing.enabled = true;
            if (_gateDrawn == _race.NextGate)
            {
                return;
            }

            _gateDrawn = _race.NextGate;
            Vector2 gate = _raceOrigin + _race.NextGatePosition;
            Vector3 foot = Ground(gate);
            _gateBeacon.startColor = _gateBeacon.endColor = RibbonBlue;
            _gateBeacon.positionCount = 2;
            _gateBeacon.SetPosition(0, foot);
            _gateBeacon.SetPosition(1, foot + Vector3.up * GateBeaconHeight);
            _gateRing.startColor = _gateRing.endColor = RibbonBlue;
            _gateRing.positionCount = RingSegments + 1;
            for (int i = 0; i <= RingSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / RingSegments;
                _gateRing.SetPosition(i, Ground(gate + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * NartesRaceRules.GateRadius) + Vector3.up * 0.4f);
            }
        }

        private void HideGate()
        {
            if (_gateBeacon != null)
            {
                _gateBeacon.enabled = false;
            }

            if (_gateRing != null)
            {
                _gateRing.enabled = false;
            }
        }

        private Vector3 Ground(Vector2 world) => new Vector3(world.x, Mathf.Max(0f, OverworldRules.SurfaceAt(_raceLayout, world)), world.y);

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_lineMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _lineMaterial = new Material(shader) { name = "Mat_ShulgiLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _lineMaterial;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private static LocalizationManager Localization() => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;
    }
}
