using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class Task_A_Boss3RetrieveFeather : Task_A_Base
{
    public Transform retreivePos;

    private Task_A_Boss3FeatherAttack[] featherAttacks;

    private readonly List<Boss3Projectile> featherInstances = new List<Boss3Projectile>();
    
    void Start()
    {
        featherAttacks = GetComponents<Task_A_Boss3FeatherAttack>();
        if (featherAttacks.Length == 0)
        {
            Debug.LogError("No feather attacks found");
            Fail();
            return;
        }
        
        foreach(var t in featherAttacks)
            t.OnLaunchFeather += AddFeatherInstance;
    }

    private void AddFeatherInstance(Boss3Projectile proj)
    {
        featherInstances.Add(proj);
    }

    [Task]
    void RetrieveFeather()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        foreach(var instance in featherInstances)
        {
            instance.DoShake(startupDuration - 0.02f);
        }
    }

    protected override void OnActiveBegin()
    {
        foreach(var instance in featherInstances)
        {
            instance.RetrieveProjectile(retreivePos.position);
        }
        featherInstances.Clear();
    }
}
