using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>EditMode tests for <see cref="KnockbackReceiver"/>'s transform-based fallback path (no Rigidbody/CharacterController).</summary>
    public class KnockbackReceiverTests
    {
        private GameObject _go;
        private KnockbackReceiver _receiver;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("KnockbackTestTarget");
            _receiver = _go.AddComponent<KnockbackReceiver>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void ApplyKnockback_SetsNonZeroVelocity()
        {
            _receiver.ApplyKnockback(Vector3.forward, 5f);

            Assert.IsTrue(_receiver.IsBeingKnockedBack);
            Assert.Greater(_receiver.CurrentVelocity.magnitude, 0f);
        }

        [Test]
        public void ApplyKnockback_WithZeroForce_DoesNothing()
        {
            _receiver.ApplyKnockback(Vector3.forward, 0f);

            Assert.IsFalse(_receiver.IsBeingKnockedBack);
        }

        [Test]
        public void ApplyKnockback_NormalizesDirection()
        {
            _receiver.ApplyKnockback(new Vector3(0f, 0f, 10f), 3f);

            Assert.AreEqual(3f, _receiver.CurrentVelocity.magnitude, 0.001f);
        }
    }
}
