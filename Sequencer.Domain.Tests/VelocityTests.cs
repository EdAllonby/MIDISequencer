using NUnit.Framework;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class VelocityTests
    {
        [Test]
        public void AddingVelocityOver127Test()
        {
            var velocity = new Velocity(20);
            Velocity newVelocity = velocity + 200;

            Assert.AreEqual(1, newVelocity.Volume);
        }

        [Test]
        public void AddingVelocityValueTest()
        {
            var velocity = new Velocity(20);
            Velocity newVelocity = velocity + 10;

            Assert.That(0.23, Is.EqualTo(newVelocity.Volume).Within(0.01));
        }

        [Test]
        public void HalfVelocityVolumeIsPoint5()
        {
            var velocity = new Velocity(127 / 2);
            Assert.That(0.5, Is.EqualTo(velocity.Volume).Within(0.01));
        }

        [Test]
        public void NullVelocity_NotEqual()
        {
            var velocity = new Velocity(127 / 2);

            Assert.IsFalse(velocity.Equals(null));
        }

        [Test]
        public void NullVelocityObject_IsNotEqual()
        {
            var velocity = new Velocity(127 / 2);

            Assert.IsFalse(velocity.Equals((object) null));
        }

        [Test]
        public void SubtractingVelocityBelow0Test()
        {
            var velocity = new Velocity(20);
            Velocity newVelocity = velocity - 200;

            Assert.AreEqual(0, newVelocity.Volume);
        }

        [Test]
        public void SubtractingVelocityValueTest()
        {
            var velocity = new Velocity(20);
            Velocity newVelocity = velocity - 10;

            Assert.That(0.078, Is.EqualTo(newVelocity.Volume).Within(0.01));
        }

        [Test]
        public void TwoEqualVelocities_AreEqual()
        {
            var velocity = new Velocity(127 / 2);
            var velocity2 = new Velocity(127 / 2);

            Assert.AreEqual(velocity, velocity2);
            Assert.AreEqual(velocity.GetHashCode(), velocity2.GetHashCode());
        }

        [Test]
        public void TwoNotEqualVelocities_AreNotEqual()
        {
            var velocity = new Velocity(127 / 2);
            var velocity2 = new Velocity(127 / 3);

            Assert.AreNotEqual(velocity, velocity2);
            Assert.AreNotEqual(velocity.GetHashCode(), velocity2.GetHashCode());
        }

        [Test]
        public void TwoReferenceEqualVelocities_AreEqual()
        {
            var velocity = new Velocity(127 / 2);

            Assert.IsTrue(velocity.Equals(velocity));
        }

        [Test]
        public void TwoReferenceEqualVelocityObjects_AreEqual()
        {
            var velocity = new Velocity(127 / 2);

            Assert.IsTrue(velocity.Equals((object) velocity));
        }

        [Test]
        public void VelocityIsClampedAt0()
        {
            var velocity = new Velocity(-10);
            Assert.AreEqual(0, velocity.Volume);
        }

        [Test]
        public void VelocityIsClampedAt1()
        {
            var velocity = new Velocity(1000);
            Assert.AreEqual(1, velocity.Volume);
        }

        [Test]
        public void VelocityToString_ShowsInternalValue()
        {
            const int value = 63;
            var velocity = new Velocity(value);
            Assert.AreEqual(value.ToString(), velocity.ToString());
        }
    }
}