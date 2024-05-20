using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using System.Linq;

public class MajTests
{
    const string cardHandStrConsec = "FF 11111b 22b 33333b consec";

    [Test]
    public void CardHand_NumericSingleColorHand_VerifyConstructorOutput()
    {
        // ARRANGE
        List<CardGroup> cardGroup = new() {
            new(2, Kind.flowerwind, null, null, Direction.flower),
            new(5, Kind.number, Col.blue, 1),
            new(2, Kind.number, Col.blue, 2),
            new(5, Kind.number, Col.blue, 3)
        };
        Pattern pattern = Pattern.consecutive;

        CardHand expected = new(cardGroup, pattern);
        
        // ACT
        CardHand cardHand = new("FF 11111b 22b 33333b consec");

        // ASSERT
        Assert.AreEqual(expected, cardHand);
    }

    [Test]
    public void CardHand_DragonWindHand_VerifyConstructorOutput()
    {
        // ARRANGE
        List<CardGroup> cardGroup = new() {
            new(2, Kind.flowerwind, null, null, Direction.flower),
            new(4, Kind.dragon, Col.green, 0),
            new(1, Kind.flowerwind, null, null, Direction.north),
            new(1, Kind.flowerwind, null, null, Direction.east),
            new(1, Kind.flowerwind, null, null, Direction.west),
            new(1, Kind.flowerwind, null, null, Direction.south),
            new(4, Kind.dragon, Col.red, 0)
        };
        Pattern pattern = Pattern.exact;

        CardHand expected = new(cardGroup, pattern);

        // ACT
        CardHand cardHand = new("FF DDDDg N E W S DDDDr exact");

        // ASSERT
        Assert.AreEqual(expected, cardHand);
    }

    [Test]
    public void CardHandToString_TranslateHandToString_VerifyOutput()
    {
        // ARRANGE
        string cardHandStr = "FF DDDDg N E W S DDDDr exact";
        CardHand cardHand = new(cardHandStr);

        // ACT
        string newCardHandStr = cardHand.ToString();

        // ASSERT
        Assert.AreEqual(cardHandStr, newCardHandStr);
    }

    [TestCase(cardHandStrConsec, 0, 0, "F F 1d 1d 1d 1d 1d 2d 2d 3d 3d 3d 3d 3d")]
    public void Permutation_UseConstructor_VerifyConstructorOutput(string cardHandStr, int suitPerm, int valPerm, string expectedResult)
    {
        // ARRANGE
        CardHand cardHand = new(cardHandStr);

        // ACT
        Permutation perm = new(cardHand, suitPerm, valPerm);
        string permStr = perm.ToString();

        // ASSERT
        Assert.AreEqual(permStr, expectedResult, permStr);
    }

    [Test]
    public void PermutationsForCardHand_WindHand_1Permutation()
    {
        // ARRANGE
        CardHand cardHand = new("NNNN EEE WWW SSSS");

        List<Permutation> expectedPermutations = new()
        { new Permutation(cardHand, 0, 0) };

        // ACT
        List<Permutation> actualPermutations = cardHand.PermutationsForCardHand();

        // ASSERT
        Assert.AreEqual(expectedPermutations, actualPermutations);
    }

    [Test]
    public void PermutationsForCardHand_DragonHand_6Permutations()
    {
        // ARRANGE
        CardHand cardHand = new("FFFF DDDg DDDDr DDDb");

        List<Permutation> expectedPermutations = new()
        {
            new Permutation(cardHand, 0, 0),
            new Permutation(cardHand, 1, 0),
            new Permutation(cardHand, 2, 0),
            new Permutation(cardHand, 3, 0),
            new Permutation(cardHand, 4, 0),
            new Permutation(cardHand, 5, 0),
        };

        // ACT
        List<Permutation> actualPermutations = cardHand.PermutationsForCardHand();

        // ASSERT
        for (int i = 0; i < actualPermutations.Count(); i++)
        {
            Assert.AreEqual(expectedPermutations[i], actualPermutations[i]);
        }
    }

    [Test]
    public void PermutationsForCardHand_LikeHand_54Permutations()
    {
        // ARRANGE
        CardHand cardHand = new("FFFF 111g 1111r 111b like");

        // ACT
        List<Permutation> actualPermutations = cardHand.PermutationsForCardHand();

        // ASSERT
        Assert.AreEqual(54, actualPermutations.Count);
    }

