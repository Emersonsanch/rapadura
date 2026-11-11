using System.Collections.Generic;
using Rapadura.Core.Systems;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Pure (MonoBehaviour-free) lane of the small message cards that pop up in the top-centre band of the screen
    /// (area banner, achievement toast, secret / house / wardrobe toast, tutorial "Dica"). Each card keeps its own
    /// place in its USS (its <em>desired</em> top); this only pushes a card down when an earlier one already occupies
    /// that band, so two cards never sit on top of each other: cards are walked by desired top (ties: lower
    /// <c>priority</c> first, then first reported) and each starts at least <see cref="Gap"/> below the previous one's
    /// bottom. Entries not reported for <see cref="StaleSeconds"/> are ignored (a controller destroyed mid-show).
    /// Coordinates are panel pixels; every runtime UIDocument shares the HUD's PanelSettings, so they agree.
    /// </summary>
    public sealed class MessageCardStack
    {
        public const float Gap = 8f;
        public const float StaleSeconds = 0.5f;

        private sealed class Entry
        {
            public string Id;
            public int Priority;
            public int Order;
            public float DesiredTop;
            public float Height;
            public float LastSeen;
            public float Applied;
            public float BaseMargin;
            public float Resolved;
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly List<Entry> _sorted = new List<Entry>();
        private int _nextOrder;

        public int Count => _entries.Count;

        /// <summary>A card is on screen at <paramref name="desiredTop"/> (its own layout, without our push) and is <paramref name="height"/> tall.</summary>
        public void Report(string id, int priority, float desiredTop, float height, float now)
        {
            Entry entry = Find(id);
            if (entry == null)
            {
                entry = new Entry { Id = id, Order = _nextOrder++ };
                _entries.Add(entry);
            }

            entry.Priority = priority;
            entry.DesiredTop = desiredTop;
            entry.Height = Mathf.Max(0f, height);
            entry.LastSeen = now;
        }

        /// <summary>The card left the screen. Returns whether it was in the lane.</summary>
        public bool Remove(string id)
        {
            Entry entry = Find(id);
            return entry != null && _entries.Remove(entry);
        }

        public bool Contains(string id) => Find(id) != null;

        /// <summary>Where the card's top must be so it clears every card above it (its desired top when nothing is in the way).</summary>
        public float TopFor(string id, float now)
        {
            Entry entry = Find(id);
            if (entry == null)
            {
                return 0f;
            }

            Resolve(now);
            return entry.Resolved;
        }

        /// <summary>How far (px, &gt;= 0) the card must be pushed down from its desired top.</summary>
        public float OffsetFor(string id, float now)
        {
            Entry entry = Find(id);
            return entry == null ? 0f : Mathf.Max(0f, TopFor(id, now) - entry.DesiredTop);
        }

        /// <summary>The push currently applied to the card's style (so its desired top can be recovered from its layout).</summary>
        public float AppliedOffset(string id)
        {
            Entry entry = Find(id);
            return entry != null ? entry.Applied : 0f;
        }

        public void SetApplied(string id, float offset, float baseMargin)
        {
            Entry entry = Find(id);
            if (entry != null)
            {
                entry.Applied = offset;
                entry.BaseMargin = baseMargin;
            }
        }

        public bool TryGetBaseMargin(string id, out float baseMargin)
        {
            Entry entry = Find(id);
            baseMargin = entry != null ? entry.BaseMargin : 0f;
            return entry != null && entry.Applied != 0f;
        }

        private void Resolve(float now)
        {
            _sorted.Clear();
            foreach (Entry entry in _entries)
            {
                if (now - entry.LastSeen <= StaleSeconds)
                {
                    _sorted.Add(entry);
                }
                else
                {
                    entry.Resolved = entry.DesiredTop;
                }
            }

            _sorted.Sort((a, b) =>
            {
                int byTop = Mathf.RoundToInt(a.DesiredTop).CompareTo(Mathf.RoundToInt(b.DesiredTop));
                if (byTop != 0) return byTop;
                int byPriority = a.Priority.CompareTo(b.Priority);
                return byPriority != 0 ? byPriority : a.Order.CompareTo(b.Order);
            });

            float floor = float.NegativeInfinity;
            foreach (Entry entry in _sorted)
            {
                entry.Resolved = Mathf.Max(entry.DesiredTop, floor);
                floor = entry.Resolved + entry.Height + Gap;
            }
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

    /// <summary>
    /// The one rule for transient message cards (toasts, the tutorial "Dica", banners, event-HUD cousin lines):
    /// <list type="bullet">
    /// <item><b>Modal open</b> (any window that holds a <see cref="GameplayPause"/>: Barcas de Luz · Mover, Mochila, Ficha,
    /// Loja, diálogo, Casa, menus...): non-critical cards are hidden and their clock/queue is held until it closes
    /// (<see cref="ShouldHold"/>). Many of them sort above the windows (60-850), so drawing them would cover the modal.</item>
    /// <item><b>Never stacked</b>: cards in the top-centre band report to <see cref="Lane"/> through <see cref="Place"/>
    /// and are pushed down below the card already there.</item>
    /// </list>
    /// </summary>
    public static class MessageCards
    {
        public const string AreaBannerId = "area-banner";
        public const string AchievementId = "achievement-toast";
        public const string SecretToastId = "secret-toast";
        public const string TutorialHintId = "tutorial-hint";

        // Lower = keeps its place, the others move down below it.
        public const int AreaBannerPriority = 0;
        public const int AchievementPriority = 1;
        public const int SecretToastPriority = 2;
        public const int TutorialHintPriority = 3;

        public static readonly MessageCardStack Lane = new MessageCardStack();

        /// <summary>Pure rule: a non-critical card waits while a modal is open; a critical one (death screen etc.) does not.</summary>
        public static bool ShouldHold(bool modalOpen, bool critical = false) => modalOpen && !critical;

        /// <summary>Any modal screen is up (they all hold a <see cref="GameplayPause"/>).</summary>
        public static bool IsModalOpen => GameplayPause.IsPaused;

        /// <summary>Like <see cref="IsModalOpen"/>, ignoring the pause <paramref name="owner"/> itself holds (a pausing tutorial step).</summary>
        public static bool IsModalOpenFor(object owner) => GameplayPause.IsPausedByOthers(owner);

        /// <summary>
        /// Puts <paramref name="card"/> in the lane while <paramref name="visible"/> and moves it down (margin-top) if
        /// another card is already in its spot; takes it out (and restores its margin) otherwise. Call it every frame the
        /// card may be up; cheap when nothing changes.
        /// </summary>
        public static void Place(VisualElement card, string id, int priority, bool visible)
        {
            if (!visible || card == null || card.panel == null || card.parent == null)
            {
                Release(card, id);
                return;
            }

            Rect layout = card.layout;
            if (float.IsNaN(layout.height) || layout.height <= 0f)
            {
                return;
            }

            float now = Time.unscaledTime;
            float applied = Lane.AppliedOffset(id);
            float desiredTop = card.parent.worldBound.y + layout.y - applied;
            if (float.IsNaN(desiredTop))
            {
                return;
            }

            Lane.Report(id, priority, desiredTop, layout.height, now);
            float offset = Lane.OffsetFor(id, now);
            if (Mathf.Abs(offset - applied) < 0.5f)
            {
                return;
            }

            float baseMargin = Lane.TryGetBaseMargin(id, out float stored) ? stored : card.resolvedStyle.marginTop;
            if (float.IsNaN(baseMargin))
            {
                baseMargin = 0f;
            }

            card.style.marginTop = baseMargin + offset;
            Lane.SetApplied(id, offset, baseMargin);
        }

        // ------------------------------------------------------------------
        // World-map notice column (event-room schedule, world boss / invasion schedule)
        // ------------------------------------------------------------------

        public const string NoticeColumnName = "msg-notice-column";
        public const float NoticeMargin = 16f;
        public const float NoticeGap = 8f;
        public const float NoticeMinWidth = 340f;

        /// <summary>
        /// Where the island map's notice column goes (pure): to the right of the HUD's top bar, under the menu buttons, when
        /// at least <see cref="NoticeMinWidth"/> fits there; otherwise full width right under the top bar. The cards used to
        /// sit at fixed tops (104 px / 172 px), on top of each other and of the top bar's action buttons, which wrap to
        /// four rows at 1280×720.
        /// </summary>
        public static Rect NoticeColumnRect(Rect panel, Rect topBar, Rect buttons)
        {
            bool hasBar = topBar.width > 0f && topBar.height > 0f;
            float underButtons = buttons.height > 0f ? buttons.yMax + NoticeGap : NoticeMargin;
            float besideX = hasBar ? topBar.xMax + NoticeGap * 2f : NoticeMargin;
            float besideWidth = panel.xMax - NoticeMargin - besideX;
            if (besideWidth >= NoticeMinWidth)
            {
                return new Rect(besideX, underButtons, besideWidth, 0f);
            }

            float below = hasBar ? Mathf.Max(underButtons, topBar.yMax + NoticeGap) : underButtons;
            return new Rect(panel.x + NoticeMargin, below, Mathf.Max(0f, panel.width - NoticeMargin * 2f), 0f);
        }

        /// <summary>
        /// The shared column of <paramref name="root"/> (created on first use): notice cards added to it stack one under the
        /// other instead of each picking its own absolute top. Positioned by <see cref="LayoutNoticeColumn"/>.
        /// </summary>
        public static VisualElement NoticeColumn(VisualElement root)
        {
            if (root == null)
            {
                return null;
            }

            VisualElement column = root.Q<VisualElement>(NoticeColumnName);
            if (column == null)
            {
                column = new VisualElement { name = NoticeColumnName, pickingMode = PickingMode.Ignore };
                column.AddToClassList(NoticeColumnName);
                column.style.position = Position.Absolute;
                column.style.flexDirection = FlexDirection.Column;
                column.style.alignItems = Align.Center;
                root.Add(column);
            }

            return column;
        }

        /// <summary>Adds a notice card to the column as a plain flow item (its USS absolute top no longer applies).</summary>
        public static void AddToNoticeColumn(VisualElement root, VisualElement notice)
        {
            VisualElement column = NoticeColumn(root);
            if (column == null || notice == null)
            {
                return;
            }

            notice.style.position = Position.Relative;
            notice.style.top = StyleKeyword.Auto;
            notice.style.left = StyleKeyword.Auto;
            notice.style.right = StyleKeyword.Auto;
            notice.style.alignSelf = Align.Stretch;
            notice.style.marginBottom = NoticeGap;
            column.Add(notice);
            LayoutNoticeColumn(root);
        }

        /// <summary>Places the column beside / under the map HUD's top bar (<c>wm-top-bar</c>, <c>wm-button-cluster</c>).</summary>
        public static void LayoutNoticeColumn(VisualElement root)
        {
            VisualElement column = root?.Q<VisualElement>(NoticeColumnName);
            if (column == null || column.panel == null)
            {
                return;
            }

            Rect panel = root.worldBound;
            if (float.IsNaN(panel.width) || panel.width <= 0f)
            {
                return;
            }

            Rect rect = NoticeColumnRect(panel, VisibleBound(root.Q<VisualElement>("wm-top-bar")), VisibleBound(root.Q<VisualElement>("wm-button-cluster")));
            Vector2 local = root.WorldToLocal(rect.position);
            SetIfChanged(column, local.x, local.y, rect.width);
        }

        private static void SetIfChanged(VisualElement column, float left, float top, float width)
        {
            if (Mathf.Abs(column.resolvedStyle.left - left) > 0.5f || float.IsNaN(column.resolvedStyle.left)) column.style.left = left;
            if (Mathf.Abs(column.resolvedStyle.top - top) > 0.5f || float.IsNaN(column.resolvedStyle.top)) column.style.top = top;
            if (Mathf.Abs(column.resolvedStyle.width - width) > 0.5f || float.IsNaN(column.resolvedStyle.width)) column.style.width = width;
        }

        private static Rect VisibleBound(VisualElement element)
        {
            if (element == null || element.panel == null || element.resolvedStyle.display == DisplayStyle.None)
            {
                return default;
            }

            Rect bound = element.worldBound;
            return float.IsNaN(bound.width) ? default : bound;
        }

        /// <summary>Whether a region label at <paramref name="label"/> sits under <paramref name="cover"/> (pure; empty rects never cover).</summary>
        public static bool IsCovered(Rect label, Rect cover) =>
            cover.width > 0f && cover.height > 0f && label.width > 0f && label.height > 0f && label.Overlaps(cover);

        /// <summary>Takes the card out of the lane and gives it back its own margin.</summary>
        public static void Release(VisualElement card, string id)
        {
            if (Lane.Contains(id) && card != null)
            {
                card.style.marginTop = StyleKeyword.Null;
            }

            Lane.Remove(id);
        }
    }
}
