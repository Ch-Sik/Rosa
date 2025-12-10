using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiSingleton : MonoBehaviour
{
    private static readonly Dictionary<string, UiSingleton> Instance = new();

    [SerializeField] private string key;

    private void Awake()
    {
        if (Instance.ContainsKey(key) && Instance[key])
        {
            Destroy(this.gameObject);
            return;
        }

        Instance[key] = this;
        DontDestroyOnLoad(gameObject);
    }
}
