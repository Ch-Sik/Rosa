using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnSector : MonoBehaviour
{
    private RespawnHandler _respawnHandler;

    [SerializeField] Transform respawnPoint;

    private void Start()
    {
        _respawnHandler = RespawnHandler.Instance;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            //respawnManager.SwitchRespawnPoint(respawnPoint);
        }
    }
}
