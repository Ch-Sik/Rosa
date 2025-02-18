using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G_DeathLeap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        RespawnManager.Instance.Respawn();
    }
}
