using UnityEngine;
using UnityEngine.AI;

public class BossMoveController
{
    private Boss boss;
    private NavMeshAgent agent;

    private enum MoveState { Approach, Circle, Retreat }
    private MoveState currentState;

    private float stateTimer;
    private int circleDirection = 1;

    public BossMoveController(Boss boss, NavMeshAgent agent)
    {
        this.boss = boss;
        this.agent = agent;
    }

    public void Tick(float distance, Transform player, float optimalDistance, float chaseSpeed, float repositionSpeed)
    {
        stateTimer -= Time.deltaTime;

        bool needsImmediateReplan = false;
        if (currentState == MoveState.Approach && distance <= optimalDistance + 0.5f) needsImmediateReplan = true;
        if (currentState == MoveState.Retreat && distance >= optimalDistance - 0.5f) needsImmediateReplan = true;

        if (stateTimer <= 0 || needsImmediateReplan) DecideState(distance, optimalDistance);

        ExecuteState(distance, player, optimalDistance, chaseSpeed, repositionSpeed);
    }

    private void DecideState(float distance, float optimalDistance)
    {
        stateTimer = Random.Range(1.8f, 3.5f);
        if (distance > optimalDistance + 2.5f) currentState = MoveState.Approach;
        else if (distance < optimalDistance - 2.5f) currentState = MoveState.Retreat;
        else
        {
            currentState = MoveState.Circle;
            circleDirection = Random.value > 0.5f ? 1 : -1;
        }
    }

    private void ExecuteState(float distance, Transform player, float optimalDistance, float chaseSpeed, float repositionSpeed)
    {
        Vector3 offset = boss.transform.position - player.position;
        offset.y = 0;
        if (offset.sqrMagnitude < 0.01f) offset = boss.transform.forward;
        Vector3 toBossDir = offset.normalized;

        switch (currentState)
        {
            case MoveState.Approach:
                agent.speed = chaseSpeed;
                agent.SetDestination(player.position);
                break;
            case MoveState.Circle:
                agent.speed = repositionSpeed;
                Vector3 tangent = Vector3.Cross(Vector3.up, toBossDir) * circleDirection;
                Vector3 circleTarget = player.position + (toBossDir * optimalDistance) + (tangent * 4f);
                agent.SetDestination(circleTarget);
                break;
            case MoveState.Retreat:
                agent.speed = repositionSpeed;
                Vector3 retreatTarget = player.position + toBossDir * (optimalDistance + 5f);
                agent.SetDestination(retreatTarget);
                break;
        }
    }
}