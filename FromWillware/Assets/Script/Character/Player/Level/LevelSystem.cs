using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : MonoBehaviour,ISaveable
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
        LevelUp();
    }

    public void LevelUp()
    {
        while (exp >= expToNextLevel)
        {
            exp -= expToNextLevel;
            level++;
            PlayerUpdate();
            Debug.Log("Level Up to " + level );
        }
    }

    public void PlayerUpdate()
    {
        player.MaxHP += HpUpdate;
        player.CurrentHP = player.MaxHP;
        player.MaxStamina += StaminaUpdate;
        player.CurrentStamina = player.MaxStamina;
    }

    public void GetExp(int exp)
    {
        this.exp += exp;
    }

    public string GetUniqueID()
    {
        return "LevelSystem";
    }

    public string CaptureState()
    {
        LevelData data = new LevelData
        {
            level = this.level,
            exp = this.exp
        };
        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string state)
    {
        LevelData data = JsonUtility.FromJson<LevelData>(state);
        level = data.level;
        exp = data.exp;
    }
}
