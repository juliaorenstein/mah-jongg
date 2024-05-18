using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class Permutation : MonoBehaviour
{
    List<PermutationGroup> groups;
    /*
    public Permutation(CardHand cardHand, int suitPerm, int valPerm)
    {
        groups = new();

        foreach (CardGroup cg in cardHand.groups)
        {


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
    */
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
