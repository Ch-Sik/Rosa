using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System;
using UnityEngine.SceneManagement;

public class TerrainShadowGenerator : MonoBehaviour
{
    public RoomExtractor roomExtractor;
    public Texture2D sourceTexture; // 입력 타일맵 텍스처
    public ComputeShader minFilterShader;   // 축소 필터
    public int minFilterIterations = 10;    // 축소 반복 적용 횟수
    public ComputeShader blurShader;        // 블러 필터
    public int blurRadius = 10;             // 블러 반경
    public int blurIterations = 10;         // 블러 반복 적용 횟수
    public string folderPath = "Assets/TilemapSprite";
    public bool useCustomFilename = false;
    [ShowIf("useCustomFilename")]
    public string fileName;  // 출력 파일명

    [HideInInspector] public string totalPath;
    RenderTexture tempTexture;      // 중간 결과
    RenderTexture resultTexture;

    DateTime startTime;

#if UNITY_EDITOR
    [Button("타일맵 이미지를 가공하여 그림자 이미지 생성")]
    public void GenerateShadowImage()
    {
        startTime = DateTime.Now;

        Debug.Assert(minFilterShader != null);
        Debug.Assert(blurShader != null);

        if(sourceTexture == null)
        {
            sourceTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(roomExtractor.completePath, typeof(Texture2D));
            Debug.Log($"텍스쳐 자동으로 가져옴: {roomExtractor.completePath}");
        }

        if (sourceTexture != null && minFilterShader != null)
        {
            // 결과를 담을 텍스처 설정
            resultTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);
            resultTexture.enableRandomWrite = true;
            resultTexture.Create();
            // Debug.Log($"resultTexture Size: {resultTexture.width} x {resultTexture.height}");

            // 임시 텍스쳐에 소스 텍스쳐 복사
            tempTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(sourceTexture, tempTexture);

            // 최소값 필터를 반복적으로 적용
            for (int i = 0; i < minFilterIterations; i++)
            {
                ApplyMinimumFilter();
            }

            // 그 다음 블러 필터를 적용
            for (int i = 0; i < blurIterations; i++)
            {
                ApplyBlurFilter();
            }

            // 결과 텍스처를 저장
            if (!useCustomFilename)
            {
                fileName = SceneManager.GetActiveScene().name + "_shadow";
            }
            totalPath = folderPath + "/" + fileName + ".png";
            SaveRenderTextureAsPNG(resultTexture, totalPath);
            Debug.Log($"타일맵 그림자를 이미지로 저장: {totalPath}\n수행 시간: {(DateTime.Now - startTime).TotalMilliseconds} ms");
        }
    }

    void ApplyMinimumFilter()
    {
        int kernelHandle = minFilterShader.FindKernel("MinimumFilter");

        // 텍스처 크기와 관련된 변수를 컴퓨트 셰이더에 전달
        minFilterShader.SetInts("textureSize", new int[] { tempTexture.width, tempTexture.height });

        // 컴퓨트 셰이더에 입력 텍스처와 출력 텍스처 설정
        minFilterShader.SetTexture(kernelHandle, "SourceTexture", tempTexture);
        minFilterShader.SetTexture(kernelHandle, "ResultTexture", resultTexture);

        // 컴퓨트 셰이더 실행 (8x8 스레드 그룹)
        minFilterShader.Dispatch(kernelHandle, tempTexture.width / 8, tempTexture.height / 8, 1);

        // 중간 결과를 다음 반복에서 입력으로 사용하기 위해 텍스처 교체
        Graphics.Blit(resultTexture, tempTexture);
    }

    void ApplyBlurFilter()
    {
        int kernelHandle = blurShader.FindKernel("ApplyBlur");

        // 텍스처 크기와 관련된 변수를 컴퓨트 셰이더에 전달
        blurShader.SetInts("textureSize", new int[] { tempTexture.width, tempTexture.height });
        blurShader.SetInt("blurRadius", blurRadius);

        // 컴퓨트 셰이더에 입력 텍스처와 출력 텍스처 설정
        blurShader.SetTexture(kernelHandle, "SourceTexture", tempTexture);
        blurShader.SetTexture(kernelHandle, "ResultTexture", resultTexture);

        // 컴퓨트 셰이더 실행 (8x8 스레드 그룹)
        blurShader.Dispatch(kernelHandle, tempTexture.width / 8, tempTexture.height / 8, 1);

        // 중간 결과를 다음 반복에서 입력으로 사용하기 위해 텍스처 교체
        Graphics.Blit(resultTexture, tempTexture);
    }

    //void ApplyGaussianBlur()
    //{
    //    int kernelHandleHorizontal = gaussianBlurShader.FindKernel("GaussianBlurHorizontal");
    //    int kernelHandleVertical = gaussianBlurShader.FindKernel("GaussianBlurVertical");
    //    intermediateTexture = new RenderTexture(tempTexture);
    //    intermediateTexture.enableRandomWrite = true;
    //    intermediateTexture.Create();

    //    float[] weights = CalculateGaussianWeights(blurRadius);
    //    gaussianBlurShader.SetFloats("weights", weights);
    //    gaussianBlurShader.SetInt("blurRadius", blurRadius);
    //    gaussianBlurShader.SetInts("textureSize", new int[] { sourceTexture.width, sourceTexture.height });

    //    // Horizontal Blur
    //    gaussianBlurShader.SetTexture(kernelHandleHorizontal, "SourceTexture", tempTexture);
    //    gaussianBlurShader.SetTexture(kernelHandleHorizontal, "ResultTexture", intermediateTexture);
    //    gaussianBlurShader.Dispatch(kernelHandleHorizontal, sourceTexture.width / 8, sourceTexture.height / 8, 1);

    //    // Graphics.Blit(resultTexture, tempTexture);

    //    // Vertical Blur
    //    gaussianBlurShader.SetTexture(kernelHandleVertical, "SourceTexture", intermediateTexture);
    //    gaussianBlurShader.SetTexture(kernelHandleVertical, "ResultTexture", resultTexture);
    //    gaussianBlurShader.Dispatch(kernelHandleVertical, sourceTexture.width / 8, sourceTexture.height / 8, 1);
    //}

    //float[] CalculateGaussianWeights(int radius)
    //{
    //    float[] weights = new float[radius + 1];
    //    float sigma = radius / 2.0f;
    //    float sum = 0.0f;
    //    for (int i = 0; i <= radius; i++)
    //    {
    //        weights[i] = Mathf.Exp(-0.5f * (i * i) / (sigma * sigma));
    //        sum += weights[i] * (i == 0 ? 1 : 2);
    //    }
    //    for (int i = 0; i <= radius; i++)
    //    {
    //        weights[i] /= sum;
    //    }
    //    gaussianFilter = weights;
    //    return weights;
    //}

    void SaveRenderTextureAsPNG(RenderTexture rt, string path)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        byte[] bytes = tex.EncodeToPNG();

        // 저장 폴더 없으면 생성
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }
        System.IO.File.WriteAllBytes(path, bytes);
        // AssetDatabase.Refresh();

        // PPU 설정을 위해 Refresh로 자동 임포트 말고 수동 임포트 수행
        AssetDatabase.ImportAsset(path);
        AssetImporter tempImporter = AssetImporter.GetAtPath(path);
        TextureImporter importer = (TextureImporter)tempImporter;
        importer.isReadable = true;  // 필요에 따라 텍스처가 읽기 가능하도록 설정
        // Pixel Per Unit 설정
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = roomExtractor.pixelPerTile;  // 원하는 Pixel Per Unit 값으로 설정
        // 필터링 설정
        importer.filterMode = FilterMode.Point;
        // 최대 크기 설정
        importer.maxTextureSize = 16384;
        // 최종 임포트
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }
#endif
}
