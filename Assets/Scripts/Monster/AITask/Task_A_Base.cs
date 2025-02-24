using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using Sirenix.OdinInspector;

public enum MonsterAtttackState { Null, Startup, Active, Recovery }

/// <summary>
/// AttackBase는 몬스터 공격 패턴 구현할 때 자주 사용되는 패턴을 미리 구현해둔 클래스.
/// 상속해서 OnAttack...() 함수들 override하면 간단히 새 패턴을 구현할 수 있음.
/// </summary>
[RequireComponent(typeof(Blackboard))]
public class Task_A_Base : Task_Base
{
    [SerializeField]
    protected Blackboard blackboard = null;
    protected MonsterDamageReceiver dmgReceiver = null;

    [FoldoutGroup("기본 설정")]
    [Tooltip("블랙보드에 공격 상태 반영")]
    [SerializeField] protected bool writeStateOnBlackboard = true;

    [FoldoutGroup("기본 설정")]
    [Tooltip("패턴 ID 사용")]
    [SerializeField] protected bool usePatternID;
    [FoldoutGroup("기본 설정"), ShowIf("usePatternID")]
    [Tooltip("몬스터가 복수의 패턴을 가지고 있을 때, 패턴간 구분용 ID\n" +
        "-1은 현재 공격을 수행중이지 않은 것으로 예약")]
    [SerializeField] protected int patternID;

    [FoldoutGroup("기본 설정")]
    [SerializeField] protected float startupDuration;
    [FoldoutGroup("기본 설정")]
    [Tooltip("패턴이 StartupTime일 때 슈퍼아머 부여")]
    [SerializeField] protected bool superArmourOnStartup = true;

    [FoldoutGroup("기본 설정")]
    [SerializeField] protected float activeDuration;
    [FoldoutGroup("기본 설정")]
    [Tooltip("패턴이 ActiveTime일 때 슈퍼아머 부여")]
    [SerializeField] protected bool superArmourOnActive = true;

    [FoldoutGroup("기본 설정")]
    [SerializeField] protected float recoveryDuration;
    [FoldoutGroup("기본 설정")]
    [Tooltip("패턴이 RecoveryTime일 때 슈퍼아머 부여")]
    [SerializeField] protected bool superArmourOnRecovery = true;

    protected Timer startupTimer;
    protected Timer activeTimer;
    protected Timer recoveryTimer;

    // 이거 없어도 구현에는 문제없긴 한데 애니메이션 컨트롤 목적도 겸해서 추가함.
    [SerializeField]
    protected MonsterAtttackState attackState {
        get { return currentState; }
        set {
            currentState = value;
            if(writeStateOnBlackboard)
                blackboard.Set(BBK.AttackState, (int)currentState); 
        } 
    }
    [SerializeField]
    private MonsterAtttackState currentState = MonsterAtttackState.Null;

    /// <summary>
    /// <br> 설정한 startup/active/recovery 타이밍에 맞게 공격 이벤트 함수들을 호출함. </br>
    /// <br> 이 이름 그대로면 다양한 패턴(AITask)을 BT에서 구별이 불가능하므로, 상속된 클래스에서
    /// 고유한 이름의 Task 함수를 만들고 이 함수를 호출해줘야 됨. </br>
    /// </summary>
    protected void ExecuteAttack()
    {
        // 사망 시 패턴 종료
        bool isDead;
        blackboard.TryGet(BBK.isDead, out isDead);
        if (isDead)
        {
            Fail();
            return;
        }

        // 슈퍼아머 아닐 시 피격되면 패턴 중단
        bool isHitt;
        blackboard.TryGet(BBK.isHitt, out isHitt);
        if (isHitt)
        {
            if ((!superArmourOnStartup && attackState == MonsterAtttackState.Startup) ||
                (!superArmourOnActive && attackState == MonsterAtttackState.Active) ||
                (!superArmourOnRecovery && attackState == MonsterAtttackState.Recovery))
            {
                Debug.Log(this.GetType().Name + ": 피격으로 인해 패턴 중단");
                Fail();
                return;
            }
        }

        // 사망 아니라면 패턴 수행
        switch(attackState)
        {
            case MonsterAtttackState.Null:
                // 선딜레이 첫 프레임
                // 필드 값 설정
                attackState = MonsterAtttackState.Startup;
                if(usePatternID)
                    blackboard.Set(BBK.CurrentPattern, patternID);
                startupTimer = Timer.StartTimer();
                if (dmgReceiver == null) 
                    dmgReceiver = GetComponent<MonsterDamageReceiver>();
                // 슈퍼아머 상태 업데이트
                dmgReceiver.SetTempSuperArmour(superArmourOnStartup);
                // 오버로딩된 함수 실행
                OnStartupBegin();
                break;
            case MonsterAtttackState.Startup:
                if(startupTimer.duration < startupDuration)
                {
                    // 선딜레이 중
                    ThisTask.debugInfo = $"선딜레이: {startupTimer.duration}";
                    OnStartupLast();
                }
                else
                {
                    // 공격 시전 첫프레임
                    // 필드 값 설정
                    attackState = MonsterAtttackState.Active;
                    activeTimer = Timer.StartTimer();
                    // 슈퍼아머 상태 업데이트
                    dmgReceiver.SetTempSuperArmour(superArmourOnActive);
                    // 오버로딩된 함수 실행
                    OnActiveBegin();
                }
                break;
            case MonsterAtttackState.Active:
                if(activeTimer.duration < activeDuration)
                {
                    // 공격 지속 중
                    ThisTask.debugInfo = $"공격 지속: {activeTimer.duration}";
                    OnActiveLast();
                }
                else
                {
                    // 후딜레이 첫 프레임
                    // 필드 값 설정
                    attackState = MonsterAtttackState.Recovery;
                    recoveryTimer = Timer.StartTimer();
                    // 슈퍼아머 상태 업데이트
                    dmgReceiver.SetTempSuperArmour(superArmourOnRecovery);
                    // 오버로딩된 함수 실행
                    OnRecoveryBegin();
                }
                break;
            case MonsterAtttackState.Recovery:
                if (recoveryTimer.duration < recoveryDuration)
                {
                    // 후딜레이 중
                    OnRecoveryLast();
                    ThisTask.debugInfo = $"후딜레이: {recoveryTimer.duration}";
                }
                else
                {
                    // 후딜 종료
                    // 슈퍼아머 상태 업데이트
                    dmgReceiver.SetTempSuperArmour(false);
                    // 오버로딩된 함수 수행
                    OnEnd();
                    // 패턴 완료
                    Succeed();
                }
                break;
        }
    }

