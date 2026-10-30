using Rapadura.Core.Events;
using Rapadura.Gameplay.Hunting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Hunting
{
    /// <summary>
    /// The running "Tarefa de caça" in the quest tracker (<c>QuestTrackerController</c>): a second small card under the
    /// quest card, with the same <c>quest-tracker-*</c> classes ("Tarefa de caça" / "Lobo: 7/25"), shown while a task
    /// runs and for a few seconds after it is finished ("Tarefa de caça concluída! ..."). Plain C# attached to the
    /// tracker's root by the controller (one line there), event-driven on the hunting events.
    /// </summary>
    public sealed class HuntingTrackerCard
    {
        public const string CardName = "hunting-tracker-card";
        public const float CompletedSeconds = 3f;

        private readonly VisualElement _card;
        private readonly Label _title;
        private readonly Label _line;
        private IVisualElementScheduledItem _hideItem;
        private bool _showingCompleted;

        private HuntingTrackerCard(VisualElement root)
        {
            _card = new VisualElement { name = CardName, pickingMode = PickingMode.Ignore };
            _card.AddToClassList("quest-tracker-card");
            _card.AddToClassList("hunting-tracker-card");
            StyleSheet sheet = Resources.Load<StyleSheet>(HuntingBoardPanel.StyleResourcePath);
            if (sheet != null)
            {
                _card.styleSheets.Add(sheet);
            }

            _title = new Label { pickingMode = PickingMode.Ignore };
            _title.AddToClassList("quest-tracker-title");
            _card.Add(_title);
            _line = new Label { pickingMode = PickingMode.Ignore };
            _line.AddToClassList("quest-tracker-objective-text");
            _card.Add(_line);

            // Right under the quest card when there is one (before the toast), else at the end.
            VisualElement questCard = root.Q<VisualElement>("quest-tracker-card");
            int index = questCard != null && questCard.parent == root ? root.IndexOf(questCard) + 1 : root.childCount;
            root.Insert(index, _card);

            EventBus.Subscribe<HuntingStateChangedEvent>(OnChanged);
            EventBus.Subscribe<HuntingTaskProgressEvent>(OnProgress);
            EventBus.Subscribe<HuntingTaskCompletedEvent>(OnCompleted);
            EventBus.Subscribe<GameLoadedEvent>(OnLoaded);
            Refresh();
        }

        /// <summary>Adds the card under <paramref name="root"/> (null root = nothing).</summary>
        public static HuntingTrackerCard Attach(VisualElement root) => root == null ? null : new HuntingTrackerCard(root);

        public void Detach()
        {
            EventBus.Unsubscribe<HuntingStateChangedEvent>(OnChanged);
            EventBus.Unsubscribe<HuntingTaskProgressEvent>(OnProgress);
            EventBus.Unsubscribe<HuntingTaskCompletedEvent>(OnCompleted);
            EventBus.Unsubscribe<GameLoadedEvent>(OnLoaded);
            _hideItem?.Pause();
            _card.RemoveFromHierarchy();
        }

        public bool IsShown => _card.style.display != DisplayStyle.None;

        private void OnChanged(HuntingStateChangedEvent evt) => Refresh();

        private void OnProgress(HuntingTaskProgressEvent evt) => Refresh();

        private void OnLoaded(GameLoadedEvent evt) => Refresh();

        private void OnCompleted(HuntingTaskCompletedEvent evt)
        {
            _showingCompleted = true;
            _title.text = HuntingTexts.Get(HuntingTexts.Live, HuntingTexts.TrackerTitle);
            _line.text = HuntingPresenter.CompletedText(HuntingTexts.Live, evt.Task);
            _card.style.display = DisplayStyle.Flex;
            _hideItem?.Pause();
            _hideItem = _card.schedule.Execute(() =>
            {
                _showingCompleted = false;
                Refresh();
            }).StartingIn((long)(CompletedSeconds * 1000f));
        }

        public void Refresh()
        {
            if (_showingCompleted)
            {
                return;
            }

            HuntingTask task = HuntingProgress.ResolveShared()?.ActiveTask;
            if (task == null)
            {
                _card.style.display = DisplayStyle.None;
                return;
            }

            _title.text = HuntingTexts.Get(HuntingTexts.Live, HuntingTexts.TrackerTitle);
            _line.text = HuntingPresenter.TrackerLine(HuntingTexts.Live, task, HuntingPresenter.CreatureName(HuntingTexts.Live, task.enemyId));
            _card.style.display = DisplayStyle.Flex;
        }
    }
}
