using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TilemapShadowPlacer : MonoBehaviour
{
#if UNITY_EDITOR
    public RoomExtractor roomExtractor;
    public TerrainShadowGenerator shadowGenerator;
    public Color shadowColor;
    public string sortingLayerName;
    public int orderInLayer;

    [ReadOnly] public string filePath;

    [Button("생성된 이미지를 씬에 배치")]
    public void PlaceShadowSprite()
    {
        // 스프라이트 가져오기 및 설정 조정
        filePath = shadowGenerator.totalPath;
        // Texture2D shadowImage = (Texture2D)AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D));
        Sprite shadowSprite = (Sprite)AssetDatabase.LoadAssetAtPath(filePath, typeof(Sprite));

        //Texture2D tex = shadowSprite.texture;
        //Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), roomExtractor.pixelPerTile);
        //string newPath = filePath.Replace(".png", ".asset");
        //AssetDatabase.CreateAsset(newSprite, newPath); // 덮어쓰기

        // 새 오브젝트를 생성하고 위치 조정
        GameObject go = new GameObject();
        go.name = "tileShadow";
        Vector3 shadowPosition = roomExtractor.bounds.center;
        shadowPosition.z = 0;
        go.transform.position = shadowPosition;

        // 렌더러 부착하고 스프라이트 설정
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = shadowSprite;

        // 기타 옵션 설정
        renderer.color = shadowColor;
        renderer.sortingLayerID = SortingLayer.NameToID(sortingLayerName);
        renderer.sortingOrder = orderInLayer;

        Debug.Log($"타일맵 그림자를 씬에다가 배치: {go}");
    }
#endif
}
