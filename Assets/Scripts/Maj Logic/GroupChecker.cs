using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class GroupChecker : MonoBehaviour
{
    List<List<Tile>> TileGroupLists; // the groups of tiles that a user has displayed
    List<Group> TileGroups;
    public GameObject TilePF;
    static List<List<Suit>> SuitCombos = new()
    {
        new() { Suit.bam,   Suit.crak,  Suit.dot    },
        new() { Suit.bam,   Suit.dot,   Suit.crak   },
        new() { Suit.crak,  Suit.bam,   Suit.dot    },
        new() { Suit.crak,  Suit.dot,   Suit.bam    },
        new() { Suit.dot,   Suit.bam,   Suit.crak   },
        new() { Suit.dot,   Suit.crak,  Suit.bam    }
    };

    // "FFFb 2222g FFFb 8888r exact x 25"

    private void Start()
    { 
        Hand2 hand = new();
        TileGroupLists = new List<List<Tile>>()
        {
            new List<Tile>()
            {
                Instantiate(TilePF).GetComponent<Tile>().InitTile(Direction.flower),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(Direction.flower),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(Direction.flower)
            },
            new()
            {
                Instantiate(TilePF).GetComponent<Tile>().InitTile(2, Suit.bam),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(2, Suit.bam),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(2, Suit.bam),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(2, Suit.bam),
            },
            new()
            {
                Instantiate(TilePF).GetComponent<Tile>().InitTile(Direction.flower),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(Direction.flower),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(Direction.flower)
            },
            new()
            {
                Instantiate(TilePF).GetComponent<Tile>().InitTile(8, Suit.crak),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(8, Suit.crak),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(8, Suit.crak),
                Instantiate(TilePF).GetComponent<Tile>().InitTile(2, Suit.bam),
            }
        };

        List<Group> test = Check(hand);
        foreach(Group testGroup in test)
        {
            Debug.Log(testGroup.ToString());
        }
    }

    List<Group> Check(Hand2 hand)
    {
        // if the hand doesn't have 14 tiles, quit out
        if (TileGroupLists.Select(group => group.Count).Sum() != 14) { return null; };

        // turn into groups
        List<Group> tileGroups = TileGroupLists.Select(list => TileListToTileGroup(list)).ToList();

        // split into exact groups (winds, flowers, suited dragons and numbers)
        // and inexact groups (nonsuited dragons and numbers)
        (List<Group> exactGroups, List<Group> inexactGroups) = ExtractGroups(hand);

        // non-suited things
        Dictionary<Group, Group> nonSuited = MatchTileGroupsToCardGroups(exactGroups);
        if (nonSuited == null) { return null; }

        Dictionary<Group, Group> suited = SoftProperties(inexactGroups, hand.pattern);
        if ( suited == null) { return null; }

        Dictionary<Group, Group> combinedDict = nonSuited.Concat(suited)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        List<Group> returnVal = new();

        foreach (Group cardGroup in hand.groups)
        {
            returnVal.Add(combinedDict[cardGroup]);
        }

        return returnVal;
    }

    // TODO: TEST ALL OF THIS

    Group TileListToTileGroup(List<Tile> tiles)
    {
        Group tileGroup = new();

        tiles.Sort();
        Tile identity = tiles[0];

        tileGroup.length = tiles.Count;
        tileGroup.kind = identity.kind;
        tileGroup.value = identity.value;
        tileGroup.suit = identity.suit;
        tileGroup.direction = identity.direction;

        return tileGroup;
    }

    (List<Group> exactGroups, List<Group> inexactGroups) ExtractGroups(Hand2 hand)
    {
        List<Group> exactGroups = new();
        List<Group> inexactGroups = new();

        foreach (Group cardGroup in hand.groups)
        {
            if ((cardGroup.kind == Kind.number || cardGroup.kind == Kind.dragon)
                && cardGroup.suit == null)
            {
                inexactGroups.Add(cardGroup);
            }

            else { exactGroups.Add(cardGroup); }
        }

        return (exactGroups, inexactGroups);
    }

    Dictionary<Group, Group> MatchTileGroupsToCardGroups(List<Group> exactGroups)
    { return ResolveCandidates(FindCandidates(exactGroups)); }

    Dictionary<Group, List<Group>> FindCandidates(List<Group> cardGroups)
    {
        Dictionary<Group, List<Group>> candidates = new();

        foreach (Group cardGroup in cardGroups)
        {
            List<Group> groupCandidates = new();

            // loop through tile groups and find candidate matches
            foreach (Group tileGroup in TileGroups)
            {
                if (CompareTileGroupToCardGroup(tileGroup, cardGroup))
                { groupCandidates.Add(tileGroup); }
            }

            // quit out of the whole method if a single group failed to
            // find any matching tile groups
            if (groupCandidates.Count == 0) { return null; }

            // otherwise add this to the candidate dictionary
            candidates[cardGroup] = groupCandidates;
        }

        return candidates;
    }

    Dictionary<Group, Group> ResolveCandidates(Dictionary<Group, List<Group>> candidates)
    {
        // if some keys have >1 candidate, see if it works out
        Dictionary<Group, Group> final = new();

        // take care of card groups that have identical possibilites for tile groups
        // for ex, if there is a hand with two "FFF" sequences

        List<Group> keysAlreadyChecked = new();

        while (candidates.Count > 0)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                Group startingKey = candidates.Keys.ElementAt(i);
                if (keysAlreadyChecked.Contains(startingKey)) { continue; }

                List<Group> keysWithDupVals = new() { startingKey };
                List<Group> vals1 = candidates.Values.ElementAt(i);

                for (int j = i + 1; j < candidates.Count; j++)
                {
                    List<Group> vals2 = candidates.Values.ElementAt(j);
                    vals1.SequenceEqual(vals2);
                    keysWithDupVals.Add(candidates.Keys.ElementAt(j));
                }

                // if this check is true, that means there's, for example,
                // only one tile group that satisfies > 1 hand group.
                // i.e. you have a group of 3 flower tiles, and you have two
                // FFFs on the card
                if (keysWithDupVals.Count > vals1.Count)
                { return null; }

                // if x hand groups have the same x possible tile groups,
                // dole them out between them
                if (keysWithDupVals.Count == vals1.Count)
                {
                    for (int k = 0; k < vals1.Count; k++)
                    {
                        final[keysWithDupVals[k]] = vals1[k];
                    }
                }

                // track the keys checked here to avoid double-checking them later
                keysAlreadyChecked.Concat(keysWithDupVals);
            }

            // now subtract finals from candidates such that there are no keys in
            // candidates that are in final, and no values in candidates that are in final
            foreach (Group key in final.Keys)
            { candidates.Remove(key); }

            foreach (Group val in final.Values)
            {
                candidates.Values.Select(list => list.Remove(val));
                if (candidates.Values.Any(list => list.Count == 0))
                { return null; }
            }
        }

        return final;
    }

    Dictionary<Group, Group> SoftProperties(List<Group> inexactGroups, Pattern pattern)
    {
        if (pattern == Pattern.exact)
        { return TrySuits(inexactGroups); }

        // test likes
        if (pattern == Pattern.like ||
            pattern == Pattern.likeEven ||
            pattern == Pattern.likeOdd)
        { return EvalLikeHand(inexactGroups, pattern); }

        // test consecs
        if (pattern == Pattern.consecutive)
        { return EvalConsecHand(inexactGroups); }

        return null;
    }

    Dictionary<Group, Group> TrySuits(List<Group> cardGroups)
    {
        foreach (List<Suit> suitCombo in SuitCombos)
        {
            Dictionary<Group, Group> candidate = MatchTileGroupsToCardGroups(
                TmpSuitedCardGroups(cardGroups, suitCombo));

            if (candidate != null) { return candidate; }
        }
        return null;
    }

    Dictionary<Group, Group> EvalLikeHand(List<Group> groups, Pattern pattern)
    {
        int likeVal = LikeHandVal();
        if (likeVal < 0) { return null; }
        if (pattern == Pattern.likeEven && likeVal % 2 == 1) { return null; }
        if (pattern == Pattern.likeEven && likeVal % 2 == 0) { return null; }

        List<Group> tmpExactGroups = new(groups);
        foreach (Group group in groups)
        { if (group.value != null) { group.value = likeVal; } }

        return TrySuits(tmpExactGroups);
    }

    // checks if a hand has like numbers, and which number it is
    int LikeHandVal()
    {
        int likeVal = 0;
        foreach (Group tileGroup in TileGroups)
        {
            // only execute logic for groups with values
            if (tileGroup.value != null) { continue; }

            // if this is the first numbered group, set likeVal
            if (likeVal == 0) { likeVal = (int)tileGroup.value; continue; }

            // if subsequent numbered groups have the same value, continue
            if (tileGroup.value == likeVal) { continue; }

            // otherwise, this hand is outtie.
            return -1;
        }

        return likeVal;
    }

    Dictionary<Group, Group> EvalConsecHand(List<Group> cardGroups)
    {
        int minVal = ConsecHandMin();
        if (minVal < 0) { return null; }
        
        List<Group> tmpExactGroups = new(cardGroups);
        foreach (Group group in cardGroups)
        { if (group.value != null) { group.value += minVal - 1; } }

        return TrySuits(tmpExactGroups);
    }

    int ConsecHandMin()
    {
        int minVal = 10;
        foreach (Group tileGroup in TileGroups)
        {
            // only execute logic for groups with values
            if (tileGroup.value != null) { continue; }

            // if this is a numbered group and it's lower than min, set min
            if (minVal > tileGroup.value) { minVal = (int)tileGroup.value; continue; }
        }

        // if there were no numbered groups, this hand is outtie.
        if (minVal == 10) { return -1; }

        return minVal;
    }

    List<Group> TmpSuitedCardGroups(List<Group> cardGroups, List<Suit> suitCombo)
    {
        List<Group> tmpGroups = new(cardGroups);

        // assign a suit to each color based on the suitCombo
        foreach (Group group in tmpGroups)
        {
            if (group.color == null) { continue; }
            group.suit = suitCombo[(int)group.color];
        }

        return tmpGroups;
    }

    bool CompareTileGroupToCardGroup(Group tileGroup, Group cardGroup)
    {
        // length is non-negotiable
        if (cardGroup.length != tileGroup.length) { return false; }

        // if all jokers and length is > 2, add this to list
        if (tileGroup.kind == Kind.joker && tileGroup.length > 2)
        { return true; }

        // if not all jokers, rule out non-matches
        if (cardGroup.kind != tileGroup.kind) { return false; }
        if (cardGroup.suit != tileGroup.suit) { return false; }
        if (cardGroup.value != tileGroup.value) { return false; }
        if (cardGroup.direction != tileGroup.direction) { return false; }

        // it's a match!
        return true;
    }
}
