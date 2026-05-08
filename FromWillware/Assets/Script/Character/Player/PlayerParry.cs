using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    public bool IsParrying;
    public float ParryWindow;
    public bool IsDefensing;
    
    [SerializeField]
    private Animator animator;
    private GetHit hitState;
    private PlayerState playerState;

    private PlayerInputHandler inputHandler;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        hitState = GetComponent<GetHit>();
        playerState = GetComponent<PlayerState>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        Defense();
    }

    public void Defense()
    {
        if (!playerState.CanParry) return;
        
        if (Input.GetKeyDown(KeyCode.K)||inputHandler.parryPressed)
        {
            animator.SetBool("IsDefensing", true);
            IsDefensing = true;
            // 开启弹反窗口（瞬间）
            StartCoroutine(Parry());
        }

        if (Input.GetKeyUp(KeyCode.K)||inputHandler.parryReleased)
        {
            animator.SetBool("IsDefensing", false);
            IsDefensing = false;
        }
    }

    IEnumerator Parry()
    {
        IsParrying = true;

        // 播放弹反动画
        //animator.SetTrigger("Parry");

        yield return new WaitForSeconds(ParryWindow);

        IsParrying = false;
    }
}
