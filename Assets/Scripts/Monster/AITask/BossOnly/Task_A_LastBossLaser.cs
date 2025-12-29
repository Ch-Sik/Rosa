using System.Collections;
using System.Collections.Generic;
using Panda;
using UnityEngine;

public class Task_A_LastBossLaser : Task_A_Laser
{
    protected override void OnStartupBegin()
    {
        base.OnStartupBegin();
        if(instanceList == null || instanceList.Count == 0)
            Debug.LogError("[Task_A_LastBossLaser] instanceList is null or empty");
        instanceList[0].isItemSpawner = true;
    }
}
