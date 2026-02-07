using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

/// <summary>
/// CommunicationManager의 출력을 담당하는 UIControll 스크립트
/// </summary>

public class CommunicationUI : MonoBehaviour
{
    [FoldoutGroup("Pre-Input")] public float tweenTime = 0.3f;                                  //트윈 시간
    [FoldoutGroup("Pre-Input")] public GameObject UI;                                           //UI수식
    [FoldoutGroup("Pre-Input")] public Image backBoard;                                         //어두운 뒷 배경
    [FoldoutGroup("Pre-Input")] public Image DialoguePanel;                                     //대화 패널
    [FoldoutGroup("Pre-Input")] public TextMeshProUGUI charName;                                //캐릭터 이름 출력 패널
    [FoldoutGroup("Pre-Input")] public TextMeshProUGUI Dialogue;                                //대사 출력 패널
    [FoldoutGroup("Pre-Input")] public List<TargetImage> targetImages = new List<TargetImage>();//타겟의 스프라이트 출력
    public bool isTalking = false;

    Sequence start;
    Sequence end;
    Sequence show;
    Sequence talk;

    private void Start()
    {
        //시작과 동시에 tweenTime 초기화
        for (int i = 0; i < targetImages.Count; i++)
            targetImages[i].tweenTime = tweenTime;
    }

    //시작 애니메이션
    public float StartAnimation()
    {
        ResetAll();
        end?.Kill();

        start = DOTween.Sequence()
        .AppendCallback(() =>
        {
            UI.SetActive(true);
            charName.text = "";
            Dialogue.text = "";
        })
        .Append(backBoard.DOFade((200f / 255f), tweenTime))
        .Join(DialoguePanel.DOFade(1, tweenTime))
        .Join(charName.DOFade(1, tweenTime))
        .Join(Dialogue.DOFade(1, tweenTime));

        return tweenTime;
    }

    //종료 애니메이션
    public void EndAnimation()
    {
        end = DOTween.Sequence()
        .AppendCallback(() =>
        {
            for (int i = 0; i < targetImages.Count; i++)
                targetImages[i].Hide();
        })
        .Append(backBoard.DOFade(0, tweenTime))
        .Join(DialoguePanel.DOFade(0, tweenTime))
        .Join(charName.DOFade(0, tweenTime))
        .Join(Dialogue.DOFade(0, tweenTime))
        .AppendCallback(() =>
        {
            charName.text = "";
            Dialogue.text = "";
            UI.SetActive(false);
        });

        ResetAll();
    }

    //모든 데이터 리셋
    public void ResetAll()
    {
        for (int i = 0; i < targetImages.Count; i++)
            targetImages[i].Reset();

        show.Restart();
        show.Pause();
    }

    //빠른 입력으로 텍스팅 1차 스킵 부
    public void EarlyDone(string text, bool isTalkDone = false)
    {
        if (isTalkDone)
        {
            //만약 TalkDone이라면, 대화의 연결성이 떨어진 것으로 파악, 부분 대화를 종료한다.
            ResetTalk();
            return;
        }

        talk.Pause();
        Dialogue.text = text;
        isTalking = false;
    }

    public void Skip()
    {
        start.Pause();
        EndAnimation();
        isTalking = false;
    }

    //대상을 위치에 출력하는 함수
    public float ShowTarget(CommunicationTarget target, CommunicationLocation location)
    {
        TargetImage targetImage = targetImages[(int)location];
        
        //보이는 대상이 없는 경우
        if (targetImage.target == CommunicationTarget.None)
        {
            targetImage.Show(target);

            return tweenTime;
        }
        //보이는 대상이 있지만, 다른 경우
        else if (targetImage.target != target)
        {
            //Hide After show
            show = DOTween.Sequence()
            .Append(targetImage.image.DOFade(0, tweenTime).SetEase(Ease.Linear))
            .AppendCallback(() => targetImage.Show(target));

            return tweenTime * 2f;
        }

        return 0f;
    }

    //대상을 숨기는 함수
    public float HideTarget(CommunicationTarget target)
    {
        int index = FindTarget(target);

        if (index == -1)
            return 0f;

        targetImages[index].Hide();
        return tweenTime;
    }

