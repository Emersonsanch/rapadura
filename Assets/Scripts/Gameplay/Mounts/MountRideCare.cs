using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// The M2 side of riding, shared by the arena's <see cref="MountController"/> and the map's
    /// <see cref="WorldMapMountRider"/>: who is riding (favourite?), the whistle length by hearts, the line on mounting,
    /// and the per-frame care while mounted (satiety falls, metres turn into XP and affection, "hungry" notice).
    /// </summary>
    public static class MountRideCare
    {
        /// <summary>The cousin being played (null when none was chosen).</summary>
        public static CharacterId? Rider => ServiceLocator.TryGet(out CharacterSelectionState selection) && selection != null ? selection.Selected : null;

        public static bool IsFavorite(MountDefinition definition) => MountCareRules.IsFavorite(definition, Rider);

        public static float WhistleSeconds(OwnedMount mount) => MountCareRules.WhistleSeconds(mount != null ? mount.Hearts : 0);

        public static float AbilityCooldown(MountDefinition definition, OwnedMount mount) => MountCareRules.AbilityCooldown(definition, mount, Rider);

        /// <summary>The line said on mounting (favourite's own, or the cousin's from 3 hearts on), or null.</summary>
        public static string MountLine(LocalizationManager localization, MountDefinition definition, OwnedMount mount)
        {
            CharacterId? rider = Rider;
            bool favorite = MountCareRules.IsFavorite(definition, rider);
            if (definition == null || mount == null || !MountCareRules.SaysLineOnMount(mount.Hearts, favorite))
            {
                return null;
            }

            if (favorite && !string.IsNullOrEmpty(definition.FavoriteLineKey))
            {
                string name = rider.HasValue ? CharacterRegistry.Get(rider.Value).DisplayName : string.Empty;
                return name + ": " + MountTexts.Get(localization, definition.FavoriteLineKey);
            }

            return rider.HasValue ? MountTexts.Get(localization, MountTexts.RiderLineKey(rider.Value)) : null;
        }

        /// <summary>One mounted step: satiety down, <paramref name="meters"/> ridden counted; announces hunger once.</summary>
        public static void TickMounted(MountCollection collection, OwnedMount mount, MountDefinition definition, float seconds, float meters)
        {
            if (collection == null || mount == null)
            {
                return;
            }

            if (collection.DecaySatiety(mount, seconds))
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                MountNotices.Raise(MountTexts.Format(localization, MountTexts.Hungry, MountTexts.MountName(localization, definition)));
            }

            if (meters > 0.001f)
            {
                collection.AddRideDistance(mount, meters, IsFavorite(definition));
            }
        }
    }
}
