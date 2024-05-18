using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class MajTests : MonoBehaviour
{
    /*
    const string cardHandStrConsec = "FF 11111b 22b 33333b consec";

    [TestCase(cardHandStrConsec, "F F 1d 1d 1d 1d 1d 2d 2d 3d 3d 3d 3d 3d")] // exact match to card hand
    [TestCase(cardHandStrConsec, "F F 3c 3c 3c 3c 3c 4c 4c 5c 5c 5c 5c 5c")] // different numbers and suits
    [TestCase(cardHandStrConsec, "F F 1d J J 1d 1d 2d 2d 3d J 3d 3d 3d")] // jokers in non-pairs
    [TestCase(cardHandStrConsec, "F F J J J J J 2d 2d 3d 3d 3d 3d 3d")] // jokers for whole group
    [TestCase(cardHandStrConsec, "1d F 1d 1d F 1d 1d 2d 3d 3d 3d 2d 3d 3d")] // tiles out of order
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
            Tile tile = Instantiate(TilePF).GetComponent<Tile>();
            switch (tileStr)
            {
                case "F":
                    tiles.Add(tile.InitTile(Direction.flower, true));
                    break;
                case "N":
                    tiles.Add(tile.InitTile(Direction.north, true));
                    break;
                case "E":
                    tiles.Add(tile.InitTile(Direction.east, true));
                    break;
                case "W":
                    tiles.Add(tile.InitTile(Direction.west, true));
                    break;
                case "S":
                    tiles.Add(tile.InitTile(Direction.south, true));
                    break;
                case "J":
                    tiles.Add(tile.InitTile(true));
                    break;
                case "R":
                    tiles.Add(tile.InitTile(0, Suit.crak, true));
                    break;
                case "G":
                    tiles.Add(tile.InitTile(0, Suit.bam, true));
                    break;
                case "0":
                    tiles.Add(tile.InitTile(0, Suit.dot, true));
                    break;
                default:
                    Numbers();
                    tiles.Add(tile);
                    break;
            }

            void Numbers()
            {
                int tileVal = tileStr[0];
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

                tile.InitTile(tileVal, tileSuit, true);
            }
        }
        return tiles;
    }
    */
}
