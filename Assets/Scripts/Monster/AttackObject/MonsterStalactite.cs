using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터 종유석 패턴에서 사용되는 오브젝트 스크립트
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MonsterStalactite : MonoBehaviour
{
    private new Rigidbody2D rigidbody;
    private new Collider2D collider;
    private SpriteRenderer spriteRenderer;


    [SerializeField]
    private float dropSpeed = 3f;
    [SerializeField]
    private bool useRandomRotation;
    [SerializeField, Tooltip("데미지 판정 있을 때의 컬러")]
    private Color activatedColor;
    [SerializeField, Tooltip("데미지 판정 비활성화 되었을 때의 컬러")]
    private Color inactivatedColor;

    public void Init()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody.isKinematic = true;
        spriteRenderer.color = activatedColor;
        if (useRandomRotation)
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
    }

    public void Launch()
    {
        rigidbody.velocity = Vector2.down * dropSpeed;
        rigidbody.isKinematic = false;
    }

    private void DoDestroy()
    {
        Destroy(gameObject, 0.3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 지면에 닿았을 경우 공격 판정 해제
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector2.zero;
            collider.enabled = false;
            spriteRenderer.color = inactivatedColor;
            DoDestroy();
        }
    }
}
