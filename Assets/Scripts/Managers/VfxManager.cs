using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance;
    [SerializeField] SO_VfxList vfxList;

    Dictionary<VfxPoolEntity, VfxPool> _vfxDict;

    private void Awake()
    {
        Instance = this;
        _vfxDict = new();
        foreach(var e in vfxList.data)
        {
            _vfxDict.Add( e.prefab,
                new VfxPool(e.prefab, e.maxPoolCount, transform) );
        }
    }

    public VfxPoolEntity SpawnVfxObject(VfxPoolEntity prefab, Vector2 position)
    {
        VfxPoolEntity vfxObject = _vfxDict[prefab].Get();
        vfxObject.transform.position = position;
        return vfxObject;
    }
}
