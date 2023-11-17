using Fusion;

public struct CallInputStruct : INetworkInput
{
    public NetworkBool call;
    public NetworkBool wait;
    public NetworkBool pass;
    public int navigateRack; // will be -1 for left or 1 for right
}
