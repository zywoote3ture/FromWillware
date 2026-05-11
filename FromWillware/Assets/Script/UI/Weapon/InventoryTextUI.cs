using System.Collections.Generic;
using UnityEngine;
using TMPro; // 保证有TMPro

public class InventoryTextUI : MonoBehaviour
{
    [Header("核心引用")]
    public GameObject inventoryPanel;       
    public WeaponBackPack weaponBackPack;   
    public WeaponSystem weaponSystem;[Header("UI 引用")]
    public Transform listContent;           
    public GameObject textSlotPrefab;       
    public TextMeshProUGUI detailText;                 

    // 【修改为 public】让 UIManager 能够读取它的状态
    public bool isUIOpen = false; 
    
    private List<InventoryTextSlotUI> spawnedSlots = new List<InventoryTextSlotUI>();
    private int currentSelectedIndex = 0;   

    void Start()
    {
        if (weaponSystem == null && weaponBackPack != null)
        {
            weaponSystem = weaponBackPack.GetComponent<WeaponSystem>();
        }
        
        // 初始化时确保界面是关闭的
        CloseUI();
    }

    void Update()
    {
        // -------------------------------------------------------------
        // 注意：这里删除了 B 键的开关逻辑！
        // 因为 C 键和 B 键的整体切换现在全部交由 InventoryUIManager 管理了。
        // -------------------------------------------------------------

        // 只要武器界面处于打开状态，就监听自己的上下按键和装备按键
        if (isUIOpen && spawnedSlots.Count > 0)
        {
            HandleKeyboardNavigation();
            HandleEquipInput();
        }
    }

    // 【新增】提供给大管家 (InventoryUIManager) 调用的打开方法
    public void OpenUI()
    {
        isUIOpen = true;
        inventoryPanel.SetActive(true);
        RefreshUI();
    }

    // 【新增】提供给大管家 (InventoryUIManager) 调用的关闭方法
    public void CloseUI()
    {
        isUIOpen = false;
        inventoryPanel.SetActive(false);
    }

    // ================== 以下保留你原来的逻辑，完全不变 ==================
    private void RefreshUI()
    {
        foreach (Transform child in listContent)
        {
            Destroy(child.gameObject);
        }
        spawnedSlots.Clear();

        for (int i = 0; i < weaponBackPack.Weapons.Count; i++)
        {
            WeaponData data = weaponBackPack.Weapons[i];
            GameObject obj = Instantiate(textSlotPrefab, listContent);
            InventoryTextSlotUI slotUI = obj.GetComponent<InventoryTextSlotUI>();
            slotUI.Setup(data);
            
            spawnedSlots.Add(slotUI);
        }

        if (spawnedSlots.Count > 0)
        {
            currentSelectedIndex = 0;
            SelectSlot(currentSelectedIndex);
        }
        else
        {
            detailText.text = "背包里没有武器。";
        }
    }

    private void HandleKeyboardNavigation()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (currentSelectedIndex > 0) SelectSlot(currentSelectedIndex - 1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (currentSelectedIndex < spawnedSlots.Count - 1) SelectSlot(currentSelectedIndex + 1);
        }
    }

    private void SelectSlot(int index)
    {
        currentSelectedIndex = index;
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            spawnedSlots[i].SetHighlight(i == currentSelectedIndex);
        }
        ShowWeaponDetails(weaponBackPack.Weapons[currentSelectedIndex]);
    }

    private void HandleEquipInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeaponToPoint(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeaponToPoint(1);
    }

    private void EquipWeaponToPoint(int pointIndex)
    {
        if (weaponSystem != null)
        {
            weaponSystem.SetToWeaponPoint(currentSelectedIndex, pointIndex);
            Debug.Log($"已将 {weaponBackPack.Weapons[currentSelectedIndex].Name} 装备到武器槽位 {pointIndex + 1} !");
        }
    }

    private void ShowWeaponDetails(WeaponData data)
    {
        if (detailText != null)
        {
            detailText.text = $"<color=#FFD700>【{data.Name}】</color>\n\n" +
                              $"伤害： {data.Damage}\n" +
                              $"耐力消耗： {data.ConsumingStamina}\n\n" +
                              $"<color=#A9A9A9>武器介绍：\n{data.Introduction}</color>\n\n" +
                              $"<color=#00FF00>按 【1】 键装备至 主武器 槽\n按 【2】 键装备至 副武器 槽</color>";
        }
    }
}