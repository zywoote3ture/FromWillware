using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableBackPack : BackPack,ISaveable
{
    // Start is called before the first frame update
    public List<ItemStack> Items = new List<ItemStack>();
    public string UniqueID = "ConsumableBackPack";
    
    private ItemPickup nearbyItem;
    private ItemBar itemBar;
    private ItemPickup itemPickup;
    private Animator animator;
    private PlayerInputHandler inputHandler;
    void Start()
    {
        itemBar = GetComponent<ItemBar>();
        animator = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();
        //Items = new List<ItemStack>(new ItemStack[MaxSize]);
    }

    // Update is called once per frame
    

    void OnTriggerStay(Collider other)
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
        if (nearbyItem != null && (Input.GetKeyDown(KeyCode.E)||inputHandler.interactPressed))
        {
            if (AddItem(nearbyItem.ItemData))
            {
                animator.SetTrigger("Pickup");
                StartCoroutine(DestroyAfterDelay(nearbyItem.gameObject, 0.5f));
                nearbyItem = null;
            }
               
           
        }

       
    }
    
    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }
    
    public bool AddItem(Item item)
    {
        // 1. 先堆叠
        foreach (var stack in Items)
        {
            if (stack != null && stack.item == item && stack.CurrentCount < item.MaxCount)
            {
                stack.CurrentCount++;
                return true;
            }
        }

        // 2. 新建
        if (Items.Count < MaxSize)
        {
            Items.Add(new ItemStack(item, 1));
            return true;
        }
        else
        {
            Debug.Log("背包满");
            return false;
        }
    }

    //整理背包
    void TidyBackPack()
    {
        List<ItemStack> newList = new List<ItemStack>();

        // 1️⃣ 收集所有非空物品
        foreach (var stack in Items)
        {
            if (stack != null)
            {
                newList.Add(stack);
            }
        }

        // 2️⃣ 补空位
        while (newList.Count < MaxSize)
        {
            newList.Add(null);
        }

        // 3️⃣ 替换
        Items = newList;
    }
    
    public void RemoveItem(int index, int amount = 1)
    {
        if (Items[index] == null) return;

        Items[index].CurrentCount -= amount;

        if (Items[index].CurrentCount <= 0)
        {
            Items[index] = null;
            TidyBackPack();
        }
    }

    // ⭐ 改进版的绑定逻辑
    public void AddToItemBar(int BackIndex, int BarIndex)
    {
        // 1. 如果选中的背包格子是空的，直接返回
        if (BackIndex >= Items.Count || Items[BackIndex] == null || Items[BackIndex].item == null) 
        {
            Debug.Log("选中格子为空，无法绑定");
            return;
        }

        // 2. 如果目标快捷栏（比如1号位）已经有东西了，先把它解绑
        if (itemBar.Items[BarIndex] != null)
        {
            itemBar.Items[BarIndex].BarIndex = -1; 
        }

        // 3. 如果我们选中的这个物品，之前已经绑在别的快捷键上了，先把它从那个快捷键上抠下来
        if (Items[BackIndex].BarIndex != -1)
        {
            itemBar.Items[Items[BackIndex].BarIndex] = null;
        }

        // 4. 正式绑定！
        Items[BackIndex].BarIndex = BarIndex;
        itemBar.Items[BarIndex] = Items[BackIndex];
        
        Debug.Log($"已将 {Items[BackIndex].item.Name} 绑定到快捷键 {BarIndex + 1}");
    }

    // ⭐ 改进版的解绑逻辑
    public void RemoveFromItemBar(int BackIndex)
    {
        if (BackIndex >= Items.Count || Items[BackIndex] == null) return;

        int currentBarIndex = Items[BackIndex].BarIndex;
        
        // 如果它身上有快捷栏编号，说明它被绑定了，我们把它解绑
        if (currentBarIndex != -1)
        {
            itemBar.Items[currentBarIndex] = null; // 清空快捷栏的引用
            Items[BackIndex].BarIndex = -1;        // 恢复成未绑定状态
            Debug.Log("物品已从快捷栏移除");
        }
    }
    
    public string GetUniqueID()
    {
        return "ConsumableBackPack";
    }

    // ================= SAVE =================
    public string CaptureState()
    {
        List<ItemStackData> data = new List<ItemStackData>();

        foreach (var stack in Items)
        {
            if (stack == null || stack.item == null ||  string.IsNullOrEmpty(stack.item.Name))  // ⭐关键修复
            {
                continue;
            }
            else
            {
                data.Add(new ItemStackData
                {
                    itemName = stack.item.Name,
                    count = stack.CurrentCount,
                    barIndex = stack.BarIndex
                });
            }
            
        }

        return JsonUtility.ToJson(new Wrapper { items = data });
    }

    // ================= LOAD =================
    public void RestoreState(string json)
    {
        var wrapper = JsonUtility.FromJson<Wrapper>(json);

        Items.Clear();

        for (int i = 0; i < wrapper.items.Count; i++)
        {
            var d = wrapper.items[i];
            if (string.IsNullOrEmpty(d.itemName))
            {
                Debug.Log("❌ 读取到空 itemName，跳过");
                continue;
            }

            if (!ItemDatabase.dict.ContainsKey(d.itemName))
            {
                Debug.Log("❌ 数据库不存在该物品: " + d.itemName);
                continue;
            }


            // ❗跳过空格（不存null）
            if (d == null) continue;

            Item item = ItemDatabase.Get(d.itemName);

            ItemStack stack = new ItemStack(item, d.count);
            stack.BarIndex = d.barIndex;

            Items.Add(stack);

            // 同步快捷栏
            if (d.barIndex >= 0 && itemBar != null)
            {
                itemBar.SetItem(d.barIndex, stack);
            }
        }
    }

    [System.Serializable]
    public class Wrapper
    {
        public List<ItemStackData> items;
    }
}




