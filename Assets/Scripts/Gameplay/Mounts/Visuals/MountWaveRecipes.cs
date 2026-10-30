using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>Head ornament of a mount model.</summary>
    public enum MountHornStyle
    {
        None = 0,
        /// <summary>One straight golden horn on the forehead.</summary>
        Unicorn,
        /// <summary>Two horns curling back (the goat).</summary>
        Goat,
        /// <summary>Two horns out to the sides and up (the boi).</summary>
        Bull,
        /// <summary>Two short antlers swept back (the kirin).</summary>
        Kirin,
    }

    /// <summary>Coat marks drawn on the flanks.</summary>
    public enum MountCoatPattern
    {
        None = 0,
        /// <summary>Big irregular patches (the pinto horse).</summary>
        Patches,
        /// <summary>Thin vertical stripes (the snow tiger).</summary>
        Stripes,
        /// <summary>Small ring-like spots (the jaguar).</summary>
        Rosettes,
        /// <summary>Little stars (the boi-bumbá's embroidered cloth).</summary>
        Stars,
        /// <summary>Small scale-like spots (the kirin).</summary>
        Scales,
        /// <summary>Leaves on the back (the Caipora's peccary).</summary>
        Leaves,
        /// <summary>One dark band from the chest up over the shoulder (the anteater).</summary>
        Band,
    }

    /// <summary>
    /// The body plans of the M3 wave-1 walkers (Docs/Mounts.md §3, §13 M3.1): the same <see cref="QuadrupedShape"/> /
    /// <see cref="MountRecipe.Build(ProceduralEnemyBlueprint, QuadrupedShape)"/> path as the starters, with the extras of
    /// <see cref="MountWaveDecor"/> (horns, shell, trunk, coat marks, flames...). The two rheas are bipeds.
    /// </summary>
    public static class MountWaveRecipes
    {
        public static bool TryShape(MountVisualRecipe recipe, out QuadrupedShape shape)
        {
            switch (recipe)
            {
                case MountVisualRecipe.Capybara:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.34f, BodyLength = 1.05f, BodyThickness = 0.5f, BodyWidth = 0.5f,
                        NeckLength = 0.12f, NeckPitch = 15f, HeadLength = 0.3f, HeadSize = 0.32f, EarLength = 0.06f,
                        TailLength = 0.06f, LegGirth = 1.3f,
                        Coat = EnemySwatch.SandstoneDark, Dark = EnemySwatch.SandstoneDark, Light = EnemySwatch.Sandstone,
                        Mane = EnemySwatch.SandstoneDark, Socks = EnemySwatch.Leather, Foot = EnemySwatch.Leather,
                        Blanket = EnemySwatch.CaveMoss, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.Goat:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.55f, BodyLength = 0.95f, BodyThickness = 0.42f, BodyWidth = 0.38f,
                        NeckLength = 0.3f, NeckPitch = 45f, HeadLength = 0.34f, HeadSize = 0.22f, EarLength = 0.14f,
                        TailLength = 0.14f, BushyTail = true, Hooves = true, Horns = MountHornStyle.Goat, Beard = true,
                        Coat = EnemySwatch.Bone, Dark = EnemySwatch.GolemStone, Light = EnemySwatch.Fang,
                        Mane = EnemySwatch.Bone, Socks = EnemySwatch.GolemStone, Foot = EnemySwatch.IronDark,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold, Accent = EnemySwatch.Horn,
                    };
                    return true;
                case MountVisualRecipe.Rhea:
                    shape = Bird(EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, EnemySwatch.Horn, EnemySwatch.Sandstone);
                    return true;
                case MountVisualRecipe.FireRhea:
                    shape = Bird(EnemySwatch.WarCape, EnemySwatch.Shadow, EnemySwatch.Gold, EnemySwatch.Leather);
                    shape.FlameCrest = true;
                    shape.Accent = EnemySwatch.Ember;
                    shape.PatternSwatch = EnemySwatch.Gold;
                    return true;
                case MountVisualRecipe.PintoHorse:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.72f, BodyLength = 1.25f, BodyThickness = 0.5f, BodyWidth = 0.44f,
                        NeckLength = 0.5f, NeckPitch = 50f, HeadLength = 0.5f, HeadSize = 0.26f, EarLength = 0.14f,
                        TailLength = 0.65f, Hooves = true, Pattern = MountCoatPattern.Patches, PatternSwatch = EnemySwatch.Leather,
                        Coat = EnemySwatch.Fang, Dark = EnemySwatch.Fang, Light = EnemySwatch.Bone,
                        Mane = EnemySwatch.Shadow, Socks = EnemySwatch.Fang, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Steel,
                    };
                    return true;
                case MountVisualRecipe.EnchantedDonkey:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.62f, BodyLength = 1.12f, BodyThickness = 0.5f, BodyWidth = 0.46f,
                        NeckLength = 0.34f, NeckPitch = 38f, HeadLength = 0.42f, HeadSize = 0.27f, EarLength = 0.36f,
                        TailLength = 0.5f, UprightMane = true, Hooves = true, Cowbell = true, Jewel = true, Accent = EnemySwatch.Rune,
                        Coat = EnemySwatch.Sandstone, Dark = EnemySwatch.SandstoneDark, Light = EnemySwatch.SandstoneLight,
                        Mane = EnemySwatch.SandstoneDark, Socks = EnemySwatch.Sandstone, Foot = EnemySwatch.IronDark,
                        Blanket = EnemySwatch.CaveMoss, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.ManedWolf:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.78f, BodyLength = 1.0f, BodyThickness = 0.4f, BodyWidth = 0.36f,
                        NeckLength = 0.3f, NeckPitch = 40f, HeadLength = 0.32f, HeadSize = 0.26f, EarLength = 0.24f,
                        TailLength = 0.45f, BushyTail = true, UprightMane = true,
                        Coat = EnemySwatch.OwlBuff, Dark = EnemySwatch.Black, Light = EnemySwatch.Bone,
                        Mane = EnemySwatch.Black, Socks = EnemySwatch.Black, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.Peccary:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.42f, BodyLength = 1.0f, BodyThickness = 0.5f, BodyWidth = 0.42f,
                        NeckLength = 0.14f, NeckPitch = 10f, HeadLength = 0.42f, HeadSize = 0.3f, EarLength = 0.1f,
                        TailLength = 0.08f, Hooves = true, UprightMane = true,
                        Pattern = MountCoatPattern.Leaves, PatternSwatch = EnemySwatch.CaveMoss, Accent = EnemySwatch.Lichen,
                        Coat = EnemySwatch.PeccaryFur, Dark = EnemySwatch.Shadow, Light = EnemySwatch.Bone,
                        Mane = EnemySwatch.Shadow, Socks = EnemySwatch.PeccaryFur, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.CaveMoss, Saddle = EnemySwatch.Bark, Trim = EnemySwatch.Lichen,
                    };
                    return true;
                case MountVisualRecipe.Anteater:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.5f, BodyLength = 1.2f, BodyThickness = 0.5f, BodyWidth = 0.42f,
                        NeckLength = 0.3f, NeckPitch = 5f, HeadLength = 0.6f, HeadSize = 0.2f, EarLength = 0.06f,
                        TailLength = 0.8f, BushyTail = true, LegGirth = 1.2f,
                        Pattern = MountCoatPattern.Band, PatternSwatch = EnemySwatch.Black,
                        Coat = EnemySwatch.WolfFur, Dark = EnemySwatch.Black, Light = EnemySwatch.WolfBelly,
                        Mane = EnemySwatch.WolfFurDark, Socks = EnemySwatch.WolfBelly, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.Sandstone, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.SnowTiger:
                    shape = Cat(EnemySwatch.GolemSnow, EnemySwatch.Fang, MountCoatPattern.Stripes, EnemySwatch.Shadow, EnemySwatch.Iron, EnemySwatch.Steel);
                    return true;
                case MountVisualRecipe.LightJaguar:
                    shape = Cat(EnemySwatch.Gold, EnemySwatch.SandstoneLight, MountCoatPattern.Rosettes, EnemySwatch.Shadow, EnemySwatch.CaveMoss,
                        EnemySwatch.StolenLight);
                    return true;
                case MountVisualRecipe.Mammoth:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.85f, BodyLength = 1.6f, BodyThickness = 0.95f, BodyWidth = 0.85f,
                        NeckLength = 0.2f, NeckPitch = 20f, HeadLength = 0.3f, HeadSize = 0.55f, EarLength = 0.22f, FloppyEars = true,
                        TailLength = 0.4f, Hooves = true, LegGirth = 2.2f, Tusks = true, Trunk = true, Shaggy = true,
                        Coat = EnemySwatch.BarkDark, Dark = EnemySwatch.WolfFurDark, Light = EnemySwatch.Bark,
                        Mane = EnemySwatch.WolfFurDark, Socks = EnemySwatch.BarkDark, Foot = EnemySwatch.Bone,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold, Accent = EnemySwatch.Fang,
                    };
                    return true;
                case MountVisualRecipe.Unicorn:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.72f, BodyLength = 1.2f, BodyThickness = 0.46f, BodyWidth = 0.4f,
                        NeckLength = 0.52f, NeckPitch = 55f, HeadLength = 0.46f, HeadSize = 0.25f, EarLength = 0.14f,
                        TailLength = 0.7f, Hooves = true, Horns = MountHornStyle.Unicorn, Accent = EnemySwatch.Gold,
                        Coat = EnemySwatch.Fang, Dark = EnemySwatch.Fang, Light = EnemySwatch.Bone,
                        Mane = EnemySwatch.StolenLight, Socks = EnemySwatch.Fang, Foot = EnemySwatch.Gold,
                        Blanket = EnemySwatch.Horn, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.Carbuncle:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.55f, BodyLength = 1.0f, BodyThickness = 0.44f, BodyWidth = 0.4f,
                        NeckLength = 0.26f, NeckPitch = 38f, HeadLength = 0.26f, HeadSize = 0.3f, EarLength = 0.28f,
                        TailLength = 0.6f, BushyTail = true, Jewel = true, Accent = EnemySwatch.WarEye,
                        Coat = EnemySwatch.CaveBelly, Dark = EnemySwatch.CaveSkin, Light = EnemySwatch.Fang,
                        Mane = EnemySwatch.CaveBelly, Socks = EnemySwatch.CaveSkin, Foot = EnemySwatch.CaveSkinDark,
                        Blanket = EnemySwatch.Crystal, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.Kirin:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.74f, BodyLength = 1.15f, BodyThickness = 0.44f, BodyWidth = 0.38f,
                        NeckLength = 0.5f, NeckPitch = 55f, HeadLength = 0.4f, HeadSize = 0.24f, EarLength = 0.14f,
                        TailLength = 0.6f, Hooves = true, Horns = MountHornStyle.Kirin, Accent = EnemySwatch.Horn,
                        Pattern = MountCoatPattern.Scales, PatternSwatch = EnemySwatch.Gold,
                        Coat = EnemySwatch.CaveMoss, Dark = EnemySwatch.CaveMoss, Light = EnemySwatch.Lichen,
                        Mane = EnemySwatch.Gold, Socks = EnemySwatch.Lichen, Foot = EnemySwatch.Gold,
                        Blanket = EnemySwatch.Bone, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.Boitata:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.38f, BodyLength = 1.7f, BodyThickness = 0.5f, BodyWidth = 0.5f,
                        NeckLength = 0.3f, NeckPitch = 10f, HeadLength = 0.4f, HeadSize = 0.32f, EarLength = 0f,
                        TailLength = 1.1f, LegGirth = 1.4f, FlameCrest = true, Accent = EnemySwatch.Ember, PatternSwatch = EnemySwatch.MoonEye,
                        Coat = EnemySwatch.VampFur, Dark = EnemySwatch.Shadow, Light = EnemySwatch.Ember,
                        Mane = EnemySwatch.Ember, Socks = EnemySwatch.VampFur, Foot = EnemySwatch.Shadow,
                        Blanket = EnemySwatch.Bark, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.BoiBumba:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.66f, BodyLength = 1.45f, BodyThickness = 0.7f, BodyWidth = 0.62f,
                        NeckLength = 0.22f, NeckPitch = 20f, HeadLength = 0.4f, HeadSize = 0.4f, EarLength = 0.14f,
                        TailLength = 0.6f, Hooves = true, LegGirth = 1.5f, Horns = MountHornStyle.Bull, Accent = EnemySwatch.Horn,
                        Pattern = MountCoatPattern.Stars, PatternSwatch = EnemySwatch.Gold,
                        Coat = EnemySwatch.Shadow, Dark = EnemySwatch.Shadow, Light = EnemySwatch.Fang,
                        Mane = EnemySwatch.Shadow, Socks = EnemySwatch.Shadow, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Gold, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.ElderTortoise:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.32f, BodyLength = 1.3f, BodyThickness = 0.5f, BodyWidth = 0.85f,
                        NeckLength = 0.3f, NeckPitch = 15f, HeadLength = 0.24f, HeadSize = 0.26f, EarLength = 0f,
                        TailLength = 0.12f, LegGirth = 1.8f, Shell = true, Accent = EnemySwatch.BarkDark, PatternSwatch = EnemySwatch.Bark,
                        Coat = EnemySwatch.Lichen, Dark = EnemySwatch.Lichen, Light = EnemySwatch.Lichen,
                        Mane = EnemySwatch.Lichen, Socks = EnemySwatch.Lichen, Foot = EnemySwatch.BarkDark,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    shape.SeatLift = MountWaveDecor.ShellSeatLift(shape);
                    return true;
                default:
                    shape = default;
                    return false;
            }
        }

        /// <summary>A rhea: two long legs, round body, long neck, small head with a beak (the muzzle in the beak colour).</summary>
        private static QuadrupedShape Bird(EnemySwatch coat, EnemySwatch dark, EnemySwatch beak, EnemySwatch blanket)
        {
            return new QuadrupedShape
            {
                Biped = true, LegLength = 0.8f, BodyLength = 0.78f, BodyThickness = 0.52f, BodyWidth = 0.46f,
                NeckLength = 0.62f, NeckPitch = 72f, HeadLength = 0.14f, HeadSize = 0.15f, EarLength = 0f,
                TailLength = 0.25f, BushyTail = true, Wings = true,
                Coat = coat, Dark = dark, Light = beak, Mane = coat, Socks = beak, Foot = beak,
                Blanket = blanket, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
            };
        }

        /// <summary>A big cat: long body, short neck, round head, long tail, marked coat.</summary>
        private static QuadrupedShape Cat(EnemySwatch coat, EnemySwatch light, MountCoatPattern pattern, EnemySwatch marks, EnemySwatch blanket,
            EnemySwatch trim)
        {
            return new QuadrupedShape
            {
                LegLength = 0.56f, BodyLength = 1.25f, BodyThickness = 0.45f, BodyWidth = 0.41f,
                NeckLength = 0.26f, NeckPitch = 30f, HeadLength = 0.24f, HeadSize = 0.33f, EarLength = 0.1f,
                TailLength = 0.8f, LegGirth = 1.15f, Pattern = pattern, PatternSwatch = marks,
                Coat = coat, Dark = coat, Light = light, Mane = coat, Socks = coat, Foot = light,
                Blanket = blanket, Saddle = EnemySwatch.Leather, Trim = trim,
            };
        }
    }

    /// <summary>
    /// The extra parts of the wave-1 models, added by <see cref="MountRecipe.Build(ProceduralEnemyBlueprint, QuadrupedShape)"/>
    /// as their own child meshes (<c>Decor_*</c>) of the body or the head, so they bob and turn with them and the
    /// leg engine never sees them. Deterministic: no randomness, the marks sit on fixed spots.
    /// </summary>
    public static class MountWaveDecor
    {
        public const string Prefix = "Decor_";

        /// <summary>How much higher the saddle sits on top of a shell.</summary>
        public static float ShellSeatLift(QuadrupedShape s) => s.BodyThickness * 0.32f;

        public static void Decorate(ProceduralEnemyBlueprint bp, QuadrupedShape s, int body, int head)
        {
            if (s.Horns != MountHornStyle.None || s.Tusks || s.Trunk || s.Jewel || s.Beard)
            {
                bp.Mesh(Prefix + "Head", head, Vector3.zero, b => DrawHeadExtras(b, s));
            }

            if (s.Pattern != MountCoatPattern.None || s.Shell || s.Shaggy || s.FlameCrest || s.Wings)
            {
                bp.Mesh(Prefix + "Body", body, Vector3.zero, b => DrawBodyExtras(b, s));
            }
        }

        private static void DrawHeadExtras(EnemyMeshBuilder b, QuadrupedShape s)
        {
            float size = s.HeadSize;
            float len = s.HeadLength;
            switch (s.Horns)
            {
                case MountHornStyle.Unicorn:
                    b.Horn(new Vector3(0f, size * 0.42f, size * 0.22f), 0.035f, 0.34f, 3, 0f, s.Accent, Quaternion.Euler(35f, 0f, 0f));
                    break;
                case MountHornStyle.Goat:
                    for (int side = -1; side <= 1; side += 2)
                    {
                        b.Horn(new Vector3(size * 0.2f * side, size * 0.4f, 0f), 0.04f, 0.3f, 4, -28f, s.Accent,
                            Quaternion.Euler(-15f, 0f, -10f * side));
                    }

                    break;
                case MountHornStyle.Bull:
                    for (int side = -1; side <= 1; side += 2)
                    {
                        b.Horn(new Vector3(size * 0.38f * side, size * 0.34f, 0.02f), 0.05f, 0.34f, 4, 20f * side, s.Accent,
                            Quaternion.Euler(0f, 0f, -75f * side), bendAxis: Vector3.forward);
                    }

                    break;
                case MountHornStyle.Kirin:
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector3 root = new Vector3(size * 0.16f * side, size * 0.42f, -size * 0.05f);
                        b.Horn(root, 0.03f, 0.24f, 3, -18f, s.Accent, Quaternion.Euler(-25f, 0f, -12f * side));
                        b.Horn(root + new Vector3(0.02f * side, 0.09f, -0.02f), 0.018f, 0.1f, 2, 0f, s.Accent, Quaternion.Euler(30f, 0f, -30f * side));
                    }

                    break;
            }

            if (s.Jewel)
            {
                b.Crystal(new Vector3(0f, size * 0.4f, size * 0.32f), 0.045f, 0.09f, s.Accent, Quaternion.Euler(30f, 0f, 0f));
            }

            if (s.Beard)
            {
                b.Blob(new Vector3(0f, -size * 0.55f, size * 0.25f + len * 0.45f), new Vector3(0.06f, 0.14f, 0.06f), s.Light, coarse: true);
            }

            if (s.Tusks)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    b.Horn(new Vector3(size * 0.24f * side, -size * 0.3f, size * 0.42f), 0.045f, 0.5f, 4, -24f, s.Accent,
                        Quaternion.Euler(125f, 0f, 12f * side));
                }
            }

            if (s.Trunk)
            {
                b.Horn(new Vector3(0f, -size * 0.18f, size * 0.25f + len * 0.6f), 0.09f, 0.7f, 5, 12f, s.Coat, Quaternion.Euler(165f, 0f, 0f));
            }
        }

        private static void DrawBodyExtras(EnemyMeshBuilder b, QuadrupedShape s)
        {
            float l = s.BodyLength;
            float t = s.BodyThickness;
            float w = s.BodyWidth;

            if (s.Shell)
            {
                // Dome over the whole body, a rim, the scute plates on top and the saddle moved up onto the shell.
                b.Blob(new Vector3(0f, t * 0.25f, 0f), new Vector3(w * 1.3f, t * 1.1f, l * 1.05f), s.Accent);
                b.Box(new Vector3(0f, -t * 0.12f, 0f), new Vector3(w * 1.32f, 0.06f, l * 1.0f), s.PatternSwatch);
                for (int k = -1; k <= 1; k++)
                {
                    b.Box(new Vector3(w * 0.32f * k, t * 0.62f - Mathf.Abs(k) * t * 0.12f, l * 0.3f), new Vector3(0.14f, 0.04f, 0.18f), s.PatternSwatch,
                        Quaternion.Euler(0f, 0f, -18f * k));
                }

                float top = t * 0.5f + ShellSeatLift(s);
                b.Box(new Vector3(0f, top - 0.02f, -0.02f), new Vector3(w * 0.8f, 0.035f, l * 0.32f), s.Blanket);
                b.Box(new Vector3(0f, top + 0.02f, -0.02f), new Vector3(w * 0.5f, 0.05f, l * 0.24f), s.Saddle);
                b.Box(new Vector3(0f, top + 0.06f, l * 0.1f), new Vector3(w * 0.22f, 0.06f, 0.05f), s.Saddle);
            }

            if (s.Shaggy)
            {
                // Long wool hanging from the flanks and a hump of hair.
                for (int side = -1; side <= 1; side += 2)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        float z = Mathf.Lerp(-l * 0.32f, l * 0.32f, k / 3f);
                        b.Box(new Vector3(w * 0.47f * side, -t * 0.38f, z), new Vector3(0.06f, t * 0.32f, l * 0.18f), s.Dark,
                            Quaternion.Euler(0f, 0f, 6f * side));
                    }
                }

                b.Blob(new Vector3(0f, t * 0.42f, l * 0.32f), new Vector3(w * 0.6f, t * 0.4f, l * 0.25f), s.Dark, coarse: true);
            }

            if (s.FlameCrest)
            {
                // Flames along the spine, skipping the saddle.
                for (int k = 0; k < 6; k++)
                {
                    float z = Mathf.Lerp(-l * 0.45f, l * 0.42f, k / 5f);
                    if (z > -l * 0.22f && z < l * 0.16f)
                    {
                        continue;
                    }

                    float height = 0.14f + 0.05f * (k % 2);
                    b.Crystal(new Vector3(0f, t * 0.44f, z), 0.045f, height, k % 2 == 0 ? s.Accent : s.PatternSwatch, Quaternion.Euler(-25f, 0f, 0f), 4);
                }
            }

            if (s.Wings)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    b.Blob(new Vector3(w * 0.5f * side, t * 0.05f, -l * 0.08f), new Vector3(0.08f, t * 0.55f, l * 0.6f), s.Dark,
                        Quaternion.Euler(-15f, 0f, 0f), coarse: true);
                }
            }

            DrawPattern(b, s, l, t, w);
        }

        private static void DrawPattern(EnemyMeshBuilder b, QuadrupedShape s, float l, float t, float w)
        {
            EnemySwatch mark = s.PatternSwatch;
            switch (s.Pattern)
            {
                case MountCoatPattern.Patches:
                    Marks(b, w, t, l, mark, new[] { new Vector3(0.3f, 0.2f, 0.25f), new Vector3(-0.2f, -0.35f, 0.18f), new Vector3(0.05f, 0.3f, -0.36f) },
                        new Vector3(0.05f, 0.24f, 0.3f));
                    break;
                case MountCoatPattern.Rosettes:
                    Marks(b, w, t, l, mark, Grid(), new Vector3(0.03f, 0.07f, 0.08f));
                    break;
                case MountCoatPattern.Scales:
                    Marks(b, w, t, l, mark, Grid(), new Vector3(0.025f, 0.05f, 0.06f));
                    break;
                case MountCoatPattern.Stars:
                    for (int side = -1; side <= 1; side += 2)
                    {
                        foreach (Vector3 p in new[] { new Vector3(0.3f, 0.1f, 0f), new Vector3(-0.3f, -0.1f, 0f), new Vector3(0f, -0.25f, 0f) })
                        {
                            b.Crystal(new Vector3(w * 0.52f * side, t * p.y, l * p.x), 0.04f, 0.04f, mark, Quaternion.Euler(0f, 0f, 90f * side), 5);
                        }
                    }

                    break;
                case MountCoatPattern.Stripes:
                    for (int side = -1; side <= 1; side += 2)
                    {
                        for (int k = 0; k < 6; k++)
                        {
                            float z = Mathf.Lerp(-l * 0.4f, l * 0.36f, k / 5f);
                            b.Box(new Vector3(w * 0.5f * side, t * 0.05f, z), new Vector3(0.02f, t * 0.62f, 0.035f), mark,
                                Quaternion.Euler(14f, 0f, 0f));
                        }
                    }

                    break;
                case MountCoatPattern.Band:
                    for (int side = -1; side <= 1; side += 2)
                    {
                        b.Box(new Vector3(w * 0.5f * side, 0f, l * 0.22f), new Vector3(0.025f, t * 0.9f, 0.09f), mark, Quaternion.Euler(-40f, 0f, 0f));
                    }

                    break;
                case MountCoatPattern.Leaves:
                    for (int k = 0; k < 4; k++)
                    {
                        float z = Mathf.Lerp(-l * 0.42f, l * 0.4f, k / 3f);
                        if (z > -l * 0.22f && z < l * 0.16f)
                        {
                            z += l * 0.2f * Mathf.Sign(z == 0f ? 1f : z);
                        }

                        b.Box(new Vector3(0.04f * (k % 2 == 0 ? 1 : -1), t * 0.5f, z), new Vector3(0.1f, 0.02f, 0.16f), k % 2 == 0 ? mark : s.Accent,
                            Quaternion.Euler(0f, 35f * (k % 2 == 0 ? 1 : -1), 12f));
                    }

                    break;
            }
        }

        /// <summary>(z along the body −0.5..0.5, y around −0.5..0.5) spots on both flanks.</summary>
        private static Vector3[] Grid()
        {
            return new[]
            {
                new Vector3(0.32f, 0.18f, 0f), new Vector3(0.12f, -0.12f, 0f), new Vector3(-0.08f, 0.22f, 0f),
                new Vector3(-0.28f, -0.05f, 0f), new Vector3(-0.4f, 0.24f, 0f), new Vector3(0.36f, -0.22f, 0f),
            };
        }

        private static void Marks(EnemyMeshBuilder b, float w, float t, float l, EnemySwatch mark, Vector3[] spots, Vector3 size)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                foreach (Vector3 p in spots)
                {
                    // Ellipse of the barrel at that height: the mark sits just outside the coat.
                    float y = t * p.y;
                    float half = w * 0.5f * Mathf.Sqrt(Mathf.Max(0.15f, 1f - 4f * p.y * p.y)) + 0.012f;
                    b.Blob(new Vector3(half * side, y, l * p.x), new Vector3(size.x, size.y + p.z, size.z), mark, coarse: true);
                }
            }
        }
    }
}
