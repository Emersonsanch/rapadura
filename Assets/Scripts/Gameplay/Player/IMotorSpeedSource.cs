namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Optional override of <see cref="PlayerMotor"/>'s speed while <see cref="IsActive"/> (a mount: the hero rides at
    /// the mount's multiplier of the run speed, whatever the walk/run/crouch stance). The motor still owns
    /// acceleration, turning and gravity; the source only maps the BuffController multiplier to the one actually used.
    /// </summary>
    public interface IMotorSpeedSource
    {
        bool IsActive { get; }

        /// <summary>The multiplier to apply to the run speed, given the rider's BuffController one (0 = rooted).</summary>
        float ResolveMultiplier(float buffMultiplier);
    }
}
