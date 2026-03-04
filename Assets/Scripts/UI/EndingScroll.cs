using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingScroll : MonoBehaviour
{
    [SerializeField] private AudioResource bgm;
    [SerializeField] private float scrollDuration;
    [SerializeField] private ScrollRect scrollRect;
    
    
    // Start is called before the first frame update
    void Start()
    {
        FadeoutPanel.FadeIn();
        if (bgm)
        {
            BGMPlayer.Instance?.PlayBGM(bgm);
            BGMPlayer.Instance?.SetLoop(false);
        }
        scrollRect.verticalNormalizedPosition = 1f;
        scrollRect.DOVerticalNormalizedPos(0, scrollDuration);
        
        float waitTime = Mathf.Max(bgm.audioClip.length, scrollDuration);
        GoToTitleScene(waitTime).Forget();
    }

    private async UniTaskVoid GoToTitleScene(float delay)
    {
        await UniTask.WaitForSeconds(delay);
        
        FadeoutPanel.Fadeout();
        
        await UniTask.WaitForSeconds(FadeoutPanel.fadeDuration + 0.1f);
        await SceneManager.LoadSceneAsync("Title");
        await UniTask.WaitForSeconds(0.5f);
        
        FadeoutPanel.FadeIn();
    }
}
