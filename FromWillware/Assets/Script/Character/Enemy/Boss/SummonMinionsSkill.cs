using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonMinionsSkill : MonoBehaviour
{
    public GameObject minionPrefab;
    public Transform[] spawnPoints;
    public int summonCount = 3;

    public void Execute()
    {
        // ¥•∑¢’ŸªΩ∂Øª≠...
        for (int i = 0; i < summonCount; i++)
        {
            if (i < spawnPoints.Length)
            {
                Instantiate(minionPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            }
        }
    }
}