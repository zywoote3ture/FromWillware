using UnityEngine;

public class BillboardFixed : MonoBehaviour
{
    // 将变量改为 public，这样它会出现在 Inspector 面板中
    public Transform camTransform; 

    void Start()
    {
        // 如果你忘了在面板里拖拽，代码会自动尝试用 Tag 找一下
        if (camTransform == null && Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (camTransform != null)
        {
            transform.LookAt(camTransform.position);
            Vector3 rot = transform.rotation.eulerAngles;
            rot.x = 0;
            rot.z = 0;
            transform.rotation = Quaternion.Euler(rot);
        }
    }
}