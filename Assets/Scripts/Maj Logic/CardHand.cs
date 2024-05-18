using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardHand
{
    /*
    public Pattern pattern;
    public List<CardGroup> groups;

    // constructor based on a list of CardGroups
    public CardHand(List<CardGroup> g, Pattern p)
    {
        groups = g;
        pattern = p;
    }

    // constructor based on a string representing a card hand
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

        // in a string like 1111g, extract the g for Col.green
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

        // deal with pattern (which should be the last "group" and defaults to exact if missing)
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

    // given a card hand, find suit and number permutations
    List<Permutation> Permutations()
    {
        
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
            if (pattern == Pattern.consecutive ||
                pattern == Pattern.like)
            {
                for (int valPerm = 0; valPerm < 9; valPerm++)
                {
                    CalculatePermutation(suitPerm, valPerm);
                }
            }
            // if the hand is likeEven or likeOdd, then increment vals by two
            else if (pattern == Pattern.likeEven ||
                pattern == Pattern.likeOdd)
            {
                for (int valPerm = 0; valPerm < 9; valPerm += 2)
                {
                    CalculatePermutation(suitPerm, valPerm);
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
            // create a tile that represents the permutation of the card group
            Tile tile = Instantiate(TilePF).GetComponent<Tile>();
            switch (cg.kind)
            {
                case Kind.flowerwind:
                    tile.InitTile((Direction)cg.dir, true);
                    break;
                case Kind.number:
                case Kind.dragon:
                    // valPerm will be 0 if it's not a consecutive hand so we don't need to check for that here
                    int actVal = RelValToActVal((int)cg.relVal, valPerm);
                    if (actVal == -1) return null;
                    tile.InitTile(actVal, ColToSuit((Col)cg.col, suitPerm), true);
                    break;
                default:
                    throw new Exception();
            }

            perm.groups.Add(new PermutationGroup(tile, cg.length));
        }

        return perm;

        // helper function to turn a color into a suit depending on iteration
        Suit ColToSuit(Col color, int i)
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
        int RelValToActVal(int relVal, int i)
        {
            if (relVal == 0) return 0; // for dragons

            int absVal = relVal + i;
            if (absVal > 9) return -1; // if absVal is invalid return -1
            return absVal;

            // TODO: unit tests
        }
    }

    bool PlayerHandSatisfiesPermutation(Permutation perm, List<Tile> playerHand)
    {
        List<Tile> unused = new(playerHand);
        List<Tile> used = new();



        foreach (PermutationGroup group in perm.groups)
        {
            List<Tile> found = unused.FindAll(tile => tile.Equals(group.tile));

            if (found.Count < group.length && !group.AllowJokers())
            {
                return false;
            }


        }





        return false;
    }

}

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
        relVal = v; // 0 for dragons
        dir = d;
        colIsSuit = cs; // used when greens reds and 0s are specified on the card
    }
    */
}
