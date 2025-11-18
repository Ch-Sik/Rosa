using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }
    #endregion
    
    public void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    IEnumerator GameOverCoroutine()
    {
        // 1. 슬로우
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(0.2f);
        // 2. 화면 페이드 아웃
        FadeoutPanel.Fadeout();
        yield return new WaitForSecondsRealtime(FadeoutPanel.fadeDuration);
        // 3. 게임 초기화
        FlagManager.Instance.Init();                // 각종 플래그 상태 초기화
        PlayerRef.Instance.state.Init();            // 체력 상태 초기화
        PlayerRef.Instance.movement.LoadFlags();    // 액션 획득 상태 초기화
        MapManager.Instance.Start();                // 맵 재로드
        // 4. 추가로 숨고르기
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(2f);
        // 5. 페이드인, 게임 재시작
        FadeoutPanel.FadeIn();
    }
}
