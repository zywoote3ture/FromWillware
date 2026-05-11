using System.Collections.Generic;
using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    [Header("References")]
    public CameraFollow cameraFollow;
    public PlayerInputHandler inputHandler;

    [Header("Lock Settings")]
    public float LockRadius = 15f;
    public LayerMask EnemyLayer;

    private readonly List<Transform> targets = new();

    private int currentIndex = 0;

    void Start()
    {
        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();

        if (inputHandler == null)
            inputHandler = FindObjectOfType<PlayerInputHandler>();

        EnemyLayer = LayerMask.GetMask("Enemy");
    }

    void Update()
    {
        HandleLockInput();

        if (cameraFollow.CurrentTarget != null)
        {
            HandleSwitchTarget();

            // 目标失效
            if (cameraFollow.CurrentTarget.gameObject.activeInHierarchy == false)
            {
                ClearTarget();
            }
        }
    }

    void HandleLockInput()
    {
        if (inputHandler.lockPressed)
        {
            if (cameraFollow.CurrentTarget == null)
            {
                FindTargets();

                if (targets.Count > 0)
                {
                    currentIndex = 0;
                    cameraFollow.CurrentTarget = targets[currentIndex];
                }
            }
            else
            {
                ClearTarget();
            }
        }
    }

    void HandleSwitchTarget()
    {
        int dir = inputHandler.switchTargetDirection;

        if (dir == 0) return;

        SwitchTarget(dir);

        // ⭐ 消费输入
        inputHandler.ConsumeSwitchTarget();
    }

    void FindTargets()
    {
        targets.Clear();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            LockRadius,
            EnemyLayer
        );

        Camera cam = Camera.main;

        foreach (Collider hit in hits)
        {
            Transform target = hit.transform;

            // 忽略自己
            if (target == transform)
                continue;

            // 只锁屏幕前方目标
            Vector3 viewportPos =
                cam.WorldToViewportPoint(target.position);

            bool isVisible =
                viewportPos.z > 0 &&
                viewportPos.x > 0 &&
                viewportPos.x < 1 &&
                viewportPos.y > 0 &&
                viewportPos.y < 1;

            if (!isVisible)
                continue;

            targets.Add(target);
        }

        // ⭐ 按距离排序
        targets.Sort((a, b) =>
        {
            float da =
                Vector3.Distance(transform.position, a.position);

            float db =
                Vector3.Distance(transform.position, b.position);

            return da.CompareTo(db);
        });
    }

    void SwitchTarget(int dir)
    {
        if (targets.Count == 0)
            return;

        // 重新刷新目标列表
        FindTargets();

        if (targets.Count == 0)
        {
            ClearTarget();
            return;
        }

        currentIndex += dir;

        if (currentIndex >= targets.Count)
            currentIndex = 0;

        if (currentIndex < 0)
            currentIndex = targets.Count - 1;

        cameraFollow.CurrentTarget = targets[currentIndex];
    }

    void ClearTarget()
    {
        cameraFollow.CurrentTarget = null;

        // ⭐ 退出锁定后同步自由视角
        cameraFollow.SyncRotationFromCamera();

        targets.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            LockRadius
        );
    }
}