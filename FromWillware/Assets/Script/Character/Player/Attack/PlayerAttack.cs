using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    private PlayerMove playerMove;
    private Rigidbody rb;
    private Player player;
    private PlayerParry playerParry;
    private WeaponSystem weaponSystem;
    private Transform currentWeapon;
    private Collider currentWeaponCollider;
    private GetHit hitState;
    private PlayerState playerState;
    private PlayerInputHandler inputHandler;
    
    private bool canCombo = false;
    private bool inputBuffered = false;
    
    public int comboStep = 0;
    public bool IsAttacking;
    public bool EnableAttacking = true;
    
    // Start is called before the first frame update
    void Start()
    {
        playerMove = GetComponent<PlayerMove>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        playerParry = GetComponent<PlayerParry>();
        weaponSystem = GetComponent<WeaponSystem>();
        hitState = GetComponent<GetHit>();
        playerState = GetComponent<PlayerState>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        currentWeapon = weaponSystem.CurrentWeapon;
        if(currentWeapon!=null) 
            currentWeaponCollider = FindChildWithTag(currentWeapon.transform,"PlayerAttack").GetComponent<Collider>();
        Attack();
        FixIsAttacking();
    }
    
    Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }

            // 递归查找子物体的子物体
            Transform result = FindChildWithTag(child, tag);
            if (result != null)
                return result;
        }
        return null;
    }

    void Attack()
    {
        if ((Input.GetKeyDown(KeyCode.J)||inputHandler.attackPressed)&&playerState.CanAttack)
        {
           
            if (IsAttacking)
            {
                inputBuffered = true;
            }
            else
            {
                StartAttack();
            }
            
        }
    }

    void StartAttack()
    {
        if (IsAttacking) return;
        if(currentWeapon==null) return;
        var weapon = currentWeapon.GetComponent<WeaponPickup>();
    
        //animator.SetFloat("AttackSpeed", weapon.AttackSpeed);
        

        rb.velocity = new Vector3(0, rb.velocity.y, 0);

        comboStep = 1;
        if (comboStep == 1)
        {
            animator.SetTrigger("Combo1");
            player.ConsumeStamina(weapon.weaponData.ConsumingStamina);
        }
        IsAttacking = true;
    }

    public void EnableCombo()
    {
        canCombo = true;

        if (inputBuffered)
        {
            inputBuffered = false;
            DoNextCombo();
        }
    }
    
    void DoNextCombo()
    {
        var weapon = currentWeapon.GetComponent<WeaponPickup>();
        if (!canCombo) return;

        comboStep++;
        
        if(comboStep==2)
        {
            animator.SetTrigger("Combo2");
            player.ConsumeStamina(weapon.weaponData.ConsumingStamina);
        }
        else if(comboStep==3)
        {
            animator.SetTrigger("Combo3");
            player.ConsumeStamina(weapon.weaponData.ConsumingStamina);
        }
        
        canCombo = false;
    }
    
    public void ResetCombo()
    {
        animator.SetInteger("ComboIndex", 0);
        comboStep = 0;
        IsAttacking = false;
        canCombo = false;
        inputBuffered = false;
        Debug.Log("Combo Reset");
    }
    
    public void SetIsAttacking()
    {
        IsAttacking = true;
    }

    public void ResetIsAttacking()
    {
        IsAttacking = false;
    }

    public void EnableWeapon()
    {
        Debug.Log("Weapon On");
        //Debug.Log("开启的Collider是: " + currentWeaponCollider.name);
        currentWeaponCollider.enabled = true;
    }

    public void DisableWeapon()
    {
        
        currentWeaponCollider.enabled = false;
    }
    
    [SerializeField]
    private bool hasEnteredAttack = false;

    public void FixIsAttacking()
    {
        if (animator.IsInTransition(0)) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        bool isAttackState =
            stateInfo.IsName("Combo1") ||
            stateInfo.IsName("Combo2") ||
            stateInfo.IsName("Combo3");

        // ✅ 标记：已经进入攻击动画
        if (isAttackState)
        {
            hasEnteredAttack = true;
        }

        // ✅ 只有真正进入过攻击，才允许退出
        if (hasEnteredAttack && !isAttackState)
        {
            IsAttacking = false;
            hasEnteredAttack = false;
        }
    }
}
