using System;
using System.Collections.Generic;

namespace Rapadura.UI.HUD
{
    /// <summary>
    /// Pure (MonoBehaviour-free) "only one gameplay screen at a time" rule used by
    /// <see cref="HudController"/> for the screens it can open (Skills, Mochila, Missões and the
    /// pause-menu-only Conquistas/Criação): opening one closes every other registered screen, so
    /// two full-screen overlays never stack on top of each other on a small phone screen.
    ///
    /// Screens are registered as delegates rather than concrete controller types so the controllers
    /// can be resolved lazily (FindFirstObjectByType fallback) and so EditMode tests can exercise
    /// the rule without any UIDocument. The coordinator never caches open state: it always asks
    /// <c>isOpen</c>, so screens closed by their own Fechar button (or opened by something else,
    /// e.g. a crafting workbench) are still handled correctly.
    ///
    /// Like the screens themselves (Inventory/SkillTree/Shop don't touch Time.timeScale), this does
    /// not pause or block gameplay — only the Pause menu does that.
    /// </summary>
    public sealed class ScreenExclusivityCoordinator
    {
        private sealed class Entry
        {
            public string Id;
            public Func<bool> IsOpen;
            public Action Open;
            public Action Close;
        }

        private readonly List<Entry> _entries = new List<Entry>();

        public int Count => _entries.Count;

        /// <summary>Registers (or replaces) a screen. All delegates are required.</summary>
        public void Register(string id, Func<bool> isOpen, Action open, Action close)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Screen id is required.", nameof(id));
            if (isOpen == null) throw new ArgumentNullException(nameof(isOpen));
            if (open == null) throw new ArgumentNullException(nameof(open));
            if (close == null) throw new ArgumentNullException(nameof(close));

            Entry existing = Find(id);

            if (existing == null)
            {
                existing = new Entry { Id = id };
                _entries.Add(existing);
            }

            existing.IsOpen = isOpen;
            existing.Open = open;
            existing.Close = close;
        }

        public bool IsRegistered(string id) => Find(id) != null;

        public bool IsOpen(string id)
        {
            Entry entry = Find(id);
            return entry != null && entry.IsOpen();
        }

        public bool IsAnyOpen
        {
            get
            {
                foreach (Entry entry in _entries)
                {
                    if (entry.IsOpen())
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>Closes every other open screen, then opens <paramref name="id"/>. Returns false for an unknown id.</summary>
        public bool Open(string id)
        {
            Entry entry = Find(id);

            if (entry == null)
            {
                return false;
            }

            CloseAll(exceptId: id);

            if (!entry.IsOpen())
            {
                entry.Open();
            }

            return true;
        }

        public void Close(string id)
        {
            Entry entry = Find(id);

            if (entry != null && entry.IsOpen())
            {
                entry.Close();
            }
        }

        /// <summary>Closes <paramref name="id"/> if it's open, otherwise opens it exclusively. Returns whether it ends up open.</summary>
        public bool Toggle(string id)
        {
            Entry entry = Find(id);

            if (entry == null)
            {
                return false;
            }

            if (entry.IsOpen())
            {
                entry.Close();
                return false;
            }

            Open(id);
            return entry.IsOpen();
        }

        /// <summary>Closes every open screen except <paramref name="exceptId"/>. Returns how many were closed.</summary>
        public int CloseAll(string exceptId = null)
        {
            int closed = 0;

            foreach (Entry entry in _entries)
            {
                if (entry.Id == exceptId || !entry.IsOpen())
                {
                    continue;
                }

                entry.Close();
                closed++;
            }

            return closed;
        }

        private Entry Find(string id)
        {
            foreach (Entry entry in _entries)
            {
                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
