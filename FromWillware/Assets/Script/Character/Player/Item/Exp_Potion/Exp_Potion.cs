using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Item/Exp Potion")]
public class Exp_Potion : Item
{
    public int ExpGet;

    public override void Fun(Player player)
    {
        player.GetComponent<LevelSystem>().GetExp(ExpGet);
    }
}
