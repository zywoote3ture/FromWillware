using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public int level = 1;
    public int exp = 0;
    public int expToNextLevel = 100;
    public int HpUpdate = 20;
    public int StaminaUpdate = 20;
    public int damageUpdate = 5;
    
    private Player player;
    private Damage damage;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        damage = GetComponentInChildren<Damage>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y)) 
            LevelUp();
    }

    public void LevelUp()
    {
        if (exp >= expToNextLevel)
        {
            exp -= expToNextLevel;
            level++;
            PlayerUpdate();
            Debug.Log("Level Up to " + level );
        }
        else
        {
            Debug.Log("exp is not enough");
            return;
        }
        
    }

    public void PlayerUpdate()
    {
        player.MaxHP += HpUpdate;
        player.CurrentHP = player.MaxHP;
        player.MaxStamina += StaminaUpdate;
        player.CurrentStamina = player.MaxStamina;
    }

    
}
