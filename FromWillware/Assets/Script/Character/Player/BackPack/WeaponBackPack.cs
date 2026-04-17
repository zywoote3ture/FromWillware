using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBackPack : BackPack
{
    // Start is called before the first frame update
    public List<Weapon> Weapons;
    public int PackIndex;
    
    void Start()
    {
        CurrentIndex = 0;
        CurrentCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WeaponAdd(Weapon weapon)
    {
        if (CurrentCount == MaxCount)
        {
            Debug.Log("The Weapon BackPack is Full");
            return;
        }
        
        Weapons.Add(weapon);
        CurrentCount = Weapons.Count;
    }

    public void WeaponDesert(int index)
    {
        CurrentIndex = index;
        Weapons.RemoveAt(CurrentIndex);
    }
}
