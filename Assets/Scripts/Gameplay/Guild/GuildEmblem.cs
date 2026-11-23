using System;

namespace Rapadura.Gameplay.Guild
{
    /// <summary>Outline of the emblem. Never renumber (saved and sent as numbers).</summary>
    public enum GuildEmblemShape
    {
        Shield = 0,
        Circle = 1,
        Diamond = 2,
        Banner = 3,
        Star = 4,
        Lamp = 5,
    }

    /// <summary>The fixed palette of the emblem (a small palette keeps every emblem readable). Never renumber.</summary>
    public enum GuildEmblemColor
    {
        White = 0,
        Gold = 1,
        Amber = 2,
        Crimson = 3,
        Violet = 4,
        Indigo = 5,
        Azure = 6,
        Teal = 7,
        Green = 8,
        Earth = 9,
        Ash = 10,
        Black = 11,
    }

    /// <summary>The symbol in the middle, from the Manichaean imagery of the world. Never renumber.</summary>
    public enum GuildEmblemSymbol
    {
        None = 0,

        /// <summary>The Sun, ship of the Living Spirit.</summary>
        Sun = 1,

        /// <summary>The Moon, ship of the Light that is gathered.</summary>
        Moon = 2,

        /// <summary>A Barca de Luz.</summary>
        LightShip = 3,

        /// <summary>The Coluna da Glória (Column of Glory).</summary>
        Column = 4,

        /// <summary>The Pearl (the soul).</summary>
        Pearl = 5,

        /// <summary>The Tree of Life.</summary>
        Tree = 6,

        /// <summary>The Wheel that lifts the Light.</summary>
        Wheel = 7,

        Flame = 8,
        Lotus = 9,
        Eye = 10,
        Wing = 11,
        Crown = 12,
    }

    /// <summary>
    /// The guild emblem ("brasão") as data, no art: shape, field and border colors, symbol and its color. The client draws
    /// it from these five numbers; the server only validates them (<see cref="Validate"/>) and sends <see cref="ToCode"/>.
    /// </summary>
    public readonly struct GuildEmblem : IEquatable<GuildEmblem>
    {
        public readonly GuildEmblemShape Shape;
        public readonly GuildEmblemColor Field;
        public readonly GuildEmblemColor Border;
        public readonly GuildEmblemSymbol Symbol;
        public readonly GuildEmblemColor SymbolColor;

        public GuildEmblem(GuildEmblemShape shape, GuildEmblemColor field, GuildEmblemColor border, GuildEmblemSymbol symbol,
            GuildEmblemColor symbolColor)
        {
            Shape = shape;
            Field = field;
            Border = border;
            Symbol = symbol;
            SymbolColor = symbolColor;
        }

        /// <summary>The emblem of a new guild that picked nothing: gold Sun on a white shield with a gold border.</summary>
        public static GuildEmblem Default =>
            new GuildEmblem(GuildEmblemShape.Shield, GuildEmblemColor.White, GuildEmblemColor.Gold, GuildEmblemSymbol.Sun, GuildEmblemColor.Gold);

        /// <summary>
        /// Every part is a defined value, the border differs from the field and a symbol's color differs from the field
        /// (otherwise it is invisible). <see cref="GuildEmblemSymbol.None"/> ignores the symbol color.
        /// </summary>
        public bool Validate()
        {
            if (!Enum.IsDefined(typeof(GuildEmblemShape), Shape) || !Enum.IsDefined(typeof(GuildEmblemColor), Field) ||
                !Enum.IsDefined(typeof(GuildEmblemColor), Border) || !Enum.IsDefined(typeof(GuildEmblemSymbol), Symbol) ||
                !Enum.IsDefined(typeof(GuildEmblemColor), SymbolColor))
            {
                return false;
            }

            if (Border == Field)
            {
                return false;
            }

            return Symbol == GuildEmblemSymbol.None || SymbolColor != Field;
        }

        /// <summary>"shape-field-border-symbol-symbolColor", e.g. "0-0-1-1-1": compact for the network and the save.</summary>
        public string ToCode() => $"{(int)Shape}-{(int)Field}-{(int)Border}-{(int)Symbol}-{(int)SymbolColor}";

        /// <summary>Reads <see cref="ToCode"/>; false (and <see cref="Default"/>) on anything malformed or invalid.</summary>
        public static bool TryParse(string code, out GuildEmblem emblem)
        {
            emblem = Default;
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            string[] parts = code.Split('-');
            if (parts.Length != 5)
            {
                return false;
            }

            var values = new int[5];
            for (int i = 0; i < 5; i++)
            {
                if (!int.TryParse(parts[i], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out values[i]))
                {
                    return false;
                }
            }

            var parsed = new GuildEmblem((GuildEmblemShape)values[0], (GuildEmblemColor)values[1], (GuildEmblemColor)values[2],
                (GuildEmblemSymbol)values[3], (GuildEmblemColor)values[4]);
            if (!parsed.Validate())
            {
                return false;
            }

            emblem = parsed;
            return true;
        }

        public bool Equals(GuildEmblem other) => Shape == other.Shape && Field == other.Field && Border == other.Border &&
                                                 Symbol == other.Symbol && SymbolColor == other.SymbolColor;

        public override bool Equals(object obj) => obj is GuildEmblem other && Equals(other);

        public override int GetHashCode() => ((((int)Shape * 16 + (int)Field) * 16 + (int)Border) * 16 + (int)Symbol) * 16 + (int)SymbolColor;
    }

    /// <summary>
    /// The permission table of the ranks (TODO.md 12.3, Guildas). One row per rank:
    /// <code>
    ///              Invite Kick Promote Demote Deposit Withdraw Board MOTD War Identity Disband
    /// Leader         x     x     x       x      x       x       x     x    x     x        x
    /// Vice           x     x     x       x      x       x       x     x    x
    /// Officer        x     x                    x       x       x
    /// Member                                    x       x
    /// Recruit                                   x
    /// </code>
    /// Kick / promote / demote additionally only reach ranks strictly below the actor (<see cref="GuildRules"/>).
    /// Withdrawals have a daily limit per rank (<see cref="GuildBankRules.DailyGoldLimit"/>).
    /// </summary>
    public static class GuildPermissions
    {
        private const GuildPermission RecruitSet = GuildPermission.BankDeposit;
        private const GuildPermission MemberSet = RecruitSet | GuildPermission.BankWithdraw;
        private const GuildPermission OfficerSet = MemberSet | GuildPermission.Invite | GuildPermission.Kick | GuildPermission.EditBoard;
        private const GuildPermission ViceSet = OfficerSet | GuildPermission.Promote | GuildPermission.Demote | GuildPermission.EditMotd |
                                                GuildPermission.DeclareWar;
        private const GuildPermission LeaderSet = ViceSet | GuildPermission.EditIdentity | GuildPermission.Disband;

        public static GuildPermission Of(GuildRank rank)
        {
            switch (rank)
            {
                case GuildRank.Leader: return LeaderSet;
                case GuildRank.Vice: return ViceSet;
                case GuildRank.Officer: return OfficerSet;
                case GuildRank.Member: return MemberSet;
                case GuildRank.Recruit: return RecruitSet;
                default: return GuildPermission.None;
            }
        }

        public static bool Has(GuildRank rank, GuildPermission permission) => permission != GuildPermission.None && (Of(rank) & permission) == permission;
    }
}
