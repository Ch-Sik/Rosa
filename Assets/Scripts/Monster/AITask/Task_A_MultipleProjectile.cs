using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Panda;


public abstract class Task_A_MultipleProjectile : Task_A_Base
{
    [Title("발사 관련")]
    [SerializeField, Tooltip("투사체가 생성될 위치 중심점")]
    protected Transform muzzle;
    [SerializeField, Tooltip("투사체 갯수")]
    protected int projectileCount;
    [SerializeField, Tooltip("투사체 시작부터 퍼진 정도")]
    protected float muzzleSpread;
    [SerializeField, Tooltip("투사체 퍼짐 각도")]
    protected float moa;

    [Title("투사체 관련")]
    [SerializeField, Tooltip("투사체 프리팹")]
    protected GameObject projectilePrefab;
    [SerializeField, Tooltip("투사체 진행 속도")]
    protected float projectileSpeed;

    [Title("디버그")]
    [SerializeField, Tooltip("기즈모 표시여부")]
    protected bool drawGizmos;

    protected Vector2 aimTarget;
    protected Vector2[] muzzlePoints;   
    protected Vector2[] launchDir;  // 각 지점 별 투사체 발사 방향

    protected void Start()
    {
        muzzlePoints = new Vector2[projectileCount];
        launchDir = new Vector2[projectileCount];

        Debug.Assert(projectilePrefab != null);
        Debug.Assert(muzzle != null);
        Debug.Assert(projectileSpeed > 0);
    }

    [Task]
    protected void MultiShot()
    {
        ExecuteAttack();
    }

    [Task]
    protected void Attack()
    {
        ExecuteAttack();
    }

    protected override void OnActiveBegin()
    {
        // 발사 위치 지정
        UpdateAimTarget();
        // 투사체 스폰 위치 계산
        CalculateMuzzleSpread();
        // 탄퍼짐 정도 계산
        CalculateProjectileSpread();
        // 투사체 스폰 & 발사
        InstantiateAndLaunch();
    }

    protected abstract void UpdateAimTarget();

    // 발사 각도와 수직인 방향으로 투사체 생성 위치 펼치기
    protected void CalculateMuzzleSpread()
    {
        Vector3 fireAngle = (Vector3)aimTarget - muzzle.position;
        Vector3 ortho_fireAngle = Vector3.Cross(fireAngle, Vector3.forward).normalized;
        // Debug.Log($"ortho_fireAngle: {ortho_fireAngle}");

        Vector2 centerPoint = muzzle.position;
        for (int i = 0; i < projectileCount; i++)
        {
            float multiplier = (0.5f + i - (projectileCount / 2.0f));
            muzzlePoints[i] = centerPoint + multiplier * (Vector2)ortho_fireAngle * muzzleSpread;
        }
    }

    protected void CalculateProjectileSpread()
    {
        Vector3 centerLine = ((Vector3)aimTarget - muzzle.position).normalized;

        for(int i=0; i < projectileCount; i++)
        {
            float multiplier = (0.5f + i - (projectileCount / 2.0f));
            float rotateDegree = multiplier * (moa / projectileCount);
            launchDir[i] = Quaternion.AngleAxis(-rotateDegree, Vector3.forward) * centerLine;
        }
    }

    protected void InstantiateAndLaunch()
    {
        for(int i=0; i<projectileCount; i++)
        {
            GameObject instance = Instantiate(projectilePrefab, muzzlePoints[i], Quaternion.identity);
            instance.GetComponent<ProjectileBase>().InitProjectile(launchDir[i] * projectileSpeed);
        }
    }

    protected override void Succeed()
    {
        base.Succeed();
    }

    protected override void Fail()
    {
        base.Fail();
    }

    protected void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        // edit mode에서는 각종 계산들 실행 안되니까 직접 수행해줘야 함.
        if(!Application.isPlaying)
        {
            Start();
            UpdateAimTarget();
            CalculateMuzzleSpread();
            CalculateProjectileSpread();
        }

        Gizmos.color = Color.white;
        for(int i=0; i<projectileCount; i++)
        {
            Gizmos.DrawSphere(muzzlePoints[i], 0.1f);
            Gizmos.DrawLine(muzzlePoints[i], muzzlePoints[i] + launchDir[i] * 1f);
        }
    }
}
