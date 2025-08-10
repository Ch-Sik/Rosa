using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

public class UIBerry : MonoBehaviour
{
    public RectTransform UI;
    public TextMeshProUGUI curBerry;
    public TextMeshProUGUI deltaBerry;

    public int curBerryAmount = 0;
    public int addedBerryAmount = 0;
    public int deltaBerryAmount = 0;

    Sequence deltaSeq;
    Sequence curSeq;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        curBerryAmount = BerryManager.Instance.berry;
        curBerry.text = curBerryAmount.ToString();
        deltaBerry.DOFade(0, 0);
    }

    [Button]
    public void AddBerryEvent(int amount)
    {
        if (deltaSeq != null)
            deltaSeq.Kill();

        addedBerryAmount += amount;
        deltaBerry.DOFade(1, 0f);
        deltaSeq = DOTween.Sequence()
        .Append(DOTween.To(() => deltaBerryAmount, x => deltaBerryAmount = x, addedBerryAmount, 0.5f))
        .AppendInterval(0.5f)
        .Append(deltaBerry.DOFade(0, 0.5f))
        .OnComplete(() =>
        {
            ChangeCurBerryEvent(curBerryAmount + addedBerryAmount);
        })
        .OnUpdate(() =>
        {
            deltaBerry.text = deltaBerryAmount > 0 ? "+" + deltaBerryAmount.ToString() : deltaBerryAmount.ToString();
        });
    }

    private void ChangeCurBerryEvent(int addedBerry)
    {
        deltaBerryAmount = 0;
        addedBerryAmount = 0;
        curSeq = DOTween.Sequence()
        .Append(DOTween.To(() => curBerryAmount, x => curBerry.text = x.ToString(), addedBerry, 0.5f))
        .AppendCallback(() =>
        {
            curBerryAmount = addedBerry;
        });
    }

    [Button]
    public void Show()
    {
        Sequence show = DOTween.Sequence()
        .AppendCallback(() =>
        {
            UI.gameObject.SetActive(true);
            Init();
        })
        .Append(UI.DOAnchorPosY(0, 0.3f).SetEase(Ease.InQuad));
    }

    [Button]
    public void Hide()
    {
        Sequence hide = DOTween.Sequence()
        .Append(UI.DOAnchorPosY(100, 0.3f).SetEase(Ease.OutQuad))
        .AppendCallback(() => UI.gameObject.SetActive(false));
    }
}
