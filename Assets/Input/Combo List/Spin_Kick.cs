using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpinKickCommand", menuName = "Commands/SpinKick")]
public class SpinKickCommand : IComboCommand
{
    public override void Execute()
    {
        Debug.Log("Spin Kick executed!");
        // Insert Spin Kick-specific logic here (e.g., animation, effects)
    }
}
