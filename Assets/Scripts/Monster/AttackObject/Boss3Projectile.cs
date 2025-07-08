using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss3Projectile : MonsterProjectile
{
    // 벽에 박혀있는지 여부
    [SerializeField] private bool isStuck = false;

    // 벽에 닿아도 사라지지 않고 남아있도록 함수 오버라이드
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy하는 코드는 삭제
        // 제자리에 박혀있기, 충돌(공격)판정은 비활성화
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rigidbody.velocity = Vector2.zero;
            rigidbody.isKinematic = true;
            foreach(var col in colliders)
            {
                col.enabled = false;
                col.excludeLayers = LayerMask.GetMask("Ground");     // 깃털 회수 중에 지형과 부딪혀 공격 판정 해제되는 것 방지
            }
        }

        if(canDestroyMushroom && (other.tag == "Mushroom"))
        {
            Debug.Log("버섯 파괴 시전");
            other.GetComponent<MagicMushroom>().DoDestroy();
        }
    }

    // 투사체 회수 기믹
    public void RetrieveProjectile(Vector3 returnPosition)
    {
        // 돌아가야할 위치 계산

        // 돌아가기 시퀀스
        DOTween.Sequence()
        .Append(rigidbody.DOMoveY(rigidbody.position.y + 0.6f, 1f))
        .InsertCallback(0.5f, () => {
            // TODO: 깃털 활성화되는 거 시각화 필요
            foreach(var col in colliders)
            {
                col.enabled = true;
            }
        })
        .Insert(0.5f, transform.DORotate(new Vector3(0, 0, 360), 0.4f)
                    .SetRelative().SetEase(Ease.InOutCubic)
                )
        .AppendInterval(0.2f)
        .Append(rigidbody.DOMove(returnPosition, 0.4f))
        .AppendCallback(()=>{
            Destroy(gameObject);
        });
    }
}
