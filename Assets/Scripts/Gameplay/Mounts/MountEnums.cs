using System;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>How a mount moves (Docs/Mounts.md §2.6). M1 only has terrestrial mounts.</summary>
    public enum MountType
    {
        Terrestre = 0,
        Voador = 1,
        Aquatico = 2,
        Escavador = 3,
    }

    /// <summary>Cosmetic fit and camera framing (§6).</summary>
    public enum MountSize
    {
        Pequeno = 0,
        Medio = 1,
        Grande = 2,
    }

    /// <summary>What a mount eats (§4.2; used from M2 on).</summary>
    [Flags]
    public enum MountDiet
    {
        None = 0,
        Herbivora = 1,
        Carnivora = 2,
        Encantada = 4,
    }

    /// <summary>Online trade rule (§5.3).</summary>
    public enum MountTradeRule
    {
        Livre = 0,
        UmaVez = 1,
        Vinculada = 2,
    }

    /// <summary>The procedural low-poly model a mount uses (<see cref="Visuals.MountRecipe"/>).</summary>
    public enum MountVisualRecipe
    {
        Pony = 0,
        BigDog = 1,
        Donkey = 2,

        // M3 wave 1 (terrestrial walkers, MountWaveRecipes). Appended only: assets store the int.
        Capybara = 3,
        Goat = 4,
        Rhea = 5,
        PintoHorse = 6,
        EnchantedDonkey = 7,
        ManedWolf = 8,
        Peccary = 9,
        Anteater = 10,
        SnowTiger = 11,
        Mammoth = 12,
        Unicorn = 13,
        LightJaguar = 14,
        Carbuncle = 15,
        FireRhea = 16,
        Kirin = 17,
        Boitata = 18,
        BoiBumba = 19,
        ElderTortoise = 20,
    }

    /// <summary>The effect family of a mount ability (§12.2). M1 implements Dash, LootMagnet and Fear; M3 wave 1 the walkers' kinds (MountWaveAbilities).</summary>
    public enum MountAbilityKind
    {
        Dash = 0,
        DoubleJump,
        Glide,
        ShortFlight,
        FreeFlight,
        Blink,
        Burrow,
        WaterWalk,
        LootMagnet,
        Cleanse,
        Fear,
        FreeGallop,
        Knockback,
        CCImmunity,
        NightVision,
        PartySpeedTrail,
        RangedInteract,
        WeatherImmunity,
        Intimidate,
        AreaStun,
        HealAura,
        PounceMark,
        Light,
        DamageTrail,
        Stealth,
        RepelRing,
        PartyBuff,
        Revive,
        Invulnerable,
        ConeBreath,
    }

    /// <summary>Where the hero is, as far as riding goes (§2.1, §2.5).</summary>
    public enum MountPlace
    {
        /// <summary>World map (the token travels the roads).</summary>
        WorldMap = 0,

        /// <summary>A city / safe zone: trot only (×1.00, no gallop), no abilities.</summary>
        SafeZone = 1,

        /// <summary>Encounter arena while the fight is on: always on foot (M1).</summary>
        ArenaFight = 2,

        /// <summary>Arena with no fight running (after the victory, or the scene played on its own).</summary>
        ArenaFree = 3,

        /// <summary>Inside a house: never mounted.</summary>
        Indoors = 4,
    }

    /// <summary>Why a summon (whistle) is refused, <see cref="MountRules.CanSummon"/>.</summary>
    public enum MountSummonBlock
    {
        None = 0,
        NoMount,
        AlreadyMounted,
        Tired,
        InCombat,
        RecallCooldown,
        NotHere,
    }
}
