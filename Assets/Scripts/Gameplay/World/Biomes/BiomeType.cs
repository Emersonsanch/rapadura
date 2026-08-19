namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// The fixed set of biomes planned for the world (see TODO.md, Fase 8 "Biomas"). Purely a
    /// data key — actual terrain/Scene content for each biome is still blocked on opening the
    /// project in the Unity Editor; see <see cref="BiomeDefinition"/> and <see cref="BiomeRegistry"/>
    /// for the data layer that will drive that future authoring work.
    /// </summary>
    public enum BiomeType
    {
        Forest = 0,
        Desert = 1,
        Mountain = 2,
        Caves = 3,
    }
}
