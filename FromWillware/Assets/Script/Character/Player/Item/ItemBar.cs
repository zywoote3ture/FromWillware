using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBar : MonoBehaviour
{
    public List<ItemStack> Items = new List<ItemStack>();
    public int MaxItems = 4;

    private Player player;
    private PlayerMove playerMove;
    private PlayerAttack playerAttack;
    private PlayerParry playerParry;
    private PlayerInputHandler inputHandler;

    void Start()
    {
        player = GetComponent<Player>();
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerParry = GetComponent<PlayerParry>();

        inputHandler = GetComponent<PlayerInputHandler>();
        Items.Clear();
        // 初始化固定槽位
        for (int i = 0; i < MaxItems; i++)
        {
            Items.Add(null);
        }
    }

    void Update()
    {
        // ==========================================
        // ⭐ 新增拦截：如果背包面板是打开状态，直接 return，禁止使用物品！
        // ==========================================
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.inventoryPanel.activeSelf)
        {
            return; 
        }
        

        bool canUse = (!playerMove.IsRolling && 
                       !playerAttack.IsAttacking && 
                       !playerParry.IsDefensing);

        if (!canUse) return;

        if (inputHandler.useItem1Pressed) UseItem(0);
        if (inputHandler.useItem2Pressed) UseItem(1);
        if (inputHandler.useItem3Pressed) UseItem(2);
        if (inputHandler.useItem4Pressed) UseItem(3);
    }

    // ⭐ 使用物品
    // ⭐ 使用物品
    public void UseItem(int index)
    {
        if (index < 0 || index >= Items.Count) return;

        ItemStack stack = Items[index];

        if (stack == null || stack.item == null) return;

        if (stack.CurrentCount <= 0)
        {
            Debug.Log("The item is empty");
            return;
        }

        // 1. 使用物品
        stack.item.Fun(player);

        // 2. 数量减少
        if(stack.item.Kind!=ItemKind.Plot)
            stack.CurrentCount--;
        
        // ==========================================
        // ⭐ 新增核心逻辑：如果物品用光了（数量 <= 0），彻底清理数据！
        // ==========================================
        if (stack.CurrentCount <= 0)
        {
            // 第一步：从快捷栏的数据列表中清空它
            Items[index] = null;

            // 第二步：从背包的数据列表中也清空它
            ConsumableBackPack backPack = GetComponent<ConsumableBackPack>();
            if (backPack != null)
            {
                for (int i = 0; i < backPack.Items.Count; i++)
                {
                    // 找到背包里对应的这一个“空壳”
                    if (backPack.Items[i] == stack)
                    {
                        backPack.Items[i] = null; // 把它变成空位
                        
                        // 【进阶可选】如果你想让背包在物品用完后自动把后面的物品往前挪（整理）
                        // 你需要去 ConsumableBackPack 脚本里，把 void TidyBackPack() 改成 public void TidyBackPack()
                        // 然后在这里取消下面这行代码的注释：
                        // backPack.TidyBackPack(); 
                        
                        break; // 找到了就停止循环
                    }
                }
            }
        }
    }

    // ⭐ 从背包加入（关键：引用）
    public void SetItem(int index, ItemStack stack)
    {
        if (index < 0 || index >= MaxItems) return;

        Items[index] = stack; // ✅ 直接引用
    }

    // ⭐ 移除快捷栏物品
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= MaxItems) return;

        Items[index] = null;
    }

    // ⭐ 从所有地方移除（同步）
    void RemoveStack(ItemStack stack)
    {
        // 清快捷栏
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] == stack)
            {
                Items[i] = null;
            }
        }

        // 👉 如果你有 Backpack，这里也应该调用：
        // backpack.RemoveStack(stack);
    }
}