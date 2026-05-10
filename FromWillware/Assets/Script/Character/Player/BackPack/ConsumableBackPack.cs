using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableBackPack : BackPack, ISaveable
{
    public List<ItemStack> Items = new List<ItemStack>();

    public string UniqueID = "ConsumableBackPack";

    [Header("交互UI")]
    public GameObject interactPromptPrefab;
    public Transform uiCanvas;

    private ItemPickup nearbyItem;

    private ItemBar itemBar;
    private Animator animator;
    private PlayerInputHandler inputHandler;
    private PlayerState playerState;

    private GameObject currentPrompt;

    void Awake()
    {
        itemBar = GetComponent<ItemBar>();
        animator = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();
        playerState = GetComponent<PlayerState>();

        if (Items.Count == 0)
        {
            Items = new List<ItemStack>(
                new ItemStack[MaxSize]
            );
        }
    }

    void Update()
    {
        // ===== 更新提示UI位置 =====
        if (currentPrompt != null && nearbyItem != null)
        {
            Vector3 screenPos =
                Camera.main.WorldToScreenPoint(
                    nearbyItem.transform.position + Vector3.up * 2f
                );

            currentPrompt.transform.position = screenPos;
        }

        // ===== 拾取 =====
        if (nearbyItem != null &&
            playerState.CanInteract &&
            (Input.GetKeyDown(KeyCode.E) ||
             inputHandler.interactPressed))
        {
            if (AddItem(nearbyItem.ItemData))
            {
                nearbyItem.isPickedUp = true;

                // 禁用碰撞体
                Collider col =
                    nearbyItem.GetComponent<Collider>();

                if (col != null)
                {
                    col.enabled = false;
                }

                // 隐藏交互UI
                if (currentPrompt != null)
                {
                    Destroy(currentPrompt);
                    currentPrompt = null;
                }

                animator.SetTrigger("PickUp");

                StartCoroutine(
                    DestroyAfterDelay(
                        nearbyItem.gameObject,
                        0.5f
                    )
                );

                nearbyItem = null;
            }
        }
    }

    // ================== 进入范围 ==================
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Item"))
            return;

        ItemPickup pickup =
            other.GetComponent<ItemPickup>();

        if (pickup == null || pickup.isPickedUp)
            return;

        nearbyItem = pickup;

        // 显示UI
        if (currentPrompt == null)
        {
            currentPrompt = Instantiate(
                interactPromptPrefab,
                uiCanvas
            );

            Vector3 screenPos =
                Camera.main.WorldToScreenPoint(
                    pickup.transform.position + Vector3.up * 2f
                );

            currentPrompt.transform.position = screenPos;
        }
    }

    // ================== 离开范围 ==================
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Item"))
            return;

        ItemPickup pickup =
            other.GetComponent<ItemPickup>();

        if (pickup == nearbyItem)
        {
            nearbyItem = null;
        }

        // 隐藏UI
        if (currentPrompt != null)
        {
            Destroy(currentPrompt);
            currentPrompt = null;
        }
    }

    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        obj.SetActive(false);
    }

    // ================== 添加物品 ==================
    public bool AddItem(Item item)
    {
        // 先堆叠
        foreach (var stack in Items)
        {
            if (stack != null &&
                stack.item == item &&
                stack.CurrentCount < item.MaxCount)
            {
                stack.CurrentCount++;

                return true;
            }
        }

        // 找空位
        for (int i = 0; i < MaxSize; i++)
        {
            if (Items[i] == null||Items[i].item==null)
            {
                Items[i] = new ItemStack(item, 1);

                return true;
            }
        }

        Debug.Log("背包满");

        return false;
    }

    // ================== 整理背包 ==================
    void TidyBackPack()
    {
        List<ItemStack> newList =
            new List<ItemStack>();

        foreach (var stack in Items)
        {
            if (stack != null)
            {
                newList.Add(stack);
            }
        }

        while (newList.Count < MaxSize)
        {
            newList.Add(null);
        }

        Items = newList;
    }

    // ================== 删除物品 ==================
    public void RemoveItem(int index, int amount = 1)
    {
        if (Items[index] == null)
            return;

        Items[index].CurrentCount -= amount;

        if (Items[index].CurrentCount <= 0)
        {
            Items[index] = null;

            TidyBackPack();
        }
    }

    // ================== 绑定快捷栏 ==================
    public void AddToItemBar(int BackIndex, int BarIndex)
    {
        if (BackIndex >= Items.Count ||
            Items[BackIndex] == null ||
            Items[BackIndex].item == null)
        {
            Debug.Log("选中格子为空，无法绑定");

            return;
        }

        // 目标栏位已有物品
        if (itemBar.Items[BarIndex] != null)
        {
            itemBar.Items[BarIndex].BarIndex = -1;
        }

        // 原本已经绑定
        if (Items[BackIndex].BarIndex != -1)
        {
            itemBar.Items[
                Items[BackIndex].BarIndex
            ] = null;
        }

        // 正式绑定
        Items[BackIndex].BarIndex = BarIndex;

        itemBar.Items[BarIndex] =
            Items[BackIndex];

        Debug.Log(
            $"已将 {Items[BackIndex].item.Name} 绑定到快捷键 {BarIndex + 1}"
        );
    }

    // ================== 解绑快捷栏 ==================
    public void RemoveFromItemBar(int BackIndex)
    {
        if (BackIndex >= Items.Count ||
            Items[BackIndex] == null)
            return;

        int currentBarIndex =
            Items[BackIndex].BarIndex;

        if (currentBarIndex != -1)
        {
            itemBar.Items[currentBarIndex] = null;

            Items[BackIndex].BarIndex = -1;

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
        List<ItemStackData> data =
            new List<ItemStackData>();

        foreach (var stack in Items)
        {
            if (stack == null ||
                stack.item == null ||
                string.IsNullOrEmpty(stack.item.Name))
            {
                continue;
            }

            data.Add(new ItemStackData
            {
                itemName = stack.item.Name,
                count = stack.CurrentCount,
                barIndex = stack.BarIndex,
                backpackIndex = Items.IndexOf(stack)
            });
        }

        return JsonUtility.ToJson(
            new Wrapper { items = data }
        );
    }

    // ================= LOAD =================
    public void RestoreState(string json)
    {
        var wrapper =
            JsonUtility.FromJson<Wrapper>(json);

        if (wrapper == null ||
            wrapper.items == null)
        {
            Debug.Log("读取背包失败");

            return;
        }

        Items = new List<ItemStack>(
            new ItemStack[MaxSize]
        );

        for (int i = 0; i < wrapper.items.Count; i++)
        {
            var d = wrapper.items[i];

            if (string.IsNullOrEmpty(d.itemName))
            {
                Debug.Log("❌ 空 itemName");

                continue;
            }

            if (!ItemDatabase.dict.ContainsKey(d.itemName))
            {
                Debug.Log(
                    "❌ 数据库不存在该物品: " + d.itemName
                );

                continue;
            }

            Item item = ItemDatabase.Get(d.itemName);

            ItemStack stack =
                new ItemStack(item, d.count);

            stack.BarIndex = d.barIndex;

            Items[d.backpackIndex] = stack;

            // 同步快捷栏
            if (d.barIndex >= 0 &&
                itemBar != null)
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