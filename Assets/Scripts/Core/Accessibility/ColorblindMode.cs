namespace Rapadura.Core.Accessibility
{
    /// <summary>
    /// Colorblind-assist palette to apply on top of the game's normal colors. Names follow the
    /// standard clinical terms used across the industry (see Game Accessibility Guidelines /
    /// Filament Games research referenced in <see cref="ColorblindPaletteMap"/>): Protanopia and
    /// Deuteranopia both confuse red/green (the two most common forms), Tritanopia confuses
    /// blue/yellow.
    /// </summary>
    public enum ColorblindMode
    {
        None,
        Protanopia,
        Deuteranopia,
        Tritanopia
    }
}
