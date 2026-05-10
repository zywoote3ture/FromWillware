using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class Boss : Character
{
    protected Animator anim;
    protected NavMeshAgent agent;

    [Header("Boss")]
    public string bossID = "Boss_1";
    public Transform playerTarget;

    [Header("移动参数")]
    public float activationRange = 20f;
    public float optimalDistance = 8f;
    public float chaseSpeed = 5f;
    public float repositionSpeed = 3.5f;
    public float actionCooldown = 3f;

    [Header("冲刺碰撞参数")]
    public float dashTriggerRange = 12f;
    public float dashSpeed = 16f;         
    public float dashCooldown = 6f;
    public float dashMaxDuration = 1.5f;
    protected float lastDashTime = -999f;
    protected bool isDashing = false;
    protected float dashTimer = 0f;
    protected Vector3 dashTargetPoint;

    [Header("破防系统")]
    public float staggerThreshold = 100f;
    public float currentStagger = 0f;

    [Header("二阶段设置")]
    public bool hasPhaseTwo = true;           
    protected float phaseTwoHPPercent = 0.5f;    
    public float phaseTwoCooldown = 3f;
    public GameObject minionPrefabA;
    public GameObject minionPrefabB;
    public Transform spawnPointA;
    public Transform spawnPointB;
    protected bool isPhaseTwoActive = false;  

    [Header("音效参数")]
    public AudioSource audioSource;
    public AudioClip hitSound;      
    public AudioClip breakSound;
    public AudioClip summonSound;
    public AudioClip dashSound;     
    public AudioClip deathSound;

    [Header("经验参数")]
    public int expReward = 500;

    [HideInInspector] public bool isExecutingSkill { get; protected set; }

    protected BossSkill currentActiveSkill;
    protected BossSkill pendingSkill;

    protected bool isMovingToAttackDistance = false;
    protected float attackDistanceTolerance = 0.5f;

    protected float lastActionTime = -999f;
    protected List<BossSkill> skills;
    protected BossMoveController moveController;
    protected BossSkillSelector skillSelector;

    protected bool isInStagger = false;
    protected float pushDistanceRemaining = 0f;

    protected Collider bodyCollider;

   
    public void Awake() 
    {
        PlayerPrefs.DeleteAll();
        // 检查 PlayerPrefs，如果该 ID 对应的值为 1，说明已经死过了
        if (PlayerPrefs.GetInt(bossID + "_IsDead", 0) == 1)
        {
            Destroy(gameObject); 
        }
    }

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        CurrentHP = MaxHP;

        agent.updateRotation = false;
        agent.stoppingDistance = 0f;
        agent.acceleration = 25f;
        agent.autoBraking = false;
        lastActionTime = Time.time;
        anim.applyRootMotion = false;

        bodyCollider = GetComponent<Collider>();
        if (bodyCollider != null) bodyCollider.isTrigger = false;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        skills = GetComponents<BossSkill>().ToList();
        foreach (var s in skills)
            s.Initialize(this);

        moveController = new BossMoveController(this, agent);
        skillSelector = new BossSkillSelector(this, skills);

        if (playerTarget == null)
            playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // 碰撞/弹反逻辑
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Damage playerDamage = other.GetComponent<Damage>();
            if (playerDamage != null) TakeDamage(playerDamage.damage);
        }

        if (isDashing && other.CompareTag("Player"))
        {
            PlayerParry playerParry = other.GetComponent<PlayerParry>();
            if (playerParry != null && playerParry.IsParrying)
            {
                GetParried();
            }
           
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead || CurrentHP <= 0) return;
        CurrentHP -= damageAmount;
        currentStagger += damageAmount;
        if (audioSource && hitSound) audioSource.PlayOneShot(hitSound);

        if (hasPhaseTwo && !isPhaseTwoActive && (float)CurrentHP / MaxHP <= phaseTwoHPPercent)
        {
            EnterPhaseTwo();
            return;
        }

        if (CurrentHP <= 0) { Die(); return; }

        bool isHyper = isExecutingSkill && currentActiveSkill != null && currentActiveSkill.isHyperArmor;
        if (!isHyper && currentStagger >= staggerThreshold)
        {
            currentStagger = 0f;
            TriggerBreak();
        }
    }

    public virtual void Update()
    {
        if (IsDead || playerTarget == null) return;

        float currentDistance = Vector3.Distance(transform.position, playerTarget.position);

        // 1. 处理受击硬直
        if (isInStagger)
        {
            if (pushDistanceRemaining > 0)
            {
                float moveStep = 4f * Time.deltaTime;
                if (moveStep > pushDistanceRemaining) moveStep = pushDistanceRemaining;
                agent.Move(-transform.forward * moveStep);
                pushDistanceRemaining -= moveStep;
            }
            if (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) EndStaggerInternal();
            UpdateAnimator();
            return;
        }

        // 2. 处理冲刺碰撞逻辑
        if (isDashing)
        {
            UpdateDashLogic();
            UpdateAnimator();
            return;
        }

        // 3. 处理技能执行/前奏
        if (isExecutingSkill)
        {
            anim.applyRootMotion = true;
            float offset = (currentActiveSkill != null) ? currentActiveSkill.angleOffset : 0f;
            FaceTargetWithOffset(12f, offset);
            if (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f) OnSkillEnd();
            UpdateAnimator();
            return;
        }

        if (isMovingToAttackDistance && pendingSkill != null)
        {
            UpdateRepositionLogic();
            UpdateAnimator();
            return;
        }

        // 4. 冲刺触发检测
        if (currentDistance > dashTriggerRange && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
            return;
        }

        // 5. 正常 AI 逻辑
        if (currentDistance > activationRange)
        {
            agent.isStopped = true;
            UpdateAnimator();
            return;
        }

        if (Time.time >= lastActionTime + actionCooldown)
        {
            var skill = skillSelector.ChooseSkill(currentDistance);
            if (skill != null)
            {
                pendingSkill = skill;
                isMovingToAttackDistance = true;
                return;
            }
        }

        agent.isStopped = false;
        FaceTarget(8f);
        moveController.Tick(currentDistance, playerTarget, optimalDistance, chaseSpeed, repositionSpeed);
        UpdateAnimator();
    }


    protected void StartDash()
    {
        isDashing = true;
        dashTimer = 0f;
        lastDashTime = Time.time;

        if (bodyCollider != null) bodyCollider.isTrigger = true;

        // 目标点设在玩家身后 4 米
        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        dashTargetPoint = playerTarget.position + dirToPlayer * 4.0f;

        agent.isStopped = true;
        agent.ResetPath();
        gameObject.tag = "EnemyAttack";
        anim.SetBool("IsDashing", true);
        if (audioSource && dashSound) audioSource.PlayOneShot(dashSound);
    }

    protected void UpdateDashLogic()
    {
        dashTimer += Time.deltaTime;

        Vector3 dir = (dashTargetPoint - transform.position).normalized;
        agent.Move(dir * dashSpeed * Time.deltaTime);

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);

        if (dashTimer >= dashMaxDuration || Vector3.Distance(transform.position, dashTargetPoint) < 0.8f)
        {
            EndDash();
        }
    }

    protected void EndDash()
    {
        isDashing = false;
        gameObject.tag = "Enemy";
        anim.SetBool("IsDashing", false);
        agent.isStopped = false;
        lastActionTime = Time.time;
        if (bodyCollider != null) bodyCollider.isTrigger = false;
    }


    protected void UpdateRepositionLogic()
    {
        float moveDistance = Vector3.Distance(transform.position, playerTarget.position);
        if (Mathf.Abs(moveDistance - pendingSkill.attackDistance) <= attackDistanceTolerance)
        {
            isMovingToAttackDistance = false;
            ExecuteSkill(pendingSkill);
            pendingSkill = null;
            return;
        }
        Vector3 dir = (transform.position - playerTarget.position).normalized;
        Vector3 targetPos = playerTarget.position + dir * pendingSkill.attackDistance;
        agent.speed = repositionSpeed;
        agent.SetDestination(targetPos);
        FaceTarget(8f);
    }

    public void GetParried()
    {
        if (isDashing) EndDash();
        if (!isExecutingSkill && !isDashing) return;
        if (currentActiveSkill != null && currentActiveSkill.isHyperArmor) return;
        StartStaggerLogic(0f);
        anim.SetTrigger("DoStagger_Minor");
    }

    protected void TriggerBreak()
    {
        if (isDashing) EndDash();
        StartStaggerLogic(2f);
        anim.SetTrigger("DoHit");
        if (audioSource && breakSound) audioSource.PlayOneShot(breakSound);
    }

    protected void StartStaggerLogic(float pushDist)
    {
        if (bodyCollider != null) bodyCollider.isTrigger = true;
        if (currentActiveSkill != null) currentActiveSkill.DisableWeapon();
        isExecutingSkill = false;
        currentActiveSkill = null;
        pendingSkill = null;
        isMovingToAttackDistance = false;
        isInStagger = true;
        pushDistanceRemaining = pushDist;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    protected void EndStaggerInternal()
    {
        isInStagger = false;
        pushDistanceRemaining = 0f;
        agent.isStopped = false;
        if (bodyCollider != null) bodyCollider.isTrigger = false;
    }

    protected void ExecuteSkill(BossSkill skill)
    {
        isExecutingSkill = true;
        currentActiveSkill = skill;
        agent.isStopped = true;
        agent.ResetPath();
        skill.Use();
        lastActionTime = Time.time;
    }

    public void OnSkillStart(BossSkill skill)
    {
        isExecutingSkill = true;
        currentActiveSkill = skill;
        agent.isStopped = true;
    }

    public void OnSkillEnd()
    {
        if (currentActiveSkill != null) currentActiveSkill.DisableWeapon();
        isExecutingSkill = false;
        currentActiveSkill = null;
        agent.isStopped = false;
        anim.applyRootMotion = false;
    }

    public override void Die()
    {
        if (isDashing) EndDash();
        if (currentActiveSkill != null) currentActiveSkill.DisableWeapon();
        agent.isStopped = true;
        agent.enabled = false;
        bodyCollider.enabled = false; 

        IsDead = true;
        anim.SetTrigger("DoDeath");
        if (audioSource && deathSound) audioSource.PlayOneShot(deathSound);

        if (playerTarget != null)
        {
            LevelSystem ls = playerTarget.GetComponent<LevelSystem>();
            if (ls != null)
            {
                ls.exp += expReward;
                ls.LevelUp();
            }
        }

        Destroy(gameObject, 7f);

        PlayerPrefs.SetInt(bossID + "_IsDead", 1); 
        PlayerPrefs.Save(); // 强制保存到硬盘
    }

    public void FaceTarget(float speed)
    {
        Vector3 dir = (playerTarget.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * speed);
    }

   public void FaceTargetWithOffset(float speed, float offsetAngle)
    {
        Vector3 dir = (playerTarget.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion baseRotation = Quaternion.LookRotation(dir);
        Quaternion offsetRotation = Quaternion.Euler(0, offsetAngle, 0);
        Quaternion targetRotation = baseRotation * offsetRotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * speed);
    }

    public void UpdateAnimator()
    {
        Vector3 currentVelocity = isDashing ? transform.forward * dashSpeed : agent.velocity;
        Vector3 local = transform.InverseTransformDirection(currentVelocity);
        anim.SetFloat("VelocityX", local.x / chaseSpeed, 0.15f, Time.deltaTime);
        anim.SetFloat("VelocityZ", local.z / chaseSpeed, 0.15f, Time.deltaTime);
    }

    // 音乐接口
    public void PlaySFX(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // 进入二阶段
    public virtual void EnterPhaseTwo()
    {
        isPhaseTwoActive = true;

        actionCooldown = phaseTwoCooldown;
        Debug.Log("Boss进入二阶段，攻击冷却缩短为: " + actionCooldown);
        
        if (isDashing) EndDash();
        isExecutingSkill = true; 
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        anim.SetTrigger("DoSummon");

        SummonMinions();
    }

    public void SummonMinions()
    {
        if (spawnPointA != null)
            Instantiate(minionPrefabA, spawnPointA.position, spawnPointA.rotation);

        if (spawnPointB != null)
            Instantiate(minionPrefabB, spawnPointB.position, spawnPointB.rotation);

        if (audioSource && summonSound) audioSource.PlayOneShot(summonSound);
    }

}