using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string Name;
    public float AttackSpeed;
    public float ConsumingStamina;
    public int WeaponDamage;
    public string Introduction;
    public Collider weaponCollider;
    
    private Damage damage;
    
    // Start is called before the first frame update
    void Start()
    {
        damage = GetComponentInChildren<Damage>(true);
        if (damage == null)
        {
            Debug.Log(gameObject.name + " has no damage");
        }
        //WeaponDamage = damage.damage;
        weaponCollider =  GetComponentInChildren<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
