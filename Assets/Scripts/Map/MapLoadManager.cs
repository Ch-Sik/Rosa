using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵 데이터를 관리하는 매니저 클래스
/// </summary>

public class MapLoadManager : MonoBehaviour
{
    private static MapLoadManager instance;
    public static MapLoadManager Instance
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

    private void Start()
    {
        GetAllMapDatas();
    }

    [SerializeField] private SceneField mainScene;
    [ShowInInspector] private List<SceneData> sceneDatas = new List<SceneData>();
    [SerializeField] public List<SceneField> loadedScene = new List<SceneField>();
    [SerializeField] public List<SceneField> sceneOnMinimap = new List<SceneField>();
    [SerializeField] public List<SO_SceneMapData> mapDatas = new List<SO_SceneMapData>();
    [SerializeField] public Minimap minimap;

    //모든 맵 데이터의 스크립터블 오브젝트 참조리스트를 구성함.
    public void GetAllMapDatas()
    {
        SO_SceneMapData[] loadedMapDatas = Resources.LoadAll<SO_SceneMapData>("MapDatas");

        if (loadedMapDatas != null && loadedMapDatas.Length > 0)
            mapDatas = new List<SO_SceneMapData>(loadedMapDatas);
            
    }

    //열려있는 씬을 토대로 맵 데이터를 반환함.
    public List<SO_SceneMapData> GetLoadedSceneMapData()
    {
        List<SO_SceneMapData> loadedSceneMapData = new List<SO_SceneMapData>();
        for (int i = 0; i < sceneDatas.Count; i++)
            loadedSceneMapData.Add(sceneDatas[i].tileData);

        return loadedSceneMapData;
    }

    public SceneField GetMainScene() { return mainScene; }

    //씬이 있는지 파악하는 함수, 없다면 -1 있다면 인덱스 반환
    public int HaveScene(SceneField scene)
    {
        for (int i = 0; i < sceneDatas.Count; i++)
            if (sceneDatas[i].scene.isEqual(scene)) //같은 씬이 있다면,
                return i;

        return -1;
    }

    public void UploadSceneData(SceneData sceneData)
    {
        int index = HaveScene(sceneData.scene);
        if (index == -1)
        {
            //기존에 없던 새로운 씬의 로드
            sceneDatas.Add(sceneData);
        }
        else
        {
            //기존 씬의 로드
            sceneDatas[index] = sceneData;
        }
    }

    public SceneData DownloadSceneData(int i)
    {
        return sceneDatas[i];
    }
}