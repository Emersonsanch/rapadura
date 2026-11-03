using System;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// The cracked wall of stacked bones hiding the gallery's secret niche (and its legendary chest). Comes down at once under
    /// a blunt impact (<see cref="DungeonRules.IsBluntImpact"/>: Joaquim's Maremoto or any Earth area skill — the same casts
    /// <c>GroundImpactSystem</c> turns into ground strikes — or Ícaro's Granada de Faísca / an explosion) landing within reach;
    /// plain hits only chip it, and after <see cref="DungeonRules.WallHitsToBreak"/> of them it crumbles too, so every cousin can
    /// find the niche. An <see cref="ICombatTarget"/> with a solid collider (tap → walk over → auto-attack, like the Torre's
    /// gate); only the hero's hits count. Breaking it raises a puff of bone dust and pebbles (<see cref="DungeonFx.WallCrumbleCue"/>),
    /// turns its colliders off (the passage opens; there is no NavMesh in the project — enemies steer around colliders) and
    /// is saved in <see cref="DungeonProgress"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class BreakableBoneWall : MonoBehaviour, ICombatTarget
    {
        private const float PunchSeconds = 0.16f;

        [SerializeField] private string _wallId = DungeonRules.BoneWallId;

        private int _hits;
        private float _punch;
        private Vector3 _baseScale = Vector3.one;
        private bool _hinted;

        /// <summary>Explicit progress (tests); null = the shared one.</summary>
        public DungeonProgress Progress { get; set; }

        public string WallId => _wallId;
        public int Hits => _hits;
        public bool IsBroken => ResolveProgress().IsWallBroken(_wallId);
        public bool IsAlive => !IsBroken;
        public Transform Transform => transform;

        /// <summary>Raised once when the wall comes down.</summary>
        public event Action<BreakableBoneWall> Broken;

        public void Configure(string wallId)
        {
            _wallId = wallId;
            _baseScale = transform.localScale;
            if (IsBroken)
            {
                HideBroken();
            }
        }

        private DungeonProgress ResolveProgress() => Progress ?? DungeonProgress.ResolveShared();

        private void OnEnable()
        {
            EventBus.Subscribe<SkillCastEvent>(OnSkillCast);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
        }

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (evt.Skill == null || evt.Caster == null || evt.Caster.GetComponentInParent<EnemyController>() != null)
            {
                return;
            }

            // Earth area skills strike around the caster; thrown explosives where they land.
            Vector3 origin = evt.Skill.Element == ElementType.Earth ? evt.Caster.transform.position : evt.TargetPoint;
            TryBluntImpact(evt.Skill.SkillId, evt.Skill.Element, evt.Skill.AreaRadius, origin);
        }

        /// <summary>A skill landed at <paramref name="origin"/>: breaks the wall when it is blunt and reaches it. True when it broke.</summary>
        public bool TryBluntImpact(string skillId, ElementType element, float areaRadius, Vector3 origin)
        {
            if (IsBroken || !DungeonRules.IsBluntImpact(skillId, element, areaRadius))
            {
                return false;
            }

            Vector3 offset = origin - transform.position;
            offset.y = 0f;
            if (offset.magnitude > Mathf.Max(0f, areaRadius) + DungeonRules.BluntReachMargin)
            {
                return false;
            }

            Break();
            return true;
        }

        public void ApplyDamage(float amount, ElementType element, GameObject source)
        {
            if (IsBroken || amount <= 0f || (source != null && source.GetComponentInParent<EnemyController>() != null))
            {
                return;
            }

            _hits++;
            _punch = PunchSeconds;
            if (!_hinted)
            {
                _hinted = true;
                DungeonFx.Say(transform.position + Vector3.up * 3f, DungeonFx.Text(DungeonTexts.WallHint));
            }

            if (_hits >= DungeonRules.WallHitsToBreak)
            {
                Break();
            }
        }

        public void ApplyHealing(float amount, GameObject source)
        {
        }

        /// <summary>Brings it down (idempotent).</summary>
        public void Break()
        {
            if (!ResolveProgress().MarkWallBroken(_wallId))
            {
                return;
            }

            DungeonFx.Play(DungeonFx.WallCrumbleCue, transform.position);
            DungeonFx.Say(transform.position + Vector3.up * 3f, DungeonFx.Text(DungeonTexts.WallBroken));
            if (Application.isPlaying)
            {
                DungeonPuff.Spawn(transform.position + Vector3.up * 1.2f, DungeonProps.BoneDust, 2.4f, 1.1f);
                DungeonPuff.Pebbles(transform.position + Vector3.up * 1.5f, DungeonProps.Ivory, 9);
            }

            HideBroken();
            Broken?.Invoke(this);
        }

        private void HideBroken()
        {
            foreach (Collider body in GetComponentsInChildren<Collider>(true))
            {
                body.enabled = false;
            }

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
            }
        }

        private void Update()
        {
            if (_punch <= 0f)
            {
                return;
            }

            _punch -= Time.deltaTime;
            float k = Mathf.Sin(Mathf.Clamp01(_punch / PunchSeconds) * Mathf.PI);
            transform.localScale = new Vector3(_baseScale.x * (1f + 0.04f * k), _baseScale.y * (1f - 0.03f * k), _baseScale.z);
            if (_punch <= 0f)
            {
                transform.localScale = _baseScale;
            }
        }
    }
}
