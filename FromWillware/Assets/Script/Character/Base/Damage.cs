using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    // Start is called before the first frame update
    public int damage;

    private Player player;
    
    void Start()
    {
        player = GetComponentInParent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            damage = player.GetComponentInChildren<Weapon>().WeaponDamage;
        }
    }
    
    
}
