using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct CardGroup
{
    public int length;
    public Kind kind;
    public Col? col;
    public int? relVal;
    public Direction? dir;
    bool colIsSuit;

    public CardGroup(int l, Kind k, Col? c = null, int? v = null, Direction? d = null, bool cs = false)
    {
        length = l;
        kind = k;
        col = c;
        relVal = v;
        dir = d;
        colIsSuit = cs; // used when greens reds and 0s are specified on the card
    }
}

public struct CardHand
{
    public Pattern pattern;
    public List<CardGroup> groups;

    public CardHand(List<CardGroup> g, Pattern p)
    {
        groups = g;
        pattern = p;
    }

    public CardHand(string cardHandStr)
    {
        groups = new();
        string[] groupStrArr = cardHandStr.Split(" ");
        string patternStr = groupStrArr.Last();
        List<string> groupStrList = groupStrArr.Take(groupStrArr.Length - 1).ToList();

        // deal with each group
        foreach (string groupStr in groupStrList)
        {
            char first = groupStr[0];
            char last = groupStr[^1];
            CardGroup group = first switch
            {
                'F' => new CardGroup(groupStr.Length, Kind.flowerwind, null, null, Direction.flower),
                'N' => new CardGroup(groupStr.Length, Kind.flowerwind, null, null, Direction.north),
                'E' => new CardGroup(groupStr.Length, Kind.flowerwind, null, null, Direction.east),
                'W' => new CardGroup(groupStr.Length, Kind.flowerwind, null, null, Direction.west),
                'S' => new CardGroup(groupStr.Length, Kind.flowerwind, null, null, Direction.south),
                'G' => new CardGroup(groupStr.Length, Kind.dragon, Col.green, null, null, true),
                'R' => new CardGroup(groupStr.Length, Kind.dragon, Col.red, null, null, true),
                '0' => new CardGroup(groupStr.Length, Kind.dragon, Col.blue, null, null, true),
                _ => new CardGroup(groupStr.Length - 1, Kind.number, GetColorFromNumberStr(groupStr), first),
            };

            groups.Add(group);
        }

        static Col GetColorFromNumberStr(string groupStr)
        {
            Col color = groupStr[^1] switch
            {
                'b' => Col.blue,
                'g' => Col.green,
                'r' => Col.red,
                _ => throw new Exception("invalid color"),
            };

            return color;
        }

        // deal with pattern
        pattern = patternStr switch
        {
            "exact" => Pattern.exact,
            "like" => Pattern.like,
            "likeEven" => Pattern.likeEven,
            "likeOdd" => Pattern.likeOdd,
            "consecutive" => Pattern.consecutive,
            _ => Pattern.exact,
        };
    }
}

public struct PermutationGroup
{
    public int length;
    public Kind kind;
    public Suit? suit;
    public int? actVal;
    public Direction? dir;

    public PermutationGroup(int l, Kind k, Suit? s = null, int? v = null, Direction? d = null)
    {
        length = l;
        kind = k;
        suit = s;
        actVal = v;
        dir = d;
    }

    public readonly bool AllowJokers()
    {
        return length > 2;
    }
}

public struct Permutation
{
    public List<PermutationGroup> groups;
}

public struct PlayerHand
{
    public List<Tile> tiles;

    public PlayerHand(string playerHandStr)
    {
        tiles = new();

        foreach (string tileStr in playerHandStr.Split(" "))
        {
            Tile tile = new();
            switch (tileStr) {
                case "F":
                    tiles.Add(tile.InitTile(Direction.flower));
                    break;
                case "N":
                    tiles.Add(tile.InitTile(Direction.north));
                    break;
                case "E":
                    tiles.Add(tile.InitTile(Direction.east));
                    break;
                case "W":
                    tiles.Add(tile.InitTile(Direction.west));
                    break;
                case "S":
                    tiles.Add(tile.InitTile(Direction.south));
                    break;
                case "J":
                    tiles.Add(tile.InitTile());
                    break;
                case "R":
                    tiles.Add(tile.InitTile(0, Suit.crak));
                    break;
                case "G":
                    tiles.Add(tile.InitTile(0, Suit.bam));
                    break;
                case "0":
                    tiles.Add(tile.InitTile(0, Suit.dot));
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
                switch(tileSuitChar)
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

                tile.InitTile(tileVal, tileSuit);
            }
            
        }
    }
}

