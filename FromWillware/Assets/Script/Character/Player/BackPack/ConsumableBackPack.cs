using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableBackPack : BackPack
{
    // Start is called before the first frame update
    public List<Item> Items = new List<Item>();
    
    private ItemPickup nearbyItem;
    void Start()
    {
        for (int i = 0; i < MaxCount; i++)
        {
            Items.Add(null);
        }
        
    }

    // Update is called once per frame
    

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            nearbyItem = other.GetComponent<ItemPickup>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            nearbyItem = null;
        }
    }

    void Update()
    {
        if (nearbyItem != null && Input.GetKeyDown(KeyCode.E))
        {
            AddItem(nearbyItem.ItemData);
            Destroy(nearbyItem.gameObject); // 拾取后消失
            nearbyItem = null;
        }
    }
    int FindEmptyIndex()
    {
        for (int i = 0; i < MaxCount; i++)
        {
            if (Items[i] == null)
            {
                return i;
            }
        }
        return -1; // 没找到
    }
    
    public void AddItem(Item item)
    {
        int index = FindEmptyIndex();
        if (index < 0)
        {
            Debug.Log("The backpack is full");
            return;
        }
        Items[index] = item;
    }
}
