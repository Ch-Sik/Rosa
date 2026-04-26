using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using System.Linq;

/// <summary>
/// 보스가 사용하는 충격파 공격 연속 버전
/// </summary>
public class Task_GA_ShockwaveCombo : Task_GA_Shockwave
{
    [SerializeField, Tooltip("ActiveDuration 동안 shockwaveCount 만큼 충격파를 발사함")]
    private int shockwaveCount;

    private int curShockwaveCount;

    [Tooltip("Active Duration 시작 시점 기준으로 작성")]
    [SerializeField] List<float> shockwaveTiming;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(activeDuration > 0, "단발기가 아닌 패턴은 activeDuration이 0보다 커야함!");
        if(shockwaveTiming.Count != shockwaveCount)
        {
            Debug.LogWarning("충격파 타이밍 세팅이 잘못되어있음");
            // 잘못된 인덱스 참조 방지
            if(shockwaveTiming.Count < shockwaveCount)
                shockwaveTiming.AddRange(Enumerable.Repeat(999f, curShockwaveCount - shockwaveTiming.Count));
        }
    }

    [Task]
    private void ShockwaveCombo()
    {
        ExecuteAttack();
    }

    [Task]
    private void Attack()
    {
        ExecuteAttack();
    }

    protected override void OnActiveBegin()
    {
        // 기존 AITask_gndShockwaveAttack에서 충격파 발사하는 내용은 Hide
        // 대신 OnAttackActiveFrames에서 타이머를 관찰하면서 충격파를 발사하도록 하고
        // 여기서는 Initializing 관련만 수행함.
        curShockwaveCount = 0;
    }

    protected override void OnActiveLast()
    {
        float nextShockwaveEmit = shockwaveTiming[curShockwaveCount];
        if(activeTimer.duration >= nextShockwaveEmit)
        {
            EmitShockwave();
            if(curShockwaveCount < shockwaveCount)
                curShockwaveCount++;
        }
    }
}
