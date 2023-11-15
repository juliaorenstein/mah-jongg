using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Hand : MonoBehaviour
{/*
    
    public ValRelation valRelation;
    public List<Group> groups;
    public int value;
    public bool closed;
    
    // format:
    // each grouping is the chars / vals from the card,
    //      followed by b, r, or g for color
    // after the groupings, is the numbering exact, like, or consec?
    // after that, x or c for open or closed
    // after that, the value
    // sections delimited by spaces

    // example:
    // FFFb 2222g FFFb 8888r exact x 25

    public Hand(string handStr)
    {
        string[] handArr = handStr.Split(" ");
        string[] groupArr = handArr[0..^3];

        groups = new();
        valRelation = (ValRelation)Enum.Parse(typeof(ValRelation), handArr[^3]);
        closed = handArr[^2] == "c";
        value = int.Parse(handArr[^1]);

        foreach (string groupStr in handArr)
        {
            groups.Add(Group.Create(groupStr));
        }
    }

    // from a Hand, create a list of tile ID combos that will achieve this
    List<List<int>> TileCombos()
    {
        List<List<int>> combos = new();
        foreach (Group group in groups)
        {
            // Flowers are easiest
            if (group.kind == Kind.flower)
            {

            }
        }
    }
    
    
    List<List<int>> Combos()
    {
        List<List<List<int>>> comboSetList = new();

        foreach (Group group in groups)
        {
            if (group.kind == Kind.flower || group.kind == Kind.wind)
            {
                comboSetList.Add(FlowerWinds(group));
            }

            if ((group.kind == Kind.dragon ||
                 group.kind == Kind.number) &&
                 group.suit != Suit.none)
            {
                comboSetList.Add(SuitedNumberDragons(group));
            }

        }


    }

    List<List<int>> FlowerWinds(Group group)
    {
        // North: 120 - 4
        // South: 124 - 4
        // East: 128 - 4
        // West: 132 - 4
        // Flowers: 136 - 8
        // Jokers: 144 - 8

        int startID = group.direction switch
        {
            Direction.north => 120,
            Direction.south => 124,
            Direction.east => 128,
            Direction.west => 132,
            _ => 144
        };

        int count = group.kind == Kind.flower ? 8 : 4;
        List<int> input = Enumerable.Range(startID, count).ToList();
        if (JokersAllowed(group)) { input.Concat(Enumerable.Range(144, 8).ToList()); }

        return PermutateIDs(input, group.length);   
    }

    List<List<int>> SuitedNumberDragons(Group group)
    {
        int startID = group.
    }
    
    bool JokersAllowed(Group group) { return group.length > 2; }

    // given a list of tileIDs representing the same tile or joker, return all
    // combos of a certain number of those tiles.
    List<List<int>> PermutateIDs(List<int> startingList, int goalLength)
    {
        List<List<int>> lists = new();
        List<List<int>> listsTBD;
        int curLength = 1;

        foreach (int i in startingList) { lists.Add(new() { i }); }

        while (curLength < goalLength)
        {
            listsTBD = new(lists);
            lists.Clear();

            foreach (List<int> list in listsTBD)
            {
                foreach (int i in startingList.GetRange(curLength, startingList.Count - curLength))
                {
                    if (i > list[^1])
                    {
                        lists.Add(new(list) { i });
                    }
                }
            }
            curLength++;
        }
        return lists;
    }

    List<List<int>> PermutateIDs(int start, int count, int goalLength)
    {
        return PermutateIDs(Enumerable.Range(start, count).ToList(), goalLength);
    }




    // given 8 identical tiles, how to get all combos of x of them

    // 1
    // 2
    // 3
    // 4
    // 5

    // 12 13 14 15
    // 23 24 25
    // 34 35
    // 45

    // 123 124 125 134 135 145
    // 234 235 245
    // 345

    // 1234 1235 1245
    // 2345

    // 12345


    // 123 124 125 134 135 145 234 235 245 345 List<List<int>>
    // ab ac ad ae bc bd be cd ce de List<List<int>>
    // together, List<List<List<int>>>
    // call List<int> combo - 123
    // call List<List<int>> comboList - 123 124 125 ...
    // call List<List<int>> comboListSet - 123 124 125, ab ac ad

    // 123ab 123ac 123ad ...
    // 124ab 124ac 124ad ...
    // 125ab 125ac 125ad ...
    // ...

    // given multiple outputs from PermutateIDs, return all possible ID
    // combinations of both groups combined.
    List<List<int>> PermutateCombos(List<List<List<int>>> comboSetList)
    {
        List<List<int>> tbd;
        List<List<int>> output = comboSetList[0];

        foreach (List<List<int>> comboSet in comboSetList.GetRange(1, comboSetList.Count - 1))
        {
            tbd = new(output);
            output.Clear();

            foreach (List<int> combo1 in tbd)
            {
                foreach (List<int> combo2 in comboSet)
                {
                    output.Add(new(combo1.Concat(combo2)));
                }
            }
        }

        return output;
    }


*/}


