using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class TilemapShadowAutomation : MonoBehaviour
{
    RoomExtractor roomExtractor;
    TerrainShadowGenerator terrainShadowGenerator;
    TilemapShadowPlacer tilemapShadowPlacer;

    [Button("원버튼 타일맵 그림자 생성")]
    void BakeAndPlaceTileShadow()
    {
        roomExtractor = GetComponent<RoomExtractor>();
        terrainShadowGenerator = GetComponent<TerrainShadowGenerator>();
        tilemapShadowPlacer = GetComponent<TilemapShadowPlacer>();

        roomExtractor.ConvertTilemapToSprite();
        terrainShadowGenerator.GenerateShadowImage();
        tilemapShadowPlacer.PlaceShadowSprite();

        bool destroySelf = EditorUtility.DisplayDialog(
            "작업 완료",
            "그림자 배치 완료. TilemapShadowGenerator를 Destroy할까요?",
            "예",   // Yes 버튼
            "아니오" // No 버튼
        );

        if (destroySelf)
            DestroyImmediate(gameObject);
    }
}
#endif