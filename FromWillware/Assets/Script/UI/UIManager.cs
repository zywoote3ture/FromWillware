using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour
{
    // --- 新增：定义全局单例 ---
    public static UIManager Instance;

    [Header("UI 引用")]
    public Slider healthSlider;  // 拖入层级中的 Slider05_UserInfo_Orange
     public TextMeshProUGUI healthText; // 用于显示血量数值的文本
    public Slider staminaSlider;// 拖入层级中的 Slider05_UserInfo_Green
     public TextMeshProUGUI staminaText; // 用于显示精力数值的文本

    public Player player;

    // --- 新增：在 Awake 中初始化单例 ---
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 防止场景中出现两个 UIManager
        }


    }

    public void Update()
    {
        if (player == null) return;
        UpdateHealthUI();
        UpdateStaminaUI();
        healthText.text = $"{(int)player.CurrentHP} / {(int)player.MaxHP}";
        staminaText.text = $"{(int)player.CurrentStamina} / {(int)player.MaxStamina}";
    }

    // 更新血条
    public void UpdateHealthUI()
    {
        healthSlider.maxValue = player.MaxHP;
        healthSlider.value = player.CurrentHP;
    }

    // 更新精力条
    public void UpdateStaminaUI()
    {
        staminaSlider.maxValue = player.MaxStamina;
        staminaSlider.value = player.CurrentStamina;
    }
}