namespace Rapadura.Gameplay.Skills
{
    /// <summary>Runtime instance of a buff/debuff currently affecting a character.</summary>
    public class ActiveStatEffect
    {
        public StatModifierDefinition Definition { get; }
        public float TimeRemaining { get; private set; }
        /// <summary>Optional identifier of what applied this effect (e.g. an equipped item id), used for targeted removal.</summary>
        public string SourceKey { get; }
        private float _tickTimer;

        /// <summary>An effect whose definition has a duration &lt;= 0 never expires on its own (e.g. passive skill bonuses) — it only goes away via explicit removal (<see cref="BuffController.RemoveEffectsBySource"/>).</summary>
        public bool IsPermanent => Definition.durationSeconds <= 0f;

        public ActiveStatEffect(StatModifierDefinition definition, string sourceKey = null)
        {
            Definition = definition;
            TimeRemaining = definition.durationSeconds;
            SourceKey = sourceKey;
        }

        /// <summary>Advances the effect's clock. Returns true once per elapsed tick interval (for damage/heal over time).</summary>
        public bool Tick(float deltaTime, out bool expired)
        {
            if (IsPermanent)
            {
                expired = false;
            }
            else
            {
                TimeRemaining -= deltaTime;
                expired = TimeRemaining <= 0f;
            }

            if (!Definition.ticksOverTime)
            {
                return false;
            }

            _tickTimer += deltaTime;

            if (_tickTimer < Definition.tickIntervalSeconds)
            {
                return false;
            }

            _tickTimer -= Definition.tickIntervalSeconds;
            return true;
        }
    }
}
