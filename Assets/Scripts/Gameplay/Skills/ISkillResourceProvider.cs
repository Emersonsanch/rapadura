namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Contract for whatever component holds the mana/energy pool a caster spends on skills.
    /// SkillManager depends on this interface rather than PlayerStats directly, so NPCs or
    /// bosses with their own resource model can cast skills through the same manager later.
    /// </summary>
    public interface ISkillResourceProvider
    {
        bool HasMana(float amount);
        void ConsumeMana(float amount);
        bool HasEnergy(float amount);
        void ConsumeEnergy(float amount);
    }
}
