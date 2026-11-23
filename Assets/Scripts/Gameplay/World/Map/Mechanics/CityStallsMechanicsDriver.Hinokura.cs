using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Kurogane's forge of the day in Hinokura (65, <see cref="HinokuraDayForgeRules"/>): by the Forja de Hinokura in the plaza he says which of the
    /// three forges is today's and whether it burns; while the hero is in the region the driver watches the sluices' saved moments and says when
    /// the forge of the day catches (with the cousin's line) or goes out. The bonus itself is read by <c>RefineService.ForgeBonus</c>.
    /// </summary>
    public sealed partial class CityStallsMechanicsDriver
    {
        private const float ForgeHintCooldown = 15f;
        private const float ForgeWatchSeconds = 0.5f;

        private bool _hinokuraOn;
        private Vector2 _plazaForge;
        private readonly long[] _forgeLitAt = new long[HinokuraForgeRules.Forges];
        private float _nextForgeHint;
        private float _nextForgeWatch;
        private bool? _dayForgeWasLit;

        private void StartHinokura(OverworldLayout layout, List<Rect> areas)
        {
            OverworldZone anchor = layout?.Zone(OverworldLayouts.HinokuraAnchorZoneId);
            if (anchor == null)
            {
                return;
            }

            _hinokuraOn = true;
            _plazaForge = anchor.Origin + OverworldLayouts.HinokuraOffset + CityTownRecipes.HinokuraForge;
            areas.Add(Around(_plazaForge, AreaHalf));
            foreach (Vector2 sluice in OverworldLayouts.ForgeSluices)
            {
                areas.Add(Around(anchor.Origin + sluice, 60f));
            }
        }

        private static string ForgeName(int forge, LocalizationManager l) => HinokuraMechanicsTexts.Get(l, HinokuraMechanicsTexts.ForgeNames[forge]);

        private void TickHinokura(Vector2 hero)
        {
            if (!_hinokuraOn || Time.unscaledTime < _nextForgeWatch)
            {
                return;
            }

            _nextForgeWatch = Time.unscaledTime + ForgeWatchSeconds;
            WorldMapProgress progress = Progress();
            if (progress == null)
            {
                return;
            }

            HinokuraMechanicsDriver.ReadForges(progress, _forgeLitAt);
            long unix = UnixNow();
            int forge = HinokuraDayForgeRules.ForgeOfDay(CityServiceRules.DayOf(unix));
            bool lit = HinokuraDayForgeRules.DayForgeLit(_forgeLitAt, unix);
            LocalizationManager l = Localization();

            if (_dayForgeWasLit.HasValue && _dayForgeWasLit.Value != lit)
            {
                if (lit)
                {
                    SayAbove(CityStallsTexts.Format(l, CityStallsTexts.ForgeDayLit, ForgeName(forge, l)));
                    SayCousin(CityStallsTexts.ForgeLine);
                }
                else
                {
                    SayAbove(CityStallsTexts.Get(l, CityStallsTexts.ForgeDayOut));
                }
            }

            _dayForgeWasLit = lit;

            if (CityServiceRules.InReach(_plazaForge, hero, HinokuraDayForgeRules.PlazaReach) && Time.unscaledTime >= _nextForgeHint)
            {
                _nextForgeHint = Time.unscaledTime + ForgeHintCooldown;
                if (lit)
                {
                    long left = HinokuraForgeRules.GoesOutAt(forge, _forgeLitAt[forge]) - unix;
                    SayAbove(CityStallsTexts.Format(l, CityStallsTexts.ForgeDayHintLit, ForgeName(forge, l), Mathf.Max(1, Mathf.CeilToInt(left / 60f))));
                }
                else
                {
                    SayAbove(CityStallsTexts.Format(l, CityStallsTexts.ForgeDayHintOut, ForgeName(forge, l)));
                }
            }
        }
    }
}
