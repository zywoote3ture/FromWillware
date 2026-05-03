using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // Start is called before the first frame update
    private Player player;
    private PlayerMove playerMove;
    private PlayerAttack playerAttack;
    private PlayerParry playerParry;
    private GetHit hitState;

    public bool CanMove;
    public bool CanAttack;
    public bool CanParry;
    public bool CanGetHit;
    public bool CanRecoverStamina;
    
    void Start()
    {
        player = GetComponent<Player>();
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerParry = GetComponent<PlayerParry>();
        hitState = GetComponent<GetHit>();
    }

    // Update is called once per frame
    void Update()
    {
        SetCanMove();
        SetCanAttack();
        SetCanParry();
        SetCanGetHit();
        SetCanRecoverStamina();
    }

    void SetCanMove()
    {
        CanMove = !playerMove.IsRolling && !playerAttack.IsAttacking && !playerParry.IsDefensing && !hitState.IsGetHit;
    }

    void SetCanAttack()
    {
        CanAttack = !playerMove.IsRolling && !player.StaminaEmpty && !playerParry.IsDefensing && !hitState.IsGetHit;
    }

    void SetCanParry()
    {
        CanParry = !hitState.IsGetHit && !playerMove.IsRolling && !playerAttack.IsAttacking;
    }

    void SetCanGetHit()
    {
        CanGetHit = !hitState.IsGetHit && !playerAttack.IsAttacking;
    }

    void SetCanRecoverStamina()
    {
        CanRecoverStamina = !playerAttack.IsAttacking && !playerMove.IsRolling;
    }
}
