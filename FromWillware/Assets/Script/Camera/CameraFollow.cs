using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform PlayerTransform;
    
    [Header("Mouse Settings")]
    public float MouseSensitivity= 200f;
    
    [Header("Camera Settings")]
    public float Distance= 3.5f;
    public float VerticalOffset = 1.5f;
    public float LookAtOffset = 1.5f;
    public float MinDistance = 0.6f;
    
    [Header("Rotation Settings")]
    public float xRotation = 30f;
    public float yRotation = 0f;
    
    [Header("Obstacle Settings")]
    public LayerMask ObstacleLayerMask = ~0;
    
    [Header("LockOn Settings")]
    public Transform CurrentTarget;
    public float LockRotationSpeed = 5f;

    private bool isResetting = false;
    private PlayerInputHandler inputHandler;
    void Start()
    {
        // 隐藏鼠标并锁定光标
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        inputHandler = PlayerTransform.GetComponent<PlayerInputHandler>();
    }

    void Update()
    {
        //if (isResetting) return;
        
        if (!IsLockOn())
        {
            Vector2 look = inputHandler.lookInput;

            bool isGamepad = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;

            float multiplier = isGamepad ? 1f : Time.deltaTime;
            
            float mouseX = look.x * MouseSensitivity * Time.deltaTime;
            float mouseY = look.y * MouseSensitivity * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -30f, 60f);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(SmoothReset());
        }
    }

    void LateUpdate()
    {
        //if (isResetting) return;
         
       if(IsLockOn())
            LockOnUpdate();
       else
           FreeLookUpdate();
    }

    bool IsLockOn()
    {
        return CurrentTarget != null;
    }

    void FreeLookUpdate()
    {
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
        Vector3 direction = rotation * new Vector3(0, 0, -Distance);

        Vector3 playerPosition = PlayerTransform.position + new Vector3(0, VerticalOffset, 0);
        
        // 检测镜头与玩家之间是否有遮挡
        float adjustedDistance = Distance;
        RaycastHit hit;
        if (Physics.Raycast(playerPosition, direction.normalized, out hit, Distance, ObstacleLayerMask))
        {
            // 如果有遮挡，将相机拉近到碰撞点
            adjustedDistance = hit.distance - 0.5f;
            adjustedDistance = Mathf.Max(adjustedDistance, MinDistance);
        }
        
        // 计算最终相机位置
        Vector3 adjustedDirection = rotation * new Vector3(0, 0, -adjustedDistance);
        Vector3 cameraPosition = playerPosition + adjustedDirection;
        
        transform.position = cameraPosition;
        transform.LookAt(PlayerTransform.position + new Vector3(0, LookAtOffset, 0));
    }

    // void LockOnUpdate()
    // {
    //     Vector3 playerPos = PlayerTransform.position + new Vector3(0, VerticalOffset, 0);
    //     Vector3 targetPos = CurrentTarget.position;
    //
    //     // 1️⃣ 只计算水平向量，不考虑y
    //     Vector3 dir = targetPos - PlayerTransform.position;
    //     dir.y = 0;
    //     if (dir.sqrMagnitude < 0.001f) dir = PlayerTransform.forward;
    //
    //     // 平滑旋转相机朝向敌人
    //     Quaternion targetRot = Quaternion.LookRotation(dir);
    //     transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * LockRotationSpeed);
    //
    //     // 相机位置
    //     Vector3 direction = transform.rotation * new Vector3(0, 0, -Distance);
    //
    //     float adjustedDistance = Distance;
    //     RaycastHit hit;
    //     if (Physics.Raycast(playerPos, direction.normalized, out hit, Distance, ObstacleLayerMask))
    //     {
    //         adjustedDistance = Mathf.Max(hit.distance - 0.5f, MinDistance);
    //     }
    //
    //     Vector3 finalPos = playerPos + direction.normalized * adjustedDistance;
    //     transform.position = finalPos;
    // }
    
    void LockOnUpdate()
    {
        Vector3 playerPos = PlayerTransform.position + new Vector3(0, VerticalOffset, 0);
        Vector3 targetPos = CurrentTarget.position + Vector3.up * LookAtOffset; // ⭐ 看高一点

        // 水平方向
        Vector3 dir = targetPos - PlayerTransform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) dir = PlayerTransform.forward;

        // ⭐ 保留俯仰角
        float targetY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(xRotation, targetY, 0);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * LockRotationSpeed
        );

        // 相机位置（和 FreeLook 一样逻辑）
        Vector3 direction = transform.rotation * new Vector3(0, 0, -Distance);

        float adjustedDistance = Distance;
        RaycastHit hit;
        if (Physics.Raycast(playerPos, direction.normalized, out hit, Distance, ObstacleLayerMask))
        {
            adjustedDistance = Mathf.Max(hit.distance - 0.5f, MinDistance);
        }

        transform.position = playerPos + direction.normalized * adjustedDistance;
    }
    
    public IEnumerator SmoothReset()
    {
        isResetting = true;

        float t = 0;
        float duration = 0.5f; // 慢一点更自然

        float startX = xRotation;
        float startY = yRotation;

        Vector3 forward = PlayerTransform.forward;
        float targetY = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        float targetX = 20f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float lerpT = t / duration;
            lerpT = Mathf.SmoothStep(0, 1, lerpT);

            // ⭐ 关键：用 LerpAngle！
            xRotation = Mathf.Lerp(startX, targetX, lerpT);
            yRotation = Mathf.LerpAngle(startY, targetY, lerpT);

            yield return null;
        }

        xRotation = targetX;
        yRotation = targetY;

        isResetting = false;
    }
    
    public void SyncRotationFromCamera()
    {
        Vector3 euler = transform.eulerAngles;

        float x = euler.x;
        float y = euler.y;

        if (x > 180f) x -= 360f;

        xRotation = Mathf.Clamp(x, -30f, 60f);
        yRotation = y;
    }
}