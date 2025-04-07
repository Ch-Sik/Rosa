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

    private void Init()
    {
        _flags = SaveLoadManager.Instance.LoadFlag();
        if(_flags == null)
        {
            Debug.LogWarning("기존 저장된 플래그 저장소가 없으므로 새로 생성");
            _flags = new Dictionary<string, int>();
        }
    }

    #region Utiles
    [Button]
    public void SetFlag(string flag, int value)
    {
        if (!_flags.ContainsKey(flag))
        {
            _flags.Add(flag, value);
            Debug.Log($"새 플래그 항목 추가: {flag}");
        }
        else
        {
            _flags[flag] = value;
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
    #endregion
}
