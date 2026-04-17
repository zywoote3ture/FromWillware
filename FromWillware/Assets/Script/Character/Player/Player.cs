using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public float MaxStamina;
    public float CurrentStamina;
    public float StaminaRecoverRate;
    public bool StaminaEmpty;
    

    private Animator animator;
   
    // Start is called before the first frame update
    void Start()
    {
        CurrentHP = MaxHP;
        CurrentStamina = MaxStamina;
        animator = GetComponent<Animator>();
     
    }

    // Update is called once per frame
    void Update()
    {
        RecoverStamina();
        if (CurrentStamina <= 1)
        {
            StaminaEmpty = true;
        }
        else
        {
            StaminaEmpty = false;
        }
    }

    public void ConsumeStamina(float amount)
    {
        CurrentStamina -= amount;
    }

    void RecoverStamina()
    {
        if (CurrentStamina <= MaxStamina)
        {
            CurrentStamina += StaminaRecoverRate * Time.deltaTime;
            CurrentStamina = Mathf.Clamp(CurrentStamina, -20, MaxStamina);//限制精力的范围0~MaxStamina
        }
    }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;

        // 1. 播放死亡动画
        animator.SetTrigger("Die");

        // 2. 禁用移动、攻击、防御脚本
        var move = GetComponent<PlayerMove>();
        if (move != null) move.enabled = false;

        var attack = GetComponent<PlayerAttack>();
        if (attack != null) attack.enabled = false;

        var parry = GetComponent<PlayerParry>();
        if (parry != null) parry.enabled = false;

        // 3. 可选：锁定刚体，防止被推动
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true; // 阻止物理干扰
        }

        // 4. 可选：锁定相机（视角不跟随）
        // var camFollow = Camera.main.GetComponent<CameraFollow>();
        // if (camFollow != null) camFollow.enabled = false;
    }
    
    public void PlayDrinkAnim()
    {
        animator.SetTrigger("Drink");
    }
}
