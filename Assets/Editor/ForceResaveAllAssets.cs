using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using UnityEngine.SceneManagement;

public class ForceResaveAllAssets : EditorWindow
{
    [MenuItem("Tools/Force Resave All Assets")]
    public static void ShowWindow()
    {
        GetWindow<ForceResaveAllAssets>("Force Resave All Assets");
    }

    private void OnGUI()
    {
        GUILayout.Label("Force Re-save All Assets", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool finds ALL prefabs, ScriptableObjects, and scenes in the project and forces them to be re-saved. " +
            "This is a brute-force method to ensure that data serialized with old field names (using [FormerlySerializedAs]) is updated to new field names.\n\n" +
            "WARNING: This will modify a large number of files, resulting in a large commit. Use this when you need to update many assets at once and are prepared for the version control impact.",
            MessageType.Warning
        );

        if (GUILayout.Button("Force Re-save All Assets"))
        {
            if (EditorUtility.DisplayDialog("Confirm Action", 
                "Are you sure you want to re-save all prefabs, ScriptableObjects, and scenes?\n\nThis action cannot be undone and will result in a large number of file changes.", 
                "Yes, proceed", "Cancel"))
            {
                ProcessAllAssets();
            }
        }
    }

    private void ProcessAllAssets()
    {
        try
        {
            // 1. Process Prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            EditorUtility.DisplayProgressBar("Processing Prefabs", "Preparing...", 0f);
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                EditorUtility.DisplayProgressBar("Processing Prefabs", $"Saving: {path}", (float)i / prefabGuids.Length);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    EditorUtility.SetDirty(prefab);
                }
            }
            Debug.Log($"[ForceResave] Marked {prefabGuids.Length} prefabs as dirty.");

            // 2. Process ScriptableObjects
            string[] soGuids = AssetDatabase.FindAssets("t:ScriptableObject");
            EditorUtility.DisplayProgressBar("Processing ScriptableObjects", "Preparing...", 0f);
            for (int i = 0; i < soGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(soGuids[i]);
                EditorUtility.DisplayProgressBar("Processing ScriptableObjects", $"Saving: {path}", (float)i / soGuids.Length);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so != null)
                {
                    EditorUtility.SetDirty(so);
                }
            }
            Debug.Log($"[ForceResave] Marked {soGuids.Length} ScriptableObjects as dirty.");

            // Save all modified non-scene assets
            EditorUtility.DisplayProgressBar("Saving Assets", "Writing changes to disk...", 0.5f);
            AssetDatabase.SaveAssets();
            Debug.Log("[ForceResave] Saved all modified prefabs and ScriptableObjects.");

            // 3. Process Scenes
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[ForceResave] Scene processing cancelled by user.");
                EditorUtility.ClearProgressBar();
                return;
            }
            
            string originalScene = EditorSceneManager.GetActiveScene().path;
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");

            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                EditorUtility.DisplayProgressBar("Processing Scenes", $"Opening: {path}", (float)i / sceneGuids.Length);
                
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            Debug.Log($"[ForceResave] Saved {sceneGuids.Length} scenes.");

            // Restore original scene
            if (!string.IsNullOrEmpty(originalScene))
            {
                EditorSceneManager.OpenScene(originalScene);
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Success",
                $"Force re-save process complete.\n\n" +
                $"- Prefabs processed: {prefabGuids.Length}\n" +
                $"- ScriptableObjects processed: {soGuids.Length}\n" +
                $"- Scenes processed: {sceneGuids.Length}\n\n" +
                "All assets have been re-serialized. It should now be safe to remove [FormerlySerializedAs] attributes.",
                "OK"
            );
        }
        catch (Exception e)
        {
            Debug.LogError($"[ForceResave] An error occurred: {e.Message}\n{e.StackTrace}");
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Error", "An error occurred during the process. Check the console for details.", "OK");
        }
    }
}
