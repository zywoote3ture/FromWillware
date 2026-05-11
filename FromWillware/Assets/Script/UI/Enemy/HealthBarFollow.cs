using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HealthBarFollow : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // 既然是子物体，位置会自动跟随，不需要在这里写 position 代码
        // 只需让它在每一帧都看向摄像机
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
