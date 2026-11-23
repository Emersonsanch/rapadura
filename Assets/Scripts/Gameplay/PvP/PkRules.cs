using System;

namespace Rapadura.Gameplay.PvP
{
    /// <summary>How the server sees a player's PvP conduct, from their stains ("manchas").</summary>
    public enum PkStanding
    {
        /// <summary>0 stains.</summary>
        Clean = 0,

        /// <summary>1 stain: name in orange, no other effect (a warning).</summary>
        Stained = 1,

        /// <summary>2–4 stains: "Fora da lei", red name, small costs (<see cref="PkRules"/>).</summary>
        Outlaw = 2,

        /// <summary>5+ stains: "Assassino", dark red name, a bit more cost, no ranked arena.</summary>
        Murderer = 3,
    }

    /// <summary>The PK counter of one character, kept by the server.</summary>
    public sealed class PkState
    {
        public int Stains { get; internal set; }

        /// <summary>Online play seconds accumulated toward fading the next stain.</summary>
        public double FadeProgressSeconds { get; internal set; }

        public PkState(int stains = 0, double fadeProgressSeconds = 0d)
        {
            Stains = Math.Max(0, Math.Min(PkRules.MaxStains, stains));
            FadeProgressSeconds = Math.Max(0d, fadeProgressSeconds);
        }

        public PkStanding Standing => PkRules.StandingOf(Stains);
    }

    /// <summary>
    /// The MU-style "PK" penalty (TODO.md 12.3 / decision 2026-10-29 "fora da lei"): killing another player outside a duel,
    /// the arena, a guild war or the Terras de Disputa leaves a <b>stain</b> on the killer. Small, clear effects that fade
    /// with play time, and nothing permanent:
    /// <list type="table">
    /// <item><term>Clean (0)</term><description>nothing.</description></item>
    /// <item><term>Stained (1)</term><description>orange name only.</description></item>
    /// <item><term>Outlaw (2–4)</term><description>red name; shops +20%; on death loses ×1.5 the normal XP.</description></item>
    /// <item><term>Murderer (5+)</term><description>dark red name; shops +40%; ×2 the death XP; cannot queue the ranked arena.</description></item>
    /// </list>
    /// Not a stain: self-defence (the victim struck the killer first in the last <see cref="SelfDefenseSeconds"/>) and
    /// killing someone already Outlaw or worse (hunting outlaws is the answer to them). One stain fades per
    /// <see cref="FadeSecondsPerStain"/> of <i>online</i> play (no dodging it by logging off), capped at
    /// <see cref="MaxStains"/>; a bail (<see cref="BailCostPerStain"/> gold, at a city) clears one at once. Items never drop
    /// because of stains, gear is never lost, the account is never locked.
    /// </summary>
    public static class PkRules
    {
        public const int MaxStains = 10;

        /// <summary>One stain fades after 1 hour of online play.</summary>
        public const double FadeSecondsPerStain = 3600d;

        /// <summary>A kill is self-defence when the victim hit the killer within this many seconds before.</summary>
        public const double SelfDefenseSeconds = 60d;

        public const int OutlawAt = 2;
        public const int MurdererAt = 5;

        /// <summary>Gold to clear one stain at a city (the Procurado's "fiança").</summary>
        public const long BailCostPerStain = 20000;

        public static PkStanding StandingOf(int stains)
        {
            if (stains >= MurdererAt)
            {
                return PkStanding.Murderer;
            }

            if (stains >= OutlawAt)
            {
                return PkStanding.Outlaw;
            }

            return stains >= 1 ? PkStanding.Stained : PkStanding.Clean;
        }

