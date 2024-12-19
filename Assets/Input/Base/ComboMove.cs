using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct InputQueue
{
    public float Delay;
    public InputCommand input; // Nullable to allow for empty
    public ScriptableObject action;
}

[CreateAssetMenu(fileName = "ComboMove", menuName = "Combo/ComboMove")]
public class ComboMove : ScriptableObject
{
    public string ComboName;
    public List<InputQueue> sequence;
}
