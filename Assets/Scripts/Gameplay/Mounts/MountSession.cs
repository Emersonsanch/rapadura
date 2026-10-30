using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// The live (unsaved) riding state of one rider, as plain C# shared by the arena's <see cref="MountController"/>
    /// and the world map's <see cref="WorldMapMountRider"/>: the whistle (hold <see cref="MountRules.SummonSeconds"/>),
    /// mounted or not, the gallop stamina and the recall/ability cooldowns. All times are on the caller's clock
    /// (game time in the arena, unscaled on the map). EditMode-tested.
    /// </summary>
    public sealed class MountSession
    {
        private bool _exhausted;

        public bool IsMounted { get; private set; }
        public bool IsWhistling { get; private set; }
        public float WhistleHeld { get; private set; }
        public float WhistleProgress => IsWhistling ? MountRules.SummonProgress(WhistleHeld) : 0f;

        public float Stamina { get; private set; } = -1f;
        public float MaxStamina { get; private set; } = 100f;
        public float StaminaFraction => MaxStamina > 0f ? Mathf.Clamp01(Stamina / MaxStamina) : 0f;
        public bool IsGalloping { get; private set; }
        public bool IsExhausted => _exhausted;

        /// <summary>When the next whistle is allowed (10 s after a dismount).</summary>
        public float RecallReadyAt { get; private set; } = float.NegativeInfinity;
        public float AbilityReadyAt { get; private set; } = float.NegativeInfinity;

        public float RecallRemaining(float now) => MountRules.CooldownRemaining(RecallReadyAt, now);
        public float AbilityRemaining(float now) => MountRules.CooldownRemaining(AbilityReadyAt, now);

        /// <summary>Starts the whistle when <see cref="MountRules.CanSummon"/> allows it; returns the block (None = started).</summary>
        public MountSummonBlock BeginWhistle(MountPlace place, bool hasActiveMount, bool tired, float secondsSinceCombat, float now)
        {
            MountSummonBlock block = MountRules.CanSummon(place, hasActiveMount, IsMounted, tired, secondsSinceCombat, RecallRemaining(now));
            if (block == MountSummonBlock.None)
            {
                IsWhistling = true;
                WhistleHeld = 0f;
            }

            return block;
        }

        /// <summary>Advances the whistle while it is held; true the step it completes (the caller then mounts).</summary>
        public bool TickWhistle(float deltaTime, bool holding, float summonSeconds = MountRules.SummonSeconds)
        {
            if (!IsWhistling)
            {
                return false;
            }

            if (!holding)
            {
                CancelWhistle();
                return false;
            }

            WhistleHeld += Mathf.Max(0f, deltaTime);
            if (MountRules.SummonProgress(WhistleHeld, summonSeconds) >= 1f)
            {
                IsWhistling = false;
                WhistleHeld = 0f;
                return true;
            }

            return false;
        }

        public void CancelWhistle()
        {
            IsWhistling = false;
            WhistleHeld = 0f;
        }

        /// <summary>Climbs on a mount with <paramref name="maxStamina"/> (the stamina of a fresh session starts full).</summary>
        public void Mount(float maxStamina)
        {
            MaxStamina = Mathf.Max(1f, maxStamina);
            Stamina = Stamina < 0f ? MaxStamina : Mathf.Min(Stamina, MaxStamina);
            IsMounted = true;
            IsGalloping = false;
            CancelWhistle();
        }

        /// <summary>Gets off; the next whistle waits <see cref="MountRules.RecallCooldownSeconds"/>.</summary>
        public void Dismount(float now, bool startRecallCooldown = true)
        {
            if (!IsMounted)
            {
                return;
            }

            IsMounted = false;
            IsGalloping = false;
            if (startRecallCooldown)
            {
                RecallReadyAt = MountRules.CooldownReadyAt(now, MountRules.RecallCooldownSeconds);
            }
        }

        /// <summary>One ride step: gallop when wanted and allowed (and not in a city), stamina spent/refilled.</summary>
        public void Tick(float deltaTime, bool wantsGallop, MountPlace place, bool hungry = false)
        {
            if (Stamina < 0f)
            {
                Stamina = MaxStamina;
            }

            bool allowed = IsMounted && MountRules.CanGallop(place, Stamina, _exhausted, hungry);
            Stamina = MountRules.StepGallopStamina(Stamina, MaxStamina, IsMounted && wantsGallop, allowed, deltaTime, ref _exhausted, out bool galloping);
            IsGalloping = galloping;
        }

        /// <summary>Motor multiplier for the current state (see <see cref="MountRules.MountedSpeedMultiplier"/>); 1 when not mounted.</summary>
        public float SpeedMultiplier(float buffMultiplier, float mountSpeed, MountPlace place, bool hungry = false)
        {
            return IsMounted
                ? MountRules.MountedSpeedMultiplier(buffMultiplier, mountSpeed, IsGalloping, place == MountPlace.SafeZone, hungry)
                : buffMultiplier;
        }

        /// <summary>Spends the ability when allowed (mounted, ready, not in a city); true when it fires.</summary>
        public bool TryUseAbility(float now, MountPlace place, float cooldownSeconds, bool allowedInSafeZone = false)
        {
            if (!MountRules.CanUseAbility(place, IsMounted, MountRules.IsReady(AbilityReadyAt, now), allowedInSafeZone))
            {
                return false;
            }

            AbilityReadyAt = MountRules.CooldownReadyAt(now, cooldownSeconds);
            return true;
        }
    }
}
