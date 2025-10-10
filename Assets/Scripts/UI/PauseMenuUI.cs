using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 일시정지 메뉴
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] GameObject uiObject;

    public static PauseMenuUI Instance;
    [SerializeField, ReadOnly] bool _isPaused = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InputManager.Instance.AM_UiInGame.FindAction("Pause").performed += OnPerformedPauseButton;

        ClosePauseMenu();
    }

    private void OnPerformedPauseButton(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(_isPaused)
        {
            ClosePauseMenu();
        }
        else
        {
            // 25.10.10) 방 이동 등 Fadeout 도중에 일시정지 불가능하게 수정
            if (FadeoutPanel.isFadeOutActivated)
            {
                Debug.Log("일시정지 메뉴를 열 수 없는 상태임");
                return;
            }
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        uiObject.SetActive(true);
        // TODO: 일시정지 메뉴 열기 전 상태(걷기/기어오르기 등)을 저장해뒀다가 일시정지 해제되면 복구
        InputManager.Instance.SetUiInputState(UiState.MENU);
    }

    public void ClosePauseMenu()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        uiObject.SetActive(false);
        InputManager.Instance.SetUiInputState(UiState.IN_GAME);
    }

    public void ToTitleScene()
    {
        StartCoroutine(Co_ToTitleScene());
    }

    private IEnumerator Co_ToTitleScene()
    {
        Time.timeScale = 1f;

        FadeoutPanel.Fadeout();
        yield return new WaitForSeconds(FadeoutPanel.fadeDuration + 0.1f);

        var op = SceneManager.LoadSceneAsync("Title");
        while (op.isDone) yield return null;
        op = SceneManager.UnloadSceneAsync(MapManager.Instance.CurrentRoom.scene);
        while (op.isDone) yield return null;

        // MainScene 언로드되기 전에 FadeIn 되어 보이는 것 방지
        DOVirtual.DelayedCall(0.5f, () => { FadeoutPanel.FadeIn(); });
        // PauseMenuUI가 MainScene에 속해서 가장 마지막에 언로드
        SceneManager.UnloadSceneAsync("MainScene");
    }

    public static void RestartGame()
    {
        string[] endings = new string[]{
        "exe", "x86", "x86_64", "app"
    };

        string executablePath = Application.dataPath + "/..";
        foreach (string file in System.IO.Directory.GetFiles(executablePath))
        {

            foreach (string ending in endings)
            {
                if (file.ToLower().EndsWith("." + ending))
                {
                    System.Diagnostics.Process.Start(executablePath + file);
                    Application.Quit();
                    return;
                }
            }

        }
    }
}
