using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputCommand
{
    Up, Down, Left, Right, Light, Heavy, Special, Shoot
}

[CreateAssetMenu(fileName = "ComboAction", menuName = "ComboAction")]
public class ComboInput : ScriptableObject
{
    public string id;
    public KeyCode keyCode;
    
    public InputCommand input;
}
