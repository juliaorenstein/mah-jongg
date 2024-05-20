using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

// Permutation is a specific example of a hand.
// For example, the CardHand FF 11111 22 33333 consecutive has the following
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

public class Permutation
{
    public List<PermutationGroup> groups;
    
    public Permutation(CardHand cardHand, int suitPerm, int valPerm)
    {
        // TODO: in one-suit hands we make 6 permutations instead of 3.

        groups = new();

        foreach (CardGroup cg in cardHand.groups)
        {
            Tile tile;
            switch (cg.kind)
            {
                case Kind.flowerwind:
                    tile = new(null, null, cg.dir);
                    break;
                case Kind.number:
                case Kind.dragon:
                    // valPerm will be 0 if it's not a consecutive hand so we don't need to check for that here
                    int actVal = RelValToActVal((int)cg.relVal, valPerm);
                    if (cg.colIsSuit) tile = new(actVal, ColToSuit((Col)cg.col, 0)); // deak with exact suits on card
                    else tile = new(actVal, ColToSuit((Col)cg.col, suitPerm));
                    break;
                default:
                    throw new Exception();
            }

            groups.Add(new PermutationGroup(tile, cg.length));
        }

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
        }

        // helper function to turn a relative number value into an absolute value
        int RelValToActVal(int relVal, int i)
        {
            if (relVal == 0) return 0; // for dragons
            int absVal = relVal + i; // otherwise add the val permutation
            if (absVal > 9) throw new ArgumentOutOfRangeException("relVal + i > 9");
            return absVal;
        }
    }

    public override string ToString()
    {
        StringBuilder str = new();

        foreach (PermutationGroup group in groups)
        {
            for (int i = 0; i < group.length; i++)
            {
                str.Append(TileToString(group));
                str.Append(" ");
            }
        }

        str.Remove(str.Length - 1, 1);

        return str.ToString();

        static string TileToString(PermutationGroup group)
        {
            if (group.tile.kind == Kind.flowerwind)
            {
                return group.tile.direction switch
                {
                    Direction.flower => "F",
                    Direction.north => "N",
                    Direction.east => "E",
                    Direction.west => "W",
                    Direction.south => "S",
                    _ => throw new Exception("invalid direction"),
                };
            }

            if (group.tile.kind == Kind.number)
            {
                StringBuilder tmp = new();
                tmp.Append(group.tile.value.ToString());

                string tmpSuit = group.tile.suit switch
                {
                    Suit.bam => "b",
                    Suit.crak => "c",
                    Suit.dot => "d",
                    _ => throw new Exception("invalid suit"),
                };

                tmp.Append(tmpSuit);

                return tmp.ToString();
            }

            if (group.tile.kind == Kind.dragon)
            {
                return group.tile.suit switch
                {
                    Suit.bam => "G",
                    Suit.crak => "R",
                    Suit.dot => "0",
                    _ => throw new Exception("invalid suit"),
                };
            }

            else throw new Exception("invalid tile");
        }
    }

    public override bool Equals(object obj)
    {
        // base checks
        if (this == obj) return true;
        if (obj == null) return false;
        if (obj.GetType() != GetType()) return false;

        Permutation that = (Permutation)obj;
        // value checks
        if (this.groups.Count != that.groups.Count) return false;
        for (int i = 0; i < groups.Count; i++)
        {
            if (!this.groups[i].Equals(that.groups[i])) return false;
        }

        // if all else passed, then it's a match
        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(groups);
    }
}

// Permutation Group: a group representing the possible groups of tiles that
// could satisfy a hand. Different than a CardGroup in that it uses absolute
// suits and numbers instead of colors and relative numbers. Does not
// specify whether jokers are used, but does specify whether they are allowed
public struct PermutationGroup
{
    public Tile tile;
    public int length;

    public PermutationGroup(Tile t, int l)
    {
        tile = t;
        length = l;
    }

    public readonly bool AllowJokers()
    {
        return length > 2;
    }
}