    /// <summary> 선딜레이 첫 프레임에서 호출되는 함수. 선딜레이가 0이여도 반드시 호출됨. </summary>
    protected virtual void OnStartupBegin() { }
    /// <summary> 선딜레이 프레임에서 반복적 호출되는 함수. 선딜레이가 0이면 호출되지 않음. </summary>
    protected virtual void OnStartupLast() { }
    /// <summary> 공격유지 첫 프레임에서 호출되는 함수. 공격유지시간이 0이여도 반드시 호출됨. </summary>
    protected virtual void OnActiveBegin() { }
    /// <summary> 공격유지 프레임에서 반복적 호출되는 함수. 공격유지시간이 0이면 호출되지 않음. </summary>
    protected virtual void OnActiveLast() { }
    /// <summary> 후딜레이 첫 프레임에서 호출되는 함수. 후딜레이가 0이여도 반드시 호출됨. </summary>
    protected virtual void OnRecoveryBegin() { }
    /// <summary> 후딜레이 프레임에서 반복적 호출되는 함수. 후딜레이가 0이면 호출되지 않음. </summary>
    protected virtual void OnRecoveryLast() { }
    /// <summary> 공격 패턴의 마지막 프레임에서 호출되는 함수. </summary>
    protected virtual void OnEnd() { }

    /// <summary>
    /// <br> 패턴이 성공한 것으로 BT에게 알리고 불필요한 요소들을 정리함. </br>
    /// <br> 필요하다면 override해서 추가 정리 수행 </br>
    /// </summary>
    protected virtual void Succeed()
    {
        ThisTask.Succeed();
        ClearOnTerminated();
    }

    /// <summary>
    /// <br> 패턴이 실패한 것으로 BT에게 알리고 불필요한 요소들을 정리함. </br>
    /// <br> 필요하다면 override해서 추가 정리 수행 </br>
    /// </summary>
    protected virtual void Fail()
    {
        ThisTask.Fail();
        ClearOnTerminated();
    }

    /// <summary>
    /// Succeed/Fail 등 패턴이 끝났을 시의 필드/블랙보드 값 정리.
    /// </summary>
    protected virtual void ClearOnTerminated()
    {
        if(usePatternID)
            blackboard.Set(BBK.CurrentPattern, -1);
        attackState = MonsterAtttackState.Null;
        startupTimer = null;
        activeTimer = null;
        recoveryTimer = null;
        // 슈퍼아머 효과 적용했다면 해제
        if (superArmourOnActive)
        {
            dmgReceiver.SetTempSuperArmour(false);
        }
    }

    /// <summary>
    /// <br> Startup 또는 Active 상태인 패턴을 즉시 Recovery 상태로 이행함.</br>
    /// <br> Succeed와 비슷하지만 이 함수는 후딜레이(Recovery)를 수행한다는 차이점이 있음.</br>
    /// </summary>
    protected virtual void SkipToRecovery()
    {
        attackState = MonsterAtttackState.Recovery;
        recoveryTimer = Timer.StartTimer();
        // 슈퍼아머 효과 적용했다면 해제
        if (superArmourOnActive)
        {
            dmgReceiver.SetTempSuperArmour(false);
        }
        OnRecoveryBegin();
    }
}
