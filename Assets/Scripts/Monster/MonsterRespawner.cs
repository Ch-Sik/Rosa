using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터가 사망하였을 때 해당 몬스터를 리스폰한다.
/// 리스폰 시에 복구하는 정보는 해당 몬스터의 종류(프리팹), 위치, transform상의 부모. 총 3가지.
/// </summary>
public class MonsterRespawner : MonoBehaviour
{
    public GameObject prefab;
    public List<MonsterState> toRespawns = new List<MonsterState>();
    
    List<Vector3> monsterPosition = new List<Vector3>();
    List<Transform> monsterParent = new List<Transform>();


    // Start is called before the first frame update
    void Start()
    {
        foreach(var monster in toRespawns)
        {
            monster.Dead += WhenSomeMonsterDie;
            monsterPosition.Add(monster.transform.position);
            // 대부분의 몬스터가 AI sensor들 때문에 empty parent를 가지고 있는 점 고려
            monsterParent.Add(monster.transform.parent.parent);
        }
    }
    
    void WhenSomeMonsterDie(GameObject go)
    {
        // 사망한 몬스터의 번호 찾기
        int index = toRespawns.FindIndex((a) => { return a.gameObject == go; });
        Debug.Assert(index >= 0, "사망한 몬스터에 대한 정보를 리스트에서 찾을 수 없음");

        // 해당 몬스터의 원래의 transform 정보 가져오기
        Vector3 position = monsterPosition[index];
        Transform parent = monsterParent[index];

        // 리스폰하기
        GameObject newInstance = Instantiate(prefab, position, Quaternion.identity, parent);

        // 다음 리스폰을 위해 정보 업데이트하기
        toRespawns[index] = newInstance.GetComponentInChildren<MonsterState>();
        toRespawns[index].Dead += WhenSomeMonsterDie;
    }
}
