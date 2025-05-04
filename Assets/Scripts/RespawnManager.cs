using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RespawnManager : MonoBehaviour
{
    private static RespawnManager _instance = null;

    public static RespawnManager Instance { get { return _instance; } }

    public bool showGizmos = false;
    [SerializeField] Vector2Int respawnPoint;
    [SerializeField] GameObject player;

    Vector2Int curPosition;

    public int healAmount = 3;
    private void Awake()
    {
        _instance = this; 
    }

    private void Update()
    {
        if (MapManager.Instance.currentRoomManager == null)
            return;

        curPosition = new Vector2Int((int)(player.transform.position.x),
                                                (int)(player.transform.position.y - 0.8f));

        if (!MapManager.Instance.currentRoomManager.safePositions.Contains(curPosition))
            return;

        SwitchRespawnPoint(curPosition);
    }

    public void SwitchRespawnPoint(Vector2Int respawnPoint)
    {
        this.respawnPoint = respawnPoint;
    }

    [Button]
    public void Respawn()
    {
        if (PlayerRef.Instance.state.GetHP() <= 0)
            PlayerRef.Instance.state.Heal(healAmount);

        if (PlayerRef.Instance.movement.isGrabCube)
            PlayerRef.Instance.grabCube.UnGrab(true);

        if (PlayerRef.Instance.movement.isWallClimbing)
            PlayerRef.Instance.movement.UnstickFromWall();

        player.transform.position = new Vector3(respawnPoint.x + 0.5f, respawnPoint.y + 0.5f, player.transform.position.z);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector2(respawnPoint.x, respawnPoint.y), 1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector2(curPosition.x, curPosition.y), 1f);
    }
}
