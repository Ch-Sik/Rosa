using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Panda;
using UnityEngine;
using Random = UnityEngine.Random;

public class Task_A_RandomSphere : Task_A_Base
{
    [Header("공격 관련")] 
    [SerializeField] private LastBossAoe attackPrefab;
    [SerializeField] private int spawnCount;
    [SerializeField] private float attackRadius;
    [SerializeField] private float overlapFactor;
    [SerializeField] private Vector2 areaCenter;
    [SerializeField] private Vector2 areaSize;
    [SerializeField] private float spawnTerm;
    
    private readonly List<LastBossAoe> _attackInstances = new();
    
    [Task]
    public void RandomSphereAttack()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        GeneratePattern().Forget();
    }

    private async UniTaskVoid GeneratePattern()
    {
        // 1. Limit 계산 (핵심 튜닝 포인트)
        // 공격 지름(2 * radius)에 팩터를 곱해서 최소 거리를 설정
        float limit = attackRadius * 2 * overlapFactor;
        
        List<Vector2> points = GeneratePoints(spawnCount, limit, -areaSize.x/2, -areaSize.y/2, areaSize.x/2, areaSize.y/2);

        foreach (var p in points)
        {
            SpawnAttack(p);
            await UniTask.WaitForSeconds(spawnTerm);
        }
    }

    private List<Vector2> GeneratePoints(int n, float limit, float minX, float minY, float maxX, float maxY)
    {
        List<Vector2> results = new List<Vector2>();
        float cellSize = limit; // 셀 크기
        float width = maxX - minX;
        float height = maxY - minY;
        
        int cols = Mathf.CeilToInt(width / cellSize);
        int rows = Mathf.CeilToInt(height / cellSize);
        
        int maxAttempts = n * 20; // 실패 방지용 카운트
        int attempts = 0;

        while (results.Count < n && attempts < maxAttempts)
        {
            attempts++;
            
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            Vector2 candidate = new Vector2(x, y);
            
            bool isValid = true;
            foreach (var p in results)
            {
                if (Vector2.SqrMagnitude(candidate - p) < limit * limit)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                results.Add(candidate);
            }
        }

        return results;
    }
    
    private void SpawnAttack(Vector2 pos)
    {
        if(attackPrefab != null)
        {
            var aoe = Instantiate(attackPrefab, pos, Quaternion.identity);
            aoe.transform.localScale = Vector3.one * (attackRadius * 2); 
            _attackInstances.Add(aoe);
        }
    }

    protected override void OnActiveBegin()
    {
        ActivateSequentially().Forget();
    }

    private async UniTaskVoid ActivateSequentially()
    {
        foreach (var instance in _attackInstances)
        {
            instance.Activate();
            await UniTask.WaitForSeconds(spawnTerm);
        }
        _attackInstances.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }

    protected override void ClearOnTerminated()
    {
        base.ClearOnTerminated();
        foreach (var instance in _attackInstances)
        {
            instance.CancelAOE();
        }
        _attackInstances.Clear();
    }
}
