using Fusion;

public enum TurnButtons
{
    call = 0,
    wait = 1,
    pass = 2
}

public struct CallInputStruct : INetworkInput
{
    public NetworkButtons turnOptions;
    public int navigateRack; // will be -1 for left or 1 for right
}
