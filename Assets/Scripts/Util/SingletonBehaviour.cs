using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance)
        {
            if (this is MonoBehaviour mb)
            {
                Destroy(mb.gameObject);
            }
            return;
        }
        Instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}
