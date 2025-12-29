using Panda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task_LastBossHitReaction : Task_A_Base
{
    [SerializeField] GameObject[] _platforms;
    [SerializeField] float platformRelocationDelay = 1f;
    [SerializeField] float platformRelocationRangeMin = -7f;
    [SerializeField] float platformRelocationRangeMax = 7f;
    [SerializeField] float enemyKnockbackPow = 5f;
    [SerializeField] private bool knockbackPlayerOnHitt = true;

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
        // 적(플레이어)를 밀쳐내기
        if(knockbackPlayerOnHitt)
            KnockBackEnemy();
    }

    protected override void OnRecoveryBegin()
    {
        base.OnRecoveryBegin();
        // 플랫폼 재생성
        RelocatePlatforms();
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
        foreach(var p in _platforms)
        {
            if (p == null) continue;
            p.SetActive(false);
        }
    }

    void ShowPlatforms()
    {
        Debug.Log("플랫폼 보이기");
        foreach (var p in _platforms)
        {
            if (p == null) continue;
            p.SetActive(true);
        }
    }

    void RelocatePlatforms()
    {
        foreach(var p in _platforms)
        {
            if (p == null) continue;
            Vector3 pos = p.transform.localPosition;
            pos.x = Random.Range(platformRelocationRangeMin, platformRelocationRangeMax);
            p.transform.localPosition = pos;
        }
        // TODO: 플랫폼을 랜덤한 x좌표로 설정

        ShowPlatforms();
    }
}
