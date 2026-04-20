using UnityEngine;

[RequireComponent(typeof(Boss))]
public abstract class BossSkill : MonoBehaviour
{
    public string skillName;
    public string animationTriggerName;

    [Header("选择范围")]
    public float minRange = 0f;
    public float maxRange = 10f;

    [Header("实际攻击距离")]
    public float attackDistance = 5f;

    [Header("霸体与弹反")]
    public bool isHyperArmor = false;
    public int parryStaggerLevel = 0;

    public int damage = 20;
    public Collider weaponCollider;

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

    public void Use()
    {
        boss.OnSkillStart(this);
        anim.SetTrigger(animationTriggerName);
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