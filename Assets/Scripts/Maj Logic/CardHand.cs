using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CardHand
{
    public Pattern pattern;
    public List<CardGroup> groups;
    private int highVal;
    private bool suitsPresent;

    // constructor based on a list of CardGroups
    public CardHand(List<CardGroup> g, Pattern p)
    {
        groups = g;
        pattern = p;
        highVal = 0;
        suitsPresent = false;

        foreach (CardGroup group in groups)
        {
            if (group.relVal > highVal) highVal = (int)group.relVal;
            if (group.kind == Kind.number || group.kind == Kind.dragon) suitsPresent = true;
        }
    }

    // constructor based on a string representing a card hand
    public CardHand(string cardHandStr)
    {
        groups = new();
        string[] groupStrArr = cardHandStr.Split(" ");
        string patternStr = groupStrArr.Last();
        List<string> groupStrList = groupStrArr.Take(groupStrArr.Length - 1).ToList();
        highVal = 0;
        suitsPresent = false;

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
                'D' => new CardGroup(groupStr.Length - 1, Kind.dragon, GetColorFromNumberStr(groupStr), 0),
                _ => new CardGroup(groupStr.Length - 1, Kind.number, GetColorFromNumberStr(groupStr), first - '0'),
            };

            if (group.relVal > highVal) highVal = (int)group.relVal;
            if (group.kind == Kind.number || group.kind == Kind.dragon) suitsPresent = true;

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
            "consec" => Pattern.consecutive,
            _ => Pattern.exact,
        };
    }

    // given a card hand, find suit and number permutations
    public List<Permutation> PermutationsForCardHand()
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

        List<Permutation> permList = new();

        // this value determines whether we cycle through suits because there
        // are suits in this hand, or if we only do one iteration here because
        // there aren't
        // FIXME: number/dragon hands that have fixed suits will break here.
        int suitPermNum = suitsPresent ? 6 : 1;

        for (int suitPerm = 0; suitPerm < suitPermNum; suitPerm++)
        {
            // when doing value permutations below, consec and like should
            // increment by 1, and like evens/odds should increment by 2.
            int delta = pattern switch
            {
                Pattern.consecutive or Pattern.like => 1,
                Pattern.likeEven or Pattern.likeOdd => 2,
                _ => 0,
            };

            // if exact numbers, use 0 as the val permutation to use the literal values
            if (pattern == Pattern.exact) permList.Add(new Permutation(this, suitPerm, 0));

            // else, do value permutations
            else for (int valPerm = 0; valPerm < 10 - highVal; valPerm += delta)
            {
                permList.Add(new Permutation(this, suitPerm, valPerm));
            }
        }

        return permList;
    }

    public override bool Equals(object obj)
    {
        // base checks
        if (this == obj) return true;
        if (obj == null) return false;
        if (obj.GetType() != GetType()) return false;

        CardHand that = (CardHand)obj;
        // value checks
        if (this.groups.Count != that.groups.Count) return false;
        if (this.pattern != that.pattern) return false;
        for (int i = 0; i < groups.Count; i++)
        {
            if (!this.groups[i].Equals(that.groups[i])) return false;
        }

        // if all else passed, then it's a match
        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(groups, pattern);
    }

    public override string ToString()
    {
        StringBuilder str = new();

        foreach (CardGroup group in groups)
        {
            for (int i = 0; i < group.length; i++)
            {
                str.Append(GroupToString(group));
                str.Append(" ");
            }
        }

        str.Append(pattern.ToString());

        return str.ToString();

        static string GroupToString(CardGroup group)
        {
            if (group.kind == Kind.flowerwind)
            {
                return group.dir switch
                {
                    Direction.flower => "F",
                    Direction.north => "N",
                    Direction.east => "E",
                    Direction.west => "W",
                    Direction.south => "S",
                    _ => throw new Exception("invalid direction"),
                };
            }

            if (group.kind == Kind.number || group.kind == Kind.dragon)
            {
                StringBuilder tmp = new();
                if (group.relVal > 0) tmp.Append(group.relVal.ToString()); // numbers
                else tmp.Append("D"); // dragons


                string tmpCol = group.col switch
                {
                    Col.blue => "b",
                    Col.green => "g",
                    Col.red => "r",
                    _ => throw new Exception("invalid color"),
                };

                tmp.Append(tmpCol);

                return tmp.ToString();
            }

            else throw new Exception("invalid tile");
        }
    }
}

public struct CardGroup
{
    public int length;
    public Kind kind;
    public Col? col;
    public int? relVal;
    public Direction? dir;
    public bool colIsSuit;

    public CardGroup(int l, Kind k, Col? c = null, int? v = null, Direction? d = null, bool cs = false)
    {
        length = l;
        kind = k;
        col = c;
        relVal = v;
        if (kind == Kind.dragon) relVal = 0; // in case anybody forgets to include that
        dir = d;
        colIsSuit = cs; // used when greens reds and 0s are specified on the card
    }
}
