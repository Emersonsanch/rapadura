using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Makarios' moon counter at his door on the East street of the Porto da Lua Branca (59, <see cref="PortoMoonStallRules"/>): [E] opens a
    /// <see cref="CityServicePanel"/> with the two bundles of tonight's moon, each once per in-game day.
    /// </summary>
    public sealed partial class CityStallsMechanicsDriver
    {
        private bool _portoOn;
        private Vector2 _moonPoint;

        private void StartPorto(OverworldLayout layout, List<Rect> areas)
        {
            OverworldZone anchor = layout?.Zone(OverworldLayouts.MarGeladoAnchorZoneId);
            if (anchor == null)
            {
                return;
            }

            _portoOn = true;
            Vector2 town = anchor.Origin + OverworldLayouts.PortoLuaBrancaOffset;
            _moonPoint = town + DoorOf(CityTownRecipes.PortoDaLuaBrancaCityId, "makarios");
            areas.Add(Around(town, 40f));
        }

        private static string MoonName(int day, LocalizationManager l) => CityStallsTexts.Get(l, CityStallsTexts.MoonNames[MoonCanalRules.MoonIndex(day)]);

        private sealed class MoonScreen : ICityServiceScreen
        {
            private readonly CityStallsMechanicsDriver _driver;
            private readonly int _day;
            private readonly int _moon;

            public MoonScreen(CityStallsMechanicsDriver driver)
            {
                _driver = driver;
                _day = GameDay();
                _moon = MoonCanalRules.MoonIndex(_day);
            }

            private TunnelFairLine[] Lines => PortoMoonStallRules.Lines[_moon];

            public string Title(LocalizationManager l) => CityStallsTexts.Get(l, CityStallsTexts.MoonTitle);

            public string Intro(LocalizationManager l) => CityStallsTexts.Format(l, CityStallsTexts.MoonIntro, MoonName(_day, l));

            public string Summary(LocalizationManager l) => CityStallsTexts.Format(l, CityStallsTexts.MoonSummary, _driver.Gold, MoonName(_day + 1, l));

            public IReadOnlyList<CityServiceRow> Rows(LocalizationManager l) => _driver.BundleRows(Lines, i => PortoMoonStallRules.LineKey(_moon, i), _day, l);

            public string Pick(int row, LocalizationManager l)
            {
                if (row < 0 || row >= Lines.Length)
                {
                    return string.Empty;
                }

                string status = _driver.BuyBundle(Lines[row], PortoMoonStallRules.LineKey(_moon, row), _day, null, l, out bool bought);
                if (bought)
                {
                    _driver.SayCousin(CityStallsTexts.MoonLine);
                }

                return status;
            }

            public string CloseLabel(LocalizationManager l) => CityStallsTexts.Get(l, CityStallsTexts.Close);
        }
    }
}
