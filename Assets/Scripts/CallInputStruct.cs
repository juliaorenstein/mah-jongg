using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

enum Buttons
{
    Space,
    Return,
    Left,
    Right
}

public struct CallInputStruct : INetworkInput
{
    public NetworkButtons buttons;
    public NetworkBool call;
    public NetworkBool wait;
    public NetworkBool pass;
    public int navigateRack; // will be -1 for left or 1 for right
}
