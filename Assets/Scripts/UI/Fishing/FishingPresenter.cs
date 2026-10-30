using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Fishing;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.WorldMap;

namespace Rapadura.UI.Fishing
{
    /// <summary>Where a landed fish ended up.</summary>
    public enum FishBagStatus
    {
        Added,
        /// <summary>Too heavy: released (the record still counts).</summary>
        BagFull,
    }

    /// <summary>One line of the "Caderno de pesca" page.</summary>
    public readonly struct FishCollectionRow
    {
        public readonly string FishId;
        public readonly bool Caught;
        public readonly FishRarity Rarity;
        public readonly string Title;
        public readonly string Where;

        public FishCollectionRow(string fishId, bool caught, FishRarity rarity, string title, string where)
        {
            FishId = fishId;
            Caught = caught;
            Rarity = rarity;
            Title = title;
            Where = where;
        }
    }

    /// <summary>
    /// Texts and small rules of the fishing panel (<see cref="FishingPanelController"/>), Unity-free like
    /// <see cref="KitchenPresenter"/>: tested without a UIDocument. A null manager means pt-BR fallbacks.
    /// </summary>
    public static class FishingPresenter
    {
        /// <summary>Whether the "Pescar" button is on screen: idle token on a region with water, nothing modal open.</summary>
        public static bool ShouldShowButton(bool hudVisible, bool tokenIdle, bool paused, bool dialogueActive, BiomeType? region)
        {
            return hudVisible && tokenIdle && !paused && !dialogueActive && FishCatalog.SpotFor(region) != null;
        }

        /// <summary>Wider timing: the panel's "Pesca tranquila" toggle, or the accessibility "Reduzir Movimento".</summary>
        public static bool IsAssist(bool relaxedPreference, bool reduceMotion) => relaxedPreference || reduceMotion;

        public static string FishName(LocalizationManager manager, string fishId) =>
            FishingTexts.Get(manager, ContentText.ItemNameKey(fishId));

        public static string RarityName(LocalizationManager manager, FishRarity rarity) =>
            FishingTexts.Get(manager, FishingTexts.RarityKey(rarity));

        public static string SpotName(LocalizationManager manager, FishingSpotSpec spot) =>
            spot == null ? string.Empty : FishingTexts.Get(manager, spot.NameKey);

        /// <summary>"850 g" below a kilo, else "1,25 kg" (pt-BR) / "1.25 kg" (en).</summary>
        public static string FormatWeight(LocalizationManager manager, int grams)
        {
            if (grams < 1000)
            {
                return FishingTexts.Format(manager, FishingTexts.WeightG, grams.ToString(CultureInfo.InvariantCulture));
            }

            string number = (grams / 1000f).ToString("0.00", CultureInfo.InvariantCulture);
            string separator = FishingTexts.Get(manager, FishingTexts.DecimalSeparator);
            if (!string.IsNullOrEmpty(separator) && separator != ".")
            {
                number = number.Replace(".", separator);
            }

            return FishingTexts.Format(manager, FishingTexts.WeightKg, number);
        }

        /// <summary>"Agora: Entardecer · Chuva" (the cave lake: no weather).</summary>
        public static string ConditionsLine(LocalizationManager manager, FishingSpotSpec spot, TimeOfDayPhase phase, WeatherType weather)
        {
            string phaseText = WorldMapTexts.Get(manager, WorldMapHudPresenter.PhaseKey(phase));
            if (spot != null && spot.Sheltered)
            {
                return FishingTexts.Format(manager, FishingTexts.ConditionsSheltered, phaseText);
            }

            return FishingTexts.Format(manager, FishingTexts.Conditions, phaseText, WorldMapHudPresenter.WeatherText(manager, weather));
        }

        /// <summary>The cousin's line for a catch of <paramref name="rarity"/> ("any" line without a cousin).</summary>
        public static string ReactionLine(LocalizationManager manager, CharacterId? hero, FishRarity rarity)
        {
            string key = FishingTexts.ReactionKey(hero, rarity);
            return FishingTexts.GetFallback(key) != null || (manager != null && manager.HasKey(key))
                ? FishingTexts.Get(manager, key)
                : FishingTexts.Get(manager, FishingTexts.ReactionKey(null, rarity));
        }

        public static string EscapeLine(LocalizationManager manager, CharacterId? hero) =>
            FishingTexts.Get(manager, FishingTexts.EscapeKey(hero));

        /// <summary>The instruction under the water for the current moment of the cast.</summary>
        public static string Hint(LocalizationManager manager, FishingSession session)
        {
            if (session == null)
            {
                return string.Empty;
            }

            switch (session.Phase)
            {
                case FishingPhase.Casting:
                    return FishingTexts.Get(manager, FishingTexts.HintCasting);
                case FishingPhase.Waiting:
                    if (session.FloatSunk)
                    {
                        return FishingTexts.Get(manager, FishingTexts.HintBite);
                    }

                    if (session.PhaseTime >= session.Plan.TrembleAt)
                    {
                        return FishingTexts.Get(manager, FishingTexts.HintTremble);
                    }

                    return FishingTexts.Get(manager, session.FloatTwitching ? FishingTexts.HintNibble : FishingTexts.HintWaiting);
                case FishingPhase.Reeling:
                    return FishingTexts.Get(manager, session.Reel.Surging ? FishingTexts.HintSurge : FishingTexts.HintReel);
                case FishingPhase.Result:
                    return string.Empty;
                default:
                    return FishingTexts.Get(manager, FishingTexts.HintReady);
            }
        }

