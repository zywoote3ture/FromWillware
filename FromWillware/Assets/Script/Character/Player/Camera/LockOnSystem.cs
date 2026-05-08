using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    public CameraFollow cameraFollow;
    public float LockRadius = 10f;
    public LayerMask EnemyLayer;
    
    private List<Transform> targets = new List<Transform>();
    private int index = 0;
    private PlayerInputHandler inputHandler;
    
    // Start is called before the first frame update
    void Start()
    {
        cameraFollow =  FindObjectOfType<CameraFollow>();
        EnemyLayer = LayerMask.GetMask("Enemy");
        inputHandler = FindObjectOfType<PlayerInputHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)||inputHandler.lockPressed)
        {
            if (cameraFollow.CurrentTarget == null)
            {
                FindTarget();
            }

            else
            {
                ClearTarget();
            }
                
        }

        if (cameraFollow.CurrentTarget != null)
        {
            if(Input.GetKeyDown(KeyCode.Q)) SwitchTarget(-1);
            if (Input.GetKeyDown(KeyCode.E)) SwitchTarget(1);
        }
    }
    
    void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position,LockRadius,EnemyLayer);
        targets.Clear();
        foreach (Collider hit in hits)
            targets.Add(hit.transform);
        
        if (targets.Count == 0) return;
        index = 0;
        cameraFollow.CurrentTarget = targets[index];
    }
    
    void SwitchTarget(int dir)
    {
        if (targets.Count == 0) return;
        index = (index +dir +targets.Count) % targets.Count;
        cameraFollow.CurrentTarget = targets[index];
    }

    void ClearTarget()
    {
        cameraFollow.CurrentTarget = null;
        // ⭐ 核心：把当前相机角度写回自由视角系统
        cameraFollow.SyncRotationFromCamera();
        targets.Clear();
    }
}
