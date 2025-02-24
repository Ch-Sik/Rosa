using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class Task_A_Laser : Task_A_Base
{
    [SerializeField, Tooltip("레이저 발사될 위치")]
    protected Transform muzzle;
    [SerializeField]
    protected GameObject laserPrefab;

    [SerializeField]
    protected int damage;
    [SerializeField, ReadOnly]
    protected MonsterLaser instance = null;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(muzzle != null);
        Debug.Assert(laserPrefab != null);
    }

    [Task]
    void LaserAttack()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        // 적(플레이어) 정보 가져와서 대상을 향해 조준
        GameObject enemy;
        if(blackboard.TryGet(BBK.Enemy, out enemy))
        {
            Vector2 toTarget = enemy.transform.position - muzzle.position;
            GameObject spawned = Instantiate(laserPrefab, muzzle.position, Quaternion.identity);
            instance = spawned.GetComponent<MonsterLaser>();
            instance.Initalize(toTarget.normalized);
        }
        else
        {
            Debug.LogError("레이저 발사 대상을 찾을 수 없음");
            Fail();
            return;
        }
    }

    protected override void OnActiveBegin()
    {
        instance.Activate(damage);
    }

    protected override void OnRecoveryBegin()
    {
        instance.Terminate();
    }

    protected override void ClearOnTerminated()
    {
        base.ClearOnTerminated();
        // 발사중인 레이저 중단
        instance.Terminate();
    }
}
