using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// c# 스크립트를 우클릭하여 해당 스크립트를 참조하는 프리팹을 검색하는 에디터 스크립트.
/// 이하의 내용은 모두 Gemini AI로 작성됨
/// </summary>
public class FindPrefabReferences : EditorWindow
{
    private MonoScript targetScript;
    private Dictionary<System.Type, List<GameObject>> foundResults = new Dictionary<System.Type, List<GameObject>>();
    private Vector2 scrollPosition;
    private bool searchQueued = false;

    // --- Context Menu Functionality ---

    private const string menuItemPath = "Assets/Find Prefab References";

    [MenuItem(menuItemPath, false, 30)]
    private static void FindReferencesFromContext()
    {
        MonoScript selectedScript = Selection.activeObject as MonoScript;
        if (selectedScript == null) return;

        FindPrefabReferences window = (FindPrefabReferences)GetWindow(typeof(FindPrefabReferences), false, "Prefab References");
        window.targetScript = selectedScript;
        window.searchQueued = true;
        window.Show();
    }

    [MenuItem(menuItemPath, true)]
    private static bool ValidateFindReferencesFromContext()
    {
        return Selection.activeObject is MonoScript;
    }

    // --- Editor Window Functionality ---

    [MenuItem("Tools/Find Prefab References")]
    public static void ShowWindow()
    {
        GetWindow<FindPrefabReferences>("Prefab References");
    }

    private void Update()
    {
        if (searchQueued)
        {
            if (targetScript != null)
            {
                foundResults = FindPrefabsWithInheritedScripts(targetScript);
                ShowNotification(new GUIContent($"Found references in {foundResults.Count} component type(s)."));
            }
            else
            {
                foundResults.Clear();
            }
            searchQueued = false;
            Repaint();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Prefabs Using Script (and its children)", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        targetScript = (MonoScript)EditorGUILayout.ObjectField("Target Script", targetScript, typeof(MonoScript), false);
        if (EditorGUI.EndChangeCheck())
        {
            foundResults.Clear();
        }

        if (GUILayout.Button("Find References"))
        {
            if (targetScript != null)
            {
                foundResults = FindPrefabsWithInheritedScripts(targetScript);
                ShowNotification(new GUIContent($"Found references in {foundResults.Count} component type(s)."));
            }
            else
            {
                ShowNotification(new GUIContent("Please assign a script first."));
            }
        }

        EditorGUILayout.Space();

        GUILayout.Label("Found Prefabs:", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (foundResults.Count == 0)
        { 
            GUILayout.Label("No references found or search not performed yet.");
        }
        else
        {
            foreach (var entry in foundResults)
            {
                // Display the component type as a header
                EditorGUILayout.LabelField(entry.Key.FullName, EditorStyles.boldLabel);

                foreach (var prefab in entry.Value)
                {
                    if (GUILayout.Button("  " + AssetDatabase.GetAssetPath(prefab), EditorStyles.objectField))
                    {
                        EditorGUIUtility.PingObject(prefab);
                        Selection.activeObject = prefab;
                    }
                }
                EditorGUILayout.Space(); // Add space between component types
            }
        }

        EditorGUILayout.EndScrollView();
    }

    // --- Core Logic ---

    private static Dictionary<System.Type, List<GameObject>> FindPrefabsWithInheritedScripts(MonoScript baseScript)
    {
        var results = new Dictionary<System.Type, List<GameObject>>();
        if (baseScript == null) return results;

        System.Type baseType = baseScript.GetClass();
        if (baseType == null)
        {
            Debug.LogWarning("Target script is not a valid class.");
            return results;
        }

        // Find all scripts in the project that inherit from the base type
        List<System.Type> typesToSearch = new List<System.Type>();
        typesToSearch.Add(baseType);

        string[] allScriptGuids = AssetDatabase.FindAssets("t:MonoScript");
        foreach (string scriptGuid in allScriptGuids)
        {
            string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (script == null) continue;

            System.Type scriptType = script.GetClass();
            if (scriptType != null && scriptType.IsSubclassOf(baseType) && !scriptType.IsAbstract)
            {
                typesToSearch.Add(scriptType);
            }
        }

        // Find all prefabs in the project
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string prefabGuid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null) continue;

            // Check for each component type (base and children)
            foreach (System.Type type in typesToSearch)
            {
                Component[] components = prefab.GetComponentsInChildren(type, true);
                if (components.Length > 0)
                {
                    if (!results.ContainsKey(type))
                    {
                        results[type] = new List<GameObject>();
                    }
                    if (!results[type].Contains(prefab))
                    {
                        results[type].Add(prefab);
                    }
                }
            }
        }

        return results;
    }
}
