using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using Random = System.Random;

public class Task_A_Laser : Task_A_Base
{
    [SerializeField]
    protected MonsterLaser laserPrefab;

    [SerializeField] private Vector2 spawnAreaOffset;
    [SerializeField] private Vector2 spawnAreaSize;

    [SerializeField] protected int spawnCount;
    
    [SerializeField]
    protected int damage;
    
    [SerializeField, ReadOnly]
    protected List<MonsterLaser> instanceList = null;

    
    private const int MaxAttempts = 30;
    private float minDistOfOrbs;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(laserPrefab != null);
    }

    [Task]
    protected virtual void LaserAttack()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        // 적(플레이어) 정보 가져와서 대상을 향해 조준
        GameObject enemy;
        if(blackboard.TryGet(BBK.Enemy, out enemy))
        {
            var points = GetNextSpawnPositions(spawnCount);
            foreach (var p in points)
                instanceList.Add(SpawnLaserOrb(p, enemy.transform.position));
        }
        else
        {
            Debug.LogError("레이저 발사 대상을 찾을 수 없음");
            Fail();
            return;
        }
    }

    protected List<Vector2> GetNextSpawnPositions(int count)
    {
        Vector2 spawnPos;
        Vector2 spawnAreaCenter = (Vector2)(transform.position) + spawnAreaOffset;
        List<Vector2> result = new();
        int attempts = 0;
        
        while (result.Count < count && attempts < MaxAttempts)
        {
            attempts++;
            
            spawnPos = new Vector2(
                spawnAreaCenter.x + UnityEngine.Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                spawnAreaCenter.y + UnityEngine.Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
            );

            bool isValid = true;
            foreach (var p in result)
            {
                if (Vector2.SqrMagnitude(p - spawnPos) <= minDistOfOrbs * minDistOfOrbs)
                {
                    isValid = false;
                    continue;
                }
            }
            
            if(isValid)
                result.Add(spawnPos);
        }

        return result;
    }

    protected MonsterLaser SpawnLaserOrb(Vector2 spawnPos, Vector2 targetPos)
    {
        var instance = Instantiate(laserPrefab, spawnPos, Quaternion.identity);
        instance.Initalize((targetPos - spawnPos).normalized);
        
        return instance;
    }

    protected override void OnActiveBegin()
    {
        foreach(var instance in instanceList)
            instance.Activate(damage);
    }

    protected override void OnRecoveryBegin()
    {
        TerminateLaser();
    }

    protected override void ClearOnTerminated()
    {
        base.ClearOnTerminated();
        // 발사중인 레이저 중단
        TerminateLaser();
    }

    private void TerminateLaser()
    {
        foreach(var instance in instanceList)
            instance.Terminate();
        instanceList.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + (Vector3)spawnAreaOffset, spawnAreaSize);
    }
}
