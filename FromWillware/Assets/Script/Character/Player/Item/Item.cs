using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public string Name;
    public ItemKind Kind;
    public int MaxCount;
    
    public abstract void Fun(Player player);
}
