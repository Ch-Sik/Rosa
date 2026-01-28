using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class Task_A_Boss3FeatherAttack : Task_A_Base
{
    [Header("공격 관련 기본 요소")]
    [SerializeField, Tooltip("투사체 프리팹")]
    protected GameObject projectilePrefab;
    [SerializeField, Tooltip("투사체 진행 속도")]
    private float projectileSpeed = 4.0f;
    [SerializeField, Tooltip("투사체 조준 보정. 플레이어보다 얼마나 뒤 지면을 조준할지")]
    private float aimOffset = 2.0f;
    [SerializeField, Tooltip("투사체 생성될 위치")]
    protected Transform muzzle;

    protected Vector2 aimPosition = Vector2.zero;

    public Action<Boss3Projectile> OnLaunchFeather;

    // Start is called before the first frame update
    void Start()
    {
        if(blackboard == null)
        {
            blackboard = GetComponent<Blackboard>();
            Debug.Assert(blackboard != null, $"{gameObject.name}: Blackboard를 찾을 수 없음");
        }
    }

    [Task]
    void FeatherAttack()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        UpdateAimPosition();
    }

    protected void UpdateAimPosition()
    {
        // 적 위치 파악
        // StartupBegin 타이밍에 적 조준이 이루어지므로 Startup 시간동안 플레이어가 피할 여유가 있음.
        GameObject enemy;
        if(!blackboard.TryGet(BBK.Enemy, out enemy) || enemy == null)
        {
            // 보스전 한정으로 패턴 도중에 플레이어를 찾을 수 없으면 그건 '오류'임.
            Debug.LogError($"{gameObject.name}: Blackboard에서 적을 찾을 수 없음. 공격 취소");
            Fail();
            return;
        }
        
        aimPosition = enemy.transform.position;
        // 에임 보정 추가
        RaycastHit2D rayhit = Physics2D.Raycast(aimPosition, Vector2.down, 100f, LayerMask.GetMask("Ground"));
        Debug.Assert(rayhit.collider != null);
        aimPosition.y = rayhit.point.y;
        aimPosition.x += GetCurrentDir().toVector2().x * aimOffset;
    }

    protected override void OnActiveBegin()
    {
        GameObject projInstance = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        var projComponent = projInstance.GetComponent<Boss3Projectile>();
        
        LaunchFeather(projComponent, (aimPosition - (Vector2)muzzle.position).normalized);
        OnLaunchFeather(projComponent);
    }

    protected virtual void LaunchFeather(Boss3Projectile projComponent, Vector2 attackDir)
    {
        projComponent.InitProjectile(attackDir * projectileSpeed);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimPosition, 0.2f);
    }
}
