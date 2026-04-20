using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossSkillSelector
{
    private Boss boss;
    private List<BossSkill> skills;

    public BossSkillSelector(Boss boss, List<BossSkill> skills)
    {
        this.boss = boss;
        this.skills = skills;
    }

    public BossSkill ChooseSkill(float distance)
    {
        var available = skills
            .Where(s => distance >= s.minRange && distance <= s.maxRange)
            .ToList();

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }
}