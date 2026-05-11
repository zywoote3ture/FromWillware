using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        // 自动查找摄像机
        if (Camera.main != null)
            camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (camTransform != null)
        {
            // 核心逻辑：让容器的朝向与摄像机保持一致
            // 这种方式不会产生复杂的位移计算，最稳妥
            transform.rotation = camTransform.rotation;
        }
    }
}