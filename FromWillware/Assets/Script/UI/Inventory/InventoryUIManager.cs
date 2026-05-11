using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;
    
    [Header("绑定外部背包数据")]
    public ConsumableBackPack backPack;

    [Header("UI 引用")]
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject slotPrefab;
    public Button closeButton;
    public RectTransform selectionBox;

    [Header("【新增】武器背包引用 (用于切换页签)")]
    public InventoryTextUI weaponUI; // <--- 关键：把武器UI引入进来

    [Header("键盘导航设置")]
    public int columns = 9; 
    private int currentSelectedIndex = 0; 

    public InventorySlotUI[] slots;

    public Player player;
    public PlayerInputHandler inputHandler; // 改为 public，方便面板拖拽防丢失

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

        // 点击关闭按钮时，关闭所有背包
        closeButton.onClick.AddListener(CloseAllUI);
        inventoryPanel.SetActive(false);

        if (selectionBox != null)
            selectionBox.gameObject.SetActive(false);
            
        if (inputHandler == null) inputHandler = FindObjectOfType<PlayerInputHandler>();
        if (player == null) player = FindObjectOfType<Player>();
    }

    void Update()
    {
        if (backPack == null) backPack = FindObjectOfType<ConsumableBackPack>();

        // ================== 核心：统筹管理页签切换逻辑 ==================
        bool isConsumableOpen = inventoryPanel.activeSelf;
        bool isWeaponOpen = weaponUI != null && weaponUI.isUIOpen;
        bool isAnyOpen = isConsumableOpen || isWeaponOpen;

        // 【C 键】：整个背包系统的 总开关
        if (Input.GetKeyDown(KeyCode.C) || (inputHandler != null && inputHandler.backPackPressed))
        {
            if (isAnyOpen)
            {
                // 如果当前有任意一个背包开着 -> 关闭所有背包
                CloseAllUI(); 
            }
            else
            {
                // 如果都关着 -> 默认打开消耗品背包
                OpenConsumableUI(); 
            }
        }

        // 【B 键】：页签切换键（只有在背包开着的时候才生效）
        if (Input.GetKeyDown(KeyCode.B) && isAnyOpen)
        {
            if (isConsumableOpen)
            {
                // 关掉消耗品，打开武器
                CloseConsumableUI();
                if (weaponUI != null) weaponUI.OpenUI();
            }
            else if (isWeaponOpen)
            {
                // 关掉武器，打开消耗品
                if (weaponUI != null) weaponUI.CloseUI();
                OpenConsumableUI();
            }
        }
        // ==============================================================

        // 鼠标控制逻辑：只要有任意背包开着，就显示鼠标
        if (isAnyOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // 当消耗品背包处于显示状态时，执行它的内部逻辑
        if (isConsumableOpen)
        {
            if (backPack != null) RefreshUI();
            HandleKeyboardNavigation(); 
            HandleQuickBarBinding();
        }
    }

    // --- 新增的 UI 状态控制方法 ---
    public void OpenConsumableUI()
    {
        inventoryPanel.SetActive(true);
        if (player != null) player.IsInventoryOn = true;
        Invoke("ShowSelectionBox", 0.05f); // 延迟显示选中框
    }

    public void CloseConsumableUI()
    {
        inventoryPanel.SetActive(false);
        if (selectionBox != null) selectionBox.gameObject.SetActive(false);
    }

    public void CloseAllUI()
    {
        CloseConsumableUI();
        if (weaponUI != null) weaponUI.CloseUI();
        
        if (player != null) player.IsInventoryOn = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void ShowSelectionBox()
    {
        SelectSlot(currentSelectedIndex);
    }

    // ================== 以下保留你原来的逻辑，完全不变 ==================
    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < backPack.Items.Count && backPack.Items[i] != null && backPack.Items[i].item != null)
                slots[i].UpdateSlot(backPack.Items[i].item, backPack.Items[i].CurrentCount);
            else
                slots[i].ClearSlot();
        }
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (backPack == null) return;
        int maxIndex = Mathf.Max(indexA, indexB);
        while (backPack.Items.Count <= maxIndex) backPack.Items.Add(null);
        var temp = backPack.Items[indexA];
        backPack.Items[indexA] = backPack.Items[indexB];
        backPack.Items[indexB] = temp;
        RefreshUI();
    }

    void HandleKeyboardNavigation()
    {
        if (Input.GetKeyDown(KeyCode.J)|| (inputHandler != null && inputHandler.chooseItemLeftPressed))
            if (currentSelectedIndex % columns != 0 && currentSelectedIndex - 1 >= 0) SelectSlot(currentSelectedIndex - 1);

        else if (Input.GetKeyDown(KeyCode.L)|| (inputHandler != null && inputHandler.chooseItemRightPressed))
            if ((currentSelectedIndex + 1) % columns != 0 && currentSelectedIndex + 1 < slots.Length) SelectSlot(currentSelectedIndex + 1);

        else if (Input.GetKeyDown(KeyCode.I)|| (inputHandler != null && inputHandler.chooseItemUpPressed))
            if (currentSelectedIndex - columns >= 0) SelectSlot(currentSelectedIndex - columns);

        else if (Input.GetKeyDown(KeyCode.K)|| (inputHandler != null && inputHandler.chooseItemDownPressed))
            if (currentSelectedIndex + columns < slots.Length) SelectSlot(currentSelectedIndex + columns);
    }

    public void SelectSlot(int index)
    {
        if (selectionBox == null || index < 0 || index >= slots.Length) return;
        currentSelectedIndex = index; 
        selectionBox.gameObject.SetActive(true);
        selectionBox.position = slots[index].transform.position; 
        selectionBox.SetAsLastSibling(); 
    }

    void HandleQuickBarBinding()
    {
        if (backPack == null || currentSelectedIndex < 0 || currentSelectedIndex >= backPack.Items.Count) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)|| (inputHandler != null && inputHandler.setItem1Pressed)) backPack.AddToItemBar(currentSelectedIndex, 0);
        if (Input.GetKeyDown(KeyCode.Alpha2)|| (inputHandler != null && inputHandler.setItem2Pressed)) backPack.AddToItemBar(currentSelectedIndex, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3)|| (inputHandler != null && inputHandler.setItem3Pressed)) backPack.AddToItemBar(currentSelectedIndex, 2);
        if (Input.GetKeyDown(KeyCode.Alpha4)|| (inputHandler != null && inputHandler.setItem4Pressed)) backPack.AddToItemBar(currentSelectedIndex, 3);

        if (Input.GetKeyDown(KeyCode.X)) backPack.RemoveFromItemBar(currentSelectedIndex);
    }
}