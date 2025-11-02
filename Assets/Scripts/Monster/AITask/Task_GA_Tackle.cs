using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Panda;
using Unity.Jobs;

[RequireComponent(typeof(MonsterDamageInflictor))]
public class Task_GA_Tackle : Task_A_Base
{
    // 컴포넌트 레퍼런스
    [SerializeField] protected new Rigidbody2D rigidbody;
    [SerializeField] protected MonsterDamageInflictor damageComponent;

    [Title("공격 관련")] [SerializeField, Tooltip("돌진 패턴 공격력")]
    protected int tackleAttackPower;

    [SerializeField, Tooltip("돌진 최대 속도 (m/s)")]
    protected float tackleMaxSpeed = 5;

    [SerializeField, Tooltip("돌진 가속도 (m/s^2)")]
    protected float tackleAccel = 1000;

    [SerializeField, Tooltip("돌진 중 방향 전환 허용")]
    protected bool allowUturn = false;

    [SerializeField, ShowIf("allowUturn"), Tooltip("돌진 중 방향 전환 시의 딜레이")]
    protected float lookBackDelay = 0f;

    [SerializeField, Tooltip("돌진 후 브레이크 계수")]
    protected float recoveryDrag = 3.0f;

    [Title("벽에 박았을 때 관련")] [SerializeField, Tooltip("돌진 중 벽에 박았을 때 스턴 활성화")]
    protected bool wallStunEnabled;

    [SerializeField, Tooltip("돌진 중 벽에 박았을 때 스턴 시간")]
    protected float wallStunDuration;

    [Title("절벽 만났을 때 관련")] [SerializeField, Tooltip("돌진 중 절벽 만났을 때 멈춤")]
    protected bool cliffStopEnabled = false;

    protected Timer stunTimer = null;
    protected Vector2 tackleDir;
    protected int defaultCollideDamage; // 몸체 충돌 판정이 기본적으로 가지고 있던 데미지

    protected Coroutine _flipCoroutine = null;

    private void Start()
    {
        if (blackboard == null)
        {
            blackboard = GetComponent<Blackboard>();
            if (blackboard == null)
                Debug.LogError($"{gameObject.name}: Blackboard를 찾을 수 없음!");
        }

        if (rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
            if (rigidbody == null)
                Debug.LogError($"{gameObject.name}: Rigidbody2D를 찾을 수 없음!");
        }

        if (damageComponent == null)
        {
            damageComponent = GetComponent<MonsterDamageInflictor>();
            if (damageComponent == null)
                Debug.LogError($"{gameObject.name}: damageComponent를 찾을 수 없음!");
        }

        defaultCollideDamage = damageComponent.damage;
    }

