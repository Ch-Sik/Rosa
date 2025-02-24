using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Video;

public class SaveLoadManager : MonoBehaviour
{
    #region Singleton
    private static SaveLoadManager instance;
    public static SaveLoadManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion

    public bool useSaveLoad = true;
    public bool useProjectSave = true;      //컴퓨터에 저장할지 프로젝트에 저장할지

    [FoldoutGroup("Paths"), ReadOnly] public string pathName = "SaveFile";
    [FoldoutGroup("Paths"), ReadOnly] public string flagPathName = "Flag";
    [FoldoutGroup("Paths"), ReadOnly] public string mapPathName = "Maps";
    [FoldoutGroup("Paths"), ReadOnly] public string playerPathName = "Player";
    [FoldoutGroup("Paths"), ReadOnly] public string optionPathName = "Option";


    private void Start()
    {
        Debug.Log($"세이브 사용이 {useSaveLoad}로 설정되어 있습니다.");
        MakeDirectoryHierarchy();
    }


    #region Utils
    //Path 병합해서 전달
    private string GetPath(string path)
    {
#if UNITY_EDITOR
        if (useProjectSave)
            return $"{Application.dataPath}/{pathName}/{path}";
        else
            return $"{Application.persistentDataPath}/{pathName}/{path}";
#else
            return $"{Application.persistentDataPath}/{pathName}/{path}";
#endif
    }

    private void MakeDirectory(string path)
    {
        //폴더가 존재하지 않는 경우 생성
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private void MakeDirectoryHierarchy()
    {
#if UNITY_EDITOR
        if (useProjectSave)
            MakeDirectory($"{Application.dataPath}/{pathName}");
        else
            MakeDirectory($"{Application.persistentDataPath}/{pathName}");
#else
            MakeDirectory($"{Application.persistentDataPath}/{pathName}");
#endif
        MakeDirectory(GetPath(flagPathName));
        MakeDirectory(GetPath(mapPathName));
        MakeDirectory(GetPath(playerPathName));
        MakeDirectory(GetPath(optionPathName));
    }
    #endregion

    #region Map
    [Button]
    public void SaveCurrentMap()
    {
        MapManager.Instance.SaveSceneState();
    }

    //세이브 처리부 - MapLoader에서 자동 호출 됌.
    public void SaveMap(string sceneName, List<int> senders)
    {
        MapSaveData Data = new MapSaveData()
        {
            sceneName = sceneName,
        };
        Data.SaveSender(senders);

        string filePath = GetPath(mapPathName) + $"/{sceneName}.json";
        string json = JsonUtility.ToJson(Data);
        File.WriteAllText(filePath, json);
    }

    public bool CanLoadMap(string sceneName) { return File.Exists(GetPath(mapPathName) + $"/{sceneName}.json"); }

    public MapSaveData LoadMap(string sceneName)
    {
        string filePath = GetPath(mapPathName) + $"/{sceneName}.json";
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[Map Data] {filePath}를 찾을 수 없다.");
            return null;
        }

        string json = File.ReadAllText(filePath);
        MapSaveData Data = JsonUtility.FromJson<MapSaveData>(json);

        return Data;
    }
    #endregion

    #region Flag
    [Button]
    public void SaveFlag()
    {
        List<string> fs = new List<string>();
        List<int> vs = new List<int>();

        (fs, vs) = FlagManager.Instance.GetFlagsData();

        FlagSaveData Data = new FlagSaveData()
        {
            Flags = fs,
            Values = vs
        };

        string filePath = GetPath(flagPathName) + "/flag.json";
        string json = JsonUtility.ToJson(Data);
        File.WriteAllText(filePath, json);
    }

    [Button]
    public FlagSaveData LoadFlag()
    {
        string filePath = GetPath(flagPathName) + "/flag.json";
        if (!File.Exists(filePath)) {
            Debug.LogError($"[Flag Data] {filePath}를 찾을 수 없다.");
            return null;
        }

        string json = File.ReadAllText(filePath);
        FlagSaveData Data = JsonUtility.FromJson<FlagSaveData>(json);

        return Data;
    }
    #endregion

    #region Player
    [Button]
    public void SavePlayerData()
    {
        PlayerSaveData Data = new PlayerSaveData()
        {
            lastScene = MapManager.Instance.currentRoom.scene.SceneName,
            lastPos = PlayerRef.Instance.transform.position
        };

        string filePath = GetPath(playerPathName) + "/player.json";
        string json = JsonUtility.ToJson(Data);
        File.WriteAllText(filePath, json);
    }

    [Button]
    public void LoadPlayerData()
    {
        string filePath = GetPath(playerPathName) + "/player.json";
        if (!File.Exists(filePath)) {
            //초기 파일 생성
            //Debug.LogError($"[Player Data] {filePath}를 찾을 수 없다.");
            return;
        }

        string json = File.ReadAllText(filePath);
        PlayerSaveData Data = JsonUtility.FromJson<PlayerSaveData>(json);

        Debug.Log($"Data : {Data.lastScene}, Pos : {Data.lastPos}");

        MapManager.Instance.OpenSceneBySceneNameWithPosition(Data.lastScene, Data.lastPos);
    }
    #endregion

    #region Option
    public void SaveOptionData()
    {
        OptionUI option = FindObjectOfType<OptionUI>();

        if (option == null)
            return;

        SaveOptionData(option.currentOption);
    }

    public void SaveOptionData(OptionSetting option)
    {
        OptionSaveData Data = new OptionSaveData(option);

        string filePath = GetPath(optionPathName) + "/option.json";
        string json = JsonUtility.ToJson(Data);
        File.WriteAllText(filePath, json);
    }

    public OptionSetting LoadOptionData()
    {
        string filePath = GetPath(optionPathName) + "/option.json";
        if (!File.Exists(filePath))
        {
            //초기 파일 생성
            //Debug.LogError($"[Player Data] {filePath}를 찾을 수 없다.");
            return null;
        }

        string json = File.ReadAllText(filePath);
        OptionSaveData Data = JsonUtility.FromJson<OptionSaveData>(json);

        return Data.GetOptionSetting();
    }
    #endregion
}

[Serializable]
public class MapSaveData 
{
    public string sceneName;
    public List<_MapSaveData> senders       = new List<_MapSaveData>();

    public void SaveSender(List<int> states)
    {
        for (int i = 0; i < states.Count; i++)
            senders.Add(new _MapSaveData(i, states[i]));
    }

    public List<int> LoadSenders()
    {
        List<int> l = new List<int>();

        for (int i = 0; i < senders.Count; i++)
            l.Add(senders[i].state);

        return l;
    }

    [Serializable]
    public class _MapSaveData
    {
        public int index;
        public int state;

        public _MapSaveData(int index, int state)
        {
            this.index = index;
            this.state = state;
        }
    }
}

[Serializable]
public class FlagSaveData
{
    public List<string> Flags = new List<string>();
    public List<int> Values = new List<int>();
}

[Serializable]
public class PlayerSaveData
{
    public string lastScene;
    public Vector2 lastPos;
}

[Serializable]
public class OptionSaveData
{
    public int Window;
    public int Resolution;
    public float VOL;
    public float BGM;
    public float SFX;

    public OptionSaveData(OptionSetting option)
    {
        Window = option.window;
        Resolution = option.resolution;
        VOL = option.vol;
        BGM = option.bgm;
        SFX = option.sfx;
    }

    public OptionSetting GetOptionSetting()
    {
        return new OptionSetting()
        {
            window = this.Window,
            resolution = this.Resolution,
            vol = this.VOL,
            bgm = this.BGM,
            sfx = this.SFX
        };
    }
}