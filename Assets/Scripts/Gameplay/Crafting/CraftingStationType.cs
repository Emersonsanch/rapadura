namespace Rapadura.Gameplay.Crafting
{
    /// <summary>
    /// Which physical "bancada" (crafting station) a recipe needs nearby to be craftable.
    /// Modeled as an enum on the recipe itself rather than a real Scene/Prefab object, since
    /// this project has never been opened in the Unity Editor yet (see TODO.md Fase 1 note) —
    /// whatever system later tracks "is the player near a station" only needs to compare this
    /// enum against the station(s) currently in range.
    /// </summary>
    public enum CraftingStationType
    {
        /// <summary>Craftable anywhere, no station required (e.g. hand-crafting basic tools).</summary>
        None,
        Campfire,
        Workbench,
        Forge,
        AlchemyTable
    }
}
