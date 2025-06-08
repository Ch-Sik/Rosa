using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 화면 UI 담당
/// </summary>
public class TitleScreenUI : MonoBehaviour
{
    [SerializeField] string LoadingSceneName;

    // Start is called before the first frame update
    void Start()
    {
        // TODO: SaveLoadManager를 통해 기존에 저장된 파일이 있는지 확인하고,
        //      없다면 이어하기 버튼 비활성화하기
    }

    public void OnClickNewGameButton()
    {
        Debug.Log("새 게임 시작");
        StartEnterSequence();
    }

    public void OnClickContinueButton()
    {
        Debug.Log("이어하기 시작");
        SaveLoadManager.Instance.DisableNewGameFlag();
        StartEnterSequence();
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
}
