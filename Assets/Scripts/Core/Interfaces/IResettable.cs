namespace Rapadura.Core.Interfaces
{
    /// <summary>
    /// Implemented by every persistent (GameManager-built, scene-surviving) system that holds
    /// <b>per-save</b> state — quests, world map progress, chosen hero, defeated bosses, time of day...
    /// <c>NewSessionReset</c> calls <see cref="ResetForNewSession"/> on all of them on "Novo jogo" and
    /// right before a save file is applied, so nothing from the previous session leaks into the next
    /// one (SaveManager.Load never touches a saveable whose key is missing from the file, so without
    /// this a pre-v2 save loaded after a v2 session kept the old hero, bosses, recipes...).
    ///
    /// Not for: per-device settings (language, audio, accessibility, frame rate), per-profile
    /// lifetime state (achievements) or scene-scoped bookkeeping (checkpoints, the pending encounter).
    /// </summary>
    public interface IResettable
    {
        /// <summary>Back to the state of a brand-new game. Must be idempotent and must not unsubscribe from events.</summary>
        void ResetForNewSession();
    }
}
