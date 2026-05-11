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

    public float switchTargetInput;
    public int switchTargetDirection;
    private bool switchAxisInUse;

    public bool switchWeapon;
    public bool resetCamera;

    public bool useItem1Pressed;
    public bool useItem2Pressed;
    public bool useItem3Pressed;
    public bool useItem4Pressed;

    public bool backPackPressed;

    public bool chooseItemUpPressed;
    public bool chooseItemDownPressed;
    public bool chooseItemLeftPressed;
    public bool chooseItemRightPressed;
    
    public bool setItem1Pressed;
    public bool setItem2Pressed;
    public bool setItem3Pressed;
    public bool setItem4Pressed;
    void Awake()
    {
        input = new PlayerInputActions();
    }

    private bool weaponSwitchConsumed;
    void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMoveCancel;

        input.Player.Look.performed += OnLook;
        input.Player.Look.canceled += OnLookCancel;
        
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

        input.Player.SwitchTarget.performed += ctx =>
        {
            float value = ctx.ReadValue<float>();

            // 防止连续触发
            if (!switchAxisInUse)
            {
                if (value > 0.5f)
                {
                    switchTargetDirection = 1;
                    switchAxisInUse = true;
                }
                else if (value < -0.5f)
                {
                    switchTargetDirection = -1;
                    switchAxisInUse = true;
                }
            }
        };

        input.Player.SwitchTarget.canceled += ctx =>
        {
            switchAxisInUse = false;
        };
        
        input.Player.WeaponSwitch.performed += ctx =>
        {
            if (weaponSwitchConsumed) return;

            switchWeapon = true;
            weaponSwitchConsumed = true;
        };
        
        input.Player.ResetCamera.performed += ctx => resetCamera = true;
        
        input.Player.UseItem1.performed += ctx => useItem1Pressed = true;
        input.Player.UseItem2.performed += ctx => useItem2Pressed = true;
        input.Player.UseItem3.performed += ctx => useItem3Pressed = true;
        input.Player.UseItem4.performed += ctx => useItem4Pressed = true;
        
        input.Player.BackPack.performed += ctx => backPackPressed = true;
        
        input.Player.ChooseItemUp.performed += ctx => chooseItemUpPressed = true;
        input.Player.ChooseItemDown.performed += ctx => chooseItemDownPressed = true;
        input.Player.ChooseItemLeft.performed += ctx => chooseItemLeftPressed = true;
        input.Player.ChooseItemRight.performed += ctx => chooseItemRightPressed = true;
        
        input.Player.SetItem1.performed += ctx => setItem1Pressed = true;
        input.Player.SetItem2.performed += ctx => setItem2Pressed = true;
        input.Player.SetItem3.performed += ctx => setItem3Pressed = true;
        input.Player.SetItem4.performed += ctx => setItem4Pressed = true;
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
        
        switchWeapon = false;
        weaponSwitchConsumed = false;

        resetCamera = false;
        
        useItem1Pressed = false;
        useItem2Pressed = false;
        useItem3Pressed = false;
        useItem4Pressed = false;
        
        backPackPressed = false;
        
        chooseItemUpPressed = false;
        chooseItemDownPressed = false;
        chooseItemLeftPressed = false;
        chooseItemRightPressed = false;

        setItem1Pressed = false;
        setItem2Pressed = false;
        setItem3Pressed = false;
        setItem4Pressed = false;
    }
    public void ConsumeSwitchTarget()
    {
        switchTargetDirection = 0;
    }
    
    void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void OnMoveCancel(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    void OnLookCancel(InputAction.CallbackContext ctx)
    {
        lookInput = Vector2.zero;
    }
}