using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

// 참고: 이 클래스는 일부 함수의 이름을 제외하고는 Task_A_Melee와 완벽하게 일치함.
public class Task_A_Roar : Task_A_Base
{
    // MonsterAOE랑 melee attack이랑 역할이 겹치는 것 같은데... 애초에 두 공격 방식을 하나로 통합해봐도 좋을 듯?
    [SerializeField, Tooltip("공격 범위를 나타내는 자식오브젝트 컴포넌트")]
    [Header("공격 관련")]
    private MonsterAOE attackInstance = null;

    protected virtual void Start()
    {
        if (blackboard == null)
        {
            blackboard = GetComponent<Blackboard>();
            Debug.Assert(blackboard != null, $"{gameObject.name}: Blackboard를 찾을 수 없음");
        }
    }

    [Task]
    private void RoarAttack()
    {
        ExecuteAttack();
    }

    // 공격 패턴을 구체적으로 지정하지 않고 대충 Attack()으로 뭉뚱그려 작성된 BT 스크립트 호환용
    [Task]
    private void Attack()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        attackInstance.Init();
    }

    protected override void OnActiveBegin()
    {
        // 공격 범위 미리보기를 실제 공격으로 변환
        attackInstance.ExecuteAttack();
    }

    protected override void OnRecoveryBegin()
    {
        attackInstance.FinishAttack();
    }
}