    [Test]
    public void PermutationsForCardHand_LikeOddHand_30Permutations()
    {
        // ARRANGE
        CardHand cardHand = new("FFFF 111g 1111r 111b likeOdd");

        // ACT
        List<Permutation> actualPermutations = cardHand.PermutationsForCardHand();

        // ASSERT
        Assert.AreEqual(30, actualPermutations.Count);
    }

    [Test]
    public void PermutationsForCardHand_LikeEvenHand_24Permutations()
    {
        // ARRANGE
        CardHand cardHand = new("FFFF 222g 2222r 222b likeEven");

        // ACT
        List<Permutation> actualPermutations = cardHand.PermutationsForCardHand();

        // ASSERT
        Assert.AreEqual(24, actualPermutations.Count);
    }

    [Test]
    public void PermutationsForCardHand_Consec3Hand_42Permutations()
    {
        // ARRANGE
        CardHand cardHand = new("FF 1111g 2222r 3333b consec");

        // ACT
        List<Permutation> actualPermutations = cardHand.PermutationsForCardHand();

        // ASSERT
        Assert.AreEqual(42, actualPermutations.Count);
    }



    [TestCase(cardHandStrConsec, "F F 1d 1d 1d 1d 1d 2d 2d 3d 3d 3d 3d 3d")] // exact match to card hand
    [TestCase(cardHandStrConsec, "F F 3c 3c 3c 3c 3c 4c 4c 5c 5c 5c 5c 5c")] // different numbers and suits
    [TestCase(cardHandStrConsec, "F F 1d J J 1d 1d 2d 2d 3d J 3d 3d 3d")] // jokers in non-pairs
    [TestCase(cardHandStrConsec, "F F J J J J J 2d 2d 3d 3d 3d 3d 3d")] // jokers for whole group
    [TestCase(cardHandStrConsec, "1d F 1d 1d F 1d 1d 2d 3d 3d 3d 2d 3d 3d")] // tiles out of order
    [TestCase("2g 0 2g 4g NN E W SS 2r 0 2r 4r exact", "2b 0 2b 4b N N E W S S 2d 0 2d 4d")] // 2024 hand with repeated tiles in separate groups (soap)
    public void PlayerHandSatisfiesCardHand_ValidHand_ReturnsTrue(string cardHandStr, string playerHandStr) {
        // ARRANGE
        CardHand cardHand = new(cardHandStr);
        List<Tile> playerHand = PlayerHand(playerHandStr);

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
        List<Tile> playerHand = PlayerHand(playerHandStr);

        // ACT
        bool result = HandLogic.PlayerHandSatisfiesCardHand(cardHand, playerHand);

        // ASSERT
        Assert.False(result);
    }

    // create tiles for a player hand from a string for testing
    public List<Tile> PlayerHand(string playerHandStr)
    {
        GameObject TilePF = Resources.Load<GameObject>("Prefabs/Tile");
        List<Tile> tiles = new();

        foreach (string tileStr in playerHandStr.Split(" "))
        {
            switch (tileStr)
            {
                case "F":
                    tiles.Add(new(null, null, Direction.flower));
                    break;
                case "N":
                    tiles.Add(new(null, null, Direction.north));
                    break;
                case "E":
                    tiles.Add(new(null, null, Direction.east));
                    break;
                case "W":
                    tiles.Add(new(null, null, Direction.west));
                    break;
                case "S":
                    tiles.Add(new(null, null, Direction.south));
                    break;
                case "J":
                    tiles.Add(new());
                    break;
                case "R":
                    tiles.Add(new(0, Suit.crak));
                    break;
                case "G":
                    tiles.Add(new(0, Suit.bam));
                    break;
                case "0":
                    tiles.Add(new(0, Suit.dot));
                    break;
                default:
                    tiles.Add(NewTileForNumbers());
                    break;
            }

            Tile NewTileForNumbers()
            {
                int tileVal = tileStr[0] - '0';
                char tileSuitChar = tileStr[1];
                Suit tileSuit;
                switch (tileSuitChar)
                {
                    case 'b':
                        tileSuit = Suit.bam;
                        break;
                    case 'c':
                        tileSuit = Suit.crak;
                        break;
                    case 'd':
                        tileSuit = Suit.dot;
                        break;
                    default:
                        throw new Exception("Invalid tile suit");
                }

                return new(tileVal, tileSuit);
            }
        }
        return tiles;
    } 
}
