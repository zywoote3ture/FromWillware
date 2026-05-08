using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon")]
public class WeaponData :ScriptableObject
{
    public string Name;
    public int Damage;
    public float ConsumingStamina;
    public string Introduction;
    public GameObject WeaponPrefab;

    public AnimationClip combo1;
    public AnimationClip combo2;
    public AnimationClip combo3;
    public AnimationClip idle;
    public AnimationClip run;
    public AnimationClip walk;
}
