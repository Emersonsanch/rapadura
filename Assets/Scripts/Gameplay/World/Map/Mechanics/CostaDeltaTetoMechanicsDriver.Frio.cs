using Rapadura.Core.DI;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The cold of the Geleira do Teto do Mundo (38, <see cref="GlacierColdRules"/>), on the same driver as the avalanches of the chutes: the meter
    /// lives here (it falls by itself off the glacier and is zero again when the driver stops); full, a short-lived slow sits on the hero's
    /// BuffController, refreshed while it applies. Orange columns mark the herders' fires near the hero. It never hurts and never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const string ColdSource = "teto_glacier_cold";
        private const float FireViewRange = 160f;
        private const float FireHeight = 3.5f;

        private static readonly Color FireColor = new Color(1f, 0.55f, 0.18f, 0.95f);

        private LineRenderer[] _fireLines;
        private StatModifierDefinition _coldSlow;
        private MealService _coldMeals;
        private float _cold;
        private GlacierColdBand _coldBand;
        private bool _coldApplied;
        private float _nextColdRefresh;

        private void TickCold(Vector2 local)
        {
            bool onGlacier = OverworldLayouts.OnGlacier(local);
            if (!onGlacier && _cold <= 0f)
            {
                HideFires();
                SetColdSlow(false);
                return;
            }

            bool alive = _stats.IsAlive;
            bool atShelter = !alive || GlacierColdRules.AtFire(OverworldLayouts.GlacierFires, local)
                || OverworldRules.SettlementAt(_layout, _tetoOrigin + local) != null;
            bool night = ServiceLocator.TryGet(out TimeOfDayManager time) && time != null && time.DaylightFactor < 0.5f;
            float before = _cold;
            _cold = GlacierColdRules.Step(_cold, onGlacier, atShelter, night, TeaActive(), Time.deltaTime);

            GlacierColdBand band = GlacierColdRules.BandOf(_cold);
            if (band > _coldBand)
            {
                SayCold(band == GlacierColdBand.Frozen ? RouteSightAirColdTexts.ColdFrozen : RouteSightAirColdTexts.ColdChilly);
            }
            else if (atShelter && alive && before >= GlacierColdRules.ChillyAt && _cold <= 0f)
            {
                SayCold(RouteSightAirColdTexts.ColdWarmed);
            }

            _coldBand = band;
            SetColdSlow(band == GlacierColdBand.Frozen && alive);
            ShowFires(local, onGlacier);
        }

        private bool TeaActive()
        {
            _coldMeals ??= MealService.ResolveShared();
            return _coldMeals != null && _coldMeals.ActiveRecipe != null && _coldMeals.ActiveRecipe.RecipeId == MealCatalog.CafeCoado;
        }

        private void SetColdSlow(bool on)
        {
            if (_buffs == null)
            {
                return;
            }

            if (!on)
            {
                if (_coldApplied)
                {
                    _buffs.RemoveEffectsBySource(ColdSource);
                    _coldApplied = false;
                }

                return;
            }

            if (_coldApplied && Time.time < _nextColdRefresh)
            {
                return;
            }

            _coldSlow ??= NewSlow("Frio", GlacierColdRules.FrozenMoveBonus);
            _coldApplied = true;
            _nextColdRefresh = Time.time + EffectRefresh;
            _buffs.ApplyEffect(_coldSlow, ColdSource);
        }

        private void ShowFires(Vector2 local, bool onGlacier)
        {
            Vector2[] fires = OverworldLayouts.GlacierFires;
            _fireLines ??= new LineRenderer[fires.Length];
            for (int i = 0; i < fires.Length; i++)
            {
                bool show = onGlacier && (fires[i] - local).sqrMagnitude <= FireViewRange * FireViewRange;
                LineRenderer line = _fireLines[i];
                if (!show)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }

                    continue;
                }

                if (line == null)
                {
                    line = _fireLines[i] = NewLine("Tt_GlacierFire", 0.6f);
                    line.startColor = line.endColor = FireColor;
                    Vector2 world = _tetoOrigin + fires[i];
                    float y = Mathf.Max(0f, Ground(world));
                    line.SetPosition(0, new Vector3(world.x, y, world.y));
                    line.SetPosition(1, new Vector3(world.x, y + FireHeight, world.y));
                }

                line.enabled = true;
                line.widthMultiplier = 0.8f + 0.25f * Mathf.PingPong(Time.time * 3f + i, 1f);
            }
        }

        private void HideFires()
        {
            if (_fireLines == null)
            {
                return;
            }

            foreach (LineRenderer line in _fireLines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private void LeaveCold()
        {
            _cold = 0f;
            _coldBand = GlacierColdBand.Warm;
            SetColdSlow(false);
            HideFires();
        }

        private void SayCold(string key)
        {
            ServiceLocator.TryGet(out Rapadura.Core.Localization.LocalizationManager localization);
            SayText(RouteSightAirColdTexts.Get(localization, key));
        }
    }
}
