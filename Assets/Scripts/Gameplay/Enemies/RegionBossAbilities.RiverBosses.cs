using System.Collections.Generic;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// The abilities of five map bosses of the Rio das Doze Horas and the Margens da Mistura (Docs/Mapas100.md 72, 75, 78, 80, 81;
    /// <see cref="RegionBossSignature"/> 90–94): what each shows when it warns and what it does when the warning runs out. Rules in RegionBossRules.RiverBosses.cs.
    /// </summary>
    public sealed partial class RegionBossAbilities
    {
        // The hero's distance when the hippo went under, the lake's flares and the lobe that flared, the instant the hero stood in when it last warned
        // and the one given back now, the mantles on the ground and how many were thrown, Ullikummi's slams and the growth of this one.
        private float _hippoRippleAt;
        private int _lakeFlares;
        private int _lakeFlare;
        private bool _hasLostInstant;
        private Vector2 _lostInstant;
        private Vector2 _instantSpot;
        private readonly List<Vector2> _hobabishMantles = new List<Vector2>();
        private int _hobabishThrown;
        private int _stoneSlams;
        private int _stoneGrowth;
        private bool _stoneGrowthAtNight;

        /// <summary>Re-armed from the pool: every count starts over, the mantles come back on Hobabish and the lost instant is forgotten.</summary>
        private void ResetRiverMapBosses()
        {
            _lakeFlares = 0;
            _hasLostInstant = false;
            _hobabishMantles.Clear();
            _hobabishThrown = 0;
            _stoneSlams = 0;
        }

        private static bool IsRiverMapBossSignature(RegionBossSignature signature) =>
            signature >= RegionBossSignature.RippleRing && signature <= RegionBossSignature.GrowingStone;

        /// <summary>The warning of one of the five: what it marks on the ground, and the line it calls out (left as it is when it has no line of its own).</summary>
        private void WarnRiverMapBoss(RegionBossSignature signature, ref string warning)
        {
            if (!IsRiverMapBossSignature(signature))
            {
                return;
            }

            Vector2 origin = Flat(_signatureOrigin);
            switch (signature)
            {
                case RegionBossSignature.RippleRing:
                {
                    // The ripple will roll out to where the hero stands now: a ring of marks at that distance round him.
                    _hippoRippleAt = FlatDistance(_signatureImpact, _signatureOrigin);
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI * 0.25f;
                        Vector2 spot = origin + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * _hippoRippleAt;
                        DungeonShockwave.Spawn(new Vector3(spot.x, _signatureOrigin.y, spot.y), null, _controller.gameObject, 0f, 0f);
                    }

                    break;
                }

                case RegionBossSignature.LakeShadow:
                {
                    // The lobe that flares, and three marks down the middle of the shadow he will cast.
                    _lakeFlare = RegionBossRules.LakeFlareOf(_lakeFlares++);
                    warning = RegionBossTexts.LakeFlareKey(_lakeFlare);
                    Vector2 shadow = -RegionBossRules.LakeFlareDirection(_lakeFlare);
                    for (int i = 1; i <= 3; i++)
                    {
                        Vector2 spot = origin + shadow * (RegionBossRules.LakeShadowLength * i / 3.5f);
                        Vector3 at = new Vector3(spot.x, _signatureOrigin.y, spot.y);
                        DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                        if (i == 3)
                        {
                            DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.LakeShadowKey));
                        }
                    }

                    break;
                }

                case RegionBossSignature.LostInstant:
                {
                    // The instant it stole when it last warned (the first time: this one), given back as the only safe place; this one it steals now.
                    Vector2 last = _hasLostInstant ? _lostInstant : Flat(_signatureImpact);
                    _instantSpot = RegionBossRules.InstantSpot(origin, last);
                    _lostInstant = Flat(_signatureImpact);
                    _hasLostInstant = true;
                    Vector3 at = new Vector3(_instantSpot.x, _signatureOrigin.y, _instantSpot.y);
                    DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.InstantSpotKey));
                    break;
                }

                case RegionBossSignature.SevenMantles:
                {
                    // A new mantle on the hero's spot; the ones already down light up again with it.
                    int mantle = RegionBossRules.ThrowMantle(_hobabishMantles, Flat(_signatureImpact), _hobabishThrown++);
                    warning = RegionBossTexts.MantleKey(mantle);
                    foreach (Vector2 spot in _hobabishMantles)
                    {
                        DungeonShockwave.Spawn(new Vector3(spot.x, _signatureImpact.y, spot.y), null, _controller.gameObject, 0f, 0f);
                    }

                    break;
                }

                case RegionBossSignature.GrowingStone:
                {
                    // How far this slam reaches: four marks on its edge.
                    _stoneGrowth = RegionBossRules.GrowthOf(_stoneSlams++);
                    _stoneGrowthAtNight = IsNight();
                    warning = RegionBossTexts.GrowthKey(_stoneGrowth);
                    float reach = RegionBossRules.GrowthReach(_stoneGrowth, _stoneGrowthAtNight);
                    for (int i = 0; i < 4; i++)
                    {
                        float a = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                        Vector2 spot = origin + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * reach;
                        DungeonShockwave.Spawn(new Vector3(spot.x, _signatureOrigin.y, spot.y), null, _controller.gameObject, 0f, 0f);
                    }

                    break;
                }
            }
        }

        /// <summary>When the warning of one of the five runs out: false for every other signature, which the main switch resolves.</summary>
        private bool ResolveRiverMapBoss(Vector2 heroFlat, ICombatTarget victim)
        {
            RegionBossSignature signature = _spawn.Signature;
            if (!IsRiverMapBossSignature(signature))
            {
                return false;
            }

            Vector2 origin = Flat(_signatureOrigin);
            switch (signature)
            {
                case RegionBossSignature.RippleRing:
                    DungeonShockwave.Spawn(_signatureOrigin, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.RippleHits(origin, _hippoRippleAt, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.RippleDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.LakeShadow:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.LakeShadowHits(_lakeFlare, Flat(transform.position), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.LakeShadowDamageFactor), ElementType.Fire, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.LostInstant:
                    DungeonShockwave.Spawn(_signatureOrigin, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.InstantHits(origin, _instantSpot, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.InstantDamageFactor), ElementType.Arcane, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.SevenMantles:
                    foreach (Vector2 spot in _hobabishMantles)
                    {
                        DungeonShockwave.Spawn(new Vector3(spot.x, transform.position.y, spot.y), null, _controller.gameObject, 0f, 0f);
                    }

                    if (RegionBossRules.MantlesHit(_hobabishMantles, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.MantleDamageFactor), ElementType.Arcane, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.GrowingStone:
                    DungeonShockwave.Spawn(_signatureOrigin, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.GrowthHits(_stoneGrowth, _stoneGrowthAtNight, origin, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.GrowthDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;
            }

            return true;
        }
    }
}
