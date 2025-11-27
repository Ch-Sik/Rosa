using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G_DeathLeap : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            PlayerRef.Instance.damageReceiver.GetDamageIgnoreInvincible(gameObject, damage);
            RespawnHandler.Instance.Respawn();
        }
    }
}
