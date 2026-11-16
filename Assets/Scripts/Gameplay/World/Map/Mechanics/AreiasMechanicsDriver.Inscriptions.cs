using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// "O painel do aviso" of the Ruínas dos Artífices (08, <see cref="ArtificesInscriptionRules"/>), on the same driver as the sandstorm: a pale
    /// gold beam over each unread stela of the avenue of ushabtis; [E] at 3.5 m reads it (the line of the warning, XP the first time, a time
    /// mark in <see cref="WorldMapProgress"/>); the third read gives the clue of the seal, more XP and a line of the cousin.
    /// </summary>
    public sealed partial class AreiasMechanicsDriver
    {
        private const float StelaBeamRange = 90f;
        private const float StelaBeamHeight = 7f;
        private const float StelaHintCooldown = 20f;

        private static readonly Color StelaUnreadColor = new Color(1f, 0.86f, 0.5f, 0.75f);

        private readonly LineRenderer[] _stelaBeams = new LineRenderer[3];
        private PlayerController _controller;
        private float _nextStelaHint;
        private int _stelaHinted = -1;
        private bool _stelaeShown;

        private void TickInscriptions(Vector2 local)
        {
            Vector2[] stelae = OverworldLayouts.ArtificesInscriptionStelae;
            if (!OverworldLayouts.ArtificesRuinsBounds.Contains(local) || !ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                HideStelae();
                return;
            }

            ShowStelae(local, stelae, progress);
            int stela = ArtificesInscriptionRules.StelaAt(local, stelae);
            if (stela < 0 || !_stats.IsAlive)
            {
                _stelaHinted = -1;
                return;
            }

            _controller ??= _player.GetComponent<PlayerController>();
            bool read = progress.GetTimeMark(ArtificesInscriptionRules.MarkKey(stela)) > 0L;
            if (!read)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
                if (_stelaHinted != stela || Time.unscaledTime >= _nextStelaHint)
                {
                    _stelaHinted = stela;
                    _nextStelaHint = Time.unscaledTime + StelaHintCooldown;
                    SayInscription(ArtificesInscriptionTexts.Prompt);
                }
            }

            if (_controller == null || _controller.Input == null || !_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            ReadStela(stela, read, stelae.Length, progress);
        }

        private void ReadStela(int stela, bool readBefore, int total, WorldMapProgress progress)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayInscriptionText(ArtificesInscriptionTexts.Get(localization, ArtificesInscriptionTexts.StelaKey(stela)), 0f);
            if (readBefore)
            {
                SayInscriptionText(ArtificesInscriptionTexts.Get(localization, ArtificesInscriptionTexts.AlreadyRead), -0.5f);
                return;
            }

            int count = 0;
            for (int i = 0; i < total; i++)
            {
                count += progress.GetTimeMark(ArtificesInscriptionRules.MarkKey(i)) > 0L ? 1 : 0;
            }

            CharacterId? hero = MapRequirementHeroState.Capture(null).Hero;
            int xp = ArtificesInscriptionRules.ExperienceFor(count, total, hero == CharacterId.Icaro);
            progress.SetTimeMark(ArtificesInscriptionRules.MarkKey(stela), 1L);
            if (xp > 0)
            {
                _stats.AddExperience(xp);
            }

            if (ArtificesInscriptionRules.Completes(count, total))
            {
                SayInscriptionText(string.Format(ArtificesInscriptionTexts.Get(localization, ArtificesInscriptionTexts.Clue), xp), -0.5f);
                SayInscriptionText(ArtificesInscriptionTexts.Get(localization, ArtificesInscriptionTexts.LineFor(hero)), -1f);
                return;
            }

            SayInscriptionText(string.Format(ArtificesInscriptionTexts.Get(localization, ArtificesInscriptionTexts.Progress), count + 1, total, xp), -0.5f);
        }

        private void ShowStelae(Vector2 local, Vector2[] stelae, WorldMapProgress progress)
        {
            _stelaeShown = true;
            for (int i = 0; i < stelae.Length && i < _stelaBeams.Length; i++)
            {
                bool unread = progress.GetTimeMark(ArtificesInscriptionRules.MarkKey(i)) <= 0L;
                bool visible = unread && (stelae[i] - local).sqrMagnitude < StelaBeamRange * StelaBeamRange;
                LineRenderer line = _stelaBeams[i];
                if (!visible)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }

                    continue;
                }

                if (line == null)
                {
                    line = _stelaBeams[i] = NewLine("ArtificesStela_" + i, 0.35f, StelaUnreadColor);
                }

                Vector2 world = _origin + stelae[i];
                float y = OverworldRules.SurfaceAt(_layout, world);
                line.SetPosition(0, new Vector3(world.x, y, world.y));
                line.SetPosition(1, new Vector3(world.x, y + StelaBeamHeight, world.y));
                line.enabled = true;
            }
        }

        private void HideStelae()
        {
            if (!_stelaeShown)
            {
                return;
            }

            _stelaeShown = false;
            HideLines(_stelaBeams);
        }

        private void SayInscription(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayInscriptionText(ArtificesInscriptionTexts.Get(localization, key), 0f);
        }

        private void SayInscriptionText(string text, float drop)
        {
            if (_player != null)
            {
                Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + drop), text);
            }
        }
    }
}
