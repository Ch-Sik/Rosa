using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class SaveLoadManager : MonoBehaviour
{
    #region Singleton
    private static SaveLoadManager instance;
    public static SaveLoadManager Instance { get => instance; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    [SerializeField] private SaveUI saveUI;
    public bool IsNewGame { get  { return _isNewGame; } }
    private bool _isNewGame = true;           // 새 게임인지 아닌지를 표시

    [FoldoutGroup("Paths"), ReadOnly] public string pathName = "SaveFile";
    [FoldoutGroup("Paths"), ReadOnly] public string flagPathName = "Flag";
    [FoldoutGroup("Paths"), ReadOnly] public string playerPathName = "Player";
    [FoldoutGroup("Paths"), ReadOnly] public string optionPathName = "Option";

    // 25.04.29) newtonsoft json으로 변경된 것으로 인해 발생한 self-loop문제 처리
    private JsonSerializerSettings serializeSetting;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        
        // 25.04.29) newtonsoft json으로 변경된 것으로 인해 발생한 self-loop문제 처리
        serializeSetting = new JsonSerializerSettings();
        serializeSetting.Formatting = Formatting.Indented;
        serializeSetting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        MakeDirectoryHierarchy();
        Debug.Log($"세이브 위치: {GetPath(flagPathName)}");
    }
    
    public void SetNewGameFlag(bool value)
    {
        _isNewGame = value;
    }

    [Button]
    public void SavePlayData()
    {
        SaveFlag();
        SavePlayerPosition();
        saveUI.ShowSaveUI();
        _isNewGame = false;     // 사망 후 최근 세이브로 돌아갈 시에 완전 처음으로 되돌아가는 것 방지 
    }

    [Button]
    public void TestSaveUI()
    {
        saveUI.ShowSaveUI();
    }
    
    #region Utils
    //Path 병합해서 전달
    private string GetPath(string path)
    {
        return $"{Application.persistentDataPath}/{pathName}/{path}";
    }

    private void MakeDirectoryHierarchy()
    {
        MakeDirectory($"{Application.persistentDataPath}/{pathName}");
        MakeDirectory(GetPath(flagPathName));
        MakeDirectory(GetPath(playerPathName));
        MakeDirectory(GetPath(optionPathName));
    }
    private void MakeDirectory(string path)
    {
        //폴더가 존재하지 않는 경우 생성
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
    #endregion

    #region Flag

    [Button]
    public void SaveFlag()
    {
        SaveFlag(FlagManager.Instance.flags);
    }

    public void SaveFlag(Dictionary<string, int> flags)
    {
        string filePath = GetPath(flagPathName) + "/flag.json";
        string json = JsonConvert.SerializeObject(flags, serializeSetting);
        File.WriteAllText(filePath, json);
        Debug.Log($"[Flag Data] {filePath}에 저장 완료."
            + "\nJSON 파일 내용:\n"
            + json);
    }

    public Dictionary<string, int> LoadFlag()
    {
        string filePath = GetPath(flagPathName) + "/flag.json";
        if (!File.Exists(filePath)) {
            Debug.LogWarning($"[Flag Data] {filePath}를 찾을 수 없다.");
            return null;
        }

        string json = File.ReadAllText(filePath);
        Dictionary<string, int> deserializedFlags = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        Debug.Log($"[Flag Data] {filePath}에서 불러오기 완료"
            + $"\n플래그 총 {deserializedFlags.Count}개");
        return deserializedFlags;
    }
    #endregion

    #region Player position
    [Button]
    public void SavePlayerPosition()
    {
        PlayerPositionSave data = new PlayerPositionSave();
        data.room = MapManager.Instance.CurrentRoom.scene.SceneName;
        data.position = PlayerRef.Instance.transform.position;

        string filePath = GetPath(playerPathName) + "/player.json";
        string json = JsonConvert.SerializeObject(data, serializeSetting);
        File.WriteAllText(filePath, json);

        Debug.Log($"[Player Position Data] {filePath}에 저장 완료."
            + "\nJSON 파일 내용:\n"
            + json);
    }

    [Button]
    public PlayerPositionSave LoadPlayerPosition()
    {
        string filePath = GetPath(playerPathName) + "/player.json";
        if (!File.Exists(filePath)) {
            //초기 파일 생성
            Debug.LogError($"플레이어 위치 세이브데이터를 다음 경로에서 찾을 수 없음: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        PlayerPositionSave data = JsonConvert.DeserializeObject<PlayerPositionSave>(json);
        Debug.Log($"플레이어 위치 정보 로드됨 : {data.room}, Pos : {data.position}");

        return data;
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
        string filePath = GetPath(optionPathName) + "/option.json";
        string json = JsonConvert.SerializeObject(option, serializeSetting);
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
        var data = JsonConvert.DeserializeObject<OptionSetting>(json);

        return data;
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
    public string key;
    public int value;

    public FlagSaveData(string k, int v) { key = k; value = v; }
}