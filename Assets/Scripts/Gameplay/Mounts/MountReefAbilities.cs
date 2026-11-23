using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// What the ability of <see cref="MountReefCatalog"/> does in the arena; called by <see cref="MountLegendAbilities.Execute"/> for the kinds it
    /// does not handle. Returns the toast key, or null.
    /// </summary>
    public static class MountReefAbilities
    {
        public static string Execute(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects)
        {
            if (ability.Kind != MountAbilityKind.ReefSong)
            {
                return null;
            }

            // Canto do Recife: the conch sings, a coral light opens round the hippocamp and the group close by heals (the relit reef's gift).
            Vector3 at = ctx.Hero.transform.position;
            effects.StartLamp(ability.DurationSeconds, MountReefRules.SongLampRadius, MountReefRules.SongLampColor);
            effects.StartHealAura(ability.DurationSeconds, ability.Radius, ability.Magnitude);
            MountAudio.Play(MountAudio.Whistle, at);
            return MountReefTexts.ToastSong;
        }
    }
}
