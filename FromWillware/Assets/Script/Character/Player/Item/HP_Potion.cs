using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Item/HP Potion")]
public class HP_Potion : Item
{
    public int RecoveryHP = 30;
    
    public override void Fun(Player player)
    {
       
        player.PlayDrinkAnim();
        
        if (player.CurrentHP + RecoveryHP < player.MaxHP)
        {
            player.CurrentHP += RecoveryHP;
        }
        else
        {
            player.CurrentHP = player.MaxHP;
        }
    }
}