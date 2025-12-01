using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using Sirenix.OdinInspector;

public enum ProjectileDirOption { 
    Deg360,         // 360도 모두 발사
    RightAngleOnly, // 상하좌우 직각방향으로만
    LRonly,         // 좌우 방향으로만
    FixedDirection, // 처음 설치된 고정방향으로만
}

public class Task_A_SimpleProjectile : Task_A_Base
{
    [Header("공격 관련 기본 요소")] [SerializeField]
    private bool useVariousProjectiles;
    [SerializeField, Tooltip("투사체 프리팹"), HideIf("useVariousProjectiles")]
    private GameObject projectilePrefab;
    [SerializeField, ShowIf("useVariousProjectiles")]
    private GameObject[] projectilePrefabs;
    [SerializeField, Tooltip("투사체가 생성되어야 할 위치")]
    private Transform muzzle;
    [SerializeField, Tooltip("투사체 진행 속도")]
    private float projectileSpeed = 1.0f;

    [Header("발사 방향 관련")]
    [SerializeField, Tooltip("공격 방향 옵션\n" +
        "Deg360: 360도 모두 발사\n" +
        "RightAngleOnly: 상하좌우 직각방향으로만\n" +
        "LRonly: 좌우 방향으로만\n" +
        "FixedDirection: 고정 방향 앞으로만"
    )]
    private ProjectileDirOption projectileDirOption;
    [SerializeField, ShowIf("projectileDirOption", ProjectileDirOption.FixedDirection)]
    [Tooltip("고정 방향일 때, 발사 방향")]
    private Transform fixedTarget;

    protected Vector2 enemyPosition;
    protected Vector2 attackDir;

    private int _projIndex = -1;

    protected void Start()
    {
        if(blackboard == null)
        {
            blackboard = GetComponent<Blackboard>();
            Debug.Assert(blackboard != null, $"{gameObject.name}: Blackboard를 찾을 수 없음");
        }
        // fixedDirection 옵션일 경우, 대상 위치를 제대로 설정했는지 체크
        if(projectileDirOption == ProjectileDirOption.FixedDirection)
        {
            Debug.Assert(fixedTarget != null, "고정 터렛의 목표 위치를 지정해주세요");
            blackboard.Set(BBK.Enemy, fixedTarget.gameObject);
        }

        Debug.Assert(projectilePrefab != null, $"{gameObject.name}: 투사체 프리팹이 설정되어있지 않음");
    }

    [Task]
    protected void SimpleProjectileAttack()
    {
        ExecuteAttack();
    }

    // 공격 패턴을 구체적으로 지정하지 않고 대충 Attack()으로 뭉뚱그려 작성된 BT 스크립트 호환용
    [Task]
    protected void Attack()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        /// 적 위치 파악하기

        // fixedDirection 옵션이라면 지정된 방향으로 enemyPosition 고정.
        if (projectileDirOption == ProjectileDirOption.FixedDirection)
        {
            enemyPosition = fixedTarget.position;
        }
        // fixedDirection 옵션이 아니라면, 블랙보드에서 '적' 정보 가져오기
        else
        {
            GameObject enemy;
            if (!blackboard.TryGet(BBK.Enemy, out enemy) || enemy == null)
            {
                Debug.Log($"{gameObject.name}: Blackboard에서 적을 찾을 수 없음. 공격 취소");
                Fail();
                return;
            }
            enemyPosition = enemy.transform.position;
        }

        // 적 위치에 따라 좌우 반전하기
        LookAt2D(enemyPosition);
    }

    protected override void OnActiveBegin()
    {
        GameObject enemy = null;

        // 고정 방향 발사 옵션이 아니라면
        if (projectileDirOption != ProjectileDirOption.FixedDirection)
        {
            // 공격 시전 준비 중에 적이 이탈했다면 공격 취소
            if (!blackboard.TryGet(BBK.Enemy, out enemy) || enemy == null)
            {
                Debug.LogWarning($"{gameObject.name}: Attack에서 적을 찾을 수 없음!");
                Fail();
                return;
            }
        }

        // 발사 방향 결정
        if (projectileDirOption == ProjectileDirOption.FixedDirection)
        {
            attackDir = (fixedTarget.position - transform.position).normalized;
        }
        else
        {
            UpdateAttackDir(enemy);
        }

        // 공격 시전
        GameObject projectile = Instantiate(GetProjectilePrefab(), muzzle.position, Quaternion.identity);
        projectile.GetComponent<ProjectileBase>().InitProjectile(attackDir * projectileSpeed);
    }

    private GameObject GetProjectilePrefab()
    {
        if (!useVariousProjectiles)
            return projectilePrefab;
        else
        {
            _projIndex = (_projIndex + 1) % projectilePrefabs.Length;
            return projectilePrefabs[_projIndex];
        }
    }

    private void UpdateAttackDir(GameObject enemy)
    {
        // 적(플레이어) 위치 파악
        Vector2 dir;

        // 발사 준비부터 실제 발사 사이의 짧은 시간 내에 플레이어가 뒤로 넘어갔을 경우
        // 왼쪽을 보고 오른쪽으로 발사되는 현상을 막기 위해 Startup 타이밍에 저장된 위치값 사용.
        dir = enemyPosition - (Vector2)gameObject.transform.position;

        // 옵션에 따라 방향 벡터 조정
        if (projectileDirOption == ProjectileDirOption.RightAngleOnly)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            // Debug.Log($"보정 전 각도: {angle}");
            angle = Mathf.FloorToInt(angle + 45) / 90 * 90;
            // Debug.Log($"보정 후 각도: {angle}");
            angle *= Mathf.Deg2Rad;

            dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        else if (projectileDirOption == ProjectileDirOption.LRonly)
        {
            dir = dir.x < 0 ? Vector2.left : Vector2.right;
        }
        attackDir = dir.normalized;
    }
}
