using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

// 동일한 Grid를 공유하는 타일맵들을 묶어주는 역할.
// 숨겨진 지형과 일반 지형을 같이 처리하기 위해 사용.
// 움직이는 플랫폼과 안 움직이는 지형은 Grid를 공유하지 않으므로 같은 그룹이 아님.
[RequireComponent(typeof(Grid))]
public class TilemapGroup : MonoBehaviour
{
    TilemapManager tilemapManager;
    Grid grid;
    List<Tilemap> tilemaps;

    // Start is called before the first frame update
    void Start()
    {
        tilemapManager = TilemapManager.Instance;
        if(grid == null)
            grid = GetComponent<Grid>();
        RefreshTilemapList();
    }

    public void RefreshTilemapList()
    {
        tilemaps = gameObject.GetComponentsInChildren<Tilemap>().ToList();
    }

    public void RemoveTilemapFromList(Tilemap element)
    {
        tilemaps.Remove(element);
    }

    // tilePosition은 world 좌표계가 아닌 grid 좌표계임
    public TileBase[] GetTiles(Vector3Int tilePosition)
    {
        List<TileBase> tileList = new List<TileBase>();
        foreach(var tilemap in tilemaps)
        {
            TileBase tile = tilemap.GetTile(tilePosition);
            if (tile!= null)
            {
                tileList.Add(tile);
            }
        }
        return tileList.ToArray();
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return grid.WorldToCell(worldPosition);
    }

    public TileData GetTileData(Vector3Int cellPosition)
    {
        TileBase[] tiles = GetTiles(cellPosition);
        var datas = (from t in tiles select tilemapManager.GetTileData(t)).ToArray();
        if (datas.Length == 1)
            return datas[0];
        else
            return TileData.Merge(datas.ToArray());
    }
}
