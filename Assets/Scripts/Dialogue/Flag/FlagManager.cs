using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    #region Singleton
    private static FlagManager instance;
    public static FlagManager Instance
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
    public Dictionary<string, int> flags { get { return _flags; } }
    [ShowInInspector] private Dictionary<string, int> _flags = new Dictionary<string, int>();

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (SaveLoadManager.Instance.IsNewGame)
        {
            _flags = new Dictionary<string, int>();
            Debug.Log("플래그 저장소 새로 생성");
        }
        else
        {
            _flags = SaveLoadManager.Instance.LoadFlag();
            if (_flags != null)
            {
                Debug.Log("플래그 저장소 로드");
            }
            else
            {
                Debug.LogError("플래그 저장소 불러오기 실패");
                _flags = new Dictionary<string, int>();
            }
        }
    }
    
    [Button]
    public void SetFlag(string flag, int value)
    {
        if (!_flags.ContainsKey(flag))
        {
            _flags.Add(flag, value);
            Debug.Log($"새 플래그 항목 추가: {flag}, 값: {value}");
        }
        else
        {
            _flags[flag] = value;
            Debug.Log($"플래그 값 업데이트: {flag}, 값: {value}");
        }
    }

    public int GetFlag(string key)
    {
        if (!_flags.ContainsKey(key))
        {
            Debug.LogWarning($"Flag [{key}]이 존재하지 않습니다. 기본값 0으로 새로 생성");
            _flags.Add(key, 0);
            return 0;
        }

        return _flags[key];
    }
}
