using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>Unity-free axis-aligned rectangle of grid cells (same semantics as <c>RectInt</c>: min inclusive, max exclusive).</summary>
    [Serializable]
    public struct CellRect : IEquatable<CellRect>
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public CellRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int XMax => X + Width;
        public int YMax => Y + Height;

        /// <summary>Touching edges do not overlap (same rule as <c>GridPlacementUtility.Overlaps</c>).</summary>
        public bool Overlaps(CellRect other) => X < other.XMax && XMax > other.X && Y < other.YMax && YMax > other.Y;

        public bool Contains(int x, int y) => x >= X && x < XMax && y >= Y && y < YMax;

        public bool Inside(int width, int height) => X >= 0 && Y >= 0 && XMax <= width && YMax <= height;

        public bool Equals(CellRect other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        public override bool Equals(object obj) => obj is CellRect other && Equals(other);
        public override int GetHashCode() => ((X * 397 ^ Y) * 397 ^ Width) * 397 ^ Height;
        public override string ToString() => $"({X},{Y} {Width}x{Height})";
    }

    /// <summary>A door or window cut in one wall: columns <see cref="Start"/>.. of the wall grid, rows <see cref="RowStart"/>..</summary>
    public sealed class HouseOpening
    {
        public readonly HouseWallSide Side;
        public readonly int Start;
        public readonly int Width;
        public readonly int RowStart;
        public readonly int Rows;
        public readonly bool IsDoor;

        public HouseOpening(HouseWallSide side, int start, int width, int rowStart, int rows, bool isDoor)
        {
            Side = side;
            Start = start;
            Width = width;
            RowStart = rowStart;
            Rows = rows;
            IsDoor = isDoor;
        }

        public CellRect WallRect => new CellRect(Start, RowStart, Width, Rows);
    }

    /// <summary>
    /// One house level (Housing.md §2.2): grids, item limits, chest slots and the upgrade price. All numbers are
    /// data so they can be tuned (the user decided upgrade prices later: the gold/material costs here are provisional).
    /// Interior cells are 0.5 m, yard cells 1 m; walls are a grid of 0.5 m columns × <see cref="WallRows"/> rows.
    /// </summary>
    public sealed class HouseLevelSpec
    {
        public const float InteriorCellSize = 0.5f;
        public const float YardCellSize = 1f;
        public const int WallRows = 5;

        public int Level;
        public string NameKey;
        public int InteriorWidth;
        public int InteriorDepth;
        public int YardWidth;
        public int YardDepth;
        public int InteriorItemLimit;
        public int YardItemLimit;
        public int ChestSlots;
        public int MaxExtraChests;
        public int MaxVisitors;
        /// <summary>Provisional (user decision pending): gold to reach this level from the previous one.</summary>
        public int UpgradeGold;
        public int RequiredCharacterLevel;
        public string RequiredBossId;
        /// <summary>Provisional (user decision pending): materials spent to reach this level (Housing.md §2.2, H4).</summary>
        public HouseMaterialCost[] UpgradeMaterials = Array.Empty<HouseMaterialCost>();
        /// <summary>Items that must be in the house (placed or in the depot) to reach this level; never spent (N5: one keepsake per boss).</summary>
        public string[] RequiredKeepsakes = Array.Empty<string>();
        public List<HouseOpening> Openings = new List<HouseOpening>();
        /// <summary>Yard cell right outside the porch steps: always kept free (the way in).</summary>
        public int YardEntranceX;
        public int YardEntranceY;
        /// <summary>The cousin's "pé de cajá" (§2.1): fixed yard decoration, outside the item limit.</summary>
        public CellRect FixedTree;

        /// <summary>Sets the provisional upgrade materials (fluent, used by <see cref="HouseLevels"/>).</summary>
        public HouseLevelSpec WithUpgrade(params HouseMaterialCost[] materials)
        {
            UpgradeMaterials = materials ?? Array.Empty<HouseMaterialCost>();
            return this;
        }

        public HouseLevelSpec WithKeepsakes(params string[] keepsakes)
        {
            RequiredKeepsakes = keepsakes ?? Array.Empty<string>();
            return this;
        }

        public int WallLength(HouseWallSide side) => side == HouseWallSide.South || side == HouseWallSide.North ? InteriorWidth : InteriorDepth;

        public HouseOpening EntranceDoor
        {
            get
            {
                foreach (HouseOpening opening in Openings)
                {
                    if (opening.IsDoor && opening.Side == HouseWallSide.South)
                    {
                        return opening;
                    }
                }

                return null;
            }
        }

        /// <summary>Interior floor cells right inside the entrance door (must stay free, flood fill starts there).</summary>
        public IEnumerable<(int X, int Y)> EntranceInsideCells()
        {
            HouseOpening door = EntranceDoor;
            if (door == null)
            {
                yield break;
            }

            for (int i = 0; i < door.Width; i++)
            {
                yield return (door.Start + i, 0);
            }
        }
    }

    /// <summary>The five levels (Housing.md §2.2): N1 from the start, N2–N5 through "Melhorar a casa" (H4, <see cref="HouseUpgradeRules"/>).</summary>
    public static class HouseLevels
    {
        public const int MinLevel = 1;
        public const int MaxLevel = 5;
        /// <summary>Offline depot entries (Housing.md §2.4); 500 online.</summary>
        public const int OfflineDepotEntryLimit = 300;
        public const int OnlineDepotEntryLimit = 500;

        public static readonly IReadOnlyList<HouseLevelSpec> All = new List<HouseLevelSpec>
        {
            Build(1, "ui.house.level_1", 10, 8, 8, 8, 40, 15, 20, 1, 4, 0, 1, null),
            Build(2, "ui.house.level_2", 14, 10, 10, 10, 70, 25, 30, 1, 6, 150, 8, "enemy_forest_alpha_wolf")
                .WithUpgrade(Cost(HouseUpgradeRules.WoodId, 30), Cost(HouseUpgradeRules.FiberId, 15), Cost(HouseUpgradeRules.IronId, 5)),
            Build(3, "ui.house.level_3", 18, 12, 12, 12, 110, 40, 45, 2, 8, 400, 18, "enemy_desert_ruin_guardian")
                .WithUpgrade(Cost(HouseUpgradeRules.WoodId, 40), Cost(HouseUpgradeRules.StoneId, 30), Cost(HouseUpgradeRules.FiberId, 20)),
            Build(4, "ui.house.level_4", 22, 16, 14, 14, 160, 60, 60, 2, 10, 1000, 28, "enemy_mountain_warlord")
                .WithUpgrade(Cost(HouseUpgradeRules.StoneId, 60), Cost(HouseUpgradeRules.IronId, 20), Cost(HouseUpgradeRules.WoodId, 30)),
            Build(5, "ui.house.level_5", 30, 20, 18, 18, 240, 90, 90, 3, 12, 2500, 38, "enemy_cave_dweller")
                .WithUpgrade(Cost(HouseUpgradeRules.StoneId, 100), Cost(HouseUpgradeRules.IronId, 40), Cost(HouseUpgradeRules.WoodId, 60))
                .WithKeepsakes(HouseUpgradeRules.BossKeepsakes),
        };

        private static HouseMaterialCost Cost(string itemId, int quantity) => new HouseMaterialCost(itemId, quantity);

        public static HouseLevelSpec Get(int level)
        {
            int index = Math.Max(MinLevel, Math.Min(MaxLevel, level)) - 1;
            return All[index];
        }

        private static HouseLevelSpec Build(int level, string nameKey, int width, int depth, int yardWidth, int yardDepth,
            int interiorLimit, int yardLimit, int chestSlots, int maxExtraChests, int maxVisitors, int upgradeGold,
            int requiredLevel, string requiredBoss)
        {
            var spec = new HouseLevelSpec
            {
                Level = level,
                NameKey = nameKey,
                InteriorWidth = width,
                InteriorDepth = depth,
                YardWidth = yardWidth,
                YardDepth = yardDepth,
                InteriorItemLimit = interiorLimit,
                YardItemLimit = yardLimit,
                ChestSlots = chestSlots,
                MaxExtraChests = maxExtraChests,
                MaxVisitors = maxVisitors,
                UpgradeGold = upgradeGold,
                RequiredCharacterLevel = requiredLevel,
                RequiredBossId = requiredBoss,
            };

            // Entrance: a 1 m door (2 columns, 2 m high) on the south wall, a little left of centre; one window
            // on the west and one on the north wall (the "flor na janela" of N1, §3.3).
            int doorStart = Math.Max(1, width / 2 - 3);
            spec.Openings.Add(new HouseOpening(HouseWallSide.South, doorStart, 2, 0, 4, true));
            spec.Openings.Add(new HouseOpening(HouseWallSide.West, depth / 2 - 1, 2, 2, 2, false));
            spec.Openings.Add(new HouseOpening(HouseWallSide.North, width - 4, 2, 2, 2, false));

            // Bigger houses get more windows (H4): one on the front right and one on the east from N2, one more on the
            // back from N4, and the casarão's tall front row (§3.3).
            if (level >= 2)
            {
                spec.Openings.Add(new HouseOpening(HouseWallSide.South, width - 5, 2, 2, 2, false));
                spec.Openings.Add(new HouseOpening(HouseWallSide.East, depth / 2 - 1, 2, 2, 2, false));
            }

            if (level >= 4)
            {
                spec.Openings.Add(new HouseOpening(HouseWallSide.North, 2, 2, 2, 2, false));
            }

            if (level >= 5)
            {
                spec.Openings.Add(new HouseOpening(HouseWallSide.South, 4, 2, 2, 2, false));
                spec.Openings.Add(new HouseOpening(HouseWallSide.North, width / 2 - 1, 2, 2, 2, false));
            }

            // Yard (in front of the porch): entrance in the middle of the side next to the house,
            // the cajá tree in the far-left corner.
            spec.YardEntranceX = yardWidth / 2 - 1;
            spec.YardEntranceY = yardDepth - 1;
            spec.FixedTree = new CellRect(0, 0, 2, 2);
            return spec;
        }
    }
}
