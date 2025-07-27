using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class RoomExtractor : MonoBehaviour
{
    public Tilemap tilemap;
    public TilemapManager tilemapManager;
    public bool refreshTileDataInTilemapManager;

    public int pixelPerTile = 16;                                      //타일의 픽셀 크기 (16x16)
    //public float pixelsPerUnit = 16f;                                   //스프라이트의 Pixel Per Unit 값
    //public FilterMode filterMode = FilterMode.Point;                    //텍스처의 필터 모드
    public string folderPath = "Assets/TilemapSprite";
    public bool useCustomFilename = false;
    [ShowIf("useCustomFilename")]
    public string fileName;

    [HideInInspector] public string completePath;
    public BoundsInt bounds;
    DateTime startTime;

#if UNITY_EDITOR
    [Button("타일맵을 이미지로 저장")]
    public void ConvertTilemapToSprite()
    {
        startTime = DateTime.Now;

        Texture2D texture = GenerateTexture2DFromTilemap();
        Sprite sprite = ConvertTextureToSprite(texture);
        
        if(!useCustomFilename)
        {
            fileName = SceneManager.GetActiveScene().name + "_image";
        }
        // 저장 폴더 없으면 생성
        if(!AssetDatabase.IsValidFolder(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }
        completePath = $"{folderPath}/{fileName}.png";
        SaveSprite(texture, completePath);

        Debug.Log($"타일맵을 이미지로 저장: {completePath}\n수행 시간: {(DateTime.Now - startTime).TotalMilliseconds} ms");
    }

    public Texture2D GenerateTexture2DFromTilemap()
    {
        // 작업 수행 전 타일맵 바운드 축소
        tilemap.CompressBounds();

        // TilemapManager의 TileData dictionary 갱신
        if (refreshTileDataInTilemapManager)
            tilemapManager.InitTileData();

        // 타일맵 바운드 가져오기
        bounds = tilemap.cellBounds;
        int width = bounds.size.x * pixelPerTile;
        int height = bounds.size.y * pixelPerTile;

        // 타일맵을 텍스쳐화
        Texture2D texture = new Texture2D(width, height);
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                bool isSolidTile = false;
                TileBase tile = tilemap.GetTile(cellPosition);
                TileData tileData;
                if (tile != null)
                {
                    tileData = tilemapManager.GetTileData(tile);
                    isSolidTile = tileData.isSubstance;
                }

                if (isSolidTile)
                {
                    //Sprite sprite = tilemap.GetSprite(cellPosition);
                    //Texture2D tileTexture = sprite.texture;
                    //Rect spriteRect = sprite.textureRect;

                    for (int ty = 0; ty < pixelPerTile; ty++)
                    {
                        for (int tx = 0; tx < pixelPerTile; tx++)
                        {
                            int pixelX = (x - bounds.xMin) * pixelPerTile + tx;
                            int pixelY = (y - bounds.yMin) * pixelPerTile + ty;

                            Color tilePixel = Color.white;
                            texture.SetPixel(pixelX, pixelY, tilePixel);
                        }
                    }
                    
                }
                else
                {
                    for (int ty = 0; ty < pixelPerTile; ty++)
                    {
                        for (int tx = 0; tx < pixelPerTile; tx++)
                        {
                            int pixelX = (x - bounds.xMin) * pixelPerTile + tx;
                            int pixelY = (y - bounds.yMin) * pixelPerTile + ty;
                            texture.SetPixel(pixelX, pixelY, Color.clear);
                        }
                    }
                }
            }
        }

        texture.Apply();
        return texture;
    }

    public Sprite ConvertTextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelPerTile);
    }

    public void SaveSprite(Texture2D texture, string path)
    {
        if (texture == null)
        {
            Debug.LogError("텍스처 없음.");
            return;
        }

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("경로 없음.");
            return;
        }

        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        // AssetDatabase.Refresh();

        // Max Texture Size 설정을 위해 Refresh로 자동 임포트 말고 수동 임포트 수행
        AssetDatabase.ImportAsset(path);
        AssetImporter tempImporter = AssetImporter.GetAtPath(path);
        TextureImporter importer = (TextureImporter)tempImporter;
        importer.isReadable = true;  // 필요에 따라 텍스처가 읽기 가능하도록 설정
        // Pixel Per Unit 설정
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = pixelPerTile;  // 원하는 Pixel Per Unit 값으로 설정
        // 필터링 설정
        importer.filterMode = FilterMode.Point;
        // 최대 크기 설정
        importer.maxTextureSize = 16384;
        // 최종 임포트
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        Debug.Log($"Saved texture to {path}");

        //Texture2D loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        //if (loadedTexture == null)
        //{
        //    Debug.LogError("로드된 텍스처 없음.");
        //    return;
        //}
        ////loadedTexture.filterMode = filterMode;


        //Sprite sprite = Sprite.Create(loadedTexture, new Rect(0, 0, loadedTexture.width, loadedTexture.height), new Vector2(0.5f, 0.5f));   //, pixelsPerUnit

        //string spritePath = path.Replace(".png", ".asset");
        //AssetDatabase.CreateAsset(sprite, spritePath);
        //AssetDatabase.SaveAssets();
    }
#endif
}
