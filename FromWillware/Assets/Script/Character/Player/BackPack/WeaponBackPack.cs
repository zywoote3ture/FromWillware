using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBackPack : BackPack,ISaveable
{
    // Start is called before the first frame update
    public List<WeaponData> Weapons;
    public Transform WeaponPoint;

    private WeaponSystem weaponSystem;
    private WeaponPickup nearbyWeapon;
    private PlayerInputHandler inputHandler;
    void Start()
    {
        CurrentIndex = 0;
        CurrentSize = 0;
        weaponSystem = GetComponent<WeaponSystem>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        WeaponPickUp();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            nearbyWeapon = other.GetComponent<WeaponPickup>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            nearbyWeapon = null;
        }
    }

    public void WeaponPickUp()
    {
        if (nearbyWeapon!=null&&(Input.GetKeyDown(KeyCode.E)||inputHandler.interactPressed))
        {
            WeaponAdd(nearbyWeapon.weaponData);
            weaponSystem.AddWeapon(nearbyWeapon.weaponData); // ✅ 关键
            Destroy(nearbyWeapon.gameObject);
        }
    }

    public void WeaponAdd(WeaponData data)
    {
        if(Weapons.Count < MaxSize)
            Weapons.Add(data);
        else
        {
            Debug.Log("BackPack is full");
            return;
        }
    }

    public string GetUniqueID()
    {
        return "WeaponBackPack";
    }

    // ================= SAVE =================
    public string CaptureState()
    {
        return null;
    }

    // ================= LOAD =================
    public void RestoreState(string json)
    {
      
    }

    
}
