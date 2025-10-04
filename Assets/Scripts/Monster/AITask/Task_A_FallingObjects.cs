using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using System.Linq;
using Sirenix.OdinInspector;

public enum FallingAttackSpawnMode 
{
    /// <summary> 대상 머리 바로 위에 배치, 낙하물이 2개가 이상인 경우는 추천하지 않음 </summary>
    Target,
    /// <summary> 가능한 범위 내에서 랜덤 위치에 배치 </summary>
    Random, 
    /// <summary> 가능한 범위 내에서 균등하게 배치 </summary>
    Evenly
}

public enum FallingAttackLaunchMode
{
    /// <summary> 모든 오브젝트를 동시에 낙하 </summary>
    Parallel,
    /// <summary> 왼쪽부터 순서대로 ActiveTime에 걸쳐 낙하 </summary>
    SerialFromLeft,
    /// <summary> 오른쪽부터 순서대로 ActiveTime에 걸쳐 낙하 </summary>
    SerialFromRight,
    /// <summary> 무작위 순서대로 ActiveTime에 걸쳐 낙하 </summary>
    Random
}

// 패턴을 보스만 쓴다고 가정, 보스방에서만 실행되며 지형은 아무런 장애물이 없다고 가정함.
public class Task_A_FallingObjects : Task_A_Base
{
    [SerializeField]
    private GameObject[] attackPrefabs;
    [SerializeField, Tooltip("동시에 소환할 낙하 공격 갯수")]
    private int numOfInstance = 1;
    [SerializeField, Tooltip("낙하물 소환 높이")]
    private float spawnHeight;
    [SerializeField, Tooltip("낙하 공격 소환 시의 위치 설정 옵션")]
    private FallingAttackSpawnMode spawnMode;
    [SerializeField, Tooltip("낙하 공격 발동 시의 순서 설정 옵션")]
    private FallingAttackLaunchMode launchMode;
    [SerializeField, ShowIf("spawnMode", Value = FallingAttackSpawnMode.Random)]
    [Tooltip("낙하물끼리의 간격 최소값")]
    private float minSpawnInterval = 0.3f;
    [SerializeField] GameObject attackItemPrefab;

    private float leftEnd, rightEnd;        // 보스방의 왼쪽/오른쪽 끝의 x좌표
    private float secondsPerDrop;           // 낙하물 하나 떨어뜨릴 때마다 시간 간격
    private int curStalactiteCount;
    private List<MonsterStalactite> attackInstanceList = new List<MonsterStalactite>();
    

    [Task]
    protected void Attack()
    {
        ExecuteAttack();
    }

    [Task]
    protected void FallingObjectsAttack(int count = -1)
    {
        if(count > 0)
        {
            numOfInstance = count;
        }
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        SetFields();
        SpawnObjects();
    }

