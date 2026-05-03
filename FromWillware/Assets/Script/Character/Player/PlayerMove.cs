using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float MoveSpeed = 5f;
    public float RunningSpeed = 8f;
    public Transform CameraTransform;
    public Animator animator;
    public bool IsRolling = false;
    public bool NextRolling;
    public float RollStamina;
    public bool IsRunning = false;
    
    [SerializeField]
    private Rigidbody rb;

    private PlayerAttack playerAttack;
    private Vector2 inputDir;
    private Player player;
    private PlayerParry playerParry;
    private GetHit hitState;
    private PlayerState playerState;
    
    // Start is called before the first frame update
    void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
        player = GetComponent<Player>();
        NextRolling = true;
        rb = GetComponent<Rigidbody>();
        playerParry = GetComponent<PlayerParry>();
        hitState = GetComponent<GetHit>();
        playerState = GetComponent<PlayerState>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerState.CanMove)
        {
            inputDir.x = Input.GetAxis("Horizontal");
            inputDir.y = Input.GetAxis("Vertical");
        }
        else
        {
            inputDir = Vector2.zero; // 翻滚或攻击时清零
        }
        Move();
        Roll();
    }

    void Move()
    {
        float h = inputDir.x;
        float v = inputDir.y;

        Vector3 forward = CameraTransform.forward;
        Vector3 right = CameraTransform.right;

        // 去掉上下分量（防止飞起来）
        forward.y = 0;
        right.y = 0;
        
        Vector3 move = forward * v + right * h;
        move = move.normalized;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            IsRunning = !IsRunning;
        }
        
        // 移动
        float currentSpeed = IsRunning ? RunningSpeed : MoveSpeed;
        float inputMagnitude = new Vector2(h, v).magnitude;
        float speedPercent = IsRunning ? inputMagnitude : inputMagnitude * 0.5f;
        
        animator.SetFloat("Speed",speedPercent);

        if (move != Vector3.zero)
        {
            Vector3 targetVelocity = new Vector3(
                move.x * currentSpeed,
                rb.velocity.y,
                move.z * currentSpeed
            );
            // 平滑过渡（关键！）
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, 10f * Time.deltaTime);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
        
        if (move != Vector3.zero)
        {
            transform.forward = move;
        }
        
    }

    public void Roll()
    {
        if (playerAttack.IsAttacking) return;

        if (Input.GetKeyDown(KeyCode.Space) && NextRolling && !player.StaminaEmpty)
        {
            player.ConsumeStamina(RollStamina);
            animator.SetTrigger("Roll");
        }
            
    }

    //设置IsRolling
    public void SetIsRolling()
    {
        IsRolling = true;
        //rb.velocity = Vector3.zero;
        
    }

    public void ResetIsRolling()
    {
        IsRolling = false;
        //rb.velocity = Vector3.zero;
        //inputDir = Vector2.zero;
    }
    
    //设置NextRolling
    public void SetNextRolling()
    {
        NextRolling = false;
    }

    public void ResetNextRolling()
    {
        NextRolling = true;
    }
    
    void OnAnimatorMove()
    {
        if (IsRolling) // 只有翻滚时才用动画位移
        {
            rb.velocity = animator.deltaPosition / Time.deltaTime;
        }
    }
}
