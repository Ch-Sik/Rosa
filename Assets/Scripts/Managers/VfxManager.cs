using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance;
    Dictionary<VfxPoolEntity, VfxPool> _vfxDict;

    private void Awake()
    {
        Instance = this;
        _vfxDict = new();
    }

    public VfxPoolEntity SpawnVfxObject(VfxPoolEntity prefab, Vector2 position)
    {
        if (prefab == null) return null;
        if (!_vfxDict.ContainsKey(prefab))
        {
            _vfxDict.Add(prefab, new VfxPool(prefab, prefab.MaxPoolSize, transform));
        }
        VfxPoolEntity vfxObject = _vfxDict[prefab].Get();
        vfxObject.transform.position = position;
        return vfxObject;
    }
}
