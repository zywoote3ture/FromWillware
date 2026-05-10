using UnityEngine;
using UnityEngine.AI;

public class Boss1 : Boss
{
    public override void Update()
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
            if (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                EndStaggerInternal();

            UpdateAnimator();
            return;
        }

        // 2. 处理技能执行
        if (isExecutingSkill)
        {
            float offset = (currentActiveSkill != null) ? currentActiveSkill.angleOffset : 0f;
            FaceTargetWithOffset(12f, offset);

            if (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f)
                OnSkillEnd();

            UpdateAnimator();
            return;
        }

        // 3. 处理技能前奏寻路
        if (isMovingToAttackDistance && pendingSkill != null)
        {
            UpdateRepositionLogic();
            UpdateAnimator();
            return;
        }

        // 4. 正常 AI 决策 
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

        // 调用父类的移动控制器
        moveController.Tick(currentDistance, playerTarget, optimalDistance, chaseSpeed, repositionSpeed);

        UpdateAnimator();
    }
}