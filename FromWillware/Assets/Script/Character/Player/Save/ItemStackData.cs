using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStackData
{
    public string itemName; // ✅ 可序列化
    public int count;
    public int barIndex;
}