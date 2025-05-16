using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 일시정지 메뉴
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] GameObject uiObject;
    bool isPaused = false;

    private void Start()
    {
        InputManager.Instance.AM_UiInGame.FindAction("Pause").performed += OnPerformedPauseButton;

        ClosePauseMenu();
    }

    private void OnPerformedPauseButton(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(isPaused)
        {
            ClosePauseMenu();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        isPaused = true;
        Time.timeScale = 0f;
        uiObject.SetActive(true);
        // TODO: 일시정지 메뉴 열기 전 상태(걷기/기어오르기 등)을 저장해뒀다가 일시정지 해제되면 복구
        InputManager.Instance.SetUiInputState(UiState.MENU);
    }

    public void ClosePauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        uiObject.SetActive(false);
        InputManager.Instance.SetUiInputState(UiState.IN_GAME);
    }

    public void ToTitleScene()
    {
        //ClosePauseMenu();
        // RestartGame();
        Process.Start(Application.dataPath + "/../Rosa.exe");
        Application.Quit();
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
