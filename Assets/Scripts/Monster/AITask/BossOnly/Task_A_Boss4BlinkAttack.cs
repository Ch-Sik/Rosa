using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using DG.Tweening;

public class Task_A_Boss4BlinkAttack : Task_A_Base
{
    [Tooltip("사라질 때까지 딜레이")]
    [SerializeField] float blinkDelay;

    [Tooltip("사라지는 이펙트")]
    [SerializeField] GameObject blinkVFX = null;

    [Tooltip("몬스터 본체의 비주얼")]
    [SerializeField] GameObject bodyVisual;

    [Tooltip("몸통 충돌판정")]
    [SerializeField] Collider2D bodyCollider;

    [Tooltip("다시 나타날 때의 높이")]
    [SerializeField] float jumpHeight;

    [Tooltip("공격 이펙트 & 판정 히트박스")]
    [SerializeField] GameObject attackColliderAndVFX;

    [Tooltip("공격 아이템")] 
    [SerializeField] private GameObject attackItemPrefab;

    private float _startHeight;  // 패턴 시작할 때의 높이 == 지면에 발디디고 있을 때의 높이 저장
    private bool _isSpawnedAttackItem = false;

    [Task]
    private void BlinkAttack()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        _startHeight = transform.position.y;
        _isSpawnedAttackItem = false;
        DOTween.Sequence()
            .AppendInterval(blinkDelay)
            .AppendCallback(()=>
            {
                // 이펙트 남겨두고 비주얼은 사라지기, 충돌판정 끄기
                if(blinkVFX != null)
                    blinkVFX?.SetActive(true);
                bodyVisual?.SetActive(false);
                bodyCollider.enabled = false;
            })
            .AppendInterval(1f)
            .AppendCallback(()=>{
                if(blinkVFX != null)
                    blinkVFX?.SetActive(false);      // 약 1초 후에 점멸 이펙트 끄기.
            });
    }

    protected override void OnActiveBegin()
    {
        GameObject enemy;
        blackboard.TryGet(BBK.Enemy, out enemy);

        // 순간이동 전에 대상(플레이어) 바라보기
        LookAt2D(enemy.transform.position);

        // 대상 머리 위로 순간이동
        Vector3 newPosition = enemy.transform.position;
        newPosition.y = _startHeight + jumpHeight;
        transform.position = newPosition;

        // 공격 & 몸통 충돌 판정 켜기
        attackColliderAndVFX.SetActive(true);
        bodyCollider.enabled = true;
        DOTween.Sequence()
            .AppendInterval(0.1f)       // 공격 판정 켜고 잠시 기다려서 '벽에 충돌하여 튕겨나오는 모습' 보이지 않도록 함.
            .AppendCallback(()=>
            {
                if(blinkVFX != null)
                    blinkVFX?.SetActive(true);           // 점멸 이펙트 켜기
                bodyVisual?.SetActive(true);
            });
        
        // 떨어지는 건 중력 영향으로 자연스럽게 될 것 같으니 일단 따로 내려오는 코드는 빼고 테스트해보도록 하자.
    }

    protected override void OnRecoveryBegin()
    {
        attackColliderAndVFX.SetActive(false);
        if(blinkVFX != null)
            blinkVFX?.SetActive(false);
    }
    
    protected override void OnRecoveryLast()
    {
        if (_isSpawnedAttackItem) return;
        blackboard.TryGet(BBK.isGrounded, out bool isGrounded);
        if (!isGrounded) return;

        Instantiate(attackItemPrefab, attackColliderAndVFX.transform.position, Quaternion.identity);
        _isSpawnedAttackItem = true;
    }
}
