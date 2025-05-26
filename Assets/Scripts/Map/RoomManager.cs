using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Runtime.CompilerServices;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoomManager : MonoBehaviour
{
    public bool showGizmos = false;

    public Tilemap tilemap;

    [Title("RoomDatas")]
    public SORoom roomData;
    public Tilemap tempTilemap;

    public Transform roomCenterTransfrom;

    [FoldoutGroup("PreDatas")]
    public TileBase portTile;
    [FoldoutGroup("PreDatas")]
    public TileBase dangerTile;
    [FoldoutGroup("PreDatas")]
    public TileBase safeTile;
    [FoldoutGroup("PreDatas")]
    public GameObject triggerParent;
    [FoldoutGroup("PreDatas")]
    public GameObject trigger;

    public HashSet<Vector2Int> safePositions = new HashSet<Vector2Int>();

    public List<GimmickSignalSender> senderRefs = new List<GimmickSignalSender>();
    public List<GimmickSignalReceiver> receiverRefs = new List<GimmickSignalReceiver>();
    public List<GimmickSignalConnector> connectorRefs = new List<GimmickSignalConnector>();

    private void Awake()
    {
        MapManager.Instance.currentRoomManager = this;
    }

    private void Start()
    {
        //Invoke("Init", 0.5f);
        Init();

        if (MapManager.Instance != null)
            MapManager.Instance.currentRoomManager = this;

        safePositions = new HashSet<Vector2Int>(GetSafeLandingPosition());
    }
    #region Save/Load

    // 25.05.27) 메모 추가
    // 그냥 씬에 존재하는 모든 Sender와 Receiver, Connetor를 찾는 함수
    // 각 기믹의 상태 세이브/로드를 각 기믹 스스로가 담당하게 바꾸면서
    // 쓸모 없어지긴 했는데 어딘가 쓸모가 있을 것 같아서 일단 남겨둠
    private void GetAllGimmicks()
    {
        senderRefs = new List<GimmickSignalSender>();
        receiverRefs = new List<GimmickSignalReceiver>();
        connectorRefs = new List<GimmickSignalConnector>();

        GameObject[] senders = GameObject.FindGameObjectsWithTag("Sender");
        senders = senders.OrderBy(x => x.transform.position.magnitude).ToArray();
        for (int i = 0; i < senders.Length; i++)
            senderRefs.Add(senders[i].GetComponent<GimmickSignalSender>());

        GameObject[] receivers = GameObject.FindGameObjectsWithTag("Receiver");
        receivers = receivers.OrderBy(x => x.transform.position.magnitude).ToArray();
        for (int i = 0; i < receivers.Length; i++)
            receiverRefs.Add(receivers[i].GetComponent<GimmickSignalReceiver>());

        GameObject[] connectors = GameObject.FindGameObjectsWithTag("Connector");
        connectors = connectors.OrderBy(x => x.transform.position.magnitude).ToArray();
        for (int i = 0; i < connectors.Length; i++)
            connectorRefs.Add(connectors[i].GetComponent<GimmickSignalConnector>());
    }
    #endregion

    public void Init()
    {
        GetAllGimmicks();

        //요청
        ClearTriggers();
        SetTriggers(roomData.topPorts);
        SetTriggers(roomData.botPorts);
        SetTriggers(roomData.rigPorts);
        SetTriggers(roomData.lefPorts);

        tempTilemap.GetComponent<TilemapRenderer>().enabled = false;
    }

    public void ClearTriggers()
    {
        int flag = 0;
#if UNITY_EDITOR
        flag = 1;
#endif
        if (flag == 0)
            for (int i = 0; i < triggerParent.transform.childCount; i++)
                Destroy(triggerParent.transform.GetChild(triggerParent.transform.childCount - 1 - i).gameObject);
        else
            for (int i = 0; i < triggerParent.transform.childCount; i++)
                DestroyImmediate(triggerParent.transform.GetChild(triggerParent.transform.childCount - 1 - i).gameObject);
    }

    public void SetTriggers(List<RoomPort> roomPorts)
    {
        foreach (var port in roomPorts)
            SetTrigger(port);
    }

    public void SetTrigger(RoomPort roomPort)
    {
        Vector2Int size = Vector2Int.one;
        Vector2 mid;
        if (roomPort.direction == PortDirection.Top ||
            roomPort.direction == PortDirection.Bot)
        {
            int min = roomPort.ports.Min(vec => vec.x);
            int max = roomPort.ports.Max(vec => vec.x);

            size = new Vector2Int(max - min + 1, 1);
            mid = new Vector2((max + min) / 2 + 0.5f + (roomPort.ports.Count % 2 == 0 ? 0.5f : 0.0f),
                               roomPort.ports[0].y + 0.5f);
        }
        else
        {
            int min = roomPort.ports.Min(vec => vec.y);
            int max = roomPort.ports.Max(vec => vec.y);
            size = new Vector2Int(1, max - min + 1);
            mid = new Vector2(roomPort.ports[0].x + 0.5f,
                              (max + min) / 2 + 0.5f + (roomPort.ports.Count % 2 == 0 ? 0.5f : 0.0f));
        }

        GameObject tri = Instantiate(trigger, mid, Quaternion.identity);
        tri.transform.SetParent(triggerParent.transform);
        tri.transform.localPosition = mid;
        //tri.transform.position = mid;
        tri.GetComponent<BoxCollider2D>().size = size;
        tri.GetComponent<RoomPortObject>().SetRoomToPort(roomPort.connectedPorts, roomPort.direction);
    }

    [Button]
    public void ClearRoom()
    {
        tempTilemap.ClearAllTiles();
    }

    //방의 데이터를 모두 정리함.
    #region BakeRoom
    [Button]
    public void BakeRoom()
    {
#if UNITY_EDITOR
        string scenePath = this.gameObject.scene.path;
        if (!string.IsNullOrEmpty(scenePath))
            roomData.scene.SetScene(AssetDatabase.LoadAssetAtPath(scenePath, typeof(Object)), gameObject.scene.name);
#endif
        HashSet<Vector2Int> tilePosition = new HashSet<Vector2Int>(GetTilesPositionInTilemap(tempTilemap));
        int minX = int.MaxValue; int maxX = int.MinValue;
        int minY = int.MaxValue; int maxY = int.MinValue;

        HashSet<Vector2Int> tempPort = new HashSet<Vector2Int>();

        //타일 크기 측정 및 Port 추적
        foreach (Vector2Int tile in tilePosition)
        {
            if (minX > tile.x)
                minX = tile.x;
            if (maxX < tile.x)
                maxX = tile.x;
            if (minY > tile.y)
                minY = tile.y;
            if (maxY < tile.y)
                maxY = tile.y;

            if (tempTilemap.GetTile((Vector3Int)tile) == portTile)
                tempPort.Add(tile);
        }

        Debug.Log(tempPort.Count);

        roomData.tiles = new HashSet<Vector2Int>(tilePosition);
        List<RoomPort> top = new List<RoomPort>();
        List<RoomPort> bot = new List<RoomPort>();
        List<RoomPort> rig = new List<RoomPort>();
        List<RoomPort> lef = new List<RoomPort>();
        (top, bot, rig, lef) = GetPorts(tempPort, minX, maxX, minY, maxY);
        roomData.SetRoomPorts(top, bot, rig, lef);
        roomData.offset = new Vector2Int(minX, minY);
        roomData.size = new Vector2Int(maxX - minX + 1, maxY - minY + 1);

#if UNITY_EDITOR
        EditorUtility.SetDirty(roomData);
        AssetDatabase.SaveAssets();
#endif
    }

    public HashSet<Vector2Int> GetSafeLandingPosition()
    {
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>(GetTilesPositionInTilemap(tilemap));
        HashSet<Vector2Int> tempTiles = new HashSet<Vector2Int>(GetTilesPositionInTilemap(tempTilemap));
        HashSet<Vector2Int> safeTiles = new HashSet<Vector2Int>();

        HashSet<Vector2Int> danger = new HashSet<Vector2Int>();

        foreach (Vector2Int tile in tempTiles)
        {
            if (tempTilemap.GetTile((Vector3Int)tile) == dangerTile)
                danger.Add(tile);
            else if (tempTilemap.GetTile((Vector3Int)tile) == safeTile)
                safeTiles.Add(tile);
        }

        foreach (Vector2Int tile in tiles)
        {
            Vector2Int pos = tile + Vector2Int.up;

            if (pos.y >= roomData.offset.y + roomData.size.y)
                continue;

            if (danger.Contains(pos))
                continue;

            if (!tiles.Contains(pos))
                safeTiles.Add(pos);
        }

        return safeTiles;
    }

    private List<Vector2Int> GetTilesPositionInTilemap(Tilemap tileMap)
    {
        List<Vector2Int> availablePlaces = new List<Vector2Int>();
        for (int n = tileMap.cellBounds.xMin; n < tileMap.cellBounds.xMax; n++)
        {
            for (int p = tileMap.cellBounds.yMin; p < tileMap.cellBounds.yMax; p++)
            {
                Vector3Int localPlace = (new Vector3Int(n, p, (int)tileMap.transform.position.y));
                if (tileMap.HasTile(localPlace))
                {
                    UnityEngine.Tilemaps.TileData data = new UnityEngine.Tilemaps.TileData();
                    tileMap.GetTile(localPlace).GetTileData(localPlace, tileMap, ref data);

                    if (data.colliderType == Tile.ColliderType.None)
                        continue;

                    availablePlaces.Add((Vector2Int)localPlace);
                }
            }
        }

        return availablePlaces;
    }

    private (List<RoomPort>, List<RoomPort>, List<RoomPort>, List<RoomPort>) GetPorts(HashSet<Vector2Int> positions, int minX, int maxX, int minY, int maxY)
    {
        List<List<Vector2Int>> ports = new List<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (var point in positions)
        {
            if (!visited.Contains(point))
            {
                List<Vector2Int> port = new List<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();

                queue.Enqueue(point);
                visited.Add(point);

                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    port.Add(current);

                    foreach (var neighbor in GetNeighbors(current))
                    {
                        if (positions.Contains(neighbor) && !visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }

                ports.Add(port);
            }
        }

        Debug.Log(ports.Count);
        List<RoomPort> topPort = new List<RoomPort>();
        List<RoomPort> botPort = new List<RoomPort>();
        List<RoomPort> rigPort = new List<RoomPort>();
        List<RoomPort> lefPort = new List<RoomPort>();
        int[] index = new int[4] { 0, 0, 0, 0 };
        foreach (var port in ports)
        {
            if (port[0].x == minX)
            {
                lefPort.Add(new RoomPort(PortDirection.Lef, port, index[0]));
                index[0]++;
            }
            else if (port[0].x == maxX)
            {
                rigPort.Add(new RoomPort(PortDirection.Rig, port, index[1]));
                index[1]++;
            }
            else if (port[0].y == minY)
            {
                botPort.Add(new RoomPort(PortDirection.Bot, port, index[2]));
                index[2]++;
            }
            else if (port[0].y == maxY)
            {
                topPort.Add(new RoomPort(PortDirection.Top, port, index[3]));
                index[3]++;
            }
        }

        return (topPort, botPort, rigPort, lefPort);
    }

    List<Vector2Int> GetNeighbors(Vector2Int point)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(point.x + 1, point.y),
            new Vector2Int(point.x - 1, point.y),
            new Vector2Int(point.x, point.y + 1),
            new Vector2Int(point.x, point.y - 1)
        };

        return neighbors;
    }
    #endregion

    #region Event
    public void Enter(PortDirection direction, int index, float percentage,Vector3 playerPosition)
    {
        Debug.Log($"[{roomData.title}] [{direction}, {index}] 플레이어 입장");
        
        MapManager.Instance?.Enter(roomData, direction, index, percentage, playerPosition);
    }

    public void Exit(PortDirection direction, int index)
    {
        Debug.Log($"[{roomData.title}] [{direction}, {index}]플레이어 퇴장");
        MapManager.Instance?.Exit(roomData);

    }
    #endregion


    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        foreach (Vector2Int pos in safePositions)
        {
            Gizmos.DrawCube(new Vector3(pos.x + 0.5f, pos.y + 0.5f), Vector3.one);
        }
    }
}