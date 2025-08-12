using NUnit.Framework;
using Godot;

namespace MyGame.Tests
{
    [TestFixture]
    public class PlayerTests
    {
        [Test]
        public void PlayerTakesDamage()
        {
            var player = new Entity()
            {
                maxHealthPoints = 100
            };
            player.TakeDamage(10, Element.Fire, false);
            Assert.AreEqual(90, player.healthPoints);
        }
    }
}
