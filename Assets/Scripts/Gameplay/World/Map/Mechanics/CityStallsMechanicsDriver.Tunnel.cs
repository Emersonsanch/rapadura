using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Mercado dos Túneis (22, <see cref="TunnelMarketStallRules"/>): Shayn's Banca de Troca at her door on the East street, the fair of the
    /// in-game day 70 m down one of the four galleries (an amber beam marks it; entering the market or a new day says which gallery), and, once
    /// O Engarrafador Guloso was beaten, his free bottles at that fair.
    /// </summary>
    public sealed partial class CityStallsMechanicsDriver
    {
        private static readonly Color FairBeamColor = new Color(0.88f, 0.63f, 0.25f, 0.9f);

        private bool _tunnelOn;
        private Vector2 _market;
        private Vector2 _barterPoint;
        private LineRenderer _fairBeam;
        private int _fairSaidDay = int.MinValue;
        private bool _inMarket;

        /// <summary>The barter screen's choice of what to give (an item id; null: none yet).</summary>
        private string _barterGive;

        private void StartTunnel(OverworldLayout layout, List<Rect> areas)
        {
            OverworldZone anchor = layout?.Zone(OverworldLayouts.LimiarAnchorZoneId);
            if (anchor == null)
            {
                return;
            }

            _tunnelOn = true;
            _market = anchor.Origin + OverworldLayouts.MercadoTuneisPost;
            _barterPoint = _market + DoorOf(CityTownRecipes.TunnelMarketCityId, "shayn");
            areas.Add(Around(_market, AreaHalf));
        }

        private TunnelGallery TodayGallery() => TunnelMarketStallRules.GalleryOf(GameDay());

        private Vector2 TodayFairSpot() => TunnelMarketStallRules.FairSpot(_market, TodayGallery());

        private static string GalleryName(TunnelGallery gallery, LocalizationManager l) => CityStallsTexts.Get(l, CityStallsTexts.GalleryNames[(int)gallery]);

        private void TickTunnel(Vector2 hero)
        {
            if (!_tunnelOn)
            {
                return;
            }

            if (_fairBeam == null)
            {
                _fairBeam = NewBeam("TunnelFairBeam", 0.35f);
            }

            ShowBeam(_fairBeam, TodayFairSpot(), 7f, FairBeamColor);

            // Entering the crossing (or a new in-game day while in it) says where the fair went.
            bool inMarket = (hero - _market).sqrMagnitude <= 22f * 22f;
            int day = GameDay();
            if (inMarket && (!_inMarket || _fairSaidDay != day))
            {
                _fairSaidDay = day;
                SayAbove(CityStallsTexts.Format(Localization(), CityStallsTexts.FairToday, GalleryName(TodayGallery(), Localization())));
            }

            _inMarket = inMarket;
        }

        // ------------------------------------------------------------------
        // Banca de Troca da Shayn
        // ------------------------------------------------------------------

        private int BarterTradesLeft()
        {
            WorldMapProgress progress = Progress();
            long day = progress != null ? progress.GetDailyMark(TunnelMarketStallRules.BarterDayKey) : 0;
            long count = progress != null ? progress.GetTimeMark(TunnelMarketStallRules.BarterCountKey) : 0;
            return TunnelMarketStallRules.TradesLeft(day, count, Today());
        }

        private string Barter(string take, LocalizationManager l)
        {
            if (string.IsNullOrEmpty(_barterGive))
            {
                return CityStallsTexts.Get(l, CityStallsTexts.BarterChoose);
            }

            int owned = _bag != null ? _bag.GetTotalCount(_barterGive) : 0;
            TunnelStallCheck check = TunnelMarketStallRules.CheckBarter(_barterGive, take, owned, BarterTradesLeft());
            if (check == TunnelStallCheck.UsedUp)
            {
                return CityStallsTexts.Get(l, CityStallsTexts.BarterUsedUp);
            }

            if (check == TunnelStallCheck.NotEnough)
            {
                return CityStallsTexts.Format(l, CityStallsTexts.BarterNotEnough, ItemName(_barterGive, l));
            }

            if (check != TunnelStallCheck.Ready || _bag == null)
            {
                return CityStallsTexts.Get(l, CityStallsTexts.BarterChoose);
            }

            // Take first, then give: if the 2 do not fit, the 3 stay.
            if (!_bag.RemoveById(_barterGive, TunnelMarketStallRules.BarterGive))
            {
                return CityStallsTexts.Format(l, CityStallsTexts.BarterNotEnough, ItemName(_barterGive, l));
            }

            if (!GiveAll(new List<(string, int)> { (take, TunnelMarketStallRules.BarterTake) }))
            {
                GiveAll(new List<(string, int)> { (_barterGive, TunnelMarketStallRules.BarterGive) });
                return CityStallsTexts.Get(l, CityStallsTexts.BagFull);
            }

            WorldMapProgress progress = Progress();
            long today = Today();
            int used = TunnelMarketStallRules.BarterTradesPerDay - BarterTradesLeft();
            progress?.SetDailyMark(TunnelMarketStallRules.BarterDayKey, (int)today);
            progress?.SetTimeMark(TunnelMarketStallRules.BarterCountKey, used + 1);
            string given = _barterGive;
            if (_bag.GetTotalCount(_barterGive) < TunnelMarketStallRules.BarterGive)
            {
                _barterGive = null;
            }

            if (used == 0)
            {
                SayCousin(CityStallsTexts.BarterLine);
            }

            return CityStallsTexts.Format(l, CityStallsTexts.BarterDone, ItemName(given, l), ItemName(take, l));
        }

        private sealed class BarterScreen : ICityServiceScreen
        {
            private readonly CityStallsMechanicsDriver _driver;

            /// <summary>What each row does: (true, id) gives, (false, id) takes.</summary>
            private readonly List<(bool Give, string Id)> _rows = new List<(bool, string)>();

            public BarterScreen(CityStallsMechanicsDriver driver)
            {
                _driver = driver;
                _driver._barterGive = null;
            }

            public string Title(LocalizationManager l) => CityStallsTexts.Get(l, CityStallsTexts.BarterTitle);

            public string Intro(LocalizationManager l) => CityStallsTexts.Get(l, CityStallsTexts.BarterIntro);

            public string Summary(LocalizationManager l) =>
                CityStallsTexts.Format(l, CityStallsTexts.BarterSummary, _driver.BarterTradesLeft(), TunnelMarketStallRules.BarterTradesPerDay);

            public IReadOnlyList<CityServiceRow> Rows(LocalizationManager l)
            {
                _rows.Clear();
                var rows = new List<CityServiceRow>();
                string give = _driver._barterGive;
                foreach (string id in TunnelMarketStallRules.BarterMaterials())
                {
                    int owned = _driver._bag != null ? _driver._bag.GetTotalCount(id) : 0;
                    if (owned < TunnelMarketStallRules.BarterGive)
                    {
                        continue;
                    }

                    bool picked = id == give;
                    string tier = CityStallsTexts.Get(l, CityStallsTexts.TierNames[TunnelMarketStallRules.TierOf(id)]);
                    rows.Add(new CityServiceRow(CityStallsTexts.Format(l, CityStallsTexts.BarterGiveRow, ItemName(id, l)),
                        CityStallsTexts.Format(l, CityStallsTexts.BarterGiveDetail, owned, tier),
                        CityStallsTexts.Get(l, picked ? CityStallsTexts.BarterPicked : CityStallsTexts.BarterPick), !picked, picked));
                    _rows.Add((true, id));
                }

                if (rows.Count == 0)
                {
                    rows.Add(new CityServiceRow(CityStallsTexts.Get(l, CityStallsTexts.BarterNothing), null, null, false));
                    _rows.Add((true, null));
                    return rows;
                }

                if (give == null)
                {
                    return rows;
                }

                int left = _driver.BarterTradesLeft();
                foreach (string[] tier in TunnelMarketStallRules.BarterTiers)
                {
                    if (Array.IndexOf(tier, give) < 0)
                    {
                        continue;
                    }

                    foreach (string take in tier)
                    {
                        if (take == give)
                        {
                            continue;
                        }

                        rows.Add(new CityServiceRow(CityStallsTexts.Format(l, CityStallsTexts.BarterTakeRow, ItemName(take, l)),
                            CityStallsTexts.Format(l, CityStallsTexts.BarterTakeDetail, ItemName(give, l)),
                            CityStallsTexts.Get(l, CityStallsTexts.BarterTrade), left > 0));
                        _rows.Add((false, take));
                    }
                }

                return rows;
            }

            public string Pick(int row, LocalizationManager l)
            {
                if (row < 0 || row >= _rows.Count || _rows[row].Id == null)
                {
                    return string.Empty;
                }

                if (_rows[row].Give)
                {
                    _driver._barterGive = _rows[row].Id;
                    return string.Empty;
                }

                return _driver.Barter(_rows[row].Id, l);
            }

            public string CloseLabel(LocalizationManager l) => CityStallsTexts.Get(l, CityStallsTexts.Close);
        }

        // ------------------------------------------------------------------
        // Feira da Galeria (+ O Engarrafador's gift)
        // ------------------------------------------------------------------

        private static bool BottlerBeaten() => ServiceLocator.TryGet(out BestiaryProgress bestiary) && bestiary != null
            && bestiary.GetKills(TunnelMarketStallRules.BottlerBossId) > 0;

        private TunnelStallCheck BottlerCheck()
        {
            WorldMapProgress progress = Progress();
            long given = progress != null ? progress.GetDailyMark(TunnelMarketStallRules.BottlerGiftDayKey) : 0;
            return TunnelMarketStallRules.CheckBottlerGift(BottlerBeaten(), given, Today());
        }

        private string TakeBottlerGift(LocalizationManager l)
        {
            if (BottlerCheck() != TunnelStallCheck.Ready)
            {
                return CityStallsTexts.Get(l, CityStallsTexts.BottlerTaken);
            }

            if (!GiveAll(new List<(string, int)> { (TunnelMarketStallRules.BottleItemId, TunnelMarketStallRules.BottlerGiftBottles) }))
            {
                return CityStallsTexts.Get(l, CityStallsTexts.BagFull);
            }

            Progress()?.SetDailyMark(TunnelMarketStallRules.BottlerGiftDayKey, (int)Today());
            SayCousin(CityStallsTexts.BottlerLine);
            return CityStallsTexts.Get(l, CityStallsTexts.BottlerGift);
        }

        private sealed class FairScreen : ICityServiceScreen
        {
            private readonly CityStallsMechanicsDriver _driver;
            private readonly TunnelGallery _gallery;
            private readonly int _day;

            public FairScreen(CityStallsMechanicsDriver driver)
            {
                _driver = driver;
                _day = GameDay();
                _gallery = TunnelMarketStallRules.GalleryOf(_day);
            }

            private TunnelFairLine[] Lines => TunnelMarketStallRules.FairLines[(int)_gallery];

            public string Title(LocalizationManager l) => CityStallsTexts.Format(l, CityStallsTexts.FairTitle, GalleryName(_gallery, l));

            public string Intro(LocalizationManager l) => CityStallsTexts.Get(l, CityStallsTexts.FairIntro);

            public string Summary(LocalizationManager l) => CityStallsTexts.Format(l, CityStallsTexts.FairSummary, _driver.Gold);

            public IReadOnlyList<CityServiceRow> Rows(LocalizationManager l)
            {
                List<CityServiceRow> rows = _driver.BundleRows(Lines, i => TunnelMarketStallRules.FairLineKey(_gallery, i), _day, l);
                TunnelStallCheck bottler = _driver.BottlerCheck();
                if (bottler == TunnelStallCheck.NoHelper)
                {
                    rows.Add(new CityServiceRow(CityStallsTexts.Get(l, CityStallsTexts.BottlerAbsentRow), CityStallsTexts.Get(l, CityStallsTexts.BottlerAbsentDetail), null, false));
                }
                else
                {
                    bool ready = bottler == TunnelStallCheck.Ready;
                    rows.Add(new CityServiceRow(CityStallsTexts.Get(l, CityStallsTexts.BottlerRow), CityStallsTexts.Get(l, CityStallsTexts.BottlerDetail),
                        CityStallsTexts.Get(l, ready ? CityStallsTexts.BottlerTake : CityStallsTexts.BottlerTaken), ready, ready));
                }

                return rows;
            }

            public string Pick(int row, LocalizationManager l)
            {
                if (row >= 0 && row < Lines.Length)
                {
                    return _driver.BuyBundle(Lines[row], TunnelMarketStallRules.FairLineKey(_gallery, row), _day, CityStallsTexts.FairBought, l, out _);
                }

                return row == Lines.Length ? _driver.TakeBottlerGift(l) : string.Empty;
            }

            public string CloseLabel(LocalizationManager l) => CityStallsTexts.Get(l, CityStallsTexts.Close);
        }
    }
}
