using Rapadura.Core.Localization;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Seasons;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.UI.Seasons
{
    /// <summary>One prize row of the Barraca de Prendas.</summary>
    public readonly struct PrizeRow
    {
        public readonly string ItemId;
        public readonly string Name;
        public readonly string Info;
        public readonly string ButtonText;
        public readonly PrendaBuyCheck Check;
        public readonly bool Owned;

        public PrizeRow(string itemId, string name, string info, string buttonText, PrendaBuyCheck check, bool owned)
        {
            ItemId = itemId;
            Name = name;
            Info = info;
            ButtonText = buttonText;
            Check = check;
            Owned = owned;
        }

        public bool CanBuy => Check == PrendaBuyCheck.Ok;
    }

    /// <summary>Texts and small rules of the "Arraiá" window (<see cref="FestaJuninaPanel"/>), Unity-free and tested. Null manager = pt-BR.</summary>
    public static class FestaJuninaPresenter
    {
        /// <summary>Seconds of music the track shows ahead of the hit zone.</summary>
        public const float Lookahead = 3f;
        /// <summary>Where the hit zone sits on the track (percent from the left).</summary>
        public const float HitZonePercent = 12f;
        public const float TrackEndPercent = 96f;

        /// <summary>The "Arraiá" button: festa on, idle token inside the Vila da Vó Edna, nothing modal open.</summary>
        public static bool ShouldShowButton(bool festaActive, bool hudVisible, bool tokenIdle, bool paused, bool dialogueActive,
            BiomeType? region, bool inSafeZone)
        {
            return festaActive && hudVisible && tokenIdle && !paused && !dialogueActive && inSafeZone
                && region.HasValue && region.Value == FastTravelRules.HomeRegion;
        }

        /// <summary>Left position (percent) of a note due at <paramref name="stepTime"/>; null when off the track.</summary>
        public static float? NotePercent(float stepTime, float now)
        {
            float ahead = stepTime - now;
            if (ahead < -0.3f || ahead > Lookahead)
            {
                return null;
            }

            return HitZonePercent + ahead / Lookahead * (TrackEndPercent - HitZonePercent);
        }

        public static string Arrow(QuadrilhaMove move)
        {
            switch (move)
            {
                case QuadrilhaMove.Anarrie: return "←";
                case QuadrilhaMove.Balance: return "↑";
                case QuadrilhaMove.Cobra: return "↓";
                default: return "→";
            }
        }

        public static string NoteClass(QuadrilhaMove move)
        {
            switch (move)
            {
                case QuadrilhaMove.Anarrie: return "festa-note--anarrie";
                case QuadrilhaMove.Balance: return "festa-note--balance";
                case QuadrilhaMove.Cobra: return "festa-note--cobra";
                default: return "festa-note--caminho";
            }
        }

        public static string TasksText(LocalizationManager manager, FestaJuninaProgress progress)
        {
            if (progress == null)
            {
                return string.Empty;
            }

            string combo = FestaJuninaTexts.Get(manager, progress.FullComboDanced ? FestaJuninaTexts.Yes : FestaJuninaTexts.No);
            return FestaJuninaTexts.Get(manager, FestaJuninaTexts.TasksTitle) + ": "
                + FestaJuninaTexts.Format(manager, FestaJuninaTexts.TasksProgress, System.Math.Min(progress.Dances, FestaJuninaRules.TaskDances),
                    FestaJuninaRules.TaskDances, combo, System.Math.Min(progress.DistinctPrizes, FestaJuninaRules.TaskDistinctPrizes), FestaJuninaRules.TaskDistinctPrizes);
        }

        public static PrizeRow BuildPrizeRow(HouseItemDefinition item, int prendas, bool festaActive, bool owned, LocalizationManager manager)
        {
            bool ptBR = manager == null || manager.CurrentLanguage != LanguageCode.en;
            PrendaBuyCheck check = FestaJuninaRules.CheckBuy(item, prendas, festaActive);
            string info = HouseItemTexts.RarityName(item.Rarity, ptBR);
            if (owned)
            {
                info += " · " + FestaJuninaTexts.Get(manager, FestaJuninaTexts.StallOwned);
            }

            if (check == PrendaBuyCheck.NotEnoughPrendas)
            {
                info += " · " + FestaJuninaTexts.Format(manager, FestaJuninaTexts.StallNotEnough, item.EventPrice);
            }
            else if (check == PrendaBuyCheck.FestaClosed)
            {
                info += " · " + FestaJuninaTexts.Get(manager, FestaJuninaTexts.StallClosed);
            }

            return new PrizeRow(item.ItemId, HouseItemTexts.Name(manager, item), info,
                FestaJuninaTexts.Format(manager, FestaJuninaTexts.StallBuy, item.EventPrice), check, owned);
        }

        public static string JudgeClass(QuadrilhaJudgement judgement)
        {
            switch (judgement)
            {
                case QuadrilhaJudgement.Perfect: return "festa-judge--perfect";
                case QuadrilhaJudgement.Good: return "festa-judge--good";
                case QuadrilhaJudgement.Miss: return "festa-judge--miss";
                default: return null;
            }
        }
    }
}
