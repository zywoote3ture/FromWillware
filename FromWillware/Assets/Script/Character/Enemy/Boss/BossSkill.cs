using UnityEngine;

[RequireComponent(typeof(Boss))]
public abstract class BossSkill : MonoBehaviour
{
    public string animationTriggerName;

    [Header("选择范围")]
    public float minRange = 0f;
    public float maxRange = 10f;

    [Header("实际攻击参数")]
    public float attackDistance = 5f;
    public int damage = 20;
    public Collider weaponCollider;

    [Header("角度修正")]
    public float angleOffset = 0f; 

    [Header("霸体")]
    public bool isHyperArmor = false;

    [Header("特效与AOE攻击")]
    public GameObject aoeVfxPrefab;
    public float forwardOffset = 3f;   
    public float lifeTime = 1f;

    protected Boss boss;
    protected Animator anim;

    public void Initialize(Boss bossController)
    {
        boss = bossController;
        anim = bossController.GetComponent<Animator>();

        if (weaponCollider != null)
        {
            var weapon = weaponCollider.GetComponent<BossWeapon>() ??
                         weaponCollider.gameObject.AddComponent<BossWeapon>();

            weapon.Initialize(bossController);
            weaponCollider.enabled = false;
        }
    }

    public virtual void Use()
    {
        boss.OnSkillStart(this);
        anim.SetTrigger(animationTriggerName);
    }

    public void SpawnAOE()
    {
        if (aoeVfxPrefab == null || boss == null) return;

        Vector3 spawnPos = boss.transform.position
                           + boss.transform.forward * forwardOffset;
        spawnPos += Vector3.up * 0.5f;

        Quaternion spawnRot = boss.transform.rotation
                        * Quaternion.Euler(0, 180f, 0);

        GameObject spike = Instantiate(aoeVfxPrefab, spawnPos, spawnRot);

        Destroy(spike, lifeTime);
    }

    public void EnableWeapon()
    {
        if (weaponCollider) weaponCollider.enabled = true;
    }

    public void DisableWeapon()
    {
        if (weaponCollider) weaponCollider.enabled = false;
    }
}