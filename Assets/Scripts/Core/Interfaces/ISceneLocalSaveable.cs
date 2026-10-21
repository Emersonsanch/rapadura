namespace Rapadura.Core.Interfaces
{
    /// <summary>
    /// Marker for an <see cref="ISaveable"/> whose state only means something in the scene it was
    /// captured in (e.g. the player's world position). It is still written to save files and restored
    /// by an explicit <c>SaveManager.Load</c>, but <c>SaveManager.RestoreCarriedState</c> never applies it
    /// to a freshly loaded scene — otherwise the player would reappear wherever the previous
    /// encounter ended instead of at the new arena's spawn.
    /// </summary>
    public interface ISceneLocalSaveable : ISaveable
    {
    }
}
