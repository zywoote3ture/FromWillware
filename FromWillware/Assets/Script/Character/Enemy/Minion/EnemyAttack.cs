using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : Damage
{

    private Collider weaponCollider;
    private Enemy enemy;
    private Animator anim;

    // 攻击状态标记
    private bool hasDealtDamage = false;

    void Start()
    {
        weaponCollider = GetComponent<Collider>();
        enemy = GetComponentInParent<Enemy>();
        anim = enemy.GetComponent<Animator>();

        // 初始默认关闭碰撞
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    // 开启武器碰撞检测状态
    public void EnableWeapon()
    {
        hasDealtDamage = false;
        if (weaponCollider != null) weaponCollider.enabled = true;
    }

    // 关闭武器
    public void DisableWeapon()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 攻击玩家，并且一次攻击未成功击中
        if (other.CompareTag("Player") && !hasDealtDamage)
        {
            // 检查玩家是否正在弹反
            PlayerParry playerParry = other.GetComponent<PlayerParry>();
            if (playerParry != null && playerParry.IsParrying)
            {
                // 弹反成功，播放破防动画
                anim.SetTrigger("DoBreakDefense");
                Debug.Log("Enemy parried");
            }
            else
            {
                Debug.Log("enenmy attack:" + damage);
            }
            // 标记已经造成伤害
            hasDealtDamage = true;
            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
            }
        }
    }
}