    [Task]
    protected void TackleAttack()
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
        // 방향 계산
        CheckAttackDir();
    }

    protected override void OnActiveBegin()
    {
        // 기존 공격력은 저장해두고 몸통 접촉 시의 데미지를 tackleAttackPower로 대체
        defaultCollideDamage = damageComponent.damage;
        damageComponent.damage = tackleAttackPower;
    }

    protected override void OnActiveLast()
    {
        // 블랙보드에서 벽에 박음 정보 가져오기
        bool isStuckAtWall;
        blackboard.TryGet(BBK.StuckAtWall, out isStuckAtWall);

        // 벽에 박았을 경우 스턴
        if (wallStunEnabled && isStuckAtWall)
        {
            // 스턴 첫 프레임
            if (stunTimer == null)
            {
                stunTimer = Timer.StartTimer();
                blackboard.Set(BBK.isStunned, true); // 애니메이션을 위한 블랙보드 설정
                Debug.Log("벽에다 대가리 꽁!!!");
            }
            // 스턴 중간 프레임
            else if (stunTimer.duration < wallStunDuration)
            {
                ThisTask.debugInfo = $"stun: {wallStunDuration - stunTimer.duration}";
                activeTimer.Reset(); // Task_A_Base에 의해 스턴 도중에 패턴 종료되는 것 방지
                return;
            }
            // 스턴 마지막 프레임
            else
            {
                stunTimer = null;
                blackboard.Set(BBK.isStunned, false);
                // Debug.Log("스턴 끝");
                Succeed();
                return;
            }
        }

        // 블랙보드에서 절벽 마주침 정보 가져오기
        bool isStuckAtCliff;
        blackboard.TryGet(BBK.StuckAtCliff, out isStuckAtCliff);

        // 절벽 만났을 경우 처리
        if (cliffStopEnabled && isStuckAtCliff)
        {
            rigidbody.velocity = Vector2.zero;
            SkipToRecovery();
        }

        // 돌진 도중 방향 전환 옵션 켜진경우, 돌진 도중에도 방향 계속 체크
        if (allowUturn)
        {
            CheckAttackDirWhileAttack();
        }

        // 실제 돌진 수행
        DoTackle();
    }

    protected override void OnRecoveryBegin()
    {
        // 기존 공격력으로 복구
        damageComponent.damage = defaultCollideDamage;

        // 브레이크
        rigidbody.drag = recoveryDrag;
    }

    protected override void Succeed()
    {
        base.Succeed();
        // 브레이크 해제
        rigidbody.drag = 0f;
    }

    protected override void Fail()
    {
        base.Fail();
        // 브레이크 해제
        rigidbody.drag = 0f;
    }

    protected virtual void CheckAttackDir()
    {
        GameObject enemy;
        if (!blackboard.TryGet(BBK.Enemy, out enemy) || enemy == null)
        {
            // if (showErrorMsg)
            //     Debug.LogError($"{gameObject.name}: 블랙보드에서 적을 찾을 수 없음!");
            Fail();
            return;
        }

        tackleDir = (enemy.transform.position.x - transform.position.x) < 0 ? Vector2.left : Vector2.right;

        // 공격 방향에 따라 좌우 반전하기
        LookAt2D(enemy.transform.position);
    }

    protected virtual void CheckAttackDirWhileAttack()
    {
        GameObject enemy;
        if (!blackboard.TryGet(BBK.Enemy, out enemy) || enemy == null)
        {
            Fail();
            return;
        }

        var newDir = (enemy.transform.position.x - transform.position.x) < 0 ? Vector2.left : Vector2.right;
        if (newDir == tackleDir)
            return;

        // 공격 방향에 따라 좌우 반전하기
        if (_flipCoroutine != null) return;
        _flipCoroutine = StartCoroutine(WaitAndLookEnemy(enemy));
    }

    protected IEnumerator WaitAndLookEnemy(GameObject enemy)
    {
        yield return new WaitForSeconds(lookBackDelay);
        
        tackleDir = (enemy.transform.position.x - transform.position.x) < 0 ? Vector2.left : Vector2.right;
        LookAt2D(enemy.transform.position);
        
        _flipCoroutine = null;
    }


    protected virtual void DoTackle()
    {
        /// 속력 즉시 변경 대신 가속도 방식으로 대체
        // Vector2 velocity = tackleDir * tackleSpeed;
        // velocity.y = rigidbody.velocity.y;
        // rigidbody.velocity = velocity;

        Vector2 targetVelocity = tackleDir * tackleMaxSpeed;
        if ((targetVelocity.x - rigidbody.velocity.x) * tackleDir.x > 0)    // 목표 속도에 미달한 경우 가속
        {
            rigidbody.AddForce(tackleDir * tackleAccel * rigidbody.mass * Time.deltaTime, ForceMode2D.Force);
        }
        else                    // 목표 속도 이상에 도달한 경우 가속하지 않음.
        {
            // do nothing
        }
    }

    private bool isTargetBehind(GameObject enemy)
    {
        Vector2 toEnemy = (enemy.transform.position - transform.position).normalized;
        float cosDist = Vector2.Dot(toEnemy, tackleDir);
        return cosDist < -0.1f;     // 아주 약간 정도는 뒤로 가도 봐줌.
    }
}
