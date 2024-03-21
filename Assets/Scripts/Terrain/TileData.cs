using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileCategory { 
    DECORATION,         // 장식용. 충돌 판정 없음. 식물 설치 불가능.
    NORMAL,             // 충돌 판정 있음. 식물 설치 가능.
    NOT_PLANTABLE       // 충돌 판정 있음. 식물 설치 불가능.
};

[CreateAssetMenu]
public class TileData : SerializedScriptableObject
{
    public TileBase[] tiles;    // 이 데이터를 적용할 타일들
    public TileCategory category;

    // 아래 둘은 기존 코드 호환용
    public bool isPlantable { get { return category == TileCategory.NORMAL; } }
    public bool isSubstance { get { return category > TileCategory.DECORATION; } }

    // 서로 다른 TileData를 가진 타일들끼리 겹쳐져있을 때 TileCategory가 더 높은 것을 우선시함.
    public static TileData Merge(TileData[] datas)
    {
        if (datas.Length == 0) return null;
        TileData mergedData = CreateInstance<TileData>();
        mergedData.category = datas.Max(x => x.category);
        return mergedData;
    }
}
