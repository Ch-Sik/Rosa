using Sirenix.OdinInspector;
using UnityEngine;

// 몬스터 없지만 있는 것처럼 구라쳐서 리스폰 막음. Blackboard는 세팅해둘 필요 없음
public class NoRespawnZone : AIPerception
{
    protected override void Awake()
    {
        // Do nothing
    }

    protected override void OnTriggerEnter2D(Collider2D col)
    {
        // Do nothing
    }

    protected override void OnTriggerExit2D(Collider2D col)
    {
        // Do nothing
    }
}
