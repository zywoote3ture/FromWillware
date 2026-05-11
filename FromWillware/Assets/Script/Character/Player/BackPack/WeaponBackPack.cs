using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBackPack : BackPack, ISaveable
{
    public List<WeaponData> Weapons;
    public Transform WeaponPoint;

    [Header("交互UI")]
    public GameObject interactPromptPrefab;
    public Transform uiCanvas;

    private GameObject currentPrompt;

    private WeaponSystem weaponSystem;
    private WeaponPickup nearbyWeapon;
    private PlayerInputHandler inputHandler;
    private PlayerState playerState;

    void Start()
    {
        CurrentIndex = 0;
        CurrentSize = 0;

        weaponSystem = GetComponent<WeaponSystem>();
        inputHandler = GetComponent<PlayerInputHandler>();
        playerState = GetComponent<PlayerState>();
    }

    void Update()
    {
        WeaponPickUp();

        // ===== UI 跟随武器 =====
        if (currentPrompt != null && nearbyWeapon != null)
        {
            Vector3 screenPos =
                Camera.main.WorldToScreenPoint(
                    nearbyWeapon.transform.position + Vector3.up * 2f
                );

            currentPrompt.transform.position = screenPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Weapon")) return;

        nearbyWeapon = other.GetComponent<WeaponPickup>();

        if (nearbyWeapon != null && currentPrompt == null)
        {
            currentPrompt = Instantiate(
                interactPromptPrefab,
                uiCanvas
            );

            Vector3 screenPos =
                Camera.main.WorldToScreenPoint(
                    nearbyWeapon.transform.position + Vector3.up * 2f
                );

            currentPrompt.transform.position = screenPos;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Weapon")) return;

        WeaponPickup pickup = other.GetComponent<WeaponPickup>();

        if (pickup == nearbyWeapon)
        {
            nearbyWeapon = null;
        }

        if (currentPrompt != null)
        {
            Destroy(currentPrompt);
            currentPrompt = null;
        }
    }

    public void WeaponPickUp()
    {
        if (nearbyWeapon != null &&
            playerState.CanInteract &&
            (Input.GetKeyDown(KeyCode.E)
            || inputHandler.interactPressed))
        {
            bool success =
                WeaponAdd(nearbyWeapon.weaponData);

            if (success)
            {
                weaponSystem.AddWeapon(
                    nearbyWeapon.weaponData
                );

                nearbyWeapon.isPickedUp = true;

                nearbyWeapon.gameObject.SetActive(false);

                // 🔥 UI 一起消失
                if (currentPrompt != null)
                {
                    Destroy(currentPrompt);
                    currentPrompt = null;
                }

                nearbyWeapon = null;
            }
        }
    }

    public bool WeaponAdd(WeaponData data)
    {
        if (Weapons.Count >= MaxSize)
        {
            Debug.Log("BackPack is full");
            return false;
        }

        Weapons.Add(data);
        return true;
    }

    public string GetUniqueID()
    {
        return "WeaponBackPack";
    }

    // ================= SAVE =================
    public string CaptureState()
    {
        WeaponBackPackSaveData saveData =
            new WeaponBackPackSaveData();

        foreach (var weapon in Weapons)
        {
            saveData.weaponIDs.Add(weapon.Name);
        }

        return JsonUtility.ToJson(saveData);
    }

    // ================= LOAD =================
    public void RestoreState(string json)
    {
        WeaponBackPackSaveData saveData =
            JsonUtility.FromJson<WeaponBackPackSaveData>(json);

        Weapons.Clear();

        foreach (string id in saveData.weaponIDs)
        {
            if (WeaponDatabase.dict.ContainsKey(id))
            {
                WeaponData data =
                    WeaponDatabase.dict[id];

                Weapons.Add(data);

                weaponSystem.AddWeapon(data);
            }
            else
            {
                Debug.LogWarning("Weapon not found: " + id);
            }
        }
    }
}