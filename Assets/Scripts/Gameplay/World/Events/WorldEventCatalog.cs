using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Events
{
    /// <summary>What one <see cref="WorldEventEffect"/> does when its choice is picked.</summary>
    public enum WorldEventEffectKind
    {
        /// <summary><see cref="WorldEventEffect.ItemId"/> × <see cref="WorldEventEffect.Amount"/> into the bag now (gold is <c>item_gold</c>).</summary>
        GainItem = 0,

        /// <summary>Pays <see cref="WorldEventEffect.ItemId"/> × <see cref="WorldEventEffect.Amount"/> from the bag now; the choice needs it (a cost).</summary>
        LoseItem = 1,

        /// <summary><see cref="WorldEventEffect.Amount"/> XP granted when the next fight starts (the map has no live PlayerStats).</summary>
        NextFightExperience = 2,

        /// <summary>Heals <see cref="WorldEventEffect.Value"/> (0..1) of max health when the next fight starts.</summary>
        NextFightHeal = 3,

        /// <summary><see cref="WorldEventEffect.Stat"/> modifier (<see cref="WorldEventEffect.Value"/>) for the whole next fight.</summary>
        NextFightBuff = 4,

        /// <summary>Token speed × <see cref="WorldEventEffect.Value"/> until it arrives.</summary>
        SlowTravel = 5,

        /// <summary>The clock jumps <see cref="WorldEventEffect.Value"/> hours (<see cref="TimeOfDayManager.AdvanceHours"/>).</summary>
        AdvanceHours = 6,

        /// <summary>Starts an ambush right away: one elite (tougher, extra loot) plus regular minions of the region.</summary>
        EliteEncounter = 7,

        /// <summary>Starts a regular encounter of the region right away.</summary>
        Encounter = 8,
    }

    /// <summary>One effect of a <see cref="WorldEventChoiceSpec"/>, as plain data. Build with the static factories.</summary>
    public sealed class WorldEventEffect
    {
        public readonly WorldEventEffectKind Kind;
        public readonly string ItemId;
        public readonly int Amount;
        public readonly float Value;
        public readonly StatType Stat;
        public readonly ModifierApplication Application;

        /// <summary>Amount grows with the region (<see cref="WorldEventRules.RegionRewardMultiplier"/>): the XP rewards.</summary>
        public readonly bool ScalesWithRegion;

        private WorldEventEffect(WorldEventEffectKind kind, string itemId, int amount, float value, StatType stat,
            ModifierApplication application, bool scalesWithRegion)
        {
            Kind = kind;
            ItemId = itemId;
            Amount = amount;
            Value = value;
            Stat = stat;
            Application = application;
            ScalesWithRegion = scalesWithRegion;
        }

        public static WorldEventEffect Gain(string itemId, int amount, bool scales = false) =>
            new WorldEventEffect(WorldEventEffectKind.GainItem, itemId, amount, 0f, default, default, scales);

        public static WorldEventEffect Pay(string itemId, int amount) =>
            new WorldEventEffect(WorldEventEffectKind.LoseItem, itemId, amount, 0f, default, default, false);

        public static WorldEventEffect Experience(int amount) =>
            new WorldEventEffect(WorldEventEffectKind.NextFightExperience, null, amount, 0f, default, default, true);

        public static WorldEventEffect Heal(float fraction) =>
            new WorldEventEffect(WorldEventEffectKind.NextFightHeal, null, 0, fraction, default, default, false);

        public static WorldEventEffect Buff(StatType stat, float value, ModifierApplication application = ModifierApplication.PercentAdditive) =>
            new WorldEventEffect(WorldEventEffectKind.NextFightBuff, null, 0, value, stat, application, false);

        public static WorldEventEffect Slow(float speedMultiplier) =>
            new WorldEventEffect(WorldEventEffectKind.SlowTravel, null, 0, speedMultiplier, default, default, false);

        public static WorldEventEffect Hours(float hours) =>
            new WorldEventEffect(WorldEventEffectKind.AdvanceHours, null, 0, hours, default, default, false);

        public static WorldEventEffect Elite() =>
            new WorldEventEffect(WorldEventEffectKind.EliteEncounter, null, 0, 0f, default, default, false);

        public static WorldEventEffect Fight() =>
            new WorldEventEffect(WorldEventEffectKind.Encounter, null, 0, 0f, default, default, false);

        public bool StartsEncounter => Kind == WorldEventEffectKind.EliteEncounter || Kind == WorldEventEffectKind.Encounter;
    }

    /// <summary>One of the two buttons of an event card: its effects, applied in order.</summary>
    public sealed class WorldEventChoiceSpec
    {
        public readonly IReadOnlyList<WorldEventEffect> Effects;

        public WorldEventChoiceSpec(params WorldEventEffect[] effects)
        {
            Effects = effects ?? Array.Empty<WorldEventEffect>();
        }

        /// <summary>The <see cref="WorldEventEffectKind.LoseItem"/> effects: what the bag must hold for the choice to be offered.</summary>
        public IEnumerable<WorldEventEffect> Costs
        {
            get
            {
                foreach (WorldEventEffect effect in Effects)
                {
                    if (effect.Kind == WorldEventEffectKind.LoseItem)
                    {
                        yield return effect;
                    }
                }
            }
        }

        public bool StartsEncounter
        {
            get
            {
                foreach (WorldEventEffect effect in Effects)
                {
                    if (effect.StartsEncounter)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    /// <summary>
    /// A world map road event, as plain data: where/when it can happen, how likely, and its two choices.
    /// Texts live under <see cref="KeyBase"/> (<c>WorldEventTexts</c>): title, body, the two choice labels,
    /// the two result lines and one reaction line per cousin.
    /// </summary>
    public sealed class WorldEventSpec
    {
        public readonly string Id;
        public readonly int Weight;

        /// <summary>Regions it can happen in; null/empty = all of them.</summary>
        public readonly IReadOnlyList<BiomeType> Regions;

        /// <summary>Times of day it can happen at; null/empty = any.</summary>
        public readonly IReadOnlyList<TimeOfDayPhase> Phases;

        /// <summary>Weight × this at night (ambushes like the dark). 1 = no change.</summary>
        public readonly float NightWeightMultiplier;

        public readonly WorldEventChoiceSpec ChoiceA;

        /// <summary>Always affordable (no cost, checked by the content tests): the card can always be closed.</summary>
        public readonly WorldEventChoiceSpec ChoiceB;

        public WorldEventSpec(string id, int weight, BiomeType[] regions, TimeOfDayPhase[] phases, float nightWeightMultiplier,
            WorldEventChoiceSpec choiceA, WorldEventChoiceSpec choiceB)
        {
            Id = id;
            Weight = weight;
            Regions = regions ?? Array.Empty<BiomeType>();
            Phases = phases ?? Array.Empty<TimeOfDayPhase>();
            NightWeightMultiplier = nightWeightMultiplier;
            ChoiceA = choiceA;
            ChoiceB = choiceB;
        }

        public WorldEventChoiceSpec Choice(int index) => index == 0 ? ChoiceA : ChoiceB;

        public string KeyBase => WorldEventCatalog.KeyPrefix + Id;
        public string TitleKey => KeyBase + ".title";
        public string BodyKey => KeyBase + ".body";
        public string ChoiceKey(int index) => KeyBase + (index == 0 ? ".choice_a" : ".choice_b");
        public string ResultKey(int index) => KeyBase + (index == 0 ? ".result_a" : ".result_b");
        public string LineKey(CharacterId hero) => KeyBase + ".line_" + hero.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// "Eventos aleatórios" of the world map: the road events a trip between regions can run into
    /// (<c>WorldEventCardController</c> shows them, <see cref="WorldEventDirector"/> decides when, <see cref="WorldEventRules"/>
    /// which). Defined in code like <c>RegionQuestCatalog</c> / <c>VillageNpcCatalog</c>, so no asset or seeder is needed.
    ///
    /// Content rules (checked by <c>WorldEventTests</c>): every item id exists in <c>Resources/Items</c>; choice B never
    /// costs anything, so a card can always be dismissed; every event has a text for each key in both languages and a
    /// line for each of the five cousins; at least one event is possible in every region at every time of day.
    /// Numbers are in Docs/Balance/README.md ("Eventos aleatórios do mapa").
    /// </summary>
    public static class WorldEventCatalog
    {
        public const string KeyPrefix = "ui.world_event.";

        public const string Gold = "item_gold";
        public const string HealthPotion = "item_health_potion";
        public const string IronOre = "item_iron_ore";

        public const string Peddler = "peddler";
        public const string CashewRain = "cashew_rain";
        public const string Pilgrims = "pilgrims";
        public const string EliteAmbush = "elite_ambush";
        public const string Campfire = "campfire_song";
        public const string Sandstorm = "sandstorm";
        public const string MountainFog = "mountain_fog";
        public const string CaveEcho = "cave_echo";

        /// <summary>Mascate: 2 potions for this much gold (2 × 15 in the general store = 30).</summary>
        public const int PeddlerPrice = 18;
        public const int PeddlerPotions = 2;

        /// <summary>Romeiros: gold for the potion (it sells back for 7 at the store).</summary>
        public const int PilgrimsGold = 8;

        private static readonly TimeOfDayPhase[] Daylight = { TimeOfDayPhase.Dawn, TimeOfDayPhase.Day, TimeOfDayPhase.Dusk };
        private static readonly TimeOfDayPhase[] NightOnly = { TimeOfDayPhase.Night };

        public static readonly IReadOnlyList<WorldEventSpec> Events = new List<WorldEventSpec>
        {
            // Mascate ambulante: a one-off cheap potion bundle (40% under the shop price).
            new WorldEventSpec(Peddler, 10, null, Daylight, 1f,
                new WorldEventChoiceSpec(WorldEventEffect.Pay(Gold, PeddlerPrice), WorldEventEffect.Gain(HealthPotion, PeddlerPotions)),
                new WorldEventChoiceSpec()),

            // Chuva de caju: heal now-ish, or a regen buff for the next fight.
            new WorldEventSpec(CashewRain, 8, new[] { BiomeType.Forest, BiomeType.Desert }, Daylight, 1f,
                new WorldEventChoiceSpec(WorldEventEffect.Heal(0.35f)),
                new WorldEventChoiceSpec(WorldEventEffect.Buff(StatType.HealthRegen, 3f, ModifierApplication.Flat))),

            // Romeiros na estrada: give a potion for gold + XP, or take their blessing (Defense).
            new WorldEventSpec(Pilgrims, 8, null, Daylight, 1f,
                new WorldEventChoiceSpec(WorldEventEffect.Pay(HealthPotion, 1), WorldEventEffect.Gain(Gold, PilgrimsGold), WorldEventEffect.Experience(15)),
                new WorldEventChoiceSpec(WorldEventEffect.Buff(StatType.Defense, 0.10f))),

            // Emboscada de elite: fight an elite (better loot) or slip away through the brush (time passes).
            new WorldEventSpec(EliteAmbush, 6, null, null, 1.5f,
                new WorldEventChoiceSpec(WorldEventEffect.Elite()),
                new WorldEventChoiceSpec(WorldEventEffect.Hours(1f))),

            // Cantoria ao pé da fogueira (night only): a damage buff, or rest (heal). +8% (was +15%, Balance §14):
            // stacked with a meal and the relics it pushed bosses well under the r8 15-22 s band.
            new WorldEventSpec(Campfire, 10, null, NightOnly, 1f,
                new WorldEventChoiceSpec(WorldEventEffect.Buff(StatType.AttackDamage, 0.08f), WorldEventEffect.Hours(1f)),
                new WorldEventChoiceSpec(WorldEventEffect.Heal(0.25f))),

            // Tempestade de areia (Deserto): push on slowly, or shelter while the clock runs.
            new WorldEventSpec(Sandstorm, 12, new[] { BiomeType.Desert }, null, 1f,
                new WorldEventChoiceSpec(WorldEventEffect.Slow(0.5f)),
                new WorldEventChoiceSpec(WorldEventEffect.Hours(2f))),

            // Neblina na serra (Montanha): wait it out, or feel the way (slower, finds ore).
            new WorldEventSpec(MountainFog, 12, new[] { BiomeType.Mountain }, null, 1f,
                new WorldEventChoiceSpec(WorldEventEffect.Hours(3f)),
                new WorldEventChoiceSpec(WorldEventEffect.Slow(0.6f), WorldEventEffect.Gain(IronOre, 2))),

            // Eco na caverna (Cavernas): shout back (a fight worth extra XP), or sneak on (crit buff).
            new WorldEventSpec(CaveEcho, 12, new[] { BiomeType.Caves }, null, 1f,
                new WorldEventChoiceSpec(WorldEventEffect.Experience(20), WorldEventEffect.Fight()),
                new WorldEventChoiceSpec(WorldEventEffect.Buff(StatType.CriticalChance, 0.05f, ModifierApplication.Flat))),
        };

        public static WorldEventSpec Get(string id)
        {
            foreach (WorldEventSpec spec in Events)
            {
                if (spec.Id == id)
                {
                    return spec;
                }
            }

            return null;
        }
    }
}
