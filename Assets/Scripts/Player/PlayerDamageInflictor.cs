using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// 공격을 판정하기 위한 투사체를 위한 스크립트입니다.
/// </summary>

public class PlayerDamageInflictor : MonoBehaviour
{
    public Rigidbody2D rb;                          // 플레이어 몸체의 rigidbody. 플레이어가 아래로 내려가는 중일 때에만 적을 밟을 수 있게 하기 위해 사용.
    PlayerCombat playerCombat;                      //PlayerCombat과 이벤트 전달을 위해 직접 연결
    [SerializeField] ObjectPool hitEffects;                       // 공격 적중시의 이펙트 풀

    float attackAngle;

    // LayerMask layer_wall;
    [SerializeField] LayerMask layer_attackable;                    //attackable한 objects의 layermask
    // LayerMask layer_butterfly;                      //butterfly인식을 위한 layermask

    [SerializeField] float hitEffectDuration;



    ////PlayerCombat에서 Init해준다. 기본적으로 
    //public void Init(PlayerCombat playerCombat, float damagePercent, LayerMask wall, LayerMask attackableObjects, LayerMask butterfly)
    //{
    //    this.playerCombat = playerCombat;
    //    this.damagePercent = damagePercent;
    //    layer_wall = wall;
    //    layer_attackable = attackableObjects;
    //    layer_butterfly = butterfly;
    //}

    ////시작과 동시에 콜라이더를 세팅한다.
    //private void Start()
    //{
    //    col = GetComponent<Collider2D>();
    //    col.enabled = false;
    //    col.isTrigger = true;

    //    // 이펙트가 플레이어 움직임에 영향받지 않도록 Root를 부모로 삼도록 한다
    //    hitEffects.transform.SetParent(null);
    //}

    // 공격 판정 활성화 or 비활성화
    // attackAngle은 공격 성공시 넉백 계산에 사용
    //public void SetAttackActive(float attackAngle)
    //{
    //    this.attackAngle = attackAngle;
    //    col.enabled = true;
    //}

    //public void SetAttackInactive()
    //{
    //    col.enabled = false;
    //}

    //public void OnTriggerStay2D(Collider2D collision)
    //{
    //    if ((layer_wall & 1 << collision.gameObject.layer) > 0)
    //    {
    //        Debug.Log("벽 충돌로 인한 공격 취소");
    //        playerCombat.StopAttack();
    //    }
    //}

    //충돌 감지
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // if (playerCombat.canInteraction) 상호작용이 필요한 오브젝트에 적용할 것.
        //if ((layer_wall & 1 << collision.gameObject.layer) > 0)
        //{
        //    Debug.Log("벽 충돌로 인한 공격 취소");
        //    playerCombat.StopAttack();
        //}

        Debug.Log($"Groundcheck collision enter: {collision.gameObject.name}");

        // 몬스터 등등과 충돌한다면,
        if (rb.velocity.y < 0 && (layer_attackable & 1 << collision.gameObject.layer) != 0)
        {
            // 몬스터보다 충분히 높은지 판단
            if(PlayerRef.Instance.transform.position.y - 0.73f < collision.transform.position.y)
            {
                Debug.Log("플레이어 높이가 충분하지 않음: 적을 밟지 못함");
                return;
            }

            // 적에게 데미지 가하기
            DamageReceiver receiver = collision.GetComponent<DamageReceiver>();
            receiver?.GetHitt(Mathf.RoundToInt(PlayerRef.Instance.state.AttackDmg), attackAngle);
            GameObject attackEffect = hitEffects.GetNextFromPool();
            // 공격 이펙트 소환
            attackEffect.transform.position = transform.position;
            attackEffect.SetActive(true);
            DOTween.Sequence().AppendInterval(hitEffectDuration)
                .AppendCallback(() => attackEffect.SetActive(false));

            // 범위 공격이 아니라 단일 공격이므로 이후 공격은 중단
            //playerCombat.StopAttack();

            // 플레이어 다시 점프하게 하기
            PlayerRef.Instance.movement.JumpUp();
        }

        /*
        //더 이상 나비와의 충돌이 아니기에 삭제함.
        //나비와 충돌한다면,
        if ((layer_butterfly.value & 1 << collision.gameObject.layer) > 0 &&
            playerCombat.canInteraction)
        {
            if (collision.GetComponent<Butterfly>().isCaged)
                return;
            Debug.Log("나비와 충돌됨");
            //PlayerCombat에 RideButterFly함수를 실행시키며, 나비의 부모 데이터를 전달한다.
            playerCombat.RideButterFly(collision.transform.parent.GetComponentInChildren<Butterfly>());
        }
        */
    }
}
