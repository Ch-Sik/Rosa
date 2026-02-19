using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

[RequireComponent(typeof(MonsterDamageInflictor))]
public class Task_GA_Leap : Task_A_Base
{
    // 컴포넌트 레퍼런스
    [SerializeField]
    protected new Rigidbody2D rigidbody;
    [SerializeField]
    protected MonsterDamageInflictor damageComponent;

    [Header("공격 관련")]
    [SerializeField, Tooltip("패턴 공격력")]
    protected int attackDmg;
    [SerializeField, Tooltip("점프력 속도")]
    protected Vector2 JumpVector;
    [SerializeField, Tooltip("점프 후 땅에 닿았는지 체크하는 루틴을 점프 후 얼마 뒤부터 실행할건지")]
    protected float minAttackDuration;

    protected LR attackDir;
    protected int defaultCollideDamage;    // 몸체 충돌 판정이 기본적으로 가지고 있던 데미지 보관
    protected Timer attackTimer;        // 점프 시작 후 시간 측정하는 타이머

    [Header("씨앗 생성")] 
    [SerializeField] private GameObject attackItemPrefab;
    [SerializeField] private float attackItemSpawnProbability = 0.3f;
    [SerializeField] private Transform attackItemSpawnPos;
    [SerializeField] private float attackItemPopVertical = 150f;
    [SerializeField] private float attackItemPopHorizontal = 100f;
    [SerializeField] private SFXPlayer jumpSfx;


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
        if(damageComponent == null)
        {
            damageComponent = GetComponent<MonsterDamageInflictor>();
            if(damageComponent == null)
                Debug.LogError($"{gameObject.name}: damageComponent를 찾을 수 없음!");
        }
        defaultCollideDamage = damageComponent.damage;
    }

    [Task]
    protected void LeapAttack()
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
        CalculateAttackDirection();
    }

    protected override void OnActiveBegin()
    {
        // 기존 공격력은 저장해두고 몸통 접촉 시의 데미지를 tackleAttackPower로 대체
        defaultCollideDamage = damageComponent.damage;
        damageComponent.damage = attackDmg;

        // 시간 측정을 위한 타이머 설정
        attackTimer = Timer.StartTimer();

        // 공격 수행
        Leap();
        jumpSfx?.PlaySfx();
    }

    protected void Leap()
    {
        // JumpVector로 속도 설정
        rigidbody.velocity = new Vector2(JumpVector.x * attackDir.toVector2().x, JumpVector.y);
    }

    protected override void OnActiveLast()
    {
        // 점프 시작 후 일정 시간이 지났으며, 땅에 발을 디디고 있을 경우 패턴 강제 종료
        bool isGrounded;
        blackboard.TryGet(BBK.isGrounded, out isGrounded);
        if (attackTimer.duration > minAttackDuration && isGrounded)
        {
            base.SkipToRecovery();
            OnLanded();
            Debug.Log("땅에 발 디딤, 패턴 강제 완료");
            return;
        }
    }

    private void OnLanded()
    {
        if(Random.Range(0, 1f) <= attackItemSpawnProbability)
            SpawnAttackItem();
    }
    
    private void SpawnAttackItem()
    {
        if (!attackItemPrefab)
            return;
        
        var instance = Instantiate(attackItemPrefab, attackItemSpawnPos.position, Quaternion.identity);
        
        // 공격 아이템에 살짝 튀어오르는 연출
        var popVector = new Vector2(Random.Range(-attackItemPopHorizontal, attackItemPopHorizontal),
                                    attackItemPopVertical);
        instance.GetComponent<Rigidbody2D>().AddForce(popVector);
    }

    protected override void OnRecoveryBegin()
    {
        // 기존 공격력으로 복구
        damageComponent.damage = defaultCollideDamage;
    }

    protected virtual void CalculateAttackDirection()
    {
        // 공격 방향 산정
        GameObject enemy;
        if (!blackboard.TryGet(BBK.Enemy, out enemy) || enemy == null)
        {
            Debug.LogError($"{gameObject.name}: PrepareAttack에서 적을 찾을 수 없음!");
        }
        attackDir = (enemy.transform.position.x - transform.position.x) < 0 ? LR.LEFT: LR.RIGHT;

        // 공격 방향에 따라 좌우 반전하기
        LookAt2D(enemy.transform.position);
    }
}
