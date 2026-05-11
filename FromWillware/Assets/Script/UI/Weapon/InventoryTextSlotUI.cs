using UnityEngine;
using UnityEngine.UI; // 如果你用的是TextMeshPro，这里换成 TMPro，下面也换成 TMP_Text
using TMPro;
public class InventoryTextSlotUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;  // 绑定子物体的Text
    public Button button;  // 绑定自己身上的Button

    // 初始化时传入武器数据
    public void Setup(WeaponData data)
    {
        if (data != null)
        {
            nameText.text = data.Name; // 只显示名字
            button.interactable = true;
        }
    }

    public void SetHighlight(bool isSelected)
    {
        // 如果被选中，文字变黄且加粗；如果没有选中，文字为白色正常体
        if (isSelected)
        {
            nameText.color = Color.yellow;
            nameText.fontStyle = FontStyles.Bold;
        }
        else
        {
            nameText.color = Color.white;
            nameText.fontStyle = FontStyles.Normal;
        }
    }
}