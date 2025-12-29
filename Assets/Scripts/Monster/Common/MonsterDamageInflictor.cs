using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[/*RequireComponent(typeof(Collider2D)),*/ DisallowMultipleComponent]
public class MonsterDamageInflictor : MonoBehaviour
{
    public const float DefaultKnockbackCoef = 0.001f;
    
    public int damage;
    public bool attackEnabled = true;

    public bool overrideInvincibleDuration = false;
    public float ignoreDuration = 2f;
    public bool overrideKnockbackPower = false;
    [ShowIf("overrideKnockbackPower")] public float knockbackPower;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(attackEnabled)
            DoDamageIfItsPlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (attackEnabled)
            DoDamageIfItsPlayer(collision.gameObject);
    }

    private void DoDamageIfItsPlayer(GameObject go)
    {
        if (go.CompareTag("Player"))
        {
            float knockback = overrideKnockbackPower ? knockbackPower : DefaultKnockbackCoef;
            // Debug.Log("damaged");
            if(!overrideInvincibleDuration)
            {
                go.GetComponent<PlayerDamageReceiver>().GetDamage(gameObject, damage, knockback);
            }
            else
            {
                go.GetComponent<PlayerDamageReceiver>()
                    .GetDamage(gameObject, damage, ignoreDuration, knockback);
            }
           
        }
    }

    private void OnDie()
    {
        // 사망했을 때 공격 기능 상실
        attackEnabled = false;
    }
}
