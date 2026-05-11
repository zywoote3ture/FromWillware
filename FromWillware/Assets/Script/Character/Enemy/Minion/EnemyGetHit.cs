using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGetHit : MonoBehaviour
{
    private Enemy enemy;
    private Animator anim;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        anim = GetComponent<Animator>();
    }

    // 处理扣血和受伤动画
    public void TakeDamage(int damageAmount)
    {
        if (enemy.CurrentHP <= 0) return;

        enemy.ClearHeldArrow();

        enemy.CurrentHP -= damageAmount;
        anim.SetTrigger("DoHit");
        anim.ResetTrigger("DoAttack");
        Debug.Log(" The enemy get hit:" + damageAmount + ",left：" + enemy.CurrentHP);

        if (enemy.CurrentHP <= 0)
        {
            enemy.Die();
        }
    }

    // 触发器检测玩家的武器
    public void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("PlayerAttack"))
        {
            // 尝试获取玩家武器上的 Damage 脚本
            Damage playerDamage = other.GetComponent<Damage>();
            if (playerDamage != null)
            {
                TakeDamage(playerDamage.damage);
            }
        }
    }
}