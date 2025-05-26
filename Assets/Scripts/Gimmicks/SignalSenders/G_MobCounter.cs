using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몹 개발이 완료되면, 죽은 몹의 이벤트를 받아서 통합하고, 이를 send하는 방식으로 쓰는 것이 좋을듯.
/// </summary>

public class G_MobCounter : GimmickSignalSender
{
    #region State

    public override void Init(GimmickSignalSenderState defaultState)
    {
        SetState(defaultState);
        switch (defaultState)
        {
            case GimmickSignalSenderState.Activated: // Active
                isInteractable = true;
                break;
            case GimmickSignalSenderState.Inactivated: // InActive
                isInteractable = false;
                break;
            default:
                Debug.LogError("잘못된 enum value");
                return;
        }
        // ImmediateSendSignal();
    }

    #endregion

    public int fullCount;
    public int deadMobCount;
    public List<MonsterState> mobs = new List<MonsterState>();

    public void Start()
    {
        foreach (var mob in mobs)
            mob.Dead += OnMonsterDie;

        fullCount = mobs.Count;
    }

    public void OnMonsterDie(GameObject monster)
    {
        //이미 클리어로 기록되었다면 스킵
        if (GetState() == GimmickSignalSenderState.Activated)
            return;

        deadMobCount++;

        if (deadMobCount == fullCount)
        {
            SetState(GimmickSignalSenderState.Activated);
            isInteractable = true;
            SendSignal();
        }
    }
}
