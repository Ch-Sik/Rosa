using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct vfxListElement
{
    public VfxPoolEntity prefab;
    public int maxPoolCount;
}

[CreateAssetMenu(fileName = "vfxList", menuName = "VFX List")]
public class SO_VfxList : ScriptableObject
{
    public List<vfxListElement> data;
}
