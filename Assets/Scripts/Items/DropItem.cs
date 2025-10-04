using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 몬스터를 잡거나 혹은 몬스터의 패턴으로 인해 일시적으로 생성되는 아이템들
/// </summary>
public class DropItem : FieldItem
{
    [SerializeField] private bool hasLifetime = false;
    [SerializeField, ShowIf("hasLifetime")] private float lifetime = 5f;

    // 낙하하다가 지면에 닿으면 그대로 멈추기. 
    // rigidbody & non-trigger collider가 부착되지 않은 npc형 dropItem에는 해당 안됨.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.isKinematic = true;

        if (hasLifetime)
            StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
