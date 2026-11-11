using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Dialogue;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// A keeper standing at their shop front in a walkable city: a trigger sphere (the interact range, from the layout)
    /// around the figure. Stepping in (the same on touch and keyboard: it is the walk that triggers it) makes a keeper with something to say speak first in the dialogue box,
    /// with the counter as a choice, and otherwise opens the counter through the same <see cref="CityShopService"/> and <see cref="ShopManager"/> as the hub (today's rotating
    /// stock, freguesia, the house depot); a resident without a counter shows their line for the cousin in play. The rules are <see cref="CityResidentTalk"/>. Same trigger
    /// shape as <c>NpcInteractable</c>; <see cref="Interact"/> is public for a future tap-to-talk. Uses resident ids, never display names.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CityResidentPost : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        /// <summary>Off in PlayMode walks that only check reachability (no window should pop and pause the walk).</summary>
        public static bool AutoInteract { get; set; } = true;

        [SerializeField] private string _cityId;
        [SerializeField] private string _residentId;
        [SerializeField] private string _shopId;
        [SerializeField] private string _lotId;

        private DialogueDefinition _talk;
        private bool _listening;

        public string CityId => _cityId;
        public string ResidentId => _residentId;
        public string ShopId => _shopId;
        public string LotId => _lotId;
        public bool PlayerInRange { get; private set; }

        public void Configure(string cityId, string lotId, string residentId, string shopId)
        {
            _cityId = cityId;
            _lotId = lotId;
            _residentId = residentId;
            _shopId = shopId;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            PlayerInRange = true;
            if (AutoInteract)
            {
                Interact();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PlayerTag))
            {
                PlayerInRange = false;
            }
        }

        /// <summary>
        /// The cousins step up: a keeper with a story or quest line, or on this cousin's first visit, speaks first in the dialogue box with the counter as a choice; with nothing
        /// pending the counter opens straight away. A resident without a counter says their line over their head. True when something opened (see <see cref="CityResidentTalk"/>).
        /// </summary>
        public bool Interact()
        {
            ServiceLocator.TryGet(out DialogueManager dialogue);
            if (dialogue != null && dialogue.IsActive)
            {
                return false;
            }

            CityResidentSpec resident = FindResident();
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.HasSelection ? selection.Selected : null;
            ServiceLocator.TryGet(out WorldMapProgress map);
            string greeted = CityResidentTalk.GreetedMark(_residentId, hero);
            string greeting = resident == null ? null : hero.HasValue ? resident.LineKey(hero.Value) : resident.AnyLineKey;

            CityResidentPlan plan = CityResidentTalk.Plan(!string.IsNullOrEmpty(_shopId) && ResolveShop(map) != null, QuestLine(hero), greeting, map == null || map.GetTimeMark(greeted) != 0L);
            switch (plan.Action)
            {
                case CityResidentAction.OpenShop:
                    return OpenShop();

                case CityResidentAction.Converse:
                    if (Converse(dialogue, resident, plan.LineKey))
                    {
                        if (plan.IsGreeting)
                        {
                            map?.SetTimeMark(greeted, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                        }

                        return true;
                    }

                    // No dialogue box in this scene: the line floats, and the counter still opens.
                    Say(resident, plan.LineKey);
                    return OpenShop();

                case CityResidentAction.Say:
                    Say(resident, plan.LineKey);
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>This resident's story or city quest line for the cousin in play (starts what is offered); the story needs a hero and their level.</summary>
        private string QuestLine(CharacterId? hero)
        {
            if (!ServiceLocator.TryGet(out QuestManager quests))
            {
                return null;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            PlayerStats stats = player != null ? player.GetComponent<PlayerStats>() : null;
            return CityResidentTalk.QuestLine(quests, _residentId, stats != null ? hero : null, stats != null ? stats.Level : 0);
        }

        private bool Converse(DialogueManager dialogue, CityResidentSpec resident, string lineKey)
        {
            if (dialogue == null || resident == null || !EnsureDialogueView())
            {
                return false;
            }

            // The box resolves its keys straight through the localization manager.
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                CityTexts.EnsureRegistered(localization);
            }

            _talk = _talk != null ? _talk : ScriptableObject.CreateInstance<DialogueDefinition>();
            _talk.SetDataForTests(CityResidentTalk.DialogueId(_residentId), CityResidentTalk.LineNodeId, CityResidentTalk.BuildNodes(_residentId, resident.NameKey, lineKey));

            dialogue.SetFlag(CityResidentTalk.OpenShopFlag, false);
            StopListening();
            EventBus.Subscribe<DialogueEndedEvent>(HandleDialogueEnded);
            _listening = true;
            dialogue.StartDialogue(_talk);
            return true;
        }

        /// <summary>The walkable towns live in the world map scene, which ships without a dialogue box (created on first use, like the map's quest-givers).</summary>
        private static bool EnsureDialogueView()
        {
            RegionNpcTalkButton talkButton = FindFirstObjectByType<RegionNpcTalkButton>(FindObjectsInactive.Include);
            if (talkButton != null)
            {
                talkButton.EnsureDialogueView();
            }

            return FindFirstObjectByType<DialogueUIController>(FindObjectsInactive.Include) != null;
        }

        private void HandleDialogueEnded(DialogueEndedEvent ended)
        {
            if (ended.DialogueId != CityResidentTalk.DialogueId(_residentId))
            {
                return;
            }

            StopListening();
            if (ServiceLocator.TryGet(out DialogueManager dialogue) && dialogue.HasFlag(CityResidentTalk.OpenShopFlag))
            {
                dialogue.SetFlag(CityResidentTalk.OpenShopFlag, false);
                OpenShop();
            }
        }

        private void StopListening()
        {
            if (_listening)
            {
                EventBus.Unsubscribe<DialogueEndedEvent>(HandleDialogueEnded);
                _listening = false;
            }
        }

        private void OnDestroy()
        {
            StopListening();
            if (_talk != null)
            {
                Destroy(_talk);
            }
        }

        private ShopDefinition ResolveShop(WorldMapProgress map)
        {
            return ServiceLocator.TryGet(out ShopManager shops) ? CityShopService.Resolve(_shopId, CityProgress.ResolveShared(), shops, map) : null;
        }

        private bool OpenShop()
        {
            if (!ServiceLocator.TryGet(out ShopManager shops))
            {
                return false;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            InventoryManager inventory = player != null ? player.GetComponent<InventoryManager>() : null;
            if (inventory == null)
            {
                return false;
            }

            ServiceLocator.TryGet(out WorldMapProgress map);
            ShopDefinition shop = ResolveShop(map);
            if (shop == null)
            {
                return false;
            }

            if (shops.IsOpen)
            {
                shops.CloseShop();
            }

            shops.OpenShop(shop, inventory);
            return true;
        }

        private void Say(CityResidentSpec resident, string lineKey)
        {
            if (resident == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            string speech = CityTexts.Format(localization, CityTexts.Speech, CityTexts.ResidentName(localization, resident), CityTexts.Get(localization, lineKey));
            CombatFxManager.SpawnLootText(transform.position + Vector3.up * 2.3f, speech);
        }

        private CityResidentSpec FindResident()
        {
            if (string.IsNullOrEmpty(_residentId))
            {
                return null;
            }

            if (CityCatalog.Mascate.ResidentId == _residentId)
            {
                return CityCatalog.Mascate;
            }

            CitySpec city = CityCatalog.Get(_cityId);
            return city?.Residents != null ? System.Linq.Enumerable.FirstOrDefault(city.Residents, r => r.ResidentId == _residentId) : null;
        }
    }
}