    // 24.12.22) 한꺼번에 모두 숨기기 추가
    public float HideAll()
    {
        for(int i=0; i<targetImages.Count; i++)
        {
            targetImages[i].Hide();
        }
        return tweenTime;
    }

    //대상의 표정을 변경하는 함수
    public void SetEmotion(CommunicationTarget target, Emotion emotion)
    {
        int index = FindTarget(target);

        if (index == -1)
            return;

        targetImages[index].SetImage(target, emotion);
    }

    //타겟에게 대화를 출력시킴
    public void Texting(CommunicationTarget target, string text)
    {
        //타겟 파인딩
        int index = FindTarget(target);
        bool hasPortrait = true;

        // 24.12.22) 없다면 없는대로 진행하도록 수정
        if (index == -1)
            hasPortrait = false;

        TargetImage targetImage = null;
        if(hasPortrait)
            targetImage = targetImages[index];
        

        talk = DOTween.Sequence()
        .AppendCallback(() =>
        {
            isTalking = true;


            if(target != CommunicationTarget.None)
                charName.text = CommunicationManager.Instance.characters[target].Name;
            Dialogue.text = "";

            for (int i = 0; i < targetImages.Count; i++)
                targetImages[i].FadeOut(true);
            
            if(hasPortrait)
                targetImage.FadeIn(true);
        })
        .Append(Dialogue.DOText(text, text.Length * 0.015f))
        .AppendCallback(() =>
        {
            isTalking = false;
        });
    }

    //부분 대화를 종료시키는 부분
    public void ResetTalk()
    {
        charName.text = "";
        Dialogue.text = "";

        for (int i = 0; i < targetImages.Count; i++)
            targetImages[i].FadeIn(true);
    }

    //타겟을 대상으로 해당 타겟의 index를 리턴한다. 없다면 -1
    private int FindTarget(CommunicationTarget target)
    {
        for (int i = 0; i < targetImages.Count; i++)
            if (targetImages[i].target == target)
                return i;

        return -1;
    }
}

[Serializable]
public class TargetImage
{
    public CommunicationTarget target;
    public Vector3 showPos;
    public Vector3 hidePos;
    public Image image;
    public float tweenTime = 0.3f;

    public Sequence seq;

    public void SetImage(CommunicationTarget target, Emotion emotion = Emotion.Normal)
    {
        Sprite sprite;
        try
        {
            sprite = CommunicationManager.Instance.characters[target]?.GetEmotionImage(emotion);
        }
        catch(Exception e)
        {
            Debug.LogError($"캐릭터 스프라이트가 설정되어있지 않음, target: {target}, emotion: {emotion}");
            return;
        }

        this.target = target;
        image.sprite = sprite;
        // 250401 추가: 1920*1080 캔버스 상에서 이미지 원본 크기를 사용하도록 설정
        image.SetNativeSize();
    }

    public void Show(CommunicationTarget target)
    {
        this.target = target;
        SetImage(target);

        seq = DOTween.Sequence()
        .AppendCallback(() => image.enabled = true)
        .Append(image.GetComponent<RectTransform>().DOAnchorPos(showPos, tweenTime)).SetEase(Ease.Linear)
        .Join(image.DOFade(1, tweenTime)).SetEase(Ease.Linear);
    }

    public void Hide()
    {
        this.target = CommunicationTarget.None;

        seq = DOTween.Sequence()
        .Append(image.GetComponent<RectTransform>().DOAnchorPos(hidePos, tweenTime)).SetEase(Ease.Linear)
        .Join(image.DOFade(0, tweenTime)).SetEase(Ease.Linear)
        .AppendCallback(() => image.enabled = false);
    }

    public void FadeIn(bool isInstant = false)
    {
        seq = DOTween.Sequence()
        .Append(image.DOFade(1, !isInstant ? tweenTime : 0)).SetEase(Ease.Linear);
    }

    public void FadeOut(bool isInstant = false)
    {
        seq = DOTween.Sequence()
        .Append(image.DOFade((100f / 255f), !isInstant ? tweenTime : 0)).SetEase(Ease.Linear);
    }

    public void Reset()
    {
        seq.Pause();

        target = CommunicationTarget.None;
        image.sprite = null;
        image.DOFade(1, 0);
        image.GetComponent<RectTransform>().DOAnchorPos(hidePos, 0);
        image.enabled = false;
    }
}