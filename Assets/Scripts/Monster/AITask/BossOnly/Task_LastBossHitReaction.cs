using Panda;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class Task_LastBossHitReaction : Task_A_Base
{
    [SerializeField] private LastBossVfx_RockFrag fragVfx;
    [FormerlySerializedAs("_platforms")]
    [SerializeField] private G_MovePlatform[] platforms;
    [SerializeField] float platformRelocationDelay = 1f;
    [SerializeField] float platformRelocationRangeMin = -7f;
    [SerializeField] float platformRelocationRangeMax = 7f;
    [SerializeField] float enemyKnockbackPow = 5f;
    [SerializeField] private bool knockbackPlayerOnHitt = true;

    [SerializeField] private SFXPlayer hitReactionSfx;

    private void Start()
    {
        RelocatePlatformsImmediately();
    }
    
    void RelocatePlatformsImmediately()
    {
        foreach(var p in platforms)
        {
            if (p == null) continue;
            Vector3 pos = p.transform.localPosition;
            pos.x = Random.Range(platformRelocationRangeMin, platformRelocationRangeMax);
            p.transform.localPosition = pos;
        }

        foreach (var p in platforms)
        {
            if (p == null) continue;
            p.ImmediateOnAct();
        }
    }
    
    [Task]
    public void IsHitt()
    {
        bool hitResult;
        if (!blackboard.TryGet(BBK.isHitt, out hitResult))
        {
            ThisTask.Fail();
        }
        if (hitResult == true)
        {
            // Debug.Log("피격 당함");
            ThisTask.Succeed();
        }
        else
        {
            ThisTask.Fail();
        }
    }

    [Task]
    public void HittReaction()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        base.OnStartupBegin();
        // 피격 당하면 일단 플랫폼 없애버리기
        HidePlatforms();
        // 뒤쪽 파편 연출 숨기기
        fragVfx.FallFrags();
        // 적(플레이어)를 밀쳐내기
        if(knockbackPlayerOnHitt)
            KnockBackEnemy();
        hitReactionSfx?.PlaySfx();
    }

    protected override void OnRecoveryBegin()
    {
        base.OnRecoveryBegin();
        // 플랫폼 재생성
        RelocatePlatforms();
    }

    protected override void OnEnd()
    {
        base.OnEnd();
        fragVfx.RiseFrags();
    }

    private void KnockBackEnemy()
    {
        Vector2 knockbackVector = (PlayerRef.Instance.transform.position - transform.position);
        knockbackVector.y = 0;
        knockbackVector = knockbackVector.normalized;
        PlayerRef.Instance.movement.Knockback(
            knockbackVector, enemyKnockbackPow
        );
    }

    void HidePlatforms()
    {
        Debug.Log("플랫폼 숨기기");
        foreach(var p in platforms)
        {
            if (p == null) continue;
            TogglePlatformWithRandomDelay(p, false).Forget();
        }
    }
    
    void RelocatePlatforms()
    {
        foreach(var p in platforms)
        {
            if (p == null) continue;
            Vector3 pos = p.transform.localPosition;
            pos.x = Random.Range(platformRelocationRangeMin, platformRelocationRangeMax);
            p.transform.localPosition = pos;
        }

        ShowPlatforms();
    }

    void ShowPlatforms()
    {
        Debug.Log("플랫폼 보이기");
        foreach (var p in platforms)
        {
            if (p == null) continue;
            TogglePlatformWithRandomDelay(p, true).Forget();
        }
    }
    
    private async UniTaskVoid TogglePlatformWithRandomDelay(G_MovePlatform platform, bool actOn)
    {
        float waitTime = Random.Range(0, 1f);
        // 플랫폼 숨기기 일때는 쉐이크 효과를 먼저 적용
        if(!actOn)
            platform.transform.DOShakePosition(waitTime, 0.3f);
        await UniTask.WaitForSeconds(waitTime);
        if(actOn)
            platform.OnAct();
        else
            platform.OffAct();
        // 플랫폼 보이기 일때는 쉐이크 효과를 나중에 적용
        if (actOn)
            platform.transform.DOShakePosition(1f, 0.3f);
    }
}
