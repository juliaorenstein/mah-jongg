using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class InputCollection : NetworkBehaviour
{
    NetworkButtons previousTurnOptions;
    public bool wait;
    public bool pass;
    public bool call;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out CallInputStruct input))
        {
            wait = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.wait);
            pass = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.pass);
            call = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.call);
        }

        previousTurnOptions = input.turnOptions;
    }
}
