namespace Rapadura.Core.Audio
{
    /// <summary>
    /// Logical mix buses for the project. Stands in for real Unity Audio Mixer groups
    /// (Master/Music/SFX/UI) until the project is opened in the Editor and a proper
    /// <c>AudioMixer</c> asset can be authored — see the note on <see cref="AudioManager"/>.
    /// </summary>
    public enum AudioCategory
    {
        Master = 0,
        Music = 1,
        Sfx = 2,
        Ui = 3,
    }
}
