using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Com.LuisPedroFonseca.ProCamera2D;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using DG.Tweening;


public class MapManager : MonoBehaviour
{
    // 싱글톤
    private static MapManager instance;

    public static MapManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MapManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    instance = obj.AddComponent<MapManager>();
                }
            }
            return instance;
        }
    }

    public MapDataSO map;
    public Transform player;
    public ProCamera2D cam;
    //시작할 씬
    public RoomManager currentRoomManager;
    // 25.04.29) startPoint를 transform 대신 수치 입력의 Vector3로 대체
    public bool drawStartpointGizmo;
    public Vector3 startPoint;
    // 25.06.06) 주석 추가
    // 인스펙터 상에서 값을 지정해서 강제로 시작 방을 설정할 수 있음.
    // 없으면 처음부터인지 이어하기인지에 따라 적절한 방을 찾아서 설정함. 
    public SORoom startRoom;
    
    public SORoom CurrentRoom { get { return currentRoom; } }
    //현재 열린 씬
    private SORoom currentRoom;

    public List<SORoom> oldRooms = new List<SORoom>();
    public TextMeshProUGUI chapterDebugUI;
    // 25.05.04) 디버그용 좌표 출력 UI 제거
    // public TextMeshProUGUI positionDebugUI;
    [ShowInInspector] private Dictionary<string, SORoom> rooms;

    public Action OnNextRoomLoaded;

    private void Update()
    {
        // positionDebugUI.text = $"{player.position.x.ToString("F1")} , {player.position.y.ToString("F1")}";
    }

    private void Awake()
    {
        LoadAllRooms();
    }

    // 각 방의 정보를 들고 있는 ScriptableObject를 일괄 로드
    private int LoadAllRooms()
    {
        int cnt = 0;

        rooms = new Dictionary<string, SORoom>();

        SORoom[] _rooms = Resources.LoadAll<SORoom>("RoomDatas");
        cnt = _rooms.Length;

        for (int i = 0; i < _rooms.Length; i++)
        {
            if (rooms.ContainsKey(_rooms[i].scene.SceneName))
            {
                Debug.LogError($"[Duplicated Scene Name Error] 동일 이름의 씬이 둘 이상 존재하기에 딕셔너리 충돌 에러 발생 : {_rooms[i].scene.SceneName}");
            }
            else
            {
                rooms.Add(_rooms[i].scene.SceneName, _rooms[i]);
            }
        }

        Debug.Log($"rooms 딕셔너리 로드 완료. Count: {rooms.Count}");
        return cnt;
    }

    private void Start()
    {
        // 새로하기 상황일 경우
        if (SaveLoadManager.Instance.IsNewGame)
        {
            if(startRoom == null)
                startRoom = FindStartRoom();
            if (startRoom == null)
            {
                Debug.LogError("시작할 방을 찾을 수 없음");
                return;
            }
            Enter(startRoom, startPoint);
        }
        // 이어하기 상황일 경우
        else
        {
            // TODO: 마지막으로 저장했을 때의 캐릭터 좌표 불러와서 startPosition에 저장
            PlayerPositionSave saveData = SaveLoadManager.Instance.LoadPlayerPosition();
            Debug.Log($"플레이어 위치 세이브데이터 로드: {saveData.room}\n위치: {saveData.position}");
            startRoom = rooms[saveData.room];
            Enter(startRoom, saveData.position);
            // Enter(startRoom, saveData.position);
        }
    }

    private SORoom FindStartRoom()
    {
        this.currentRoomManager = GetComponent<RoomManager>();
        // if (this.room == null) return null;
        SORoom room = this.currentRoomManager.roomData;

        return room;
    }

    #region Room Events
    public void Enter(PortDirection direction, List<ConnectedPort> ports)
    {
        SORoom nextRoom = GetRoomSOtoConnectedPorts(ports);
        player.SetParent(transform);
        Vector2Int nextRoomPortPosition = nextRoom.GetRoomPort(direction, ports[0].index).ports[0];
        Vector3 frontOfPortPosition = new Vector3(nextRoomPortPosition.x, nextRoomPortPosition.y) + GetMargin(direction);
        Enter(nextRoom, frontOfPortPosition);
    }

    public void Enter(SORoom room, Vector2 position)
    {
        StartCoroutine(EnterCoroutine());
        IEnumerator EnterCoroutine()
        {
            bool wasClimbing;
            PreservePlayerStates(out wasClimbing);
            float fadeTime = FadeoutPanel.fadeDuration + 0.1f;

            AsyncOperation loadOp = StartLoadNextScene(room);

            // 페이드아웃 효과 없었다면 적용
            if (!FadeoutPanel.isFadeOutActivated)
            {
                FadeoutPanel.Fadeout();
                yield return new WaitForSeconds(fadeTime);      // 최소한 페이드 효과 시간만큼은 기다리고
            }
            else
            {
                Debug.Log("이미 페이드 아웃 효과 적용되어있으므로 추가 적용은 생략");
            }

            while (loadOp.progress < 0.9f)                  // 로딩 시간 필요하면 더 기다리고
            {
                Debug.Log($"씬 로딩 중: {loadOp.progress * 100}%");
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);          // 로딩 마무리 될 때까지 기다림
            ActivateNextScene(loadOp, room);
            SetPlayerPositionAndStates(position, wasClimbing);

            // OnNextRoomLoaded는 currentRoom이 갱신된 이후에 호출
            OnNextRoomLoaded?.Invoke();

            // 페이드아웃 효과 정리
            FadeoutPanel.FadeIn();
        }
    }

    public SORoom GetRoomSOtoConnectedPorts(List<ConnectedPort> ports)
    {
        SORoom room = null;

        string flag = "";
        string sceneName = "";

        flag = ports[0].flag;
        sceneName = ports[0].scene.SceneName;

        //플래그 없을 때
        if (string.IsNullOrEmpty(flag))
            return map.GetSORoomBySceneName(sceneName);

        return null;
    }

    public Vector3 GetMargin(PortDirection direction)
    {
        switch (direction)
        {
            case PortDirection.Top:
                return new Vector3(0, -2);
            case PortDirection.Bot:
                return new Vector3(0, 2);
            case PortDirection.Rig:
                return new Vector3(-2, 2f);
            case PortDirection.Lef:
                return new Vector3(2, 2f);

            default: return Vector3.zero;
        }
    }

    //ConnectedPort를 통해 월드 포지션을 얻어오자.
    public Vector3 GetTransportPostion(ConnectedPort port, PortDirection direction)
    {
        Vector3 position = Vector3.zero;
        //        SORoom targetRoom = port.room;
        SORoom targetRoom = map.GetSORoomBySceneName(port.scene);
        Vector2Int portPosition = targetRoom.GetPort(direction, port.index).ports[0];

        //position += targetRoom.tilemapWorldPosition;

        return position;
    }

    public PortDirection GetOppositeDirection(PortDirection direction)
    {
        switch (direction)
        {
            case PortDirection.Top: return PortDirection.Bot;
            case PortDirection.Bot: return PortDirection.Top;
            case PortDirection.Rig: return PortDirection.Lef;
            case PortDirection.Lef: return PortDirection.Rig;
            default: return PortDirection.Top;
        }
    }
    #endregion

    #region Scene Methods
    
    private void PreservePlayerStates(out bool wasClimbing)
    {
        // 덩굴 기어올라서 맵 이동하는 경우 고려
        wasClimbing = false;
        if (PlayerRef.Instance.movement.isWallClimbing)
        {
            wasClimbing = true;

            Debug.Log("매달린 상태 해제");
            PlayerRef.Instance.movement.wallClimbEnabled = false;
            PlayerRef.Instance.movement.UnstickFromWall();
        }

        // 플레이어가 덩굴 등의 자식 오브젝트로 설정되어 Scene Unload 때 같이 unload되는 것 방지
        player.SetParent(null);
        cam.MoveCameraInstantlyToPosition(player.position);
    }

    private AsyncOperation StartLoadNextScene(SORoom room)
    {
        SceneField sceneF = room.scene;
        // 해당 씬이 이미 로드되어있다면 리턴
        if (SceneManager.GetSceneByName(sceneF).isLoaded)
        {
            Debug.LogError("[MapManager] 해당 씬은 이미 로드되어있음");
            return null;
        }

        //비동기 로드 개시
        Debug.Log($"[MapManager] 다음 방 로드 시작: {room.name}");
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneF.SceneName, LoadSceneMode.Additive);
        sceneLoadOperation.allowSceneActivation = false;

        return sceneLoadOperation;
    }

    private void ActivateNextScene(AsyncOperation sceneLoadOperation, SORoom room)
    {
        // 로드된 씬 활성화 허용
        sceneLoadOperation.allowSceneActivation = true;
        // 기존 씬 있다면 언로드
        if (currentRoom != null)
        {
            Debug.Log($"[MapManager] 기존 방 언로드 시작: {currentRoom.name}");
            CloseScene(currentRoom);
        }
        currentRoom = room;
    }

    private void SetPlayerPositionAndStates(Vector2 playerPosition, bool isClimbing)
    {
        player.position = playerPosition;
        cam.MoveCameraInstantlyToPosition(playerPosition);
        if (isClimbing)
            PlayerRef.Instance.movement.wallClimbEnabled = true;
    }

    public void CloseScene(SORoom room)
    {
        SceneField scene = room.scene;

        SceneManager.UnloadSceneAsync(scene);
    }
    #endregion

    private void OnDrawGizmos()
    {
        if(drawStartpointGizmo)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(startPoint, Vector3.one);
        }
    }
}