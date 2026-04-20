using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class Boss : Character
{
    private Animator anim;
    private NavMeshAgent agent;

    [Header("目标")]
    public Transform playerTarget;

    [Header("移动参数")]
    public float activationRange = 20f;
    public float optimalDistance = 8f;
    public float chaseSpeed = 5f;
    public float repositionSpeed = 3.5f;
    public float actionCooldown = 3f;

    [Header("破防系统")]
    public float staggerThreshold = 100f;
    public float staggerResetTime = 3f;
    public float staggerDecaySpeed = 20f;

    private float staggerValue = 0f;
    private float lastHitTime = -999f;

    [HideInInspector] public bool isExecutingSkill { get; private set; }

    private BossSkill currentActiveSkill;
    private BossSkill pendingSkill;

    private bool isMovingToAttackDistance = false;
    private float attackDistanceTolerance = 0.5f;

    private float lastActionTime = -999f;
    private List<BossSkill> skills;
    private BossMoveController moveController;
    private BossSkillSelector skillSelector;

    private float pauseTimer = 0f;
    private bool isInStagger = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        CurrentHP = MaxHP;

        agent.updateRotation = false;
        agent.stoppingDistance = 0f;
        agent.acceleration = 25f;
        agent.autoBraking = false;

        anim.applyRootMotion = false;

        skills = GetComponents<BossSkill>().ToList();
        foreach (var s in skills)
            s.Initialize(this);

        moveController = new BossMoveController(this, agent);
        skillSelector = new BossSkillSelector(this, skills);

        if (playerTarget == null)
            playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (IsDead || playerTarget == null) return;

        transform.position = agent.nextPosition;

        float currentDistance = Vector3.Distance(transform.position, playerTarget.position);

        // ===== 破防条衰减 =====
        if (Time.time > lastHitTime + staggerResetTime)
        {
            staggerValue -= Time.deltaTime * staggerDecaySpeed;
            staggerValue = Mathf.Max(0f, staggerValue);
        }

        // ===== 破防状态 =====
        if (isInStagger)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            if (!anim.IsInTransition(0) && state.normalizedTime >= 1f)
                EndStaggerInternal();

            UpdateAnimator();
            return;
        }

        // ===== 后摇 =====
        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;

            if (pauseTimer <= 0)
            {
                anim.speed = 1f;
                agent.isStopped = false;
            }

            UpdateAnimator();
            return;
        }

        // ===== 移动到技能攻击距离 =====
        if (isMovingToAttackDistance && pendingSkill != null)
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
            UpdateAnimator();
            return;
        }

        // ===== 技能播放中 =====
        if (isExecutingSkill)
        {
            FaceTarget(10f);

            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            if (!anim.IsInTransition(0) && state.normalizedTime >= 1f)
                OnSkillEnd();

            UpdateAnimator();
            return;
        }

        // ===== 超出激活范围 =====
        if (currentDistance > activationRange)
        {
            agent.isStopped = true;
            UpdateAnimator();
            return;
        }

        // ===== 技能选择 =====
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

        // ===== 常规移动 =====
        agent.isStopped = false;
        FaceTarget(8f);
        moveController.Tick(currentDistance, playerTarget, optimalDistance, chaseSpeed, repositionSpeed);

        UpdateAnimator();
    }

    void OnAnimatorMove()
    {
        if (!isInStagger) return;

        agent.nextPosition += anim.deltaPosition;
        transform.rotation *= anim.deltaRotation;
    }

    // =========================
    //  玩家攻击入口
    // =========================
    public void OnHitByPlayer(int damage)
    {
        if (IsDead) return;

        CurrentHP -= damage;
        CurrentHP = Mathf.Max(0, CurrentHP);

        if (CurrentHP <= 0)
        {
            Die();
            return;
        }

        lastHitTime = Time.time;
        staggerValue += damage;

        bool isHyper = isExecutingSkill &&
                       currentActiveSkill != null &&
                       currentActiveSkill.isHyperArmor;

        if (!isHyper && staggerValue >= staggerThreshold)
        {
            staggerValue = 0f;
            TriggerBreak();
        }
    }

    // =========================
    //  弹反
    // =========================
    public void GetParried()
    {
        if (!isExecutingSkill || currentActiveSkill == null)
            return;

        if (currentActiveSkill.isHyperArmor)
            return;

        int level = currentActiveSkill.parryStaggerLevel;

        StartStaggerLogic();

        if (level == 0)
            anim.SetTrigger("DoStagger_Minor");
        else
            anim.SetTrigger("DoStagger_Major");
    }

    private void TriggerBreak()
    {
        StartStaggerLogic();
        anim.SetTrigger("DoHit");
    }

    private void StartStaggerLogic()
    {
        isExecutingSkill = false;
        currentActiveSkill = null;

        isInStagger = true;
        pauseTimer = 0f;
        anim.speed = 1f;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private void EndStaggerInternal()
    {
        isInStagger = false;
        agent.nextPosition = transform.position;
        agent.isStopped = false;
    }

    // =========================
    //  技能
    // =========================
    private void ExecuteSkill(BossSkill skill)
    {
        isExecutingSkill = true;
        currentActiveSkill = skill;
        agent.isStopped = true;
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
        isExecutingSkill = false;
        currentActiveSkill = null;
        agent.isStopped = false;
    }

    public override void Die()
    {
        agent.isStopped = true;
        IsDead = true;
        anim.SetTrigger("DoDeath");
    }

    public void ExecutePause(float duration)
    {
        pauseTimer = duration;
        agent.isStopped = true;
        anim.speed = 0f;
    }

    public void ExecutePause()
    {
        ExecutePause(0.3f);
    }

    void FaceTarget(float speed)
    {
        Vector3 dir = (playerTarget.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * speed);
    }

    void UpdateAnimator()
    {
        Vector3 local = transform.InverseTransformDirection(agent.velocity);

        anim.SetFloat("VelocityX", local.x / chaseSpeed, 0.15f, Time.deltaTime);
        anim.SetFloat("VelocityZ", local.z / chaseSpeed, 0.15f, Time.deltaTime);
    }
}