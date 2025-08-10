using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using Sirenix.OdinInspector;

public class Task_A_Mortar : Task_A_Base
{
    [Header("공격 관련")]
    [SerializeField, Tooltip("투사체 프리팹")]
    protected GameObject projectilePrefab;
    [SerializeField, Tooltip("투사체 생성될 위치")]
    protected Transform muzzle;
    [SerializeField, Tooltip("곡사 궤적의 고점 (muzzle 기준 상대 y좌표)")]
    protected float yHeight;

    // Start is called before the first frame update
    void Start()
    {
        if(blackboard == null)
        {
            blackboard = GetComponent<Blackboard>();
            Debug.Assert(blackboard != null, $"{GetType().Name}: 몬스터가 적을 인식할 수 없음");
        }
    }

    [Task]
    private void MortarAttack()
    {
        ExecuteAttack();
    }

    protected override void OnActiveBegin()
    {
        // 각도 계산
        if(ComputeAngle(out Vector2 launchVector))
        {
            // 공격 시전
            Launch(launchVector);
        }
        else
        {
            Debug.LogWarning("각도 계산 실패");
        }
    }

    // return value가 true이면 계산에 성공, false이면 계산에 실패
    private bool ComputeAngle(out Vector2 launchVector)
    {
        // 적 가져오기
        GameObject enemy;
        blackboard.TryGet(BBK.Enemy, out enemy);
        if(enemy == null)
        {
            Debug.LogWarning($"{gameObject.name}, {GetType().Name}: enemy를 찾을 수 없음!");
            Fail();
            launchVector = Vector2.zero;
            return false;
        }

        // 투사체의 중력계수 가져오기
        float projectileGravityScale = -Physics2D.gravity.y * projectilePrefab.GetComponent<Rigidbody2D>().gravityScale;

        // 2차 곡선의 꼭짓점 위치 구하기
        float targetX = enemy.transform.position.x;

        // ↓ 중력가속도 식 두번 적분하고 t=... 꼴로 정리한 거
        float timeForUp = Mathf.Sqrt(2 * (yHeight - muzzle.position.y) / projectileGravityScale);
        float timeForDown = Mathf.Sqrt(2 * (yHeight - enemy.transform.position.y) / projectileGravityScale);

        float launchVectorX = (targetX - muzzle.position.x) / (timeForUp + timeForDown);
        float launchVectorY = projectileGravityScale * timeForUp;

        launchVector = new Vector2(launchVectorX, launchVectorY);
        return true;
    }

    private void Launch(Vector2 launchVector)
    {
        // 투사체 소환
        GameObject instance = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);

        // 투사체 초기화 & 속도 설정
        ProjectileBase component = instance.GetComponent<ProjectileBase>();
        component.InitProjectile(launchVector);
    }
}
