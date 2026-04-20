using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Character
{
    private Animator anim;
    private NavMeshAgent agent;

    [Header("目标设置")]
    public Transform playerTarget;
    private Player targetPlayerScript;

    [Header("战斗参数")]
    public float attackRange = 2.0f;
    public float attackCooldown = 2.0f;
    public float activationRange = 8.0f; 

    private bool isActivated = false;
    public bool isDead = false;

    private float lastAttackTime = -999f;

    private EnemyAttack enemyWeapon;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true;
        enemyWeapon = GetComponentInChildren<EnemyAttack>();

        //  初始化血量（使用 Character 基类的变量）
        CurrentHP = MaxHP;

        if (playerTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        if (playerTarget != null)
        {
            targetPlayerScript = playerTarget.GetComponent<Player>();
        }
    }

    protected virtual void Update()
    {

        if (isDead || playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (!isActivated)
        {
            // 如果未激活，只检查玩家是否进入激活范围
            if (distanceToPlayer <= activationRange)
            {
                isActivated = true;
            }
            return;
        }
        // 玩家跑出激活范围，取消激活
        if (distanceToPlayer > activationRange)
        {
            isActivated = false;
            agent.isStopped = true;
            anim.SetBool("isWalking", false);
            //agent.SetDestination(originalPosition); 
            return;
        }

        // 防止鞭尸
        if (targetPlayerScript != null && targetPlayerScript.IsDead == true)
        {
            agent.isStopped = true;             
            anim.SetBool("isWalking", false);    
            anim.ResetTrigger("DoAttack");      
            return;                              
        }

        // 硬直检测
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
     
        if (stateInfo.IsName("Attack") || stateInfo.IsName("Hit") || stateInfo.IsName("BreakDefense"))
        {
            agent.isStopped = true;
            return; // 拦截下面的寻路和攻击逻辑
        }


        //追击和攻击
        if (distanceToPlayer > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
            anim.SetBool("isWalking", true);
        }
        else
        {
            agent.isStopped = true;
            anim.SetBool("isWalking", false);

            Vector3 direction = (playerTarget.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                anim.SetTrigger("DoAttack");
               
            }
        }
    }


    // 重写基类死亡方法
    public override void Die()
    {
        base.Die(); 

        isDead = true;
        anim.SetTrigger("DoDeath");
        agent.isStopped = true;
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        anim.applyRootMotion = true;
        //销毁尸体
        Destroy(gameObject, 5f);
    }

    // 开启武器伤害与重置锁
    public void EnableWeaponHitboxEvent()
    {
        if (enemyWeapon != null) enemyWeapon.EnableWeapon();
    }

    // 关闭武器伤害
    public void DisableWeaponHitboxEvent()
    {
        if (enemyWeapon != null) enemyWeapon.DisableWeapon();
    }
}