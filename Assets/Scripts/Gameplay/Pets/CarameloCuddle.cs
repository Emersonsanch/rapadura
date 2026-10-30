using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Pets
{
    /// <summary>
    /// "Fazer carinho" (world map button, <c>CarameloCuddleButton</c>): what petting Caramelo does. The
    /// first time ever Vó Zefa introduces him (<see cref="CompanionPreferences.Introduced"/>); after that each
    /// cuddle shows the next line of the current cousin (<see cref="CarameloTexts.CuddleLineKey"/>). Either
    /// way it arms a tiny buff, <see cref="Buff"/> (+5% movement speed for <see cref="BuffSeconds"/> s), which
    /// <see cref="CarameloCompanion"/> applies to the hero at the start of the next encounter: the map has no
    /// combat stats to buff. Cuddling again only refreshes it, it never stacks. Session state (static,
    /// cleared on domain reload / app start): a pending cuddle isn't worth a save field.
    /// </summary>
    public static class CarameloCuddle
    {
        /// <summary><see cref="BuffController"/> source key (one effect, refreshed instead of stacked).</summary>
        public const string BuffSource = "pet_caramelo";
        public const float BuffSpeedBonus = 0.05f;
        public const float BuffSeconds = 90f;

        private static int _cuddles;

        /// <summary>True while a cuddle buff waits for the next encounter.</summary>
        public static bool BuffPending { get; private set; }

        /// <summary>Cuddles this session (drives which cousin line comes next).</summary>
        public static int Count => _cuddles;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _cuddles = 0;
            BuffPending = false;
        }

        /// <summary>
        /// Pets Caramelo: arms the buff and returns the key of the line to show. The intro wins while
        /// <paramref name="introduced"/> is false; the caller then marks him introduced.
        /// </summary>
        public static string Cuddle(CharacterId? hero, bool introduced)
        {
            BuffPending = true;
            if (!introduced || !hero.HasValue)
            {
                return CarameloTexts.Intro;
            }

            return CarameloTexts.CuddleLineKey(hero.Value, _cuddles++);
        }

        /// <summary>
        /// <see cref="Cuddle(CharacterId?, bool)"/> once the campaign moved on: <paramref name="cameFrom"/> (the region of
        /// the latest interlude seen, <c>StoryBeats.LastRegionCleared</c>) adds the cousin's stage line to the rotation,
        /// first (<see cref="CarameloTexts.CuddleLineKey(CharacterId, int, BiomeType?)"/>).
        /// </summary>
        public static string Cuddle(CharacterId? hero, bool introduced, BiomeType? cameFrom)
        {
            BuffPending = true;
            if (!introduced || !hero.HasValue)
            {
                return CarameloTexts.Intro;
            }

            return CarameloTexts.CuddleLineKey(hero.Value, _cuddles++, cameFrom);
        }

        /// <summary>Takes the pending buff (once): true if one was waiting.</summary>
        public static bool TryConsumeBuff()
        {
            if (!BuffPending)
            {
                return false;
            }

            BuffPending = false;
            return true;
        }

        /// <summary>The buff applied to the hero: MoveSpeed +5% (PercentAdditive), <see cref="BuffSeconds"/> s.</summary>
        public static StatModifierDefinition Buff(string displayName = "Carinho do Caramelo")
        {
            return new StatModifierDefinition
            {
                displayName = displayName,
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = BuffSpeedBonus,
                durationSeconds = BuffSeconds,
                isDebuff = false,
            };
        }

        /// <summary>Test hook: back to a fresh session.</summary>
        public static void ResetForTests() => ResetStatics();
    }
}
