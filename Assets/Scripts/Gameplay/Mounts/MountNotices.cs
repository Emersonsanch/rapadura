using System;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>Short lines for the HUD (refusals, ability results, "tired", the Primeira Sela scene). The HUDs listen; nobody listening = the caller shows a floating text instead.</summary>
    public static class MountNotices
    {
        public static event Action<string> Raised;

        public static bool HasListeners => Raised != null;

        /// <summary>Sends <paramref name="text"/> to the HUD; false when no HUD listens.</summary>
        public static bool Raise(string text)
        {
            Action<string> handler = Raised;
            if (handler == null || string.IsNullOrEmpty(text))
            {
                return false;
            }

            handler(text);
            return true;
        }
    }
}
