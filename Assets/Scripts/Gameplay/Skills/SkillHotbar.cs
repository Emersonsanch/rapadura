using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Runtime glue between the player's input, the HUD hotbar buttons and <see cref="SkillManager"/>:
    /// owns the persisted <see cref="HotbarLoadout"/> (slot → skill id), keeps it default-filled from
    /// whatever the <see cref="SkillManager"/> has learned, and casts through
    /// <see cref="SkillManager.TryCast"/> when a hotbar key (Skill1..Skill5 in
    /// PlayerControls.inputactions) or touch button (see <c>HudController</c>) is pressed.
    ///
    /// Default fill is driven purely off SkillManager's own state — first its
    /// <see cref="SkillManager.EquippedSlots"/> (in slot order, e.g. what <c>CharacterSkillSet.GrantTo</c>
    /// equips for the selected character), then any other learned non-passive skill — so it works
    /// the same whether skills come from a per-character set, the skill tree, or a save file.
    /// </summary>
    [RequireComponent(typeof(SkillManager))]
    public class SkillHotbar : MonoBehaviour
    {
        [SerializeField] private int _slotCount = HotbarLoadout.DefaultSlotCount;

        [Tooltip("Automatically drop newly learned castable skills into empty hotbar slots. Never overwrites an assigned slot.")]
        [SerializeField] private bool _autoFillEmptySlots = true;

        [Tooltip("Optional — resolved from this GameObject if left empty.")]
        [SerializeField] private PlayerInputHandler _input;

        [Header("Soft Targeting")]
        [Tooltip("Half-angle (degrees) of the forward cone preferred when picking the nearest living enemy to aim a cast at.")]
        [SerializeField] private float _softTargetConeHalfAngle = SkillTargeting.DefaultConeHalfAngle;

        [Tooltip("Snap the caster's facing (yaw only) toward the soft target when a targeted cast fires.")]
        [SerializeField] private bool _faceSoftTarget = true;

        private SkillManager _skillManager;
        private HotbarLoadout _loadout;
        private SaveManager _saveManager;

        public SkillManager SkillManager
        {
            get { EnsureInitialized(); return _skillManager; }
        }

        public HotbarLoadout Loadout
        {
            get { EnsureInitialized(); return _loadout; }
        }

        public int SlotCount => Loadout.SlotCount;

        /// <summary>Soft target picked by the last successful cast (null for self casts / nothing in reach). For HUD/tutorial hooks.</summary>
        public ICombatTarget LastCastTarget { get; private set; }

        /// <summary>
        /// Overrides where soft-target candidates come from (tests). Null = every enabled
        /// <see cref="EnemyController"/>'s <see cref="Health"/>.
        /// </summary>
        public Func<IEnumerable<ICombatTarget>> TargetCandidatesProvider { get; set; }

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>Same Edit Mode guard as <see cref="SkillManager"/>: AddComponent doesn't run Awake outside Play Mode.</summary>
        private void EnsureInitialized()
        {
            if (_loadout != null)
            {
                return;
            }

            _loadout = new HotbarLoadout(_slotCount);
            _skillManager = GetComponent<SkillManager>();

            if (_input == null)
            {
                _input = GetComponent<PlayerInputHandler>();
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SkillLearnedEvent>(OnSkillLearned);
            EventBus.Subscribe<SkillEquippedChangedEvent>(OnSkillEquippedChanged);
            EventBus.Subscribe<SkillRespecEvent>(OnSkillRespec);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SkillLearnedEvent>(OnSkillLearned);
            EventBus.Unsubscribe<SkillEquippedChangedEvent>(OnSkillEquippedChanged);
            EventBus.Unsubscribe<SkillRespecEvent>(OnSkillRespec);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void Start()
        {
            TryRegisterWithSaveManager();
            FillDefaults();
        }

        private void OnDestroy()
        {
            _saveManager?.Unregister(Loadout);
        }

        private void Update()
        {
            // GameManager registers SaveManager in its own Awake, which Unity doesn't order relative
            // to this object's Start — retry (only while still unresolved) rather than resolving once
            // and staying null forever (same race fixed in SaveLoadMenuController/SettingsMenuController).
            if (_saveManager == null)
            {
                TryRegisterWithSaveManager();
            }

            if (_input == null)
            {
                return;
            }

            int count = Mathf.Min(Loadout.SlotCount, PlayerInputHandler.SkillActionCount);

            for (int i = 0; i < count; i++)
            {
                if (_input.WasSkillPressedThisFrame(i))
                {
                    TryCastSlot(i);
                }
            }
        }

        private void TryRegisterWithSaveManager()
        {
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                _saveManager = saveManager;
                _saveManager.Register(Loadout);
            }
        }

        // ---------------------------------------------------------------
        // Slots
        // ---------------------------------------------------------------

        /// <summary>Learned skill instance in <paramref name="slotIndex"/>, or null if the slot is empty or its skill isn't learned.</summary>
        public SkillInstance GetSlotSkill(int slotIndex)
        {
            string skillId = Loadout.GetSkillId(slotIndex);
            return string.IsNullOrEmpty(skillId) || SkillManager == null ? null : SkillManager.GetLearnedSkill(skillId);
        }

        /// <summary>Assigns a learned skill to a slot (swapping if it was elsewhere on the bar). Unlearned ids are rejected.</summary>
        public bool AssignSlot(int slotIndex, string skillId)
        {
            if (!string.IsNullOrEmpty(skillId) && (SkillManager == null || !SkillManager.IsSkillLearned(skillId)))
            {
                return false;
            }

            return Loadout.Assign(slotIndex, skillId);
        }

        /// <summary>Casts the skill in <paramref name="slotIndex"/> via <see cref="SkillManager.TryCast"/>. Returns false if empty, on cooldown or unaffordable.</summary>
        public bool TryCastSlot(int slotIndex)
        {
            SkillInstance instance = GetSlotSkill(slotIndex);

            if (instance == null || instance.Definition.SkillType == SkillType.Passive)
            {
                return false;
            }

            SkillAim aim = ComputeAim(instance.Definition);

            // Single-target skills hit the soft target directly (explicit target) so collider
            // placement/size can't make a correctly aimed cast whiff; area skills overlap at the aim point.
            GameObject explicitTarget = aim.Mode == SkillAimMode.SingleTarget && aim.HasTarget ? aim.Target.Transform.gameObject : null;

            if (!SkillManager.TryCast(instance, aim.Point, explicitTarget))
            {
                return false;
            }

            LastCastTarget = aim.Target;

            if (_faceSoftTarget && aim.HasTarget)
            {
                FaceToward(aim.Target.Transform.position);
            }

            return true;
        }

        /// <summary>
        /// Soft target selection (see <see cref="SkillTargeting"/>): self buffs/heals at the caster,
        /// area/single-target skills at the nearest living enemy in reach (forward cone preferred),
        /// otherwise straight ahead at Range.
        /// </summary>
        public SkillAim ComputeAim(SkillDefinition skill)
        {
            return SkillTargeting.ResolveAim(skill, transform.position, transform.forward, CollectCandidates(), _softTargetConeHalfAngle);
        }

        private IEnumerable<ICombatTarget> CollectCandidates()
        {
            if (TargetCandidatesProvider != null)
            {
                return TargetCandidatesProvider();
            }

            return EnemyHealthCandidates();
        }

        private static IEnumerable<ICombatTarget> EnemyHealthCandidates()
        {
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy != null && enemy.Health != null)
                {
                    yield return enemy.Health;
                }
            }
        }

        private void FaceToward(Vector3 worldPoint)
        {
            Vector3 toTarget = worldPoint - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(toTarget, Vector3.up);
            }
        }

        // ---------------------------------------------------------------
        // Default fill
        // ---------------------------------------------------------------

        /// <summary>
        /// Clears every slot and default-fills again from <see cref="SkillManager"/> (equipped slots
        /// first) - e.g. after the selected hero changed, so the new hero's skills sit in slot order
        /// instead of filling whatever gaps the previous hero's left behind.
        /// </summary>
        public void ResetLoadout()
        {
            Loadout.ClearAll();
            FillDefaults();
        }

        /// <summary>Prunes slots whose skill is no longer learned, then (if enabled) default-fills empty slots.</summary>
        public void RefreshFromSkillManager()
        {
            PruneUnlearned();
            FillDefaults();
        }

        private void FillDefaults()
        {
            if (_autoFillEmptySlots && SkillManager != null)
            {
                Loadout.FillEmptySlots(CollectDefaultCandidates(SkillManager));
            }
        }

        private void PruneUnlearned()
        {
            if (SkillManager != null)
            {
                Loadout.RemoveWhere(id => !SkillManager.IsSkillLearned(id));
            }
        }

        /// <summary>
        /// Default-fill order: SkillManager's equipped slots first (in slot order), then every other
        /// learned castable (non-passive) skill. Duplicates are fine — <see cref="HotbarLoadout.FillEmptySlots"/> skips them.
        /// </summary>
        public static IEnumerable<string> CollectDefaultCandidates(SkillManager skillManager)
        {
            if (skillManager == null)
            {
                yield break;
            }

            foreach (SkillInstance equipped in skillManager.EquippedSlots)
            {
                if (IsCastable(equipped))
                {
                    yield return equipped.Definition.SkillId;
                }
            }

            foreach (SkillInstance learned in skillManager.LearnedSkills)
            {
                if (IsCastable(learned))
                {
                    yield return learned.Definition.SkillId;
                }
            }
        }

        private static bool IsCastable(SkillInstance instance)
        {
            return instance != null && instance.Definition != null && instance.Definition.SkillType != SkillType.Passive;
        }

        private void OnSkillLearned(SkillLearnedEvent evt) => FillDefaults();
        private void OnSkillEquippedChanged(SkillEquippedChangedEvent evt) => FillDefaults();
        private void OnSkillRespec(SkillRespecEvent evt) => PruneUnlearned();
        private void OnGameLoaded(GameLoadedEvent evt) => RefreshFromSkillManager();
    }
}
