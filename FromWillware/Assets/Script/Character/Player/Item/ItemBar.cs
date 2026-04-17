using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBar : MonoBehaviour
{
    // Start is called before the first frame update
    public List<ItemStack> Items = new List<ItemStack>();
    public Item CurrentItem;
    public int CurrentItemIndex = 0;
    public int MaxItems;
    public bool CanUseItem;
    
    private Player player;
    private PlayerMove playerMove;
    private PlayerAttack playerAttack;
    private PlayerParry playerParry;
    
    void Start()
    {
        player = GetComponent<Player>();
        playerParry = GetComponent<PlayerParry>();
        playerAttack = GetComponent<PlayerAttack>();
        playerParry = GetComponent<PlayerParry>();
        playerMove = GetComponent<PlayerMove>();
        
        CurrentItem = new HP_Potion();
        
        for (int i = 0; i < MaxItems; i++)
        {
            Items.Add(null);
        }
        Items[0] = new ItemStack(){item = CurrentItem, CurrentCount = 5};
    }

    // Update is called once per frame
    void Update()
    {
        CanUseItem = (!playerMove.IsRolling && !playerAttack.IsAttacking && !playerParry.IsDefensing);
        if (CanUseItem)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UseItem(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UseItem(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UseItem(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                UseItem(3);
            }
        }
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= Items.Count)
            return;

        ItemStack stack = Items[index];

        if (stack == null || stack.item == null)
            return;

        if (stack.CurrentCount <= 0)
        {
            Debug.Log("数量为0，无法使用");
            return;
        }

        // 使用物品
        stack.item.Fun(player);

        // 数量减少
        stack.CurrentCount--;

        Debug.Log("剩余数量: " + stack.CurrentCount);
    }

    public void AddItem(Item item)
    {
        // 先看有没有同类物品
        foreach (var stack in Items)
        {
            if (stack.item == item)
            {
                stack.CurrentCount++;
                return;
            }
        }

        // 没有就新建
        if (Items.Count >= MaxItems)
        {
            Debug.Log("ItemBar is full!");
            return;
        }

        Items.Add(new ItemStack
        {
            item = item,
            CurrentCount = item.MaxCount
        });
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= Items.Count)
            return;

        Items.RemoveAt(index);
    }
}
