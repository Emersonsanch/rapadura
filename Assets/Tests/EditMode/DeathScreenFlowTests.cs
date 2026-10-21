using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World;
using Rapadura.UI.Death;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Death screen flow: the pure <see cref="DeathScreenFlow"/> state
    /// machine (when respawn is allowed to happen) and <see cref="CheckpointManager"/>'s
    /// <see cref="CheckpointManager.DeferRespawn"/> contract (instant respawn stays the default;
    /// deferred respawn only happens on an explicit <see cref="CheckpointManager.RespawnPlayer"/>).
    /// </summary>
    public class DeathScreenFlowTests
    {
        // ---------------------------------------------------------------
        // DeathScreenFlow
        // ---------------------------------------------------------------

        [Test]
        public void Begin_StartsFadingIn_AndRejectsInputUntilFadeEnds()
        {
            var flow = new DeathScreenFlow(1f, 0f);

            Assert.IsTrue(flow.Begin());
            Assert.AreEqual(DeathScreenState.FadingIn, flow.State);
            Assert.IsFalse(flow.TryContinue(), "Continue must be ignored while fading in.");
            Assert.IsFalse(flow.TryOpenLoadMenu());

            flow.Tick(0.5f);
            Assert.AreEqual(0.5f, flow.FadeAlpha, 0.0001f);

            flow.Tick(0.6f);
            Assert.AreEqual(DeathScreenState.AwaitingChoice, flow.State);
            Assert.AreEqual(1f, flow.FadeAlpha);
            Assert.IsTrue(flow.TryContinue());
            Assert.AreEqual(DeathScreenState.Hidden, flow.State);
        }

        [Test]
        public void Begin_WhileActive_IsIgnored()
        {
            var flow = new DeathScreenFlow(1f, 0f);
            flow.Begin();
            flow.Tick(0.5f);

            Assert.IsFalse(flow.Begin());
            Assert.AreEqual(0.5f, flow.FadeAlpha, 0.0001f, "A duplicate death event must not restart the fade.");
        }

        [Test]
        public void ZeroFadeDuration_GoesStraightToAwaitingChoice()
        {
            var flow = new DeathScreenFlow(0f, 0f);
            flow.Begin();

            Assert.IsTrue(flow.CanChoose);
            Assert.AreEqual(1f, flow.FadeAlpha);
        }

        [Test]
        public void Tick_AutoContinueTimeout_ReturnsTrueOnceAndHides()
        {
            var flow = new DeathScreenFlow(0.5f, 2f);
            flow.Begin();

            Assert.IsFalse(flow.Tick(0.5f), "Fade completion is not a respawn.");
            Assert.AreEqual(2f, flow.AutoContinueRemaining, 0.0001f);
            Assert.IsFalse(flow.Tick(1.5f));
            Assert.IsTrue(flow.Tick(0.6f), "Timeout should request the respawn.");
            Assert.AreEqual(DeathScreenState.Hidden, flow.State);
            Assert.IsFalse(flow.Tick(10f), "Once hidden, ticking never requests another respawn.");
        }

        [Test]
        public void Tick_NoTimeoutConfigured_WaitsForever()
        {
            var flow = new DeathScreenFlow(0f, 0f);
            flow.Begin();

            Assert.IsFalse(flow.Tick(1000f));
            Assert.AreEqual(DeathScreenState.AwaitingChoice, flow.State);
            Assert.AreEqual(-1f, flow.AutoContinueRemaining);
        }

        [Test]
        public void LoadMenuOpen_PausesTimeout_AndClosingRestartsIt()
        {
            var flow = new DeathScreenFlow(0f, 2f);
            flow.Begin();
            flow.Tick(1.9f);

            Assert.IsTrue(flow.TryOpenLoadMenu());
            Assert.IsFalse(flow.Tick(100f), "Timeout must not fire while the Load menu is open.");
            Assert.IsFalse(flow.TryContinue(), "Continue is not reachable while the Load menu is open.");

            flow.CloseLoadMenu();
            Assert.AreEqual(DeathScreenState.AwaitingChoice, flow.State);
            Assert.AreEqual(2f, flow.AutoContinueRemaining, 0.0001f);
        }

        [Test]
        public void CompleteViaLoad_EndsActiveFlow_ButIsNoOpWhenHidden()
        {
            var flow = new DeathScreenFlow(0f, 0f);

            Assert.IsFalse(flow.CompleteViaLoad(), "Loading from the Pause menu (no death) must not be treated as a death-screen completion.");

            flow.Begin();
            flow.TryOpenLoadMenu();

            Assert.IsTrue(flow.CompleteViaLoad());
            Assert.AreEqual(DeathScreenState.Hidden, flow.State);
            Assert.AreEqual(0f, flow.FadeAlpha);
        }

        // ---------------------------------------------------------------
        // CheckpointManager.DeferRespawn
        // ---------------------------------------------------------------

        private GameObject _playerGo;
        private PlayerStats _playerStats;
        private GameObject _spawnGo;
        private SpawnPoint _spawnPoint;
        private CheckpointManager _checkpointManager;

        private void SetUpCheckpointScene()
        {
            EventBus.Clear();
            ServiceLocator.Clear();

            _checkpointManager = new CheckpointManager();
            _checkpointManager.Initialize();

            _playerGo = new GameObject("DeathFlowTestPlayer") { tag = "Player" };
            _playerStats = _playerGo.AddComponent<PlayerStats>();

            _spawnGo = new GameObject("DeathFlowTestSpawn");
            _spawnGo.transform.position = new Vector3(10f, 0f, -5f);
            _spawnPoint = _spawnGo.AddComponent<SpawnPoint>();
            _checkpointManager.SetInitialCheckpoint(_spawnPoint);
        }

        [TearDown]
        public void TearDown()
        {
            if (_checkpointManager != null)
            {
                _checkpointManager.Shutdown();
                _checkpointManager = null;
            }

            if (_playerGo != null) Object.DestroyImmediate(_playerGo);
            if (_spawnGo != null) Object.DestroyImmediate(_spawnGo);

            EventBus.Clear();
            ServiceLocator.Clear();
        }

        [Test]
        public void PlayerDied_DefaultContract_RespawnsInstantly()
        {
            SetUpCheckpointScene();

            _playerStats.ApplyDamage(100000f);

            Assert.IsFalse(_playerStats.IsDead, "Without a Death screen, death still respawns instantly (original behaviour).");
            Assert.AreEqual(_spawnPoint.Position, _playerGo.transform.position);
        }

        [Test]
        public void PlayerDied_WithDeferRespawn_StaysDeadUntilRespawnPlayerIsCalled()
        {
            SetUpCheckpointScene();
            _checkpointManager.DeferRespawn = true;

            _playerStats.ApplyDamage(100000f);

            Assert.IsTrue(_playerStats.IsDead, "Deferred: the Death screen owns when respawn happens.");
            Assert.AreEqual(Vector3.zero, _playerGo.transform.position);

            _checkpointManager.RespawnPlayer();

            Assert.IsFalse(_playerStats.IsDead);
            Assert.AreEqual(_playerStats.MaxHealth, _playerStats.CurrentHealth);
            Assert.AreEqual(_spawnPoint.Position, _playerGo.transform.position);
        }
    }
}
