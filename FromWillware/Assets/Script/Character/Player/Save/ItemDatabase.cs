using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static Dictionary<string, Item> dict = new Dictionary<string, Item>();

    public List<Item> items; // 在Inspector拖所有物品

    void Start()
    {
        foreach (var item in items)
        {
            dict[item.Name] = item;
        }
    }

    public static Item Get(string id)
    {
        return dict[id];
    }
}
