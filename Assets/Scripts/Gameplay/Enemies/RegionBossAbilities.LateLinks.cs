using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// What the map mechanics change in four late bosses (2026-11-16, Docs/Mapas100.md 76, 79, 80, 82): the freed barque against Apep's drink
    /// (<see cref="ApepBarqueLinkRules"/>), fire on O Rei do Piche's tar (<see cref="PitchKingBurnRules"/>, <see cref="PitchKingBurnBoard"/>), the resin
    /// lanterns taking Hobabish's mantles (<see cref="ResinLanternRules"/>, <see cref="ResinLanternBoard"/>) and Hahya and Ohya beside the cousins
    /// against the Dream of Destruction (<see cref="DreamGiantsRules"/>). The rules are pure; this only reads the boards, the save and the registry.
    /// </summary>
    public sealed partial class RegionBossAbilities
    {
        private double _tarWarnedAt = double.NegativeInfinity;
        private float _giantsFightSeconds;
        private bool _giantsSpoke;

        private void ResetLateLinks()
        {
            _tarWarnedAt = double.NegativeInfinity;
            _giantsFightSeconds = 0f;
            _giantsSpoke = false;
        }

        // ---------------- 76 · the freed barque ----------------

        /// <summary>Whether the rowers pushed the stranded barque free on this game day (the daily mark the Rio driver writes).</summary>
        private static bool BarqueFreedToday()
        {
            int today = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.Day : 1;
            int marked = ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null ? progress.GetDailyMark(ApepBarqueLinkRules.FreedMarkKey) : 0;
            return RioBarqueDefenseRules.FreedToday(marked, today);
        }

        /// <summary>Apep drinks the river: weaker and with no healing while the freed barque sails.</summary>
        private void ResolveApepDrink(Transform hero, ICombatTarget victim)
        {
            bool freed = BarqueFreedToday();
            DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
            if (freed)
            {
                DungeonFx.Warn(transform.position + Vector3.up * 4f, LinkText(LateBossLinkTexts.ApepBarque));
            }

            if (!RegionBossRules.DrinkHits(FlatDistance(hero.position, transform.position)))
            {
                return;
            }

            victim.ApplyDamage(Damage(ApepBarqueLinkRules.DrinkDamageFactor(freed)), ElementType.Dark, _controller.gameObject);
            float heal = ApepBarqueLinkRules.DrinkHealFraction(freed);
            if (_health != null && heal > 0f)
            {
                _health.ApplyHealing(_health.MaxHealth * heal, _controller.gameObject);
            }
        }

        // ---------------- 79 · fire on the King's tar ----------------

        /// <summary>
        /// The King's tar hardens: unless a fire burnt it during the warning, whoever stood in it long enough is stuck; burnt, nobody is, the flash
        /// scalds whoever is still in it and the King loses some of his health to his own pitch.
        /// </summary>
        private void ResolvePitchTar(Transform hero, ICombatTarget victim)
        {
            DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
            bool burnt = PitchKingBurnBoard.BurnsTar(Flat(transform.position), _tarWarnedAt, Time.timeAsDouble);
            if (!burnt)
            {
                if (RegionBossRules.TarSoakHits(_tarSeconds))
                {
                    victim.ApplyDamage(Damage(RegionBossRules.TarDamageFactor), ElementType.Dark, _controller.gameObject);
                }

                return;
            }

            DungeonFx.Warn(transform.position + Vector3.up * 4f, LinkText(LateBossLinkTexts.PitchBurnt));
            if (PitchKingBurnRules.FlashHits(FlatDistance(hero.position, transform.position)))
            {
                victim.ApplyDamage(Damage(PitchKingBurnRules.FlashDamageFactor), ElementType.Fire, _controller.gameObject);
            }

            if (_health != null && !_health.IsDead)
            {
                _health.ApplyDamage(_health.MaxHealth * PitchKingBurnRules.KingBurnFraction, ElementType.Fire, hero.gameObject);
            }
        }

        // ---------------- 80 · the resin lanterns ----------------

        /// <summary>Takes back the mantles the lanterns lit today forbid him (the newest stays).</summary>
        private void TrimHobabishMantles() => ResinLanternRules.TrimMantles(_hobabishMantles, ResinLanternBoard.MantleCap);

        // ---------------- 82 · Hahya and Ohya ----------------

        /// <summary>The freed giants strike the Dream on their turns, while the hero is in the fight.</summary>
        private void TickDreamGiants(float deltaTime, Transform hero)
        {
            if (_spawn.Signature != RegionBossSignature.DreamFall || hero == null || _health == null || _health.IsDead
                || FlatDistance(hero.position, transform.position) > RegionBossRules.AbilityRange)
            {
                return;
            }

            ServiceLocator.TryGet(out DefeatedEnemyRegistry registry);
            bool hahya = registry != null && registry.IsDefeated(DreamGiantsRules.HahyaId);
            bool ohya = registry != null && registry.IsDefeated(DreamGiantsRules.OhyaId);
            if (DreamGiantsRules.Allies(hahya, ohya) == 0)
            {
                return;
            }

            float before = _giantsFightSeconds;
            _giantsFightSeconds += deltaTime;
            if (hahya && DreamGiantsRules.StrikesBetween(before, _giantsFightSeconds, 0f))
            {
                GiantBlow(hero, LateBossLinkTexts.GiantHahya);
            }

            if (ohya && DreamGiantsRules.StrikesBetween(before, _giantsFightSeconds, DreamGiantsRules.OhyaOffsetSeconds))
            {
                GiantBlow(hero, LateBossLinkTexts.GiantOhya);
            }
        }

        private void GiantBlow(Transform hero, string shoutKey)
        {
            DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
            DungeonFx.Say(transform.position + Vector3.up * 5f, LinkText(shoutKey));
            _health.ApplyDamage(DreamGiantsRules.BlowDamage(_health.MaxHealth), ElementType.Earth, hero.gameObject);
            if (!_giantsSpoke)
            {
                _giantsSpoke = true;
                string line = LateBossLinkTexts.LineFor(LateBossLinkTexts.Moment.GiantsHelp, MapRequirementHeroState.Capture(null).Hero);
                DungeonFx.Say(hero.position + Vector3.up * 2.4f, LinkText(line));
            }
        }

        private static string LinkText(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return LateBossLinkTexts.Get(localization, key);
        }
    }
}