        /// <summary>The label of the single action pad.</summary>
        public static string ActionLabel(LocalizationManager manager, FishingSession session)
        {
            FishingPhase phase = session != null ? session.Phase : FishingPhase.Ready;
            switch (phase)
            {
                case FishingPhase.Casting:
                    return FishingTexts.Get(manager, FishingTexts.ActionCasting);
                case FishingPhase.Waiting:
                    return FishingTexts.Get(manager, FishingTexts.ActionHook);
                case FishingPhase.Reeling:
                    return FishingTexts.Get(manager, FishingTexts.ActionReel);
                case FishingPhase.Result:
                    return FishingTexts.Get(manager, FishingTexts.ActionAgain);
                default:
                    return FishingTexts.Get(manager, FishingTexts.ActionCast);
            }
        }

        /// <summary>
        /// The result card of a finished cast: what happened, then (landed) species/rarity/weight, new species,
        /// record and where the fish went, then the cousin's line.
        /// </summary>
        public static string ResultText(LocalizationManager manager, FishingResult result, FishSpec fish, FishCatchOutcome outcome, FishBagStatus bag, CharacterId? hero)
        {
            var lines = new List<string>();
            switch (result)
            {
                case FishingResult.Landed when fish != null:
                    lines.Add(FishingTexts.Format(manager, FishingTexts.ResultLanded, FishName(manager, fish.Id), RarityName(manager, fish.Rarity), FormatWeight(manager, outcome.Grams)));
                    if (outcome.NewSpecies)
                    {
                        lines.Add(FishingTexts.Get(manager, FishingTexts.NewSpecies));
                    }
                    else if (outcome.NewRecord)
                    {
                        lines.Add(FishingTexts.Format(manager, FishingTexts.NewRecord, FormatWeight(manager, outcome.PreviousBestGrams)));
                    }
                    else
                    {
                        lines.Add(FishingTexts.Format(manager, FishingTexts.RecordToBeat, FormatWeight(manager, outcome.PreviousBestGrams)));
                    }

                    lines.Add(FishingTexts.Get(manager, bag == FishBagStatus.Added ? FishingTexts.AddedToBag : FishingTexts.BagFull));
                    lines.Add("\"" + ReactionLine(manager, hero, fish.Rarity) + "\"");
                    break;

                case FishingResult.TooEarly:
                    lines.Add(FishingTexts.Get(manager, FishingTexts.ResultTooEarly));
                    break;

                case FishingResult.TooLate:
                    lines.Add(FishingTexts.Get(manager, FishingTexts.ResultTooLate));
                    break;

                case FishingResult.Snapped:
                    lines.Add(FishingTexts.Get(manager, FishingTexts.ResultSnapped));
                    lines.Add("\"" + EscapeLine(manager, hero) + "\"");
                    break;

                case FishingResult.Escaped:
                    lines.Add(FishingTexts.Get(manager, FishingTexts.ResultEscaped));
                    lines.Add("\"" + EscapeLine(manager, hero) + "\"");
                    break;
            }

            return string.Join("\n", lines);
        }

        public static string CollectionButton(LocalizationManager manager, FishingProgress progress) =>
            FishingTexts.Format(manager, FishingTexts.CollectionButton, progress != null ? progress.CaughtSpeciesCount : 0, FishCatalog.Fish.Count);

        /// <summary>One row per species (catalog order): caught ones with their record, the rest as "???" with a hint of where.</summary>
        public static List<FishCollectionRow> CollectionRows(LocalizationManager manager, FishingProgress progress)
        {
            var rows = new List<FishCollectionRow>();
            foreach (FishSpec fish in FishCatalog.Fish)
            {
                bool caught = progress != null && progress.HasCaught(fish.Id);
                string title = caught
                    ? FishingTexts.Format(manager, FishingTexts.CollectionRow, FishName(manager, fish.Id), RarityName(manager, fish.Rarity),
                        FormatWeight(manager, progress.GetBestGrams(fish.Id)), progress.GetCaughtCount(fish.Id))
                    : FishingTexts.Format(manager, FishingTexts.CollectionUnknown, RarityName(manager, fish.Rarity));

                var spots = new List<string>();
                foreach (FishingSpotId spotId in fish.Spots)
                {
                    spots.Add(SpotName(manager, FishCatalog.GetSpot(spotId)));
                }

                rows.Add(new FishCollectionRow(fish.Id, caught, fish.Rarity, title, FishingTexts.Format(manager, FishingTexts.CollectionWhere, string.Join(", ", spots))));
            }

            return rows;
        }

        /// <summary>USS modifier for a rarity (<c>fishing-rarity--rare</c>).</summary>
        public static string RarityClass(FishRarity rarity) => "fishing-rarity--" + rarity.ToString().ToLowerInvariant();

        /// <summary>Hero name for the panel's subtitle (the cousin holding the rod).</summary>
        public static string HeroName(LocalizationManager manager, CharacterId? hero) => WorldMapHudPresenter.HeroName(manager, hero);
    }
}
