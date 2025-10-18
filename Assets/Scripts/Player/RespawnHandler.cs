using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RespawnHandler : MonoBehaviour
{
    private static RespawnHandler _instance = null;
    public static RespawnHandler Instance { get { return _instance; } }

    [SerializeField] private bool showGizmos = false;
    [SerializeField] private int healAmount = 3;
    [SerializeField] private CircularBuffer<Vector2Int> respawnPoints = new CircularBuffer<Vector2Int>(6);
    private GameObject _player;
    
    [SerializeField, ReadOnly] private int enemyCount = 0;
    private Vector2Int _lastAddedRespawnPoint;

    private void Awake()
    {
        _instance = this; 
    }

    private void Start()
    {
        MapManager.Instance.OnNextRoomLoaded += () => { respawnPoints.Reset(); };
        _player = PlayerRef.Instance.gameObject;
    }

    private void Update()
    {
        if (MapManager.Instance.currentRoomManager == null)
            return;

        if (!PlayerRef.Instance.movement.isGrounded) return;
        // 땅 끝에 발이 걸치고 있을 경우를 대비해 한 번 더 검사
        var rayHit = Physics2D.Raycast(PlayerRef.Instance.transform.position, Vector2.down, 1.0f, 1 << LayerMask.NameToLayer("Ground"));
        if (rayHit.collider == null) return;
        
        if (enemyCount > 0)
        {
            UpdateRespawnPoint(_lastAddedRespawnPoint);
            return;
        }

        Vector2Int curPosition = new Vector2Int((int)(_player.transform.position.x),
                                                (int)(_player.transform.position.y - 0.8f));
        UpdateRespawnPoint(curPosition);
        _lastAddedRespawnPoint = curPosition;
    }

    public void UpdateRespawnPoint(Vector2Int curPos)
    {
        if (respawnPoints.GetLastNth(1) == curPos) return;
        respawnPoints.Add(curPos);
    }

    [Button]
    public void Respawn()
    {
        if (PlayerRef.Instance.state.CurrentHP <= 0)
            PlayerRef.Instance.state.Heal(healAmount);

        if (PlayerRef.Instance.movement.isGrabCube)
            PlayerRef.Instance.grabCube.UnGrab(true);

        if (PlayerRef.Instance.movement.isWallClimbing)
            PlayerRef.Instance.movement.UnstickFromWall();

        Vector2Int respawnPoint = respawnPoints.GetLastNth(5);
        _player.transform.position = new Vector3(respawnPoint.x + 0.5f, respawnPoint.y + 0.5f, _player.transform.position.z);
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.GetComponent<AIPerception>()) return;
        enemyCount++;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.GetComponent<AIPerception>()) return;
        enemyCount--;
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;
        
        Color[] col = {Color.red, Color.yellow, Color.green, Color.cyan, Color.magenta};

        for (int i = 0; i < 5; i++)
        {
            Vector2Int respawnPoint = respawnPoints.GetLastNth(i+1);
            Gizmos.color = col[i];
            Gizmos.DrawWireSphere(new Vector2(respawnPoint.x + 0.5f, respawnPoint.y + 0.5f), 1f);
        }
    }
}
