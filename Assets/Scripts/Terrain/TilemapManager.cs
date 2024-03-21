using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : SerializedMonoBehaviour
{
    public static TilemapManager Instance;

    public List<TileData> tileDatas;
    [SerializeField]
    private Dictionary<TileBase, TileData> dataFromTiles;

    [SerializeField]
    private TileData defaultTileData;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;

        // 딕셔너리가 컴파일타임에 값 할당이 안되서 우선 리스트에 데이터를 담아두고 런타임에 이를 옮겨야 함.
        dataFromTiles = new Dictionary<TileBase, TileData>();       
        foreach(var tileData in tileDatas)
        {
            foreach(var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
        }
    }

    public TileData GetTileData(TileBase tile)
    {
        TileData result;
        try
        {
            result = dataFromTiles[tile];
        }
        catch(KeyNotFoundException)
        {
            Debug.LogWarning($"타일 데이터가 설정되어있지 않음! 기본 설정을 사용합니다\n타일: {tile}");
            return defaultTileData;
        }
        return result;
    }
}
