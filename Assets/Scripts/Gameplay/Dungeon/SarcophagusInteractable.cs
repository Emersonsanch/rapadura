using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// A stone sarcophagus of the ossuary (or, with <see cref="IsLegendary"/>, the Artificers' chest in the secret niche).
    /// Walking up to it slides the lid aside with a heavy stone grind (<see cref="DungeonFx.StoneSlideCue"/>) and rolls
    /// <see cref="DungeonRules.RollSarcophagus"/>: 70% loot from the catacombs' table (<see cref="DungeonLoot"/>), 30% an
    /// ambush of two floating skulls with a fright sting and a puff of purple smoke (<see cref="AmbushRequested"/>, spawned by
    /// the director). The chest never ambushes: it always gives its fixed piece plus table rolls. Each opens once per save
    /// (<see cref="DungeonProgress.MarkSarcophagusOpened"/>); a lid already opened loads slid aside. A soft amber light
    /// brightens while the hero is near (the "you can open me" highlight).
    /// </summary>
    [DisallowMultipleComponent]
    public class SarcophagusInteractable : MonoBehaviour, IInteractable
    {
        private const float LidSlideSeconds = 0.9f;

        [SerializeField] private string _sarcophagusId;
        [SerializeField] private Transform _lid;
        [SerializeField] private Light _highlight;
        [SerializeField] private bool _legendary;
        [SerializeField] private string _guaranteedItemId;

        private readonly DungeonProximity _proximity = new DungeonProximity();
        private Vector3 _lidClosed;
        private Quaternion _lidClosedRotation;
        private float _lidProgress;
        private bool _sliding;

        /// <summary>Explicit progress (tests); null = the shared one.</summary>
        public DungeonProgress Progress { get; set; }

        /// <summary>Randomness of the roll (the director passes the run's seeded one).</summary>
        public System.Random Random { get; set; } = new System.Random();

        /// <summary>Explicit loot table (tests); null = the catacombs' table.</summary>
        public BiomeDefinition Table { get; set; }

        /// <summary>Asks for <c>count</c> floating skulls at a position (the director spawns them).</summary>
        public Action<Vector3, int> AmbushRequested { get; set; }

        public string SarcophagusId => _sarcophagusId;
        public bool IsLegendary => _legendary;
        public bool IsOpened => _legendary ? ResolveProgress().IsChestTaken(_sarcophagusId) : ResolveProgress().IsSarcophagusOpened(_sarcophagusId);
        public bool CanInteract => !IsOpened && enabled;

        /// <summary>What the last opening gave (None before).</summary>
        public SarcophagusOutcome? LastOutcome { get; private set; }

        public void Configure(string id, Transform lid, Light highlight, bool legendary = false, string guaranteedItemId = null)
        {
            _sarcophagusId = id;
            _lid = lid;
            _highlight = highlight;
            _legendary = legendary;
            _guaranteedItemId = guaranteedItemId;
            if (_lid != null)
            {
                _lidClosed = _lid.localPosition;
                _lidClosedRotation = _lid.localRotation;
            }

            if (IsOpened)
            {
                _lidProgress = 1f;
                ApplyLid();
                SetHighlighted(false);
            }
        }

        private DungeonProgress ResolveProgress() => Progress ?? DungeonProgress.ResolveShared();

        public void SetHighlighted(bool highlighted)
        {
            if (_highlight != null)
            {
                _highlight.intensity = highlighted ? 1.8f : 0f;
            }
        }

        public bool Interact(GameObject actor)
        {
            SarcophagusOutcome? outcome = Open(actor != null ? actor.GetComponentInParent<InventoryManager>() : null, out _);
            return outcome.HasValue;
        }

        /// <summary>
        /// Opens it once: marks it in the save, rolls the outcome and hands out the loot / asks for the ambush. Null when it was
        /// already open. <paramref name="drops"/> = what was rolled (also when the bag could not fit all of it).
        /// </summary>
        public SarcophagusOutcome? Open(InventoryManager inventory, out List<LootDrop> drops)
        {
            drops = new List<LootDrop>();
            DungeonProgress progress = ResolveProgress();
            bool first = _legendary ? progress.MarkChestTaken(_sarcophagusId) : progress.MarkSarcophagusOpened(_sarcophagusId);
            if (!first)
            {
                return null;
            }

            System.Random random = Random ?? new System.Random();
            SarcophagusOutcome outcome = _legendary ? SarcophagusOutcome.Loot : DungeonRules.RollSarcophagus(random.NextDouble());
            LastOutcome = outcome;
            _sliding = true;
            SetHighlighted(false);
            DungeonFx.Play(DungeonFx.StoneSlideCue, transform.position);

            if (outcome == SarcophagusOutcome.Loot)
            {
                BiomeDefinition table = Table != null ? Table : DungeonLoot.CatacombsTable;
                if (_legendary && !string.IsNullOrEmpty(_guaranteedItemId) && ItemDatabase.TryGetById(_guaranteedItemId, out ItemDefinition piece) && piece != null)
                {
                    drops.Add(DungeonLoot.MakeDrop(piece, 1, true, random, ItemRarity.Epic));
                }

                drops.AddRange(DungeonLoot.RollTable(table, _legendary ? DungeonRules.LegendaryChestRolls : DungeonRules.SarcophagusLootRolls, _legendary, random));
                DungeonFx.Say(transform.position + Vector3.up * 2.2f, DungeonFx.Text(_legendary ? DungeonTexts.ChestOpened : DungeonTexts.SarcophagusLoot));
                DungeonLoot.GrantToHero(inventory, drops, gameObject);
            }
            else
            {
                DungeonFx.Play(DungeonFx.AmbushCue, transform.position);
                DungeonFx.Warn(transform.position + Vector3.up * 2.2f, DungeonFx.Text(DungeonTexts.SarcophagusAmbush));
                SpawnSmoke();
                AmbushRequested?.Invoke(transform.position + transform.forward * 1.2f, DungeonRules.SarcophagusAmbushSkulls);
            }

            return outcome;
        }

        private void Update()
        {
            _proximity.Tick(this, transform, Time.deltaTime);
            if (_sliding)
            {
                _lidProgress = Mathf.Clamp01(_lidProgress + Time.deltaTime / LidSlideSeconds);
                ApplyLid();
                _sliding = _lidProgress < 1f;
            }
        }

        private void ApplyLid()
        {
            if (_lid == null)
            {
                return;
            }

            float eased = Mathf.SmoothStep(0f, 1f, _lidProgress);
            _lid.localPosition = _lidClosed + new Vector3(0.65f * eased, -0.15f * eased * eased, 0.1f * eased);
            _lid.localRotation = _lidClosedRotation * Quaternion.Euler(0f, 18f * eased, -10f * eased);
        }

        /// <summary>A short puff of purple smoke (plain primitives fading out, no particle material needed).</summary>
        private void SpawnSmoke()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            DungeonPuff.Spawn(transform.position + Vector3.up * 0.9f, DungeonProps.SmokePurple, 1.6f, 0.9f);
        }
    }
}
