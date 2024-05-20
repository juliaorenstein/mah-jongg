using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class HandLogic
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
    

    public static bool PlayerHandSatisfiesCardHand(CardHand cardHand, List<Tile> playerHand)
    {
        // 14 tiles check
        if (playerHand.Count != 14) return false;

        // find all permutations of a card hand
        List<Permutation> permutations = cardHand.PermutationsForCardHand();

        // determine if the list of tiles satisfies any of the permutations
        foreach (Permutation perm in permutations)
        {
            if (PlayerHandSatisfiesPermutation(perm, playerHand)) return true;
        }

        return false;
    }

    static bool PlayerHandSatisfiesPermutation(Permutation perm, List<Tile> playerHand)
    {
        List<Tile> unused = new(playerHand);
        List<Tile> used = new();

        Tile joker = new();

        foreach (PermutationGroup group in perm.groups)
        {
            List<Tile> found = unused.FindAll(tile => tile.Equals(group.tile));

            // we don't have enough tiles and jokers aren't allowed
            if (found.Count < group.length && !group.AllowJokers()) return false;

            // we have exactly the right number of tiles
            if (found.Count == group.length)
            {
                unused.RemoveAll(tile => tile.Equals(group.tile));
                used.Concat(found);
                continue;
            }

            // we have more than enough tiles - take only what's needed
            if (found.Count > group.length)
            {
                for (int i = 0; i < group.length; i++)
                {
                    Tile tileToTransfer = unused.First(tile => tile.Equals(group.tile));
                    unused.Remove(tileToTransfer);
                    used.Add(tileToTransfer);
                }
                continue;
            }

            // if we get here, we don't have enough tiles but might have enough
            // with jokers
            List<Tile> foundJokers = unused.FindAll(tile => tile.Equals(joker));

            // if we still don't have enough jokers, return false
            if (found.Count + foundJokers.Count < group.length) return false;

            // if we do have enough, transfer over the tiles
            unused.RemoveAll(tile => tile.Equals(group.tile));
            used.Concat(found);

            // and add only as many jokers as needed
            for (int i = 0; i < group.length - found.Count; i++)
            {
                Tile tileToTransfer = unused.First(tile => tile.Equals(joker));
                unused.Remove(tileToTransfer);
                used.Add(tileToTransfer);
            }

        }

        return true;
    }

}
