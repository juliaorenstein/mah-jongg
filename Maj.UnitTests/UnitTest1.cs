namespace Maj.UnitTests;

[TestFixture]
public class HandLogicTests
{
    [SetUp]
    public void Setup() {
        Assert.AreEqual(1, 1);
    }

    [Test]
    public void IsValidHand_ValidExactHand_ReturnsTrue() { }

    [Test]
    public void IsValidHand_ExactHandWithWrongTiles_ReturnsFalse() { }

    [Test]
    public void IsValidHand_ValidExactHandWithJokers_ReturnsTrue() { }

    [Test]
    public void IsValidHand_ExactHandWithJokersInPairs_ReturnsFalse() { }

    [Test]
    public void IsValidHand_ValidConsecutiveHandWithExactValues_ReturnsTrue() { }

    [Test]
    public void IsValidHand_ValidConsecutiveHandWithDifferentValues_ReturnsTrue() { }

    [Test]
    public void IsValidHand_ValidConsecutiveHandWithDifferentSuits_ReturnsTrue() { }

    [Test]
    public void IsValidHand_ConsecutiveHandWithWrongTiles_ReturnsFalse() { }

    [Test]
    public void IsValidHand_ValidConsecutiveHandWithJokers_ReturnsTrue() { }

    [Test]
    public void IsValidHand_ConsecutiveHandWithJokersInPairs_ReturnsFalse() { }


}
