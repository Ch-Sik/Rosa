using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;

/// <summary>
/// ButterFlyAction을 위해서라도, 얘는 최종부모에 넣어두자.
/// </summary>

public class PlayerCombat : MonoBehaviour
{
    public bool showGizmo = false;          //기즈모 가시 여부

    private int damage = 1;
    Sequence attack;                        //공격 시퀀스
    public GameObject attackEntity;         //공격을 위한 AttackObject의 게임오브젝트
    public AttackObject attackObject;       //공격 이벤트를 위한 AttackObject
    public bool isAttack = false;           //공격중이라면 true, 아니라면 false;
    public bool isFly = false;              //나비를 타고 있다면 true, 아니라면 false;

    [Header("CombatOptions")]
    public LayerMask attackableObjects;     //공격가능한 대상 레이어 마스크
    public LayerMask butterfly;             //나비 레이어 마스크

    [Header("Combat Properties")]
    public float attackDistance;            //공격 사거리
    public float attackTime;                //공격 시간
    public float attackCooltime;            //공격을 위한 쿨타임
    public float knockbackCoef= 5.0f;             //넉백 계수
    public float knockbackTime = 0.3f;              //넉백 시간
    public float hitTime = 0.3f;                   //Hit 상태의 시간
    public float invincibleTime = 1.5f;            //무적 시간
    bool isHit = false;
    bool isInvincible = false;

    public float angle;                         //마우스의 Euler Angle
    [HideInInspector] public Vector2 mouse;     //마우스 좌표
    public Vector2 direction;                   //방향벡터

    //시작하면서 AttackEntity에 존재하는 attackObject를 얻어오며, attackObject를 Init해준다.
    private void Start()
    {
        attackObject = attackEntity.GetComponent<AttackObject>();
        attackObject.Init(this, attackableObjects, butterfly);
    }

    public void SetDamage(int changedDamage) { damage = changedDamage; }

    //SetData는 호출되면, 현재의 마우스 위치를 토대로 각도와 방향벡터 Data를 Set해준다.
    void SetData()
    {
        mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);                                            //마우스의 월드 좌표 반환
        angle = Mathf.Atan2(mouse.y - transform.position.y, mouse.x - transform.position.x) * Mathf.Rad2Deg;    //해당 좌표 데이터를 기반으로 각도 얻음
        direction = new Vector2(mouse.x - transform.position.x, mouse.y - transform.position.y).normalized;     //해당 좌표 데이터를 기반으로 방향벡터 얻음
        if (transform.lossyScale.x < 0)
            direction = new Vector2(-1 * direction.x, direction.y);
    }

    [Button]
    public void Hit(int dmg, LR dir)
    {
        PlayerRef.Instance.State.TakeDamage(dmg);
        TakeKnockBack(dir);
        OnHit();
    }

    [Button]
    public void TakeKnockBack(LR dir)
    {
        Debug.Log(dir);

        Vector2 knockbackVec = dir == LR.LEFT ? Vector2.left : Vector2.right;
        knockbackVec += Vector2.up * 0.5f;
        knockbackVec *= knockbackCoef;

        Transform player = PlayerRef.Instance.rb.gameObject.transform;

        StartCoroutine(Knockback(player, knockbackVec));

        Debug.Log(knockbackVec);
    }

    public IEnumerator Knockback(Transform player, Vector2 knockbackVec)
    {
        float time = 0.0f;
        while (time < knockbackTime)
        {
            player.Translate(knockbackVec * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
        }
    }

    //공격 함수
    public void Attack()
    {
        //공격중이라면, 리턴
        if (isAttack)
            return;

        //공격전에 마우스 데이터를 추출한다.
        SetData();

        //시퀀스를 할당한다.
        attack = DOTween.Sequence()
        //공격전 이벤트와 함께, 공격판정체의 방향을 변경해주고(사실 의미 없으나 일단 넣은 것입니다.), 충돌체를 켜준다.
        .AppendCallback(() =>
        {
            OnStartAttack();
            attackEntity.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            attackObject.StartAttack();
        })
        //얻은 방향벡터로 공격한다.
        .Append(attackEntity.transform.DOLocalMove(attackDistance * direction, attackTime).SetRelative(true))
        //공격이 끝난다면, 공격판정체의 위치를 초기화시켜 회수한다.
        .AppendCallback(() => attackEntity.transform.localPosition = Vector2.zero)
        //시퀀스가 끝나며, 공격끝 이벤트와 함께 공격판정체의 충돌체를 끈다.
        .OnComplete(() =>
        {
            attackObject.EndAttack();
            OnEndAttack();
        });
    }

    [Button]
    //공격을 취소하기 위해 만듦. 공격판정체가 나비에 닿거나 임의로 공격을 중단시킬 경우 사용한다.
    public void StopAttack()
    {
        attack.Pause();
        attackEntity.transform.localPosition = Vector2.zero;
        attackObject.EndAttack();
        OnEndAttack();
    }

    //공격 시작 이벤트
    private void OnStartAttack()
    {
        isAttack = true;
        Debug.Log("Start Attack");
    }

    //공격 종료 이벤트
    private void OnEndAttack()
    {
        isAttack = false;
        //쿨타임은 여기 넣자.
        Debug.Log("End Attack");
    }

    //추후에 공격시 대미지나 공격력을 계산하기 위해 만들었다. AttackObejct에서 충돌판정부에서 사용하자.
    public void CombatCalcultor(GameObject target)
    { 
    }

    //AttackObject가 나비에 닿을 경우, 해당 나비데이터를 PlayerCombat에 전달해주고, 그 데이터를 나비에 전달하기 위한 함수
    public void RideButterFly(Butterfly butterFly)
    {
        if (isFly)
            return;

        StopAttack();
        butterFly.ButterFlyAct(transform);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo)
            return;


        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 마우스 위치를 월드 좌표로 변환

        Vector3 direction = new Vector2(mousePosition.x - transform.position.x, mousePosition.y - transform.position.y).normalized;

        // 기준점에서 직사각형의 중심을 계산
        Vector3 center = transform.position + direction * (attackDistance / 2);

        // 직사각형의 회전 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 회전된 직사각형 그리기
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), new Vector3(attackDistance, 1, 1));
        Gizmos.matrix = rotationMatrix;
        Gizmos.color = Color.red; // 직사각형 색상 설정
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one); // 회전된 직사각형 그리기
        Gizmos.matrix = Matrix4x4.identity; // 다음 Gizmo에 영향을 미치지 않도록 기본 매트릭스로 돌아가기
    }

    public void OnHit()
    {
        Sequence invincible = DOTween.Sequence()
        .AppendCallback(() =>
        {
            isInvincible = true;
            PlayerRef.Instance.gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        })
        .AppendInterval(invincibleTime)
        .AppendCallback(() =>
        {
            isInvincible = false;
            PlayerRef.Instance.gameObject.layer = LayerMask.NameToLayer("Player");
        });

        Sequence hit = DOTween.Sequence()
        .AppendCallback(() => isHit = true)
        .AppendInterval(hitTime)
        .AppendCallback(() => isHit = false);
    }

    //트리거 충돌인 경우
    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //무적시간이라면 리턴
        if (isInvincible)
            return;

        //몬스터와 충돌한 경우
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            //넉백을 위한 충돌 방향 설정
            LR dir = collision.transform.position.x > transform.position.x ? LR.LEFT : LR.RIGHT;
            Hit(1, dir);
        }
    }
}