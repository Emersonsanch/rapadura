using NUnit.Framework;
using Rapadura.Core.Pooling;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the generic, reusable object pool (Fase 10 → Performance) — get/release/
    /// reuse/prewarm bookkeeping, using plain Transform components so no scene/prefab is required.
    /// </summary>
    public class GenericObjectPoolTests
    {
        private GameObject _root;
        private int _factoryCallCount;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("GenericObjectPoolTestsRoot");
            _factoryCallCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        private Transform Factory()
        {
            _factoryCallCount++;
            var go = new GameObject("PooledInstance_" + _factoryCallCount);
            go.transform.SetParent(_root.transform);
            return go.transform;
        }

        [Test]
        public void Get_WhenPoolEmpty_CreatesNewInstanceViaFactory()
        {
            var pool = new GenericObjectPool<Transform>(Factory);

            Transform instance = pool.Get();

            Assert.IsNotNull(instance);
            Assert.AreEqual(1, _factoryCallCount);
            Assert.AreEqual(1, pool.ActiveCount);
            Assert.AreEqual(0, pool.InactiveCount);
        }

        [Test]
        public void Release_MovesInstanceFromActiveToInactive()
        {
            var pool = new GenericObjectPool<Transform>(Factory);
            Transform instance = pool.Get();

            pool.Release(instance);

            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(1, pool.InactiveCount);
        }

        [Test]
        public void Get_AfterRelease_ReusesInstanceWithoutCallingFactoryAgain()
        {
            var pool = new GenericObjectPool<Transform>(Factory);
            Transform first = pool.Get();
            pool.Release(first);

            Transform second = pool.Get();

            Assert.AreSame(first, second);
            Assert.AreEqual(1, _factoryCallCount);
        }

        [Test]
        public void Prewarm_CreatesInstancesUpFrontWithoutActivatingThem()
        {
            var pool = new GenericObjectPool<Transform>(Factory);

            pool.Prewarm(3);

            Assert.AreEqual(3, _factoryCallCount);
            Assert.AreEqual(3, pool.InactiveCount);
            Assert.AreEqual(0, pool.ActiveCount);
        }

        [Test]
        public void Prewarm_ThenGet_ReusesPrewarmedInstances()
        {
            var pool = new GenericObjectPool<Transform>(Factory);
            pool.Prewarm(2);

            pool.Get();
            pool.Get();

            Assert.AreEqual(2, _factoryCallCount);
            Assert.AreEqual(0, pool.InactiveCount);
            Assert.AreEqual(2, pool.ActiveCount);
        }

        [Test]
        public void Release_UnknownOrNullInstance_IsNoOp()
        {
            var pool = new GenericObjectPool<Transform>(Factory);
            Transform stray = Factory();

            pool.Release(stray);
            pool.Release(null);

            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(0, pool.InactiveCount);
        }

        [Test]
        public void Release_Twice_OnlyEnqueuesOnce()
        {
            var pool = new GenericObjectPool<Transform>(Factory);
            Transform instance = pool.Get();

            pool.Release(instance);
            pool.Release(instance);

            Assert.AreEqual(1, pool.InactiveCount);
        }

        [Test]
        public void OnGetAndOnRelease_HooksAreInvoked()
        {
            int getCount = 0;
            int releaseCount = 0;
            var pool = new GenericObjectPool<Transform>(
                Factory,
                onGet: _ => getCount++,
                onRelease: _ => releaseCount++);

            Transform instance = pool.Get();
            pool.Release(instance);

            Assert.AreEqual(1, getCount);
            Assert.AreEqual(1, releaseCount);
        }

        [Test]
        public void ReleaseAll_ReturnsEveryActiveInstance()
        {
            var pool = new GenericObjectPool<Transform>(Factory);
            pool.Get();
            pool.Get();

            pool.ReleaseAll();

            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(2, pool.InactiveCount);
        }

        [Test]
        public void MaxSize_EvictsInsteadOfEnqueuingBeyondCap()
        {
            int destroyCount = 0;
            var pool = new GenericObjectPool<Transform>(Factory, onDestroy: _ => destroyCount++, maxSize: 1);
            Transform a = pool.Get();
            Transform b = pool.Get();

            pool.Release(a);
            pool.Release(b);

            Assert.AreEqual(1, pool.InactiveCount);
            Assert.AreEqual(1, destroyCount);
        }

        [Test]
        public void CountAll_ReflectsActivePlusInactive()
        {
            var pool = new GenericObjectPool<Transform>(Factory);
            pool.Prewarm(2);
            pool.Get();

            Assert.AreEqual(3, pool.CountAll);
        }
    }
}
