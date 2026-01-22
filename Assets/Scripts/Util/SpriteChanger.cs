using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpriteChanger : EditorWindow
{
    public SpriteChangeOption settings;

    [MenuItem("Tools/Process All Scenes")]
    public static void ShowWindow()
    {
        GetWindow<SpriteChanger>("Scene Processor");
    }

    private void OnGUI()
    {
        GUILayout.Label("스프라이트 교체 설정", EditorStyles.boldLabel);
        
        // Scriptable Object 할당 필드
        settings = (SpriteChangeOption)EditorGUILayout.ObjectField("설정 파일", settings, typeof(SpriteChangeOption), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("모든 씬에서 스프라이트 교체 시작"))
        {
            if (settings == null || settings.TargetSprite == null 
                                 || settings.ResultSprites == null || settings.ResultSprites.Length == 0)
            {
                EditorUtility.DisplayDialog("오류", "설정 파일의 필드를 모두 지정해주세요.", "확인");
                return;
            }

            if (EditorUtility.DisplayDialog("경고", "모든 씬을 열어 작업을 수행합니다. 계속하시겠습니까?", "예", "아니오"))
            {
                ExecuteReplaceProcess();
            }
        }
    }

    private void ExecuteReplaceProcess()
    {
        // 프로젝트 내 모든 씬 경로 가져오기
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

        foreach (var guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            
            // 씬 열기
            Scene currentScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Debug.Log($"[Process] Processing Scene: {currentScene.name}");

            // 치환 작업 수행
            bool isChanged = PerformReplace();

            if (isChanged)
            {
                EditorSceneManager.MarkSceneDirty(currentScene);
                EditorSceneManager.SaveScene(currentScene);
            }
        }

        Debug.Log("모든 씬의 작업이 완료되었습니다.");
        EditorUtility.DisplayDialog("완료", "모든 작업이 끝났습니다.", "확인");
    }

    private bool PerformReplace()
    {
        // 씬 내의 모든 오브젝트 중 TargetSprite와 이름이나 프리팹 소스가 같은 것 찾기
        // (가장 단순하고 확실한 이름 비교 방식을 예시로 사용합니다)
        var spriteRenderers = GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);

        bool result = false;
        foreach (var sr in spriteRenderers)
        {
            if (sr.sprite != settings.TargetSprite)
                continue;

            int count = settings.ResultSprites.Length;
            sr.sprite = settings.ResultSprites[Random.Range(0, count)];
            result = true;
        }

        return result;
    }
}   