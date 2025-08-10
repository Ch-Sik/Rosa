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
    public int damage;
    public float ignoreDuration = 2f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
            DoDamage(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
            DoDamage(collision.gameObject);
    }

    private void DoDamage(GameObject go)
    {
        // 데미지를 줄 목표 MonsterDamageReceiver 가져오기
        MonsterDamageReceiver target = go.GetComponent<MonsterDamageReceiver>();
        if(target == null)
        {
            Debug.LogError("대상이 MonsterDamageReceiver를 가지고 있지 않음!");
            return;
        }
        // 넉백 각도 계산을 위해 공격 투사체의 현재 진행방향 가져오기
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 projectileVelocity = rb.velocity;
        float projectileAngle = Mathf.Atan2(projectileVelocity.y, projectileVelocity.x) * Mathf.Rad2Deg;
        // 데미지와 넉백각도 전달
        target.GetHitt(damage, projectileAngle);
    }
}
