using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class Hand2
{
    public Pattern pattern;
    public bool odd;    // mod on Pattern.like
    public bool even;   // mod on Pattern.like
    public List<Group> groups;
    public int value;
    public bool closed;
    string HandStr = "FFFb 2222g FFFb 8888r exact x 25";

    void BuildHand()
    {
        string[] handArr = HandStr.Split(" ");
        string[] groupArr = handArr[0..^3];

        groups = new();
        pattern = (Pattern)Enum.Parse(typeof(Pattern), handArr[^3]);
        closed = handArr[^2] == "c";
        value = int.Parse(handArr[^1]);

        foreach (string groupStr in groupArr)
        {
            groups.Add(Group.Create(groupStr));
        }
    }

    /*
    void CheckTilesAgainstHand(List<Tile> tiles)
    {
        if (tiles.Count != 14) { return; } // definitely not a maj if not 14 tiles

        foreach (Group group in groups)
        {
            if (group.kind == Kind.flowerwind || group.kind == Kind.flowerwindwind)
            {
                GetFlowerWinds(group.direction, group.length, tiles);
            }
        }
    }

    
    List<Tile> GetFlowerWinds(Direction dir, int count, List<Tile> tiles)
    {
        List<Tile> outList = new();

        foreach(Tile tile in tiles)
        {
            FlowerWind fw = tile.GetComponent<FlowerWind>();
            if (fw && fw.Direction == dir)
            {
                outList.Add(tile);
                if (outList.Count == count) { return outList; }
            }
        }

        foreach(Tile ti)

        return outList;
    }

    bool JokersAllowed(int count) { return count > 2; }
    */
}