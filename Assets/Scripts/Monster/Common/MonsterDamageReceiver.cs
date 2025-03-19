using AnyPortrait;
using Panda;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public class MonsterDamageReceiver : DamageReceiver
{
    [Title("컴포넌트 레퍼런스")]

    [SerializeField] Blackboard blackboard;
    [SerializeField] MonsterState monsterState;
    [SerializeField] new Rigidbody2D rigidbody;
    [SerializeField] apPortrait portrait; // 몬스터 피격 시 깜빡이는 연출에 필요
    [SerializeField] PandaBehaviour pandaBT;
    [Tooltip("피격 시 플레이어에게 맞공격되지 않게 하려면 레퍼런스 설정")]
    [SerializeField] MonsterDamageInflictor monsterAttackComponent;

    [Title("슈퍼 아머 옵션")]

    [Tooltip("기본적으로 슈퍼아머 보유 여부")] 
    [SerializeField] private bool isSuperArmour;
    [Tooltip("공격 패턴 등으로 인해 잠깐 얻는 슈퍼아머 효과")]
    [SerializeField, ReadOnly] private bool tempSuperArmour;


    [Title("무적 관련 옵션")]

    [Tooltip("슈퍼아머 여부와 별개로 데미지를 입지 않음 옵션")]
    [SerializeField] private bool isInvincible;
    // [Tooltip("플레이어 공격에 피격된 후 잠시 플레이어와의 충돌 무시하는 시간 길이")]
    // [SerializeField] private float ignoreDuration = 0.1f;
    [Tooltip("공격 패턴 등으로 인해 잠깐 얻는 무적 효과")]
    [SerializeField, ReadOnly] private bool tempInvincible;


    [Title("넉백 관련 옵션")]

    [Tooltip("넉백 계수")]
    [SerializeField] private float knockbackCoeff;
    [Tooltip("슈퍼아머 시에 넉백 무시 옵션")]
    [SerializeField] private bool ignoreKnockbackOnSuperArmour;


    [Title("사망 관련 옵션")]

    [SerializeField] private bool useRagdoll;       // 사망 시에 이리저리 굴러다니게 하는 효과 사용할 것인지?
    [SerializeField] private bool addTorqueOnDie;   // 사망 시에 랜덤 회전 값 부여
    [ShowIf("addTorqueOnDie")]
    [SerializeField] private float maxTorqueOnDie;  // 부여할 랜덤 회전 값 최대치


    bool isAlive = true;
    Coroutine blinkCoroutine;
    float reactionBrightness = 0.75f;               // 피격/사망 시의 밝기

    private void Start()
    {
        if (blackboard == null)
        {
            blackboard = GetComponent<Blackboard>();
            Debug.Assert(blackboard != null, $"{gameObject.name}: Blackboard를 찾을 수 없음!");
        }

        // 무적 상태인 기믹형 적은 체력 관리 필요 없음
        if (!isInvincible && monsterState == null)
        {
            monsterState = GetComponent<MonsterState>();
            Debug.Assert(monsterState != null, $"{gameObject.name}: MonsterState를 찾을 수 없음!");
        }

        // 넉백계수가 0인 적은 넉백 적용을 위한 rigidbody 필요 없음
        // but, 래그돌 효과 사용한다면 rigidbody 필요함
        if ((knockbackCoeff > float.Epsilon || useRagdoll ) && rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
            Debug.Assert(rigidbody != null, $"{gameObject.name}: Rigidbody2D를 찾을 수 없음!");
        }

        // 무적 상태인 기믹형 적은 피격 연출 필요 없음
        if (!isInvincible && portrait == null)
        {
            portrait = GetComponentInChildren<apPortrait>();
            if(portrait == null)
            {
                Debug.LogWarning("apPortrait를 찾을 수 없음!", gameObject);
            }
        }

        if (pandaBT == null)
        {
            pandaBT = GetComponent<PandaBehaviour>();
            Debug.Assert(pandaBT != null, $"{gameObject.name}: Panda Behaviour를 찾을 수 없음!");
        }
    }

    protected virtual void InitFields()
    {

    }

    /// <summary>
    /// 데미지 입기 함수. 주로 DamageInflictor에서 호출됨
    /// </summary>
    public override bool GetHitt(int damage, float attackAngle)
    {
        // 디버깅용
        Debug.Log($"{gameObject.name}: 피격당함");

        // 이미 죽어있을 경우 피격 무시
        if (!isAlive) return false;

        // BlinkEffect 수행이 mosterState.TakeDamage->BroadcastMessage("OnDie")->this.OnDie 보다 앞서야 함.
        // 그래야 Die로 인한 밝기 변경이 Blink에 의해 덮어씌워지지 않음.
        BlinkEffect();

        // 피격되었을 시 플레이어를 공격하지 않도록 잠시 공격 꺼놓음
        // Debug.Log("플레이어와 충돌 무시 수행");
        ToggleAttackComponent(false);
        StartCoroutine(RestoreCollisionWithPlayer());

        IEnumerator RestoreCollisionWithPlayer()
        {
            yield return new WaitForSeconds(0.1f);
            // Debug.Log("플레이어와 충돌 복구됨");
            ToggleAttackComponent(true);        // 플레이어를 공격 활성화
        }

        // 슈퍼아머일 경우
        if(isSuperArmour || !tempSuperArmour)
        {
            // 넉백 무시 옵션이 꺼져있고 넉백 계수가 0보다 크다면 넉백 수행
            if (!ignoreKnockbackOnSuperArmour && knockbackCoeff > float.Epsilon)
            {
                KnockBack(attackAngle);
            }
        }
        // 슈퍼아머가 아닐 경우
        else
        {
            // 넉백 계수가 0보다 크다면 넉백 수행
            if(knockbackCoeff > float.Epsilon)
            {
                KnockBack(attackAngle);
            }

            // 블랙보드에다가 플래그 기록
            blackboard.Set(BBK.isHitt, true);
            
            // 다음 프레임에서 플래그 False로 만들기
            StartCoroutine(SetFlagFalse());
            IEnumerator SetFlagFalse()
            {
                yield return new WaitForNextFrameUnit();     // 다음프레임까지 대기
                blackboard.Set(BBK.isHitt, false);
            }
        }

        // 무적일 경우, 데미지 입지 않음
        if (isInvincible || tempInvincible)
        {
            return false;
        }
        // 무적이 아닐 경우, 데미지 입고 사망 여부 판단
        else
        {
            monsterState.TakeDamage(damage);
            if (monsterState.HP < damage)
            {
                blackboard.Set(BBK.isDead, true);
            }
            return true;
        }
    }

    private void ToggleAttackComponent(bool value)
    {
        // 충돌
        if(monsterAttackComponent != null)
        {
            monsterAttackComponent.attackEnabled = value;
        }
    }

    public void SetTempSuperArmour(bool value)
    {
        this.tempSuperArmour = value;
    }

    public void SetTempInvincible(bool value)
    {
        this.tempInvincible = value;
    }

    private void KnockBack(float attackAngle)
    {
        StartCoroutine(DoKnockBack());

        // Hit Stop 연출 및 몬스터 패턴 캔슬 시 강제로 속도 0으로 고정되는 것 고려, 넉백은 잠시 기다린 후 수행함.
        IEnumerator DoKnockBack()
        {
            // TODO: Hit Stop 연출 추가할 경우 주석 해제
            // yield return new WaitForSeconds(0.1f);
            yield return 0;

            // Debug.LogWarning(attackAngle);
            // Debug.LogWarning(-11 / 2);
            // 넉백 방향을 4방향으로 고정?
            attackAngle =  (((int)(attackAngle + 180 + 45)) / 90 - 2)  * 90f;
            // Debug.Log(attackAngle);

            attackAngle *= Mathf.Deg2Rad;
            // attackDir는 PlayerCombat에서 Normalize되어서 Unit Vector임이 보장됨
            Vector2 knockbackVector = new Vector2(Mathf.Cos(attackAngle), Mathf.Sin(attackAngle)) * knockbackCoeff;
            // Debug.Log(knockbackVector);
            rigidbody.velocity = knockbackVector;
        }
    }

    // 피격되었을 때 스프라이트 깜빡이기
    private void BlinkEffect()
    {
        if (portrait == null)
            return;         // 아직 애니메이션이 적용되지 않은 녀석들 예외 처리

        // 딱히 추가로 수정할 필요 없어보여서 수치들을 하드코딩했는데 필요하다면 필드값으로 빼낼 것
        const float timePerBlink = 0.2f;
        // int blinkCount = Mathf.Max(2, Mathf.CeilToInt(ignoreDuration / timePerBlink));
        int blinkCount = 2;
        Color blinkColor = Color.gray * reactionBrightness;
        blinkColor.a = 1;

        blinkCoroutine = StartCoroutine(DoBlink());

        IEnumerator DoBlink()
        {
            for (int i = 0; i < blinkCount; i++)
            {
                portrait.SetMeshColorAll(blinkColor);
                yield return new WaitForSeconds(timePerBlink / 2);
                portrait.ResetMeshMaterialToBatchAll();
                yield return new WaitForSeconds(timePerBlink / 2);
            }
            blinkCoroutine = null;
        }
    }

    // OnDie 메세지는 MonsterState.TakeDamage에서 발생됨.
    private void OnDie()
    {
        isAlive = false;
        if(pandaBT != null)
            pandaBT.enabled = false;
        if (useRagdoll)
        {
            rigidbody.isKinematic = false;
            rigidbody.gravityScale = 1;
            rigidbody.freezeRotation = false;
        }
        if (addTorqueOnDie)
            AddTorque();
        FadeEffect();
    }

    // 래그돌 효과 사용할 때 회전값 부여하기
    private void AddTorque()
    {
        float randomTorque = Random.Range(-maxTorqueOnDie, maxTorqueOnDie);
        rigidbody.AddTorque(randomTorque);
        Debug.Log($"사망 시 랜덤 회전값 부여: {randomTorque}");
    }

    // 사망했을 때 스프라이트 색 어두워지기
    private void FadeEffect()
    {
        if (portrait == null)
            return;         // 아직 애니메이션이 적용되지 않은 녀석들 예외 처리

        const int step = 5;
        const float timePerBlink = 0.2f;
        const float fadeTime = 0.3f;
        Color defaultColor = Color.gray;   // AP의 기본 색상
        Color goalColor = Color.gray * reactionBrightness;
        goalColor.a = 1;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        StartCoroutine(DoFadeOut());

        // 피격 확인을 위해 일단 한번 깜빡인 후에 페이드
        IEnumerator DoFadeOut()
        {
            portrait.SetMeshColorAll(goalColor);
            yield return new WaitForSeconds(timePerBlink / 2);
            portrait.ResetMeshMaterialToBatchAll();
            yield return new WaitForSeconds(timePerBlink / 2);
            float fadeTick = fadeTime / step;
            for (int i = 1; i <= step; i++)
            {
                Color newColor = Color.Lerp(defaultColor, goalColor, i / (float)step);
                // Debug.Log("newColor: " + newColor);
                portrait.SetMeshColorAll(newColor);
                yield return new WaitForSeconds(fadeTick);
            }
        }
    }
}
