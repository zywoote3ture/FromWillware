using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponSystem : MonoBehaviour
{
    public Transform WeaponPoint;
    public List<Transform> Weapons;
    public int CurrentWeaponIndex = 0;
    public Transform CurrentWeapon;
    public int MaxSzie = 2;
    
    public Animator animator;
    public AnimatorOverrideController baseOverride;

    private AnimatorOverrideController runtimeOverride;
    private Player player;
    private WeaponBackPack weaponBackPack;
    private PlayerInputHandler inputHandler;
    
    // Start is called before the first frame update
    void Start()
    {
        Weapons = new List<Transform>(2);
        animator = GetComponent<Animator>();
        weaponBackPack = GetComponent<WeaponBackPack>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputHandler.switchWeapon)
        {
            ChangeWeapon();
        }
    }

    public void EquipWeapon(int index)
    {
        if (Weapons.Count == 0) return;

        // 关闭所有武器
        foreach (var w in Weapons)
            w.gameObject.SetActive(false);

        CurrentWeaponIndex = index;
        CurrentWeapon = Weapons[index];

        CurrentWeapon.gameObject.SetActive(true);
    }

    public void ChangeWeapon()
    {
        if (Weapons.Count == 0) return;

        Weapons[CurrentWeaponIndex].gameObject.SetActive(false);

        CurrentWeaponIndex = (CurrentWeaponIndex + 1) % Weapons.Count;

        EquipWeapon(CurrentWeaponIndex);
        ApplyWeaponAnimation(CurrentWeapon.GetComponent<WeaponPickup>().weaponData);
    }
    public void AddWeapon(WeaponData data)
    {
        if (Weapons.Count >= MaxSzie)
        {
            Debug.Log("装备的武器数已满");
            return;
        }
        GameObject weapon = Instantiate(data.WeaponPrefab, WeaponPoint, false);

        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        weapon.transform.localScale = Vector3.one;

        weapon.gameObject.SetActive(false);
        weapon.GetComponentInChildren<Collider>().enabled = false;
        
        if(Weapons.Count<MaxSzie)
            Weapons.Add(weapon.transform);

        if (CurrentWeapon == null)
        {
            EquipWeapon(0);
            ApplyWeaponAnimation(Weapons[0]
                .GetComponent<WeaponPickup>().weaponData);
        }
    }

    public void SetToWeaponPoint(int BackIndex, int PointIndex)
    {
        GameObject weapon = weaponBackPack.Weapons[BackIndex].WeaponPrefab;
        Weapons[PointIndex] = weapon.transform;
        if (PointIndex != CurrentWeaponIndex)
        {
            Weapons[PointIndex].gameObject.SetActive(false);
        }
        else
        {
            Weapons[PointIndex].gameObject.SetActive(true);
        }
    }
    
    void ApplyWeaponAnimation(WeaponData weapon)
    {
        runtimeOverride = new AnimatorOverrideController(baseOverride);

        runtimeOverride["SwordAttack1"] = weapon.combo1;
        runtimeOverride["SwordAttack2"] = weapon.combo2;
        runtimeOverride["SwordAttack3"] = weapon.combo3;
        runtimeOverride["Sword And Shield Idle"] = weapon.idle;
        runtimeOverride["Running"] = weapon.run;
        runtimeOverride["Walking"] = weapon.walk;
        
        animator.runtimeAnimatorController = runtimeOverride;
    }
}
