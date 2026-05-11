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
    public AudioClip MoveAudio;
    public AudioSource audioSource;
    
    [SerializeField]
    private Rigidbody rb;

    private PlayerAttack playerAttack;
    private Vector2 inputDir;
    private Player player;
    private PlayerParry playerParry;
    private GetHit hitState;
    private PlayerState playerState;
    private PlayerInputHandler inputHandler;
    
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
        inputHandler = GetComponent<PlayerInputHandler>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerState.CanMove)
        {
            inputDir = Vector2.zero;

            rb.velocity = new Vector3(0, rb.velocity.y, 0);

            animator.SetFloat("Speed", 0);

            return; // 直接退出Update
        }
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

        if (inputHandler.runningPressed)
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
    public void PlayFootstep()
    {
        Vector3 horizontalVelocity =
            new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (horizontalVelocity.magnitude < 0.5f)
            return;

        //audioSource.PlayOneShot(MoveAudio);
    }

    public void Roll()
    {

        if (inputHandler.rollPressed && playerState.CanRoll)
        {
            player.ConsumeStamina(RollStamina);
            animator.SetTrigger("Roll");
        }
        else
        {
            return;
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
        if (!IsRolling) return;

        rb.MovePosition(rb.position + animator.deltaPosition);
    }
}
