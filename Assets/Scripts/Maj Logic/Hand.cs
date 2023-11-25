using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using UnityEngine;
using System.IO;

public class Hand : MonoBehaviour
{
    /*
    public Pattern pattern;
    public bool odd;    // mod on Pattern.like
    public bool even;   // mod on Pattern.like
    public List<Group> groups;
    public int value;
    public bool closed;
    StringBuilder str = new();
    static string filePath = "/Users/juliaorenstein/Unity/Maj/HandTester.txt";


    public GameObject RackPrefab;
    string HandStr = "FFFb 2222g FFFb 8888r exact x 25";

    private void Start()
    {
        
        
    }

    // format:
    // each grouping is the chars / vals from the card,
    //      followed by b, r, or g for color
    // after the groupings, is the numbering exact, like, or consec?
    // after that, x or c for open or closed
    // after that, the value
    // sections delimited by spaces

    // example:
    // FFFb 2222g FFFb 8888r exact x 25

    // VOCABULARY:
    // combo: { 4, 15, 32 } - a list of integers representing tiles, or a part of a specific hand
    // comboSet: { comboA, comboB, comboC } - a set of combos, all permutations of a handful of tiles
    // comboSetList: { comboSetA, comboSetB, comboSetC } - a list of comboSets,
    // where we need to find all possibilities of combos from each set joining with eachother
    // comboSetOptions: same structure as comboSetList, but this is a list of comboSets that are
    // mutually exclusive - don't want to permutate them with each other

    public static void Go()
    {
        string handStr = "FFFb 2222g FFFb 8888r exact x 25";
        string[] handArr = handStr.Split(" ");
        string[] groupArr = handArr[0..^3];

        Hand handTest = new();
        handTest.groups = new();
        handTest.pattern = (Pattern)Enum.Parse(typeof(Pattern), handArr[^3]);
        handTest.closed = handArr[^2] == "c";
        handTest.value = int.Parse(handArr[^1]);

        foreach (string groupStr in groupArr)
        {
            handTest.groups.Add(Group.Create(groupStr));
        }

        handTest.ComboSetForOneHand();
    }  
    
    public List<List<int>> ComboSetForOneHand()
    {
        List<List<int>> comboSetForHand = new();
        List<List<List<int>>> comboSetListForHand = new();
        List<Group> patternedGroups = new();

        foreach (Group group in groups)
        {
            // flowers and winds can be dealt with right away
            if (group.kind == Kind.flowerwind || group.kind == Kind.flowerwind)
            {
                comboSetListForHand.Add(ComboSetForFlowerWinds(group));
            }

            // suited numbers and dragons can be dealt with right away
            if ((group.kind == Kind.dragon ||
                 group.kind == Kind.number) &&
                 group.suit != Suit.none)
            {
                comboSetListForHand.Add(ComboSetForSuitedNumberDragons(group));
            }

            // everything else is part of a pattern and needs to be collected for later
            else { patternedGroups.Add(group); }
        }

        List<List<List<int>>> patternedComboSetOptions = ComboSetOptionsForPatternedGroups(patternedGroups);

        // for each option in comboSetOptions, combine that with all the other comboSets
        foreach (List<List<int>> patternedComboSet in patternedComboSetOptions)
        {
            List<List<List<int>>> inputComboSetList =
                new(comboSetListForHand) { patternedComboSet };

            comboSetForHand.Concat(ComboSetFromComboSetList(inputComboSetList));
        }

        return comboSetForHand;
    }

    List<List<int>> ComboSetForFlowerWinds(Group group)
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
        int count = group.kind == Kind.flowerwind ? 8 : 4;
        return ComboSetForGroup(startID, count, group.length);
    }

    List<List<int>> ComboSetForSuitedNumberDragons(Group group)
    {
        // Bam: 0
        // Crak: 40
        // Dot: 80

        int startID = 4 * group.value;
        startID += group.suit switch
        {
            Suit.bam => 0,
            Suit.crak => 40,
            Suit.dot => 80,
            _ => 500
        };

        int count = 4;
        List<int> input = Enumerable.Range(startID, count).ToList();

        return ComboSetForGroup(startID, count, group.length);
    }

    List<List<Suit>> SuitCombos = new()
    {
        new() { Suit.bam,   Suit.crak,  Suit.dot    },
        new() { Suit.bam,   Suit.dot,   Suit.crak   },
        new() { Suit.crak,  Suit.bam,   Suit.dot    },
        new() { Suit.crak,  Suit.dot,   Suit.bam    },
        new() { Suit.dot,   Suit.bam,   Suit.crak   },
        new() { Suit.dot,   Suit.crak,  Suit.bam    }
    };

    List<List<List<int>>> ComboSetOptionsForPatternedGroups(List<Group> patternedGroups)
    {
        if (pattern == Pattern.exact) { return ComboSetOptionsForExact(patternedGroups); }
        if (pattern == Pattern.like) { return ComboSetOptionsForLike(patternedGroups); }
        if (pattern == Pattern.consecutive) { return ComboSetOptionsForConsecutive(patternedGroups); }
        else { return null; }
    }

    List<List<List<int>>> ComboSetOptionsForExact(List<Group> patternedGroups)
    {
        List<List<List<int>>> patternedComboSetOptions = new();
        foreach (List<Suit> suitCombo in SuitCombos)
        {
            List<List<List<int>>> suitedComboSetList = new();
            foreach (Group group in patternedGroups)
            {
                Group suitedGroup = group;
                suitedGroup.suit = suitCombo[(int)suitedGroup.color];
                suitedComboSetList.Add(ComboSetForSuitedNumberDragons(suitedGroup));
            }
            patternedComboSetOptions.Add(ComboSetFromComboSetList(suitedComboSetList));
        }
        return patternedComboSetOptions;
    }

    List<List<List<int>>> ComboSetOptionsForLike(List<Group> patternedGroups)
    {
        // this could just be absorped into ComboSetOptionsForConsecutive
        List<List<List<int>>> likeComboSetOptions = new();

        // account for odds/evens
        int startVal = 1;
        int delta = 1;
        if (odd) { delta = 2; }
        if (even) { startVal = 2; delta = 2; }

        for (int val = startVal; val < 10; val += delta)
        {
            List<Group> likePatternedGroups = new();
            foreach (Group group in patternedGroups)
            {
                if (group.kind == Kind.dragon) { likePatternedGroups.Add(group); }
                else
                {
                    Group likeGroup = group;
                    likeGroup.value = val;
                    likePatternedGroups.Add(likeGroup);
                }
            }
            likeComboSetOptions.Concat(ComboSetOptionsForExact(likePatternedGroups));
        }
        return likeComboSetOptions;
    }

    List<List<List<int>>> ComboSetOptionsForConsecutive(List<Group> patternedGroups)
    {
        List<List<List<int>>> consecComboSetOptions = new();

        // get low and high values
        List<int> values = patternedGroups.Select(group => group.value).OrderBy(val => val).Distinct().ToList();
        int low = values[0] > 0 ? values[0] : values[1]; // if lowest value is dragon, get next lowest
        int high = values[^1];

        for (int val = 0; val < 10 - high; val++)
        {
            List<Group> consecPatternedGroups = new();
            foreach (Group group in patternedGroups)
            {
                if (group.kind == Kind.dragon) { consecPatternedGroups.Add(group); }
                else
                {
                    Group consecGroup = group;
                    consecGroup.value += val;
                    consecPatternedGroups.Add(consecGroup);
                }
            }
            consecComboSetOptions.Concat(ComboSetOptionsForExact(consecPatternedGroups));
        }

        return consecComboSetOptions;
    }

    List<List<int>> ComboSetForGroup(int startID, int count, int groupLength)
    {
        List<int> input = Enumerable.Range(startID, count).ToList();
        if (JokersAllowed(groupLength)) { input.Concat(Enumerable.Range(144, 8).ToList()); }
        return ComboSetFromTileIDs(input, groupLength);
    }
    
    bool JokersAllowed(int groupLength) { return groupLength > 2; }

    // given a list of tileIDs representing the same tile or joker, return all
    // combos of a certain number of those tiles.
    List<List<int>> ComboSetFromTileIDs(List<int> startingList, int goalLength)
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
        foreach (List<int> list in lists)
        {
            str.Clear();
            foreach (int i in list)
            {
                str.Append(i + ", ");
            }
            WriteToFile(str.ToString());
        }
        return lists;
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
    // call List<List<List<int>>> comboListSet - 123 124 125, ab ac ad

    // 123ab 123ac 123ad ...
    // 124ab 124ac 124ad ...
    // 125ab 125ac 125ad ...
    // ...

    // given multiple outputs from PermutateIDs, return all possible ID
    // combinations of both groups combined.
    List<List<int>> ComboSetFromComboSetList(List<List<List<int>>> comboSetList)
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

    void WriteToFile(string line)
    {
        

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(line);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }
    }
    */
}


