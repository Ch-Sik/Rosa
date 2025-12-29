using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

[RequireComponent(typeof(MonsterState))]
public class Task_BossPhaseChange : Task_Base
{
    [SerializeField]
    Blackboard blackboard;
    [SerializeField]
    private MonsterState bossState;
    [SerializeField]
    private int[] phaseMinHP = null;

    [ReadOnly] public int currentPhase = 0;     // 페이즈 숫자는 0부터 시작함

    private void Start()
    {
        if (bossState == null)
        {
            bossState = GetComponent<MonsterState>();
            Debug.Assert(bossState != null, $"{gameObject.name}: BossState를 찾을 수 없음!");
        }
        if(blackboard == null)
        {
            blackboard = GetComponent<Blackboard>();
            Debug.Assert(blackboard != null);
        }
        Debug.Assert(phaseMinHP != null, "보스의 페이즈 별 HP를 설정해야 함!");

        blackboard.Set(BBK.CurrentPhase, currentPhase);
    }

    [Task]
    private bool isHpOverPhaseLimit(int phaseNum)
    {
        if (bossState.HP >= phaseMinHP[phaseNum])
        {
            return true;
        }
        else
        {
            if (currentPhase < phaseNum + 1)
            {
                currentPhase = phaseNum + 1;
                blackboard.Set(BBK.CurrentPhase, currentPhase);
            }
            return false;
        }
    }
}
