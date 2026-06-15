using UnityEngine;
using Panda;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;

// Melee와 똑같은 기능인데 함수명만 다른 클래스를 구성하기 위한 간단한 상속.
public class Task_A_Boss4MeleeCombo : Task_A_Melee
{
    [InfoBox("Active Time 설정은 MeleeComboOption에 의해 자동으로 덮어써짐에 주의")]
    [SerializeField] MeleeComboOption[] comboSetting;

    new Rigidbody2D rigidbody;

    protected override void Start()
    {
        base.Start();

        this.rigidbody = GetComponent<Rigidbody2D>();

        // comboSetting에 설정된 값으로 나머지 요소들 초기화
        float attackActiveTime = 0;
        foreach(var atk in comboSetting)
        {
            attackActiveTime += atk.activeDuration;
            attackActiveTime += atk.recoveryDuration;
        }
        this.activeDuration = attackActiveTime;
    }

    [Task]
    private void MeleeCombo()
    {
        ExecuteAttack();
    }

    protected override void OnActiveBegin()
    {
        var seq = DOTween.Sequence();
        foreach(var atk in comboSetting)
        {
            // 공격 판정과 이펙트 활성화하기
            seq.AppendCallback(()=>
            {
                attackInstance.ExecuteAttack();
                // atk.attackColliderAndVFX.SetActive(true);
            });
            // forwardDist만큼 전진
            seq.Append(rigidbody.DOMoveX(
                rigidbody.position.x + GetCurrentDir().toFloat() * atk.forwardDist,
                atk.activeDuration));
            // 공격 사이 딜레이에는 이펙트 끄기
            seq.AppendCallback(()=>
            {
                attackInstance.FinishAttack();
                // atk.attackColliderAndVFX.SetActive(false);
            });
            // 공격 사이 딜레이
            seq.AppendInterval(atk.recoveryDuration);
        }
        seq.Restart();
    }
}

[Serializable]
public class MeleeComboOption
{
    [Tooltip("공격하면서 앞으로 전진하는 거리")]
    public float forwardDist;
    [Tooltip("공격 유지 시간. 선딜은 별도로 존재하지 않음")]
    public float activeDuration;
    [Tooltip("후딜레이")]
    public float recoveryDuration;
    [Tooltip("공격 범위와 이펙트를 담당하는 게임오브젝트")]
    public GameObject attackColliderAndVFX;
}
