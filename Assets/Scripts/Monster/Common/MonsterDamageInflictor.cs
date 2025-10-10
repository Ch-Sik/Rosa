using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[/*RequireComponent(typeof(Collider2D)),*/ DisallowMultipleComponent]
public class MonsterDamageInflictor : MonoBehaviour
{
    public int damage;
    public bool attackEnabled = true;

    public bool overrideInvincibleDuration = false;
    public float ignoreDuration = 2f; 
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
            // Debug.Log("damaged");
            if(!overrideInvincibleDuration)
            {
                go.GetComponent<PlayerDamageReceiver>().GetDamage(gameObject, damage);
            }
            else
            {
                go.GetComponent<PlayerDamageReceiver>().GetDamage(gameObject, damage, ignoreDuration);
            }
           
        }
    }

    private void OnDie()
    {
        // 사망했을 때 공격 기능 상실
        attackEnabled = false;
    }
}
