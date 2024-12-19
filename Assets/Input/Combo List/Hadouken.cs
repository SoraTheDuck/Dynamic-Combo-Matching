using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HadoukenCommand", menuName = "Commands/Hadouken")]
public class HadoukenCommand : IComboCommand
{
    public override void Execute()
    {
        Debug.Log("Hadouken executed!");
        // Insert Hadouken-specific logic here (e.g., animation, effects)
    }
}
