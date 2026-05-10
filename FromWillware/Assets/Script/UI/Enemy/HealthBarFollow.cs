using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HealthBarFollow : MonoBehaviour
{
    public Transform target;       // 小怪的头部节点（或者小怪的根节点）
    public Vector3 offset;         // 偏置值，用于调整血条在头顶的高度
    public Camera mainCamera;      // 引用主相机

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 跟随位置
        transform.position = target.position + offset;

        // 2. 始终面向摄像机（让血条永远正对玩家）
        transform.rotation = mainCamera.transform.rotation;
    }
}