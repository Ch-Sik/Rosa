using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilesetChanger : MonoBehaviour
{
    public List<Tilemap> targetTilemaps;
    [SerializeField] TilesetChangeSetting changeSetting;
    public bool useDetailedLogging = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    [Button]
    void Execute()
    {
        ChangeTileSet();
    }

    void ChangeTileSet()
    {
        foreach (var tilemap in targetTilemaps)
        {
            int changeCount = 0;

            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;

            Vector3Int pos = new Vector3Int();
            for (int row = bounds.min.y; row <= bounds.max.y; row++)
            {
                for (int col = bounds.min.x; col <= bounds.max.x; col++)
                {
                    pos.x = col;
                    pos.y = row;
                    TileBase targetTile = tilemap.GetTile(pos);
                    if(targetTile == null)
                    {
                        if(useDetailedLogging)
                            Debug.Log(pos + "의 위치에 타일이 없음. 해당 위치에 대한 동작은 생략");
                        continue;
                    }

                    if (changeSetting.tilesToSwap.ContainsKey(targetTile))
                    {
                        if(useDetailedLogging)
                            Debug.Log(pos + "의 타일 교체\n" + targetTile + "->" + changeSetting.tilesToSwap[targetTile]);
                        tilemap.SetTile(pos, changeSetting.tilesToSwap[targetTile]);
                        changeCount++;
                    }
                    else
                    {
                        if (useDetailedLogging)
                            Debug.LogWarning(pos + "의 위치에 있는 타일이 tilesToSwap에 지정되지 않음");
                    }
                }
            }
            Debug.Log(tilemap + "에서 " + changeCount + "개의 타일 교체");
        }
    }
}
