using Rapadura.Core.Events;

namespace Rapadura.Core.Audio
{
    /// <summary>Raised whenever a category's mix volume changes (settings UI, accessibility options, etc.).</summary>
    public readonly struct AudioCategoryVolumeChangedEvent : IGameEvent
    {
        public readonly AudioCategory Category;
        public readonly float Volume;

        public AudioCategoryVolumeChangedEvent(AudioCategory category, float volume)
        {
            Category = category;
            Volume = volume;
        }
    }

    /// <summary>Raised whenever the currently playing ambient/music track changes (crossfade started).</summary>
    public readonly struct MusicTrackChangedEvent : IGameEvent
    {
        public readonly string TrackName;

        public MusicTrackChangedEvent(string trackName)
        {
            TrackName = trackName;
        }
    }
}
