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
            EnterInitialRoom(startRoom, null);
        }
        // 이어하기 상황일 경우
        else
        {
            // TODO: 마지막으로 저장했을 때의 캐릭터 좌표 불러와서 startPosition에 저장
            PlayerPositionSave saveData = SaveLoadManager.Instance.LoadPlayerPosition();
            Debug.Log($"플레이어 위치 세이브데이터 로드: {saveData.room}\n위치: {saveData.position}");
            startRoom = rooms[saveData.room];
            EnterInitialRoom(startRoom, saveData.position);
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

    public void EnterInitialRoom(SORoom room, Vector2? position)
    {
        Sequence seq = DOTween.Sequence()
        .AppendCallback(() =>
            {
                if (position.HasValue)
                {
                    OpenScene(room, position.Value);
                }
                else
                {
                    OpenScene(room, startPoint);
                }
                currentRoom = room;
            })
        .AppendInterval(1)
        .AppendCallback(() =>
        {
            // 페이드 인 효과 적용
            FadeoutPanel.FadeIn();
        });
    }

    // 25.06.05) 참조 없는 함수 주석 처리
    ////강제 엔터
    //public void Enter(SORoom room)
    //{
    //    float fadeTime = FadeoutPanel.fadeDuration;
    //    Sequence seq = DOTween.Sequence()
    //    .AppendCallback(()=> { FadeoutPanel.Fadeout(); })  // 페이드아웃
    //    .AppendInterval(fadeTime)
    //    .AppendCallback(() =>
    //    {
    //        OpenScene(room);
    //        currentRoom = room;
    //        // Invoke("MoveStartPoint", 0.3f);
    //    })
    //    .AppendInterval(1)
    //    .AppendCallback(MoveToStartPoint)
    //    .AppendCallback(()=> { FadeoutPanel.FadeIn(); }); // 페이드 인
    //}

    public void Enter(SORoom room, Vector2 position)
    {
        float fadeTime = FadeoutPanel.fadeDuration + 0.1f;
        Sequence seq = DOTween.Sequence()
        .AppendCallback(() => { FadeoutPanel.Fadeout(); })  // 페이드아웃
        .AppendInterval(fadeTime)
        .AppendCallback(() =>
        {
            OpenScene(room, position);
            currentRoom = room;
        })
        .AppendInterval(1)
        .AppendCallback(() => { FadeoutPanel.FadeIn(); }); // 페이드 인
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

    public void Enter(PortDirection direction, List<ConnectedPort> ports)
    {
        /*
        currentRoom = room;

        List<SORoom> newRooms = new List<SORoom>();

        newRooms.Clear();
        newRooms.Add(room);
        newRooms.AddRange(room.GetConnectedRooms());

        CloseScenes(newRooms);
        OpenScenes(newRooms);

        oldRooms = new List<SORoom>(newRooms);
        */
        float fadeTime = FadeoutPanel.fadeDuration + 0.1f;
        Sequence seq = DOTween.Sequence()
        .AppendCallback(() => { FadeoutPanel.Fadeout(); })  // 페이드아웃
        .AppendInterval(fadeTime)
        .AppendCallback(() =>
        {
            //        currentRoom = ports[0].room;     //flag
            SORoom nextRoom = GetRoomSOtoConnectedPorts(ports);
            player.SetParent(transform);

            //        Vector2Int position = ports[0].room.(direction, ports[0].index).ports[0];
            //        Vector3 destination = new Vector3(position.x, position.y) + GetMargin(direction);
            Vector2Int position = nextRoom.GetRoomPort(direction, ports[0].index).ports[0];
            Vector3 destination = new Vector3(position.x, position.y) + GetMargin(direction);

            Debug.Log($"destination: {destination}");

            OpenScene(nextRoom, destination);
            currentRoom = nextRoom;
        })
        .AppendInterval(1)
        .AppendCallback(() => { FadeoutPanel.FadeIn(); }); // 페이드 인
    }

    //포트 충돌 엔터
    public void Enter(SORoom room, PortDirection direction, int index, float percentage, Vector3 playerPosition)
    {
        /*
        currentRoom = room;

        List<SORoom> newRooms = new List<SORoom>();

        newRooms.Clear();
        newRooms.Add(room);
        newRooms.AddRange(room.GetConnectedRooms());

        CloseScenes(newRooms);
        OpenScenes(newRooms);

        oldRooms = new List<SORoom>(newRooms);
        */

        CloseScene(currentRoom);
        currentRoom = room;
        StartCoroutine(AsyncOpenScene(currentRoom, Vector3.zero));

        //플래그
        FindConnectedPosition(room, direction, index, percentage, playerPosition);
    }

    public void Exit(SORoom room)
    {
    }

    public void FindConnectedPosition(SORoom room, PortDirection direction, int index, float percentage, Vector3 playerPosition, int flag = 0)
    {
        if (!oldRooms.Contains(room))
            return; //연결된 방 로드되지 않음.

        //대상 Port
        Debug.Log($"{room.title}의 {direction}의 {index}는 {percentage}");

        RoomPort port = room.GetPort(direction, index);
        List<ConnectedPort> connects = room.GetConnectedPort(direction, index);

        ConnectedPort exitPort = connects[flag];

//        Debug.Log($"{exitPort.room.title}의 {GetOppositeDirection(direction)}의 {exitPort.index}의 {port.GetPortPosition(percentage)}연결됨");

        Vector3 transportPosition = port.GetPortPosition(percentage);
        if (port.isHorizontal())
            transportPosition.y = playerPosition.y;
        else
            transportPosition.x = playerPosition.x;

        transportPosition += GetMargin(GetOppositeDirection(direction));
        transportPosition += GetTransportPostion(exitPort, direction);

        Debug.Log($"transportPosition: {transportPosition}");

        player.position = transportPosition;
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
    public void OpenScenes(List<SORoom> rooms)
    {
        //이미 열려있는 씬이라면, 리턴
        foreach (SORoom room in rooms)
        {
            if (IsOpenScene(room))
                continue;

            OpenScene(room);
        }
    }

    private bool IsOpenScene(SORoom room)
    {
        if(oldRooms.Contains(room))
            return true;

        return false;
    }

    public void OpenScene(SORoom room)
    {
        OpenScene(room, Vector2.zero);
    }

    public void OpenScene(SORoom room, Vector2 pos)
    {
        if (currentRoom != null)
        {
            Debug.Log($"[MapManager] 기존 방 언로드 시작: {currentRoom.name}");
            CloseScene(currentRoom);
        }
        Debug.Log($"[MapManager] 다음 방 로드 시작: {room.name}");
        StartCoroutine(AsyncOpenScene(room, pos));
    }

    //동기화를 위한 코루틴 사용
    public IEnumerator AsyncOpenScene(SORoom room, Vector3 playerPosition)
    {
        SceneField scene = room.scene;
        if (!SceneManager.GetSceneByName(scene.SceneName).isLoaded)
        {
            bool isClimbing = false;    

            if (PlayerRef.Instance.movement.isWallClimbing)
            {
                Debug.Log("매달린 상태 해제");
                PlayerRef.Instance.movement.wallClimbEnabled = false;
                PlayerRef.Instance.movement.UnstickFromWall();

                isClimbing = true;
            }
            player.SetParent(transform);
            player.SetParent(null);
            player.position = playerPosition;
            cam.MoveCameraInstantlyToPosition(player.position);

            //비동기 로드
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Additive);

            //로드 완료될 때까지 대기
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 25.05.27)
            // 방 내부의 기믹 세이브 로드 책임을 각 기믹 스스로에게로 이동
            // LoadSceneState();

            if (isClimbing)
                PlayerRef.Instance.movement.wallClimbEnabled = true;

            chapterDebugUI.text = room.scene.SceneName;

            // 25.03.03 추가
            // 로드 후 이벤트 발생시킴
            OnNextRoomLoaded?.Invoke();
        }

        /*
        Scene sce = SceneManager.GetSceneByName(scene.SceneName);
        GameObject[] objects = sce.GetRootGameObjects();
        foreach (GameObject obj in objects)
            obj.GetComponent<Room>()?.Init();
        */
    }

    public void CloseScenes(List<SORoom> rooms)
    {
        //차집합이라면, 클로즈
        List<SORoom> differences = oldRooms.Except(rooms).ToList();

        differences.Remove(currentRoom);

        foreach (SORoom room in differences)
            CloseScene(room);
    }

    public void CloseScene(SORoom room)
    {
        SceneField scene = room.scene;

        SceneManager.UnloadSceneAsync(scene);
    }

    // 25.05.27) 세이브로드를 모아서 하는 게 아니라 각 기믹 스스로가 
    //           플래그를 조작하도록 하여 세이브/로드가 이루어질 수 있도록 수정

    //public void SaveSceneState()
    //{
    //    // TODO: 방 내부의 기믹 상태 저장
    //    return;     // 기능 정상 작동하지 않으므로 일단 비활성화

    //    //현재 룸에 대한 저장
    //    List<int> senders = new List<int>();

    //    senders = currentRoomManager.GetAllGimmicksStates();
    //    SaveLoadManager.Instance.SaveMap(currentRoom.scene.SceneName, senders);
    //}

    //public void LoadSceneState()
    //{
    //    if (SaveLoadManager.Instance.CanLoadSceneState(currentRoom.scene.SceneName))
    //    {
    //        MapSaveData Data = SaveLoadManager.Instance.LoadSceneState(currentRoom.scene.SceneName);

    //        if (Data != null)
    //            currentRoomManager.SetAllGimmickStates(Data.LoadSenders());
    //    }
    //}

    public bool OpenSceneBySceneNameWithPosition(string SceneName, Vector2 Position)
    {
        if (!rooms.ContainsKey(SceneName))
        {
            Debug.LogError($"[Scene Load Error] 해당 이름의 씬을 찾을 수 없다. : {SceneName}");
            return false;
        }

        if (SceneManager.GetSceneByName(currentRoom.scene.SceneName).isLoaded)
            CloseScene(currentRoom);
        StartCoroutine(AsyncOpenScene(rooms[SceneName], Position));

        PlayerRef.Instance.transform.position = Position;

        return true;
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