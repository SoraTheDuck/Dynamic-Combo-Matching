using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class IComboCommand: ScriptableObject
{
    public abstract void Execute();
}