    private void SetFields()
    {
        curStalactiteCount = 0;
        secondsPerDrop = activeDuration / numOfInstance;

        // 맵 사이즈 측정
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, Vector2.left, 100f, LayerMask.GetMask("Ground"));
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, Vector2.right, 100f, LayerMask.GetMask("Ground"));
        leftEnd = leftHit.point.x;
        rightEnd = rightHit.point.x;
    }

    private void SpawnObjects()
    {
        // 소환되어야할 좌표를 우선 산정. spawnPosX 배열은 launch 순서를 반영함.
        float[] spawnPosX = GetSpawnCoordinates();

        // 순서대로 스폰하고 attackInstance에 넣고 Init 수행시킴
        for (int i = 0; i < numOfInstance; i++)
        {
            int randomIndex = Random.Range(0, attackPrefabs.Length);
            GameObject instance = Instantiate(attackPrefabs[randomIndex], 
                        new Vector3(spawnPosX[i], transform.position.y + spawnHeight, 0), Quaternion.identity);
            attackInstanceList.Add(instance.GetComponent<MonsterStalactite>());
            instance.GetComponent<MonsterStalactite>().Init();
        }

        // 공격 아이템도 스폰해서 떨구기
        if(attackItemPrefab != null)
        {
            float posX = GetSpawnCoordinate();
            GameObject instance = Instantiate(attackItemPrefab, 
                    new Vector3(posX, transform.position.y + spawnHeight, 0), Quaternion.identity);
        }
    }

    private float GetSpawnCoordinate()
    {
        return Random.Range(leftEnd, rightEnd);
    }

    private float[] GetSpawnCoordinates()
    {
        float[] xCoords = new float[numOfInstance];
        // 일단 소환되어야 할 좌표 산정
        switch (spawnMode)
        {
            case FallingAttackSpawnMode.Target:
                GameObject enemy;
                blackboard.TryGet(BBK.Enemy, out enemy);
                for (int i = 0; i < numOfInstance; i++)
                {
                    xCoords[i] = enemy.transform.position.x;
                }
                break;
            case FallingAttackSpawnMode.Random:
                List<Transform> instanceTransforms = new List<Transform>();
                for (int i = 0; i < numOfInstance; i++)
                {
                    // 랜덤으로 위치 지정하더라도 서로 너무 가깝지 않도록 함.
                    bool isCloseEnough = false;
                    float newPos = Random.Range(leftEnd, rightEnd);
                    foreach (float x in xCoords)
                    {
                        if (Mathf.Abs(x - newPos) < minSpawnInterval)
                        {
                            isCloseEnough = true;
                            break;
                        }
                    }
                    if (isCloseEnough)
                    {
                        i--;
                        continue;
                    }
                    else
                    {
                        xCoords[i] = newPos;
                    }
                }
                break;
            case FallingAttackSpawnMode.Evenly:
                float bossRoomXSize = rightEnd - leftEnd;
                float attackIntervalDist = numOfInstance == 1 ? 0 : bossRoomXSize / (numOfInstance);
                for (int i = 0; i < numOfInstance; i++)
                {
                    xCoords[i] = leftEnd + (0.5f * attackIntervalDist) + i * attackIntervalDist;
                }
                break;
        }

        // launchMode에 따라 xCoords 배열 정렬
        switch (launchMode)
        {
            case FallingAttackLaunchMode.Parallel:
                // 정렬 필요 없음.
                break;
            case FallingAttackLaunchMode.Random:
                System.Random rand = new System.Random();
                xCoords = xCoords.OrderBy(e => rand.Next()).ToArray();
                break;
            case FallingAttackLaunchMode.SerialFromLeft:
                xCoords = xCoords.OrderBy(e => e).ToArray();
                break;
            case FallingAttackLaunchMode.SerialFromRight:
                xCoords = xCoords.OrderByDescending(e => e).ToArray();
                break;
        }

        return xCoords;
    }

    protected override void OnActiveLast()
    {
        // 랜덤이던 왼쪽에서부터던 오른쪽에서부터건 순차적으로 내려와야 하는 경우
        if (launchMode != FallingAttackLaunchMode.Parallel)
        {
            float nextShockwaveEmit = secondsPerDrop * (curStalactiteCount);
            if (activeTimer.duration >= nextShockwaveEmit)
            {
                // 소환 위치를 잘못설정해서 낙하물이 낙하하기도 전에 지형에 부딪혀 
                // 파괴되는 경우를 대비해서 null check 해야 함.
                if (attackInstanceList[curStalactiteCount] != null)
                    attackInstanceList[curStalactiteCount].Launch();
                curStalactiteCount++;
            }
        }
        // 한꺼번에 내려와야 하는 경우
        else
        {
            if (curStalactiteCount < numOfInstance)
            {
                foreach (var instance in attackInstanceList)
                {
                    // 여기도 동일하게 null check 필요
                    if(instance != null)
                        instance.Launch();
                }
                curStalactiteCount += numOfInstance + 1;
            }
        }
    }

    protected override void Fail()
    {
        base.Fail();
        attackInstanceList.Clear();
    }

    protected override void Succeed()
    {
        base.Succeed();
        attackInstanceList.Clear();
    }

    private void OnDie()
    {
        if (attackInstanceList.Count > 0)
        {
            Debug.Log("몬스터 사망으로 인해 남아있는 낙하물 정리");
            foreach (var instance in attackInstanceList)
            {
                if (instance != null)
                    Destroy(instance);
            }
        }
    }
}
