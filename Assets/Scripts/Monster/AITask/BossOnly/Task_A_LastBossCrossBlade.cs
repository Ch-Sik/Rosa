using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class Task_A_LastBossCrossBlade : Task_A_Base
{
    [Tooltip("동시에 몇뭉치 소환할건지")]
    [SerializeField] private int clusterCount = 1;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector2 spawnAreaSize;  // 맵의 임의 지점을 선정할 때 사용될 "범위"
    [SerializeField] private float projSpeed = 3f;
    [SerializeField] private bool clearProjectilesOnCanceled;
    [SerializeField] private bool clearProjectilesOnTerminated;
#if UNITY_EDITOR
    [SerializeField] bool drawGizmo;
#endif

    List<GameObject> instances;

    [Task]
    void CrossBladeAttack()
    {
        ExecuteAttack();
    }

    // 맵의 임의 지점에 칼날 4개를 십자 모양으로 형성
    protected override void OnStartupBegin()
    {
        instances = new List<GameObject>();
        for (int i = 0; i < clusterCount; i++)
        {
            // 랜덤 범위 선정
            Vector3 spawnPoint = transform.position;
            spawnPoint.x += Random.Range(-0.5f * spawnAreaSize.x, 0.5f * spawnAreaSize.x);
            spawnPoint.y += Random.Range(-0.5f * spawnAreaSize.y, 0.5f * spawnAreaSize.y);
            // 칼날 뭉치 소환. 이 시점에서 콜라이더는 꺼져있는 상태
            instances.Add(Instantiate(prefab, spawnPoint, Quaternion.identity));
        }
    }

    // 칼날 움직이기 시작
    protected override void OnActiveBegin()
    {
        for (int i = 0; i < clusterCount; i++)
        {
            instances[i].GetComponent<LastBossCrossBladeCluster>().LaunchProjectiles(projSpeed);
        }
    }

    // 칼날 사라지기는 투사체 수명으로 관리하니 상관 안써도 됨.
    protected override void OnRecoveryBegin()
    {

    }

    protected override void ClearOnTerminated()
    {
        base.ClearOnTerminated();

        blackboard.TryGet(BBK.isHitt, out bool isHitt);
        if (clearProjectilesOnCanceled && isHitt)       // 피격으로 인한 공격 종료
        {
            RemoveAllCluster();
            return;
        }

        if (!isHitt && clearProjectilesOnTerminated)    // 시간으로 인한 공격 종료
        {
            RemoveAllCluster();
            return;
        }
    }

    private void RemoveAllCluster()
    {
        if(instances != null)
        {
            foreach(GameObject cluster in instances)
            {
                if (cluster == null) continue;
                cluster.GetComponent<LastBossCrossBladeCluster>().DisappearAllClusterImmediatly();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmo) return;
        Gizmos.color = Color.red;
        Vector3[] corners = new Vector3[4];
        corners[0] = transform.position + Vector3.left * spawnAreaSize.x / 2 + Vector3.up * spawnAreaSize.y / 2;
        corners[1] = transform.position + Vector3.right * spawnAreaSize.x / 2 + Vector3.up * spawnAreaSize.y / 2;
        corners[2] = transform.position + Vector3.right * spawnAreaSize.x / 2 + Vector3.down * spawnAreaSize.y / 2;
        corners[3] = transform.position + Vector3.left * spawnAreaSize.x / 2 + Vector3.down * spawnAreaSize.y / 2;
        Gizmos.DrawLineStrip(corners, true);
    }
#endif
}
