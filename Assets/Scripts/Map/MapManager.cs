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
    public Room room;
    public Transform startPoint;
    public SORoom startRoom;
    //현재 열린 씬
    public SORoom currentRoom;
    public List<SORoom> oldRooms = new List<SORoom>();
    public TextMeshProUGUI chapter;
    public TextMeshProUGUI position;
    [ShowInInspector] private Dictionary<string, SORoom> rooms;

    public Image fadePanel;

    private void Update()
    {
        position.text = $"{player.position.x.ToString("F1")} , {player.position.y.ToString("F1")}";
    }

    private void Awake()
    {
        LoadAllRooms();
    }

    private void Start()
    {
        if (startRoom == null)
            startRoom = FindStartRoom();
        if (startRoom == null)
            return;
        Init(startRoom);
    }

    private void Init(SORoom startRoom)
    {
        Enter(startRoom);

        startPoint.GetComponent<SpriteRenderer>().enabled = false;
    }

    private int LoadAllRooms()
    {
        int cnt = 0;

        rooms = new Dictionary<string, SORoom>();

        SORoom[] _rooms = Resources.LoadAll<SORoom>("RoomDatas");
        cnt = _rooms.Length;

        for (int i = 0; i < _rooms.Length; i++)
            if (rooms.ContainsKey(_rooms[i].scene.SceneName))
                Debug.LogError($"[Duplicated Scene Name Error] 동일 이름의 씬이 둘 이상 존재하기에 딕셔너리 충돌 에러 발생 : {_rooms[i].scene.SceneName}");
        else
            rooms.Add(_rooms[i].scene.SceneName, _rooms[i]);

        return cnt;
    }

    private SORoom FindStartRoom()
    {
        this.room = GetComponent<Room>();
        // if (this.room == null) return null;
        SORoom room = this.room.roomData;

        return room;
    }

    #region Room Events
    //강제 엔터
    public void Enter(SORoom room)
    {
        Sequence seq = DOTween.Sequence()
        .Append(fadePanel.DOFade(1, 0.5f))
        .AppendCallback(() =>
        {
            OpenScene(room);
            currentRoom = room;
            Invoke("MoveStartPoint", 0.3f);
        })
        .AppendInterval(1)
        .Append(fadePanel.DOFade(0, 0.5f));
    }

    public void Enter(SORoom room, Vector2 position)
    {
        Sequence seq = DOTween.Sequence()
        .Append(fadePanel.DOFade(1, 0.5f))
        .AppendCallback(() =>
        {
            OpenScene(room, position);
            currentRoom = room;
        })
        .AppendInterval(1)
        .Append(fadePanel.DOFade(0, 0.5f));
    }

    public void MoveStartPoint()
    {
        player.position = startPoint.position;
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

        // 다른 방으로 진입할 경우, 현재 방의 상태 저장
        SaveSceneState();

        Sequence seq = DOTween.Sequence()
        .Append(fadePanel.DOFade(1, 0.5f))
        .AppendCallback(() =>
        {
            //        currentRoom = ports[0].room;     //flag
            SORoom nextRoom = GetRoomSOtoConnectedPorts(ports);
            player.SetParent(transform);

            //        Vector2Int position = ports[0].room.(direction, ports[0].index).ports[0];
            //        Vector3 destination = new Vector3(position.x, position.y) + GetMargin(direction);
            Vector2Int position = nextRoom.GetRoomPort(direction, ports[0].index).ports[0];
            Vector3 destination = new Vector3(position.x, position.y) + GetMargin(direction);

            OpenScene(nextRoom, destination);
            currentRoom = nextRoom;
        })
        .AppendInterval(1)
        .Append(fadePanel.DOFade(0, 0.5f));
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
                return new Vector3(-2, 0);
            case PortDirection.Lef:
                return new Vector3(2, 0);

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
        if (currentRoom != null) CloseScene(currentRoom);
        StartCoroutine(AsyncOpenScene(room, Vector3.zero));
    }

    public void OpenScene(SORoom room, Vector2 pos)
    {
        if (currentRoom != null) CloseScene(currentRoom);
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

            // 로드 완료되면 해당 방 안의 기믹들 상태 로드하여 복구
            LoadSceneState();

            if (isClimbing)
                PlayerRef.Instance.movement.wallClimbEnabled = true;

            chapter.text = room.scene.SceneName;
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

    public void SaveSceneState()
    {
        //현재 룸에 대한 저장
        List<int> senders = new List<int>();

        senders = room.GetAllGimmicksStates();
        SaveLoadManager.Instance.SaveMap(currentRoom.scene.SceneName, senders);
    }

    public void LoadSceneState()
    {
        if (SaveLoadManager.Instance.CanLoadMap(currentRoom.scene.SceneName))
        {
            MapSaveData Data = SaveLoadManager.Instance.LoadMap(currentRoom.scene.SceneName);

            if (Data != null)
                room.SetAllGimmickStates(Data.LoadSenders());
        }
    }

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
}