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
            bool requireRespawn = PlayerRef.Instance.state.CurrentHP <= damage;
            PlayerRef.Instance.damageReceiver.GetDamageAndRespawn(damage);
        }
    }
}