public class HandLogic : MonoBehaviour
{
    // for each hand:
    //
    // determine all permutations of hand (different suits & numbers but not
    // specific tile IDs or jokers)
    //
    // for each permutation:
    // assign tiles from hand in order until there isn't a tile/joker available
    // to satisfy a requirement

    // example: Quint: FF 11111 22 33333


    public static bool PlayerHandSatisfiesCardHand(CardHand cardHand, PlayerHand playerHand)
    {
        return false;
    }

    // given a card hand, find suit and number permutations
    List<Permutation> Permutations(CardHand cardHand)
    {
        // TODO: unit tests. for quint example, we should see:

        // original: FF 11111 22 33333
        // permutations:
        //  FF 11111 (bam)  22 (bam)    33333 (bam)
        //  FF 11111 (crak) 22 (crak)   33333 (crak)
        //  FF 11111 (dot)  22 (dot)    33333 (dot)
        //  FF 22222 (bam)  33 (bam)    44444 (bam)
        //  FF 22222 (crak) 33 (crak)   44444 (crak)
        //  FF 22222 (dot)  33 (dot)    44444 (dot)
        //  FF 33333 (bam)  44 (bam)    55555 (bam)
        //  FF 33333 (crak) 44 (crak)   55555 (crak)
        //  FF 33333 (dot)  44 (dot)    55555 (dot)
        // etc...

        // if there are suits in the hand, do suit permutations
        // TODO: add suit check
        for (int suitPerm = 0; suitPerm < 6; suitPerm++)
        {
            // if this hand is consecutive or like, then do val permutations
            if (cardHand.pattern == Pattern.consecutive ||
                cardHand.pattern == Pattern.like)
            {
                for (int valPerm = 0; valPerm < 9; valPerm++)
                {
                    CalculatePermutation(cardHand, suitPerm, valPerm);
                }
            }
            // if the hand is likeEven or likeOdd, then increment vals by two
            else if (cardHand.pattern == Pattern.likeEven ||
                cardHand.pattern == Pattern.likeOdd)
            {
                for (int valPerm = 0; valPerm < 9; valPerm += 2)
                {
                    CalculatePermutation(cardHand, suitPerm, valPerm);
                }
            }
            // else, use 0 as the val permutation to use the literal values
            else CalculatePermutation(cardHand, suitPerm, 0);
        }

        return new List<Permutation>();
    }


    // based on a card hand and values for the suit and value permutation
    // indices, returns an exact form of the hand.
    Permutation? CalculatePermutation(CardHand cardHand, int suitPerm, int valPerm)
    {
        Permutation perm = new() { groups = new() };

        foreach (CardGroup cg in cardHand.groups)
        {
            PermutationGroup pg = new();
            // length, kind, direction will always be the same
            pg.length = cg.length;
            pg.kind = cg.kind;
            pg.dir = cg.dir;

            // the color needs to be translated to the suit
            if (cg.col != null) pg.suit = colToSuit((Col)cg.col, suitPerm);

            // the value needs to be translated to the actual value
            if (cg.relVal != null)
            {
                pg.actVal = relValToActVal((int)cg.col, valPerm);
                if (pg.actVal == 0) return null;
                // this check is to see if we're running off the end of the
                // possible numbers (i.e. 10 crak doesn't exist)
                // and returns null for this permutation if so.
            }
            // else, the value is literal
            else pg.actVal = cg.relVal;

            perm.groups.Add(pg);
        }

        return perm;
    }

    // helper function to turn a color into a suit depending on iteration
    Suit colToSuit(Col color, int i)
    {
        // permutate suits

        // suit = (color + i) % 3:
        // i=0: 0-0, 1-1, 2-2 - 012
        // i=1: 0-1, 1-2, 2-0 - 120
        // i=2: 0-2, 1-0, 2-1 - 201

        // suit = (i - color) % 3:
        // i=3: 0-0, 1-2, 2-1 - 021
        // i=4: 0-1, 1-0, 2-2 - 102
        // i=5: 0-2, 1-1, 2-0 - 210

        if (i < 3) return (Suit)((i + (int)color) % 3);
        else return (Suit)((i - (int)color) % 3);

        // TODO: unit tests should should the results shown above
    }

    // helper function to turn a relative number value into an absolute value
    int relValToActVal(int relVal, int i)
    {
        int absVal = relVal + i;
        if (absVal > 9) return 0; // if absVal is invalid return 0
        return absVal;

        // TODO: unit tests
    }
}
