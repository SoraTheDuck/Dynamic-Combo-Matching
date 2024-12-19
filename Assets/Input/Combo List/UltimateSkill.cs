using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpinHadoukenCommand", menuName = "Commands/SpinHadoukenSkill")]
public class SpinHadoukenCommand : IComboCommand
{
    public override void Execute()
    {
        Debug.Log("Ultimate Skill activated!");
        // Add logic for the Ultimate Skill (e.g., special effects, damage)
    }
}
