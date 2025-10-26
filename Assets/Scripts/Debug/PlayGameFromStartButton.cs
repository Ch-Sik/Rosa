#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;


[InitializeOnLoad]
class PlayGameFromStartButton : EditorWindow
{
    private const string EditingSceneName = "EditingSceneName";
    private const string StartingScenePath = "Assets/Scenes/Title.unity";
    
    private static string _lastScenePath
    {
        get => PlayerPrefs.GetString(EditingSceneName, string.Empty);
        set => PlayerPrefs.SetString(EditingSceneName, value);
    }
    
    static PlayGameFromStartButton()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        EditorApplication.playModeStateChanged += (PlayModeStateChange change) =>
        {
            if (change == PlayModeStateChange.EnteredEditMode)
                ReturnToLastScene();
        };
    }

    static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace();

        if(GUILayout.Button(new GUIContent("Start Game", "Start Game"), ToolbarStyles.commandButtonStyle))
        {
            RunMainScene();
        }
    }
    
    [MenuItem("Play/Execute starting scene _%h")]
    public static void RunMainScene()
    {
        _lastScenePath = SceneManager.GetActiveScene().path;
        EditorApplication.OpenScene(StartingScenePath);
        EditorApplication.isPlaying = true;
    }
	
    [MenuItem("Play/Reload editing scnee _%g")]
    public static async void ReturnToLastScene()
    {
        EditorApplication.OpenScene(_lastScenePath);
        _lastScenePath = null;
    }
}

static class ToolbarStyles
{
    public static readonly GUIStyle commandButtonStyle;

    static ToolbarStyles()
    {
        commandButtonStyle = new GUIStyle("Command")
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter,
            imagePosition = ImagePosition.ImageAbove,
            fontStyle = FontStyle.Normal,
            fixedWidth = 100,
            stretchWidth = true
        };
    }
}
#endif