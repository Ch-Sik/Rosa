using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeoutPanel : MonoBehaviour
{
    static FadeoutPanel instance;

    public static float fadeDuration { get { 
            if (instance == null)
            {
                Debug.LogError("FadeoutPanel의 인스턴스가 없음");
                return 0f;
            }
            return instance._fadeDuration;
        } }
    public static bool isTweening { get {
            if (instance == null)
            {
                Debug.LogError("FadeoutPanel의 인스턴스가 없음");
                return false;
            }
            return instance._isTweening; 
        } }
    public static bool isFadeOutActivated { get
        {
            if (instance == null)
            {
                Debug.LogError("FadeoutPanel의 인스턴스가 없음");
                return false;
            }
            return instance._imageComponent.color.a == 1f;
        } }

    [SerializeField] private Image _imageComponent;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private bool _fadeInOnStart = false;
    [SerializeField, ReadOnly] private bool _isTweening = false;

    // Start is called before the first frame update
    void Start()
    {
        if(instance != null)
        {
            Debug.LogWarning("FadeoutPanel 중복 감지, 새 인스턴스는 자동 Destroy");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        if(_fadeInOnStart)
        {
            _imageComponent.color = Color.black;
            DoFade(false);
        }
    }
    
    public static void Fadeout()
    {
        instance.DoFade(true);
    }

    public static void FadeIn()
    {
        instance.DoFade(false);
    }

    private void DoFade(bool fadeOutToBlack)
    {
        if(_isTweening)
        {
            Debug.LogError(
                "이미 Fade효과 수행 중인데 추가로 Fade 효과가 수행되면 의도치 않은 효과가 발생할 수 있음"
            );
        }
        _isTweening = true;
        // alpha가 1이 되면 화면을 검은색으로 덮어버리면서 Fade Out 효과
        // alpha가 0이 되면 투명해지면서 Fade In 효과
        DOTween.Sequence()
            .Append(_imageComponent.DOFade(fadeOutToBlack ? 1 : 0, _fadeDuration))
            .AppendCallback(() => { _isTweening = false; });
    }
}