        /// <summary>
        /// Whether a player kill stains the killer. Only open PvP in a Frontier region does; never a duel (which cannot kill
        /// anyway), the arena, a guild war or the Terras de Disputa; never self-defence; never killing an Outlaw / Murderer.
        /// </summary>
        public static bool KillStains(PvpHarmBasis basis, PvpAreaKind areaOfKill, PkStanding victimStanding, double secondsSinceVictimHitKiller)
        {
            if (basis != PvpHarmBasis.OpenArea || areaOfKill != PvpAreaKind.Frontier)
            {
                return false;
            }

            if (victimStanding >= PkStanding.Outlaw)
            {
                return false;
            }

            bool selfDefense = secondsSinceVictimHitKiller >= 0d && secondsSinceVictimHitKiller <= SelfDefenseSeconds;
            return !selfDefense;
        }

        /// <summary>Applies one kill to the killer's counter. Returns whether a stain was added. A fresh stain restarts the fade clock.</summary>
        public static bool RegisterKill(PkState killer, PvpHarmBasis basis, PvpAreaKind areaOfKill, PkStanding victimStanding,
            double secondsSinceVictimHitKiller)
        {
            if (killer == null || !KillStains(basis, areaOfKill, victimStanding, secondsSinceVictimHitKiller))
            {
                return false;
            }

            killer.Stains = Math.Min(MaxStains, killer.Stains + 1);
            killer.FadeProgressSeconds = 0d;
            return true;
        }

        /// <summary>Online play time passing (server tick). Returns how many stains faded.</summary>
        public static int AdvancePlayTime(PkState state, double onlineSeconds)
        {
            if (state == null || onlineSeconds <= 0d || state.Stains <= 0)
            {
                if (state != null && state.Stains <= 0)
                {
                    state.FadeProgressSeconds = 0d;
                }

                return 0;
            }

            int faded = 0;
            double progress = state.FadeProgressSeconds + onlineSeconds;
            while (state.Stains > 0 && progress >= FadeSecondsPerStain)
            {
                progress -= FadeSecondsPerStain;
                state.Stains--;
                faded++;
            }

            state.FadeProgressSeconds = state.Stains > 0 ? progress : 0d;
            return faded;
        }

        /// <summary>Online seconds until every stain is gone.</summary>
        public static double SecondsUntilClean(PkState state)
        {
            if (state == null || state.Stains <= 0)
            {
                return 0d;
            }

            return state.Stains * FadeSecondsPerStain - state.FadeProgressSeconds;
        }

        /// <summary>Pays the bail for one stain. Refused outside a city, with no stain or not enough gold; <paramref name="cost"/> is what to take.</summary>
        public static PvpRefusal PayBail(PkState state, bool inCity, long gold, out long cost)
        {
            cost = 0;
            if (state == null || state.Stains <= 0)
            {
                return PvpRefusal.NoStains;
            }

            if (!inCity)
            {
                return PvpRefusal.NotInCity;
            }

            if (gold < BailCostPerStain)
            {
                return PvpRefusal.NotEnoughGold;
            }

            cost = BailCostPerStain;
            state.Stains--;
            if (state.Stains == 0)
            {
                state.FadeProgressSeconds = 0d;
            }

            return PvpRefusal.None;
        }

        /// <summary>Multiplier on shop buy prices.</summary>
        public static float ShopPriceMultiplier(PkStanding standing)
        {
            switch (standing)
            {
                case PkStanding.Outlaw: return 1.2f;
                case PkStanding.Murderer: return 1.4f;
                default: return 1f;
            }
        }

        /// <summary>Multiplier on the normal XP lost at death (the open-world death, never a duel).</summary>
        public static float DeathXpLossMultiplier(PkStanding standing)
        {
            switch (standing)
            {
                case PkStanding.Outlaw: return 1.5f;
                case PkStanding.Murderer: return 2f;
                default: return 1f;
            }
        }

        public static bool CanQueueArena(PkStanding standing) => standing != PkStanding.Murderer;

        /// <summary>Stains never raise the item drop chance at death (no cruel permanent loss).</summary>
        public static float DeathItemDropMultiplier(PkStanding standing) => 1f;
    }
}
