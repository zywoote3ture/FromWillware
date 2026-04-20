// 文件名: BossWeapon.cs
using UnityEngine;

public class BossWeapon : Damage
{
    private Boss bossOwner;

    public void Initialize(Boss boss)
    {
        this.bossOwner = boss;
    }

    void OnTriggerEnter(Collider other)
    {
        if (bossOwner == null || !other.CompareTag("Player")) return;

        // 主动读取玩家的弹反状态
        PlayerParry playerParry = other.GetComponent<PlayerParry>();
        if (playerParry != null && playerParry.IsParrying)
        {
            // 通知Boss处理弹反事件
            bossOwner.GetParried();
        }
    }
}