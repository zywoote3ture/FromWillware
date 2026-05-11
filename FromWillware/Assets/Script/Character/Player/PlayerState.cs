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
    public bool CanRoll;
    public bool CanAttack;
    public bool CanParry;
    public bool CanGetHit;
    public bool CanRecoverStamina;
    public bool CanInteract;
    
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
        SetCanRoll();
        SetCanAttack();
        SetCanParry();
        SetCanGetHit();
        SetCanRecoverStamina();
        SetCanInteract();
    }

    void SetCanMove()
    {
        CanMove = !player.IsInventoryOn&&!playerMove.IsRolling && !playerAttack.IsAttacking && !playerParry.IsDefensing && !hitState.IsGetHit;
    }

    void SetCanRoll()
    {
        CanRoll = !player.IsInventoryOn&&!playerAttack.IsAttacking && !playerParry.IsDefensing && !hitState.IsGetHit && !player.StaminaEmpty &&
                  playerMove.NextRolling;
    }

    void SetCanAttack()
    {
        CanAttack = !player.IsInventoryOn&&!playerMove.IsRolling && !player.StaminaEmpty && !playerParry.IsDefensing && !hitState.IsGetHit;
    }

    void SetCanParry()
    {
        CanParry = !player.IsInventoryOn&&!hitState.IsGetHit && !playerMove.IsRolling && !playerAttack.IsAttacking && !player.IsDead;
        //CanParry = !hitState.IsGetHit && !playerMove.IsRolling;
    }

    void SetCanGetHit()
    {
        CanGetHit = !hitState.IsGetHit && !playerAttack.IsAttacking;
    }

    void SetCanRecoverStamina()
    {
        CanRecoverStamina = !playerAttack.IsAttacking && !playerMove.IsRolling && !playerParry.IsDefensing;
    }

    void SetCanInteract()
    {
        CanInteract = !player.IsInventoryOn&&!hitState.IsGetHit;
    }
}
