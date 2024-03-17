using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMushroom : MagicObject
{
    [SerializeField] float jumpPower; // 점프력

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if (PlayerRef.Instance.Controller.isMIDAIR)
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = (Vector2.up * jumpPower);
        }
    }
}
