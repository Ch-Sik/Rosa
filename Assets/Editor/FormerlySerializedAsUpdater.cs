
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class FormerlySerializedAsUpdater : EditorWindow
{
    private MonoScript targetScript = null;

    [MenuItem("Tools/FormerlySerializedAs Updater")]
    public static void ShowWindow()
    {
        GetWindow<FormerlySerializedAsUpdater>("FormerlySerializedAs Updater");
    }

    private void OnGUI()
    {
        GUILayout.Label("Update Assets for FormerlySerializedAs", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool finds all prefabs, ScriptableObjects, and scenes that use the specified component " +
            "and re-saves them. This forces Unity to serialize the data with the new field names, " +
            "making it safe to remove the [FormerlySerializedAs] attribute afterwards.",
            MessageType.Info
        );

        targetScript = (MonoScript)EditorGUILayout.ObjectField(
            "Component Script",
            targetScript,
            typeof(MonoScript),
            false
        );

        if (GUILayout.Button("Update All Referencing Assets"))
        {
            if (targetScript == null)
            {
                EditorUtility.DisplayDialog("Error", "Please specify a component script.", "OK");
                return;
            }

            Type componentType = targetScript.GetClass();
            if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
            {
                EditorUtility.DisplayDialog("Error", "The specified script must be a Component.", "OK");
                return;
            }

            UpdateAssets(componentType);
        }
    }

    private void UpdateAssets(Type componentType)
    {
        try
        {
            // 1. Process Prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            int updatedPrefabs = 0;
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                EditorUtility.DisplayProgressBar("Processing Prefabs", $"Checking: {path}", (float)i / prefabGuids.Length);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && prefab.GetComponentInChildren(componentType, true) != null)
                {
                    Debug.Log($"Updating Prefab: {path}");
                    EditorUtility.SetDirty(prefab);
                    updatedPrefabs++;
                }
            }
            Debug.Log($"[FormerlySerializedAsUpdater] Found and updated {updatedPrefabs} prefabs.");

            // 2. Process ScriptableObjects
            string[] soGuids = AssetDatabase.FindAssets("t:ScriptableObject");
            int updatedSOs = 0;
            for (int i = 0; i < soGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(soGuids[i]);
                EditorUtility.DisplayProgressBar("Processing ScriptableObjects", $"Checking: {path}", (float)i / soGuids.Length);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                
                // In this context, we assume the SO itself is not the component, but it might be relevant
                // if the component type *was* a ScriptableObject. This part can be expanded if needed.
                // For now, we just re-save any SO to be safe, but a more targeted approach is possible.
                // A common case is an SO that holds a reference to the component, which is harder to track.
                // Re-saving all is a brute-force but effective method.
                // A simple check could be if the SO's type name matches, for SOs that were renamed.
                if (so != null && so.GetType().Assembly == componentType.Assembly) // A heuristic to only check project scripts
                {
                    // A more precise check would involve reflection to see if it holds a reference.
                    // For now, we mark it dirty to be safe.
                    EditorUtility.SetDirty(so);
                    updatedSOs++;
                }
            }
            Debug.Log($"[FormerlySerializedAsUpdater] Found and updated {updatedSOs} ScriptableObjects.");


            // Save all modified assets so far
            AssetDatabase.SaveAssets();

            // 3. Process Scenes
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[FormerlySerializedAsUpdater] Scene processing cancelled by user.");
                return;
            }
            
            string originalScene = EditorSceneManager.GetActiveScene().path;
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            int updatedScenes = 0;

            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                EditorUtility.DisplayProgressBar("Processing Scenes", $"Opening: {path}", (float)i / sceneGuids.Length);
                
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                bool sceneModified = false;

                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject root in rootObjects)
                {
                    if (root.GetComponentInChildren(componentType, true) != null)
                    {
                        sceneModified = true;
                        break; 
                    }
                }

                if (sceneModified)
                {
                    Debug.Log($"Updating Scene: {path}");
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    updatedScenes++;
                }
            }
            Debug.Log($"[FormerlySerializedAsUpdater] Found and updated {updatedScenes} scenes.");

            // Restore original scene
            if (!string.IsNullOrEmpty(originalScene))
            {
                EditorSceneManager.OpenScene(originalScene);
            }

            EditorUtility.ClearProgressBar();

            bool userAgrees = EditorUtility.DisplayDialog("Assets Updated Successfully",
                $"Update process complete.\n\n" +
                $"- Prefabs updated: {updatedPrefabs}\n" +
                $"- ScriptableObjects updated: {updatedSOs}\n" +
                $"- Scenes updated: {updatedScenes}\n\n" +
                "Do you want to automatically remove all [FormerlySerializedAs] attributes from the script now?",
                "Yes, Remove Them", "No, I'll do it manually"
            );

            if (userAgrees)
            {
                RemoveFormerlySerializedAsAttributes();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FormerlySerializedAsUpdater] An error occurred: {e.Message}\n{e.StackTrace}");
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Error", "An error occurred during the update process. Check the console for details.", "OK");
        }
    }

    private void RemoveFormerlySerializedAsAttributes()
    {
        string scriptPath = AssetDatabase.GetAssetPath(targetScript);
        if (string.IsNullOrEmpty(scriptPath) || !System.IO.File.Exists(scriptPath))
        {
            Debug.LogError("[FormerlySerializedAsUpdater] Could not find the script file to modify.");
            return;
        }

        try
        {
            string[] lines = System.IO.File.ReadAllLines(scriptPath);
            List<string> newLines = new List<string>();
            List<string> removalLogs = new List<string>();
            int removedCount = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmedLine = lines[i].Trim();
                if (trimmedLine.StartsWith("[FormerlySerializedAs"))
                {
                    removedCount++;
                    string fieldName = "<unknown_field>";
                    
                    // Search forward from the attribute to find the actual field declaration line
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        string nextLine = lines[j].Trim();

                        // Skip empty lines, comments, or other attributes
                        if (string.IsNullOrEmpty(nextLine) || nextLine.StartsWith("//") || nextLine.StartsWith("/*") || nextLine.StartsWith("["))
                        {
                            continue;
                        }

                        // This should be the field declaration. Parse it.
                        // This simple parser finds the last word before a semicolon or equals sign.
                        string[] parts = nextLine.Split(new[] { ' ', ';', '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            fieldName = parts[parts.Length - 1];
                        }
                        break; // Found the declaration, so exit the inner search loop
                    }

                    removalLogs.Add($"Line {i + 1}: [FormerlySerializedAs] on field '{fieldName}' was removed.");
                    continue; // Skip adding the attribute line itself
                }
                newLines.Add(lines[i]);
            }

            if (removedCount > 0)
            {
                System.IO.File.WriteAllLines(scriptPath, newLines);
                AssetDatabase.Refresh(); // Reload the script in Unity

                Debug.Log($"[FormerlySerializedAsUpdater] Successfully removed {removedCount} [FormerlySerializedAs] attribute(s) from {targetScript.name}. Details below:");
                foreach (string log in removalLogs)
                {
                    Debug.Log(log);
                }

                EditorUtility.DisplayDialog("Attributes Removed",
                    $"Successfully removed {removedCount} [FormerlySerializedAs] attribute(s) from '{targetScript.name}'.\n\nSee the console for detailed line-by-line changes.",
                    "OK");
            }
            else
            {
                Debug.LogWarning($"[FormerlySerializedAsUpdater] No [FormerlySerializedAs] attributes were found in {targetScript.name}.");
                EditorUtility.DisplayDialog("No Attributes Found",
                    $"No [FormerlySerializedAs] attributes were found in the script '{targetScript.name}'. No changes were made.",
                    "OK");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FormerlySerializedAsUpdater] An error occurred while modifying the script: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Error", "An error occurred while modifying the script. Check the console for details.", "OK");
        }
    }
}
