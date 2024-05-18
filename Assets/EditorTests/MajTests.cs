using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MajTests
{

    const string cardHandStrConsec = "FF 11111b 22b 33333b consec";

    [TestCase(cardHandStrConsec, "F F 1d 1d 1d 1d 1d 2d 2d 3d 3d 3d 3d 3d")] // exact match to card hand
    [TestCase(cardHandStrConsec, "F F 3c 3c 3c 3c 3c 4c 4c 5c 5c 5c 5c 5c")] // different numbers and suits
    [TestCase(cardHandStrConsec, "F F 1d J J 1d 1d 2d 2d 3d J 3d 3d 3d")] // jokers in non-pairs
    [TestCase(cardHandStrConsec, "F F J J J J J 2d 2d 3d 3d 3d 3d 3d")] // jokers for whole group
    [TestCase(cardHandStrConsec, "1d F 1d 1d F 1d 1d 2d 3d 3d 3d 2d 3d 3d")] // tiles out of order

    public void PlayerHandSatisfiesCardHand_ValidHand_ReturnsTrue(string cardHandStr, string playerHandStr) {
        // ARRANGE
        CardHand cardHand = new(cardHandStr);
        PlayerHand playerHand = new(playerHandStr);

        // ACT
        bool result = HandLogic.PlayerHandSatisfiesCardHand(cardHand, playerHand);

        // ASSERT
        Assert.True(result);
    }

    [TestCase(cardHandStrConsec, "N S 1d 2d 3c 1d 1d 2d 2d 3d 3d 3d 3d 3d")] // wrong tiles
    [TestCase(cardHandStrConsec, "J F 1d 1d 1d 1d 1d 2d 2d 3d 3d 3d 3d 3d")] // jokers in pairs
    [TestCase(cardHandStrConsec, "F F 1d 1d 1d 1d 1d 2c 2c 3d 3d 3d 3d 3d")] // suits don't line up across groups
    [TestCase(cardHandStrConsec, "F F F 1d 1d 1d 1d 1d 2d 2d 3d 3d 3d 3d 3d")] // too many tiles

    public void PlayerHandSatisfiesCardHand_InvalidHand_ReturnsFalse(string cardHandStr, string playerHandStr)
    {
        // ARRANGE
        CardHand cardHand = new(cardHandStr);
        PlayerHand playerHand = new(playerHandStr);

        // ACT
        bool result = HandLogic.PlayerHandSatisfiesCardHand(cardHand, playerHand);

        // ASSERT
        Assert.False(result);
    }
}
