using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;





public class HandLogic : MonoBehaviour
{
    GameObject TilePF;
    Tile joker;

    // for each hand:
    //
    // determine all permutations of hand (different suits & numbers but not
    // specific tile IDs or jokers)
    //
    // for each permutation:
    // assign tiles from hand in order until there isn't a tile/joker available
    // to satisfy a requirement

    // example: Quint: FF 11111 22 33333
    /*

    public static bool PlayerHandSatisfiesCardHand(CardHand cardHand, List<Tile> playerHand)
    {
        // find all permutations of a card hand
        List<Permutation> permutations = Permutations(cardHand);

        // determine if the list of tiles satisfies any of the permutations
        foreach (Permutation perm in permutations)
        {
            PlayerHandSatisfiesPermutation(perm);
        }
        
    }
    */

    
}
