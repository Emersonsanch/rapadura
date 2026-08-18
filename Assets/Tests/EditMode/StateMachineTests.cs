using NUnit.Framework;
using Rapadura.Core.StateMachine;

namespace Rapadura.Tests
{
    /// <summary>Plain C# tests for the generic StateMachine — no MonoBehaviour/scene required.</summary>
    public class StateMachineTests
    {
        private class DummyContext
        {
        }

        private class RecordingState : IState<DummyContext>
        {
            public int EnterCount;
            public int ExitCount;
            public int TickCount;

            public void Enter(DummyContext context) => EnterCount++;
            public void Tick(DummyContext context, float deltaTime) => TickCount++;
            public void FixedTick(DummyContext context, float fixedDeltaTime) { }
            public void Exit(DummyContext context) => ExitCount++;
        }

        private class OtherState : IState<DummyContext>
        {
            public void Enter(DummyContext context) { }
            public void Tick(DummyContext context, float deltaTime) { }
            public void FixedTick(DummyContext context, float fixedDeltaTime) { }
            public void Exit(DummyContext context) { }
        }

        [Test]
        public void ChangeState_CallsEnter_OnFirstTransition()
        {
            var machine = new StateMachine<DummyContext>(new DummyContext());
            var state = new RecordingState();
            machine.RegisterState(state);

            machine.ChangeState<RecordingState>();

            Assert.AreEqual(1, state.EnterCount);
            Assert.AreEqual(0, state.ExitCount);
        }

        [Test]
        public void ChangeState_ToSameState_DoesNotReenter()
        {
            var machine = new StateMachine<DummyContext>(new DummyContext());
            var state = new RecordingState();
            machine.RegisterState(state);

            machine.ChangeState<RecordingState>();
            machine.ChangeState<RecordingState>();

            Assert.AreEqual(1, state.EnterCount);
        }

        [Test]
        public void ChangeState_ExitsPreviousState()
        {
            var machine = new StateMachine<DummyContext>(new DummyContext());
            var stateA = new RecordingState();
            var stateB = new OtherState();
            machine.RegisterState(stateA);
            machine.RegisterState(stateB);

            machine.ChangeState<RecordingState>();
            machine.ChangeState<OtherState>();

            Assert.AreEqual(1, stateA.ExitCount);
        }

        [Test]
        public void ChangeState_ToUnregisteredState_Throws()
        {
            var machine = new StateMachine<DummyContext>(new DummyContext());

            Assert.Throws<System.InvalidOperationException>(() => machine.ChangeState<RecordingState>());
        }

        [Test]
        public void Tick_ForwardsToCurrentState()
        {
            var machine = new StateMachine<DummyContext>(new DummyContext());
            var state = new RecordingState();
            machine.RegisterState(state);
            machine.ChangeState<RecordingState>();

            machine.Tick(0.016f);

            Assert.AreEqual(1, state.TickCount);
        }

        [Test]
        public void IsInState_ReflectsCurrentState()
        {
            var machine = new StateMachine<DummyContext>(new DummyContext());
            machine.RegisterState(new RecordingState());

            machine.ChangeState<RecordingState>();

            Assert.IsTrue(machine.IsInState<RecordingState>());
            Assert.IsFalse(machine.IsInState<OtherState>());
        }
    }
}
