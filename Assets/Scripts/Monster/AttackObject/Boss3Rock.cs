using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Boss3Rock : ProjectileBase
{
    [SerializeField] private LayerMask mushroomLayer;

    [Tooltip("버섯과 충돌하여 튕겨났을 때 되돌아가서 맞아야 할 위치")]
    [ReadOnly] public Vector3 returnPosition;

    [Tooltip("플레이어가 '튕겨내기'를 성공시켰을 때 데미지 입힐 컴포넌트")]
    [ReadOnly] public MonsterDamageReceiver damageReceiver;

    [Tooltip("플레이어가 '튕겨내기'를 성공시켰을 때 투사체가 튕겨내어져 돌아가는 시간")]
    [SerializeField] private float returnTime = 1.8f;

    // Start is called before the first frame update
    void Start()
    {
        // Debug.Log(LayerMask.GetMask("Ground", "Cube", "PlayerGrab"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);
        // 버섯과 충돌했다면
        if(collider.CompareTag("Mushroom"))
        {
            // 현재 진행중인 속도 초기화
            rigidbody.velocity = Vector2.zero;

            // 철두루미쪽으로 돌아가기
            DOTween.Sequence()
            .Append(rigidbody.DOJump(returnPosition, 2, 1, returnTime))
            .AppendCallback(() => {
                damageReceiver.GetHitt(1, 0);   // 보스는 어차피 넉백 없으니 attackAngle 무시
                Disappear(1f);
            });
        }
    }
}
