using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider slider; // 拖入UI Slider
    
    // 初始化/重置血条
    public void Setup(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
    }

    // 更新血条数值
    public void UpdateHealth(float currentHealth)
    {
        slider.value = currentHealth;
    }
}