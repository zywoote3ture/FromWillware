using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;[Header("绑定外部背包数据")]
    public ConsumableBackPack backPack;

    [Header("UI 引用")]
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject slotPrefab;
    public Button closeButton;
    public RectTransform selectionBox;

    
    [Header("键盘导航设置")]
    public int columns = 9; // 你的背包每行有几个格子？(根据截图应该是9)
    private int currentSelectedIndex = 0; // 当前选中的格子序号

    public InventorySlotUI[] slots;

    public Player player;
    private PlayerInputHandler inputHandler;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        slots = new InventorySlotUI[27];
        for (int i = 0; i < 27; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsParent);
            slots[i] = slotGO.GetComponent<InventorySlotUI>();
            slots[i].slotIndex = i;
        }

        closeButton.onClick.AddListener(CloseInventory);
        inventoryPanel.SetActive(false);

        if (selectionBox != null)
            selectionBox.gameObject.SetActive(false);
        inputHandler = FindObjectOfType<PlayerInputHandler>();
        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        if (backPack == null)
        {
            backPack = FindObjectOfType<ConsumableBackPack>();
        }

        if (inputHandler.backPackPressed)
        {
            ToggleInventory();
        }

        if (inventoryPanel.activeSelf)
        {
            // 1. 处理鼠标显示与锁定（强行夺取控制权）
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 2. 刷新 UI
            if (backPack != null) RefreshUI();
            
            // 3. 处理输入（只调用一次！）
            HandleKeyboardNavigation(); 
            HandleQuickBarBinding();
        }
    }

    void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        player.IsInventoryOn = inventoryPanel.activeSelf;
        UpdateMouseCursor();

        if (inventoryPanel.activeSelf)
        {
            // 使用 Invoke 延迟 0.05 秒，确保布局已经计算完毕
            Invoke("ShowSelectionBox", 0.05f); 
        }
        else
        {
            if (selectionBox != null) selectionBox.gameObject.SetActive(false);
        }
    }

    // 专门用来处理打开时的逻辑
    void ShowSelectionBox()
    {
        SelectSlot(currentSelectedIndex);
    }

    void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        UpdateMouseCursor();
    }

    void UpdateMouseCursor()
    {
        Cursor.visible = inventoryPanel.activeSelf;
        Cursor.lockState = inventoryPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < backPack.Items.Count && backPack.Items[i] != null && backPack.Items[i].item != null)
            {
                slots[i].UpdateSlot(backPack.Items[i].item, backPack.Items[i].CurrentCount);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (backPack == null) return;

        // 获取最大的那个目标索引
        int maxIndex = Mathf.Max(indexA, indexB);

        // 如果底层数据列表不够长，就用 null（空位）把它补齐
        while (backPack.Items.Count <= maxIndex)
        {
            backPack.Items.Add(null);
        }

        // 现在可以安全地进行交换了
        var temp = backPack.Items[indexA];
        backPack.Items[indexA] = backPack.Items[indexB];
        backPack.Items[indexB] = temp;

        RefreshUI();
    }

    // ================== 键盘导航核心逻辑 ==================
    void HandleKeyboardNavigation()
    {
        // 向左移动 (J)
        if (Input.GetKeyDown(KeyCode.J)||inputHandler.chooseItemLeftPressed)
        {
            // 只要当前不在最左边一列（对列数取余不为0），且序号 > 0
            if (currentSelectedIndex % columns != 0 && currentSelectedIndex - 1 >= 0)
            {
                SelectSlot(currentSelectedIndex - 1);
            }
        }
        // 向右移动 (L)
        else if (Input.GetKeyDown(KeyCode.L)||inputHandler.chooseItemRightPressed)
        {
            // 只要移动后不在下一行的第一列，且没超出总格子数
            if ((currentSelectedIndex + 1) % columns != 0 && currentSelectedIndex + 1 < slots.Length)
            {
                SelectSlot(currentSelectedIndex + 1);
            }
        }
        // 向上移动 (I)
        else if (Input.GetKeyDown(KeyCode.I)||inputHandler.chooseItemUpPressed)
        {
            // 只要减去一行的数量不小于0（即不在第一行）
            if (currentSelectedIndex - columns >= 0)
            {
                SelectSlot(currentSelectedIndex - columns);
            }
        }
        // 向下移动 (K)
        else if (Input.GetKeyDown(KeyCode.K)||inputHandler.chooseItemDownPressed)
        {
            // 只要加上一行的数量没超出总格子数
            if (currentSelectedIndex + columns < slots.Length)
            {
                SelectSlot(currentSelectedIndex + columns);
            }
        }
    }

    // 更新选择框的位置，并记录当前选中的序号
    public void SelectSlot(int index)
    {
        if (selectionBox == null || index < 0 || index >= slots.Length) return;

        currentSelectedIndex = index; // 记录当前序号
        
        selectionBox.gameObject.SetActive(true);
        selectionBox.position = slots[index].transform.position; // 移动到目标位置
        
        // 【关键】强制让高亮框显示在最前面，防止被其他UI挡住
        selectionBox.SetAsLastSibling(); 
    }

    // ================== 快捷键绑定核心逻辑 ==================
    void HandleQuickBarBinding()
    {
        // 防错检测
        if (backPack == null || currentSelectedIndex < 0 || currentSelectedIndex >= backPack.Items.Count) return;

        // 按下数字键 1, 2, 3, 4 绑定到对应的快捷栏 (索引为 0, 1, 2, 3)
        if (Input.GetKeyDown(KeyCode.Alpha1)||inputHandler.setItem1Pressed) backPack.AddToItemBar(currentSelectedIndex, 0);
        if (Input.GetKeyDown(KeyCode.Alpha2)||inputHandler.setItem2Pressed) backPack.AddToItemBar(currentSelectedIndex, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3)||inputHandler.setItem3Pressed) backPack.AddToItemBar(currentSelectedIndex, 2);
        if (Input.GetKeyDown(KeyCode.Alpha4)||inputHandler.setItem4Pressed) backPack.AddToItemBar(currentSelectedIndex, 3);

        // 按下 X 键解除绑定
        if (Input.GetKeyDown(KeyCode.X))
        {
            backPack.RemoveFromItemBar(currentSelectedIndex);
        }
    }
}