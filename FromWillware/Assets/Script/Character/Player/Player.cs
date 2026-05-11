using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character,ISaveable
{
    public float MaxStamina;
    public float CurrentStamina;
    public float StaminaRecoverRate;
    public bool StaminaEmpty;
    public bool IsInventoryOn = false;

    private Animator animator;
    private PlayerState playerState;
   
    // Start is called before the first frame update
    void Start()
    {
        CurrentHP = MaxHP;
        CurrentStamina = MaxStamina;
        animator = GetComponent<Animator>();
        playerState = GetComponent<PlayerState>();
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
        if (!playerState.CanRecoverStamina) return;
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

        animator.SetBool("IsDefensing", false);

        animator.ResetTrigger("GetHit");
        animator.ResetTrigger("Roll");
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Parry");

        // 强制切死亡动画
        animator.CrossFade("Death", 0.01f);
        // 1. 播放死亡动画
        //animator.SetTrigger("Die");

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
    
    public string GetUniqueID()
    {
        return "Player";
    }

    public string CaptureState()
    {
        PlayerData data = new PlayerData()
        {
            MaxHP = this.MaxHP,
            MaxStamina = this.MaxStamina
        };
        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string state)
    {
        PlayerData data = JsonUtility.FromJson<PlayerData>(state);
        MaxHP = data.MaxHP;
        MaxStamina = data.MaxStamina;
        CurrentHP = data.MaxHP;
        CurrentStamina = data.MaxStamina;
    }
}
