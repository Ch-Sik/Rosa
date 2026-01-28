using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Panda;
using UnityEngine;

public class Task_A_Boss3FeatherAttack2 : Task_A_Boss3FeatherAttack
{
    [SerializeField] private Transform launchTransform;
    [SerializeField] private float launchDelay;
    
    const float stopTime = 0.2f;
    
    [Task]
    void FeatherAttack2()
    {
        ExecuteAttack();
    }
    
    protected override void OnActiveBegin()
    {
        launchDelay = Mathf.Max(launchDelay, stopTime);
            
        LaunchWithDelay(launchDelay);
    }

    private async UniTaskVoid LaunchWithDelay(float t)
    {
        // 0. 시작시의 깃털 각도 계산
        var muzzleToStopPoint = launchTransform.position - muzzle.position;
        float startRotation = Mathf.Atan2(muzzleToStopPoint.y, muzzleToStopPoint.x) * Mathf.Rad2Deg;
        
        // 1. 깃털을 생성하고 지정한 위치로 이동시킨다.
        GameObject projInstance = Instantiate(projectilePrefab, muzzle.position, Quaternion.Euler(0, 0, startRotation));
        
        await projInstance.transform.DOMove(launchTransform.position, launchDelay - stopTime)
                .AsyncWaitForCompletion();
        
        // 2. 적 위치, StartupBegin에서 계산했지만 한 번 더 계산
        UpdateAimPosition();
        var v = (aimPosition - (Vector2)launchTransform.position);
        float toPlayerDeg = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        
        // 3. 적 위치 계산 & 잠깐 멈춰서면서 빙글 회전하는 연출
        // 이때, 회전 목표는 플레이어를 향함.
        await projInstance.transform
                .DORotate(Vector3.forward * toPlayerDeg, stopTime, RotateMode.FastBeyond360)
                .AsyncWaitForCompletion();
        
        // 4. 발사
        var projComponent = projInstance.GetComponent<Boss3Projectile>();
        LaunchFeather(projComponent, (aimPosition - (Vector2)launchTransform.position).normalized);
        OnLaunchFeather(projComponent);
    }
}
