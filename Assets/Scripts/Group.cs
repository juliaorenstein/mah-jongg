using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Group
{
    public int length;
    public Kind kind;
    public Col color;
    public int value;
    public Direction direction;
    public Suit suit;

    public static Group Create(string groupStr)
    {
        Group group;
        group.length = groupStr.Length - 1; // -1 because last char is color
        group.value = -1;
        group.suit = Suit.none;
        group.color = Col.blue;
        group.direction = Direction.none;

        // figure out what kind it is
        group.kind = groupStr[0].ToString() switch
        {
            "F" => Kind.flower,
            "D" or "G" or "R" or "0" => Kind.dragon,
            "N" or "E" or "W" or "S" => Kind.wind,
            _ => Kind.number
        };

        // if flower, you're done

        // if wind, add direction
        if (group.kind == Kind.wind)
        {
            group.direction = groupStr[0].ToString() switch
            {
                "N" => Direction.north,
                "E" => Direction.east,
                "W" => Direction.west,
                "S" => Direction.south,
                _ => Direction.none
            };
        }

        // if number or dragon, add color
        else if (group.kind == Kind.number || group.kind == Kind.dragon)
        {
            group.color = groupStr[^1].ToString() switch
            {
                "r" => Col.red,
                "g" => Col.green,
                _ => Col.blue
            };

            // if specific suit, add that
            group.suit = groupStr[0].ToString() switch
            {
                "G" => Suit.bam,
                "R" => Suit.crak,
                "0" => Suit.dot,
                _ => Suit.none
            };

            group.value = groupStr[0].ToString() switch
            {
                "D" or "G" or "R" or "0" => 0,
                _ => groupStr[0]
            };
        }

        return group;
    }
}
