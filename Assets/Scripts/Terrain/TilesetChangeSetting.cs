using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "", fileName = "")]
public class TilesetChangeSetting : SerializedScriptableObject
{
    public Dictionary<TileBase, TileBase> tilesToSwap = new();
}
