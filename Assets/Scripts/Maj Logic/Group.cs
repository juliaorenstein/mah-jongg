using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group
{
    public int length;
    public Kind kind;
    public Col? color;
    public int? value;
    public Direction? direction;
    public Suit? suit;

    public static Group Create(string groupStr)
    {
        Group group = new();
        group.length = groupStr.Length - 1; // -1 because last char is color
        group.value = -1;

        // figure out what kind it is
        group.kind = groupStr[0].ToString() switch
        {
            "F" => Kind.flowerwind,
            "D" or "G" or "R" or "0" => Kind.dragon,
            "N" or "E" or "W" or "S" => Kind.flowerwind,
            _ => Kind.number
        };

        // if flowerwind, add direction
        if (group.kind == Kind.flowerwind)
        {
            group.direction = groupStr[0].ToString() switch
            {
                "N" => Direction.north,
                "E" => Direction.east,
                "W" => Direction.west,
                "S" => Direction.south,
                "F" => Direction.flower,
                _ => null
            };
        }

        // if number or dragon, add color
        else if (group.kind == Kind.number || group.kind == Kind.dragon)
        {
            group.color = groupStr[^1].ToString() switch
            {
                "r" => Col.red,
                "g" => Col.green,
                "b" => Col.blue,
                _ => null
            };

            // if specific suit, add that
            group.suit = groupStr[0].ToString() switch
            {
                "G" => Suit.bam,
                "R" => Suit.crak,
                "0" => Suit.dot,
                _ => null
            };

            group.value = groupStr[0].ToString() switch
            {
                "D" or "G" or "R" or "0" => 0,
                _ => groupStr[0]
            };
        }

        return group;
    }

    public override string ToString()
    {
        return $"Group\n" +
            $"Kind: {kind}\n" +
            $"Suit: {suit}\n" +
            $"Val: {value}\n" +
            $"Direction: {direction}\n" +
            $"Length: {length}";
    }
}
