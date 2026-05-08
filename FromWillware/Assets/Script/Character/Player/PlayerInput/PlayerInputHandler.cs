using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions input;

    public Vector2 moveInput;
    public Vector2 lookInput;

    public bool attackPressed;
    public bool rollPressed;
    public bool interactPressed;
    public bool lockPressed;
    public bool parryPressed;   // 按下瞬间
    public bool parryHeld;      // 持续按住
    public bool parryReleased;  // 松开瞬间
    public bool runningPressed;
    
    public Vector2 itemInput;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        input.Player.Attack.performed += ctx => attackPressed = true;
        input.Player.Roll.performed += ctx => rollPressed = true;
        input.Player.Interact.performed += ctx => interactPressed = true;
        input.Player.Lock.performed += ctx => lockPressed = true;
        
        input.Player.Parry.performed += ctx =>
        {
            parryPressed = true;
            parryHeld = true;
        };

        input.Player.Parry.canceled += ctx =>
        {
            parryReleased = true;
            parryHeld = false;
        };
        
        input.Player.Running.performed += ctx => runningPressed = true;

        
    }

    void LateUpdate()
    {
        // 模拟 GetKeyDown（只触发一帧）
        attackPressed = false;
        rollPressed = false;
        interactPressed = false;
        lockPressed = false;
        
        parryPressed = false;
        parryReleased = false;

        runningPressed = false;
    }
}