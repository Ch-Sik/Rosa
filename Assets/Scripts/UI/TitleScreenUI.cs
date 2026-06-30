using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면 UI 담당
/// </summary>
public class TitleScreenUI : MonoBehaviour
{
    [SerializeField] string LoadingSceneName;
    [SerializeField] Button button_newGame, button_contiue;

    // Start is called before the first frame update
    void Start()
    {
        // TODO: SaveLoadManager를 통해 기존에 저장된 파일이 있는지 확인하고,
        //      없다면 이어하기 버튼 비활성화하기
        Debug.Log("[TitleScreenUI] 세이브 테이터 있는지 체크");
        if(SaveLoadManager.Instance.HasSaveData())
        {
            button_contiue.interactable = true;
        }
        else
        {
            button_contiue.interactable = false;
        }
    }

    public void OnClickNewGameButton()
    {
        Debug.Log("새 게임 시작");
        SaveLoadManager.Instance.SetNewGameFlag(true);
        StartEnterSequence();

        // EnterSequence 두번 시작되어 오류 발생하는 것 방지
        DeactivateButtons();
    }

    public void OnClickContinueButton()
    {
        Debug.Log("이어하기 시작");
        SaveLoadManager.Instance.SetNewGameFlag(false);
        StartEnterSequence();

        DeactivateButtons();
    }

    public void OnClickOptionsButton()
    {
        OptionUI.Instance.Open();
    }

    public void OnClickExitButton()
    {
        Application.Quit();
    }

    private void StartEnterSequence()
    {
        float fadeTime = FadeoutPanel.fadeDuration;
        FadeoutPanel.Fadeout();

        AsyncOperation asyncOper = SceneManager.LoadSceneAsync(LoadingSceneName);
        asyncOper.allowSceneActivation = false;

        StartCoroutine(SceneLoadCoroutine());
        
        IEnumerator SceneLoadCoroutine()
        {
            yield return new WaitForSeconds(fadeTime);
            while(!asyncOper.isDone)
            {
                if (asyncOper.progress >= 0.9f) break;
                yield return null;
            }
            asyncOper.allowSceneActivation = true;
        }
    }

    private void DeactivateButtons()
    {
        button_contiue.interactable = false;
        button_newGame.interactable = false;
    }
}
