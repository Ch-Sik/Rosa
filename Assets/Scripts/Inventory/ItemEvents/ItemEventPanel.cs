using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

public class ItemEventPanel : MonoBehaviour
{
    public RectTransform panel;
    public Image dataImage;
    public TextMeshProUGUI dataText;

    [Button]
    public void SetData(SO_Item item, int quantity)
    {
        dataImage.sprite = item.sprite;
        dataText.text = item.itemName;

        if (quantity == 1)
            dataText.text += "";
        else
            dataText.text += $"\n  x {quantity}";
    }

    [Button]
    public void SetData(SO_Skill skill)
    {
        dataImage.sprite = skill.icon;
        dataText.text = skill.skillName;
    }
    [Button]
    public void PlayEvent()
    {
        Sequence seq = DOTween.Sequence()
        .Append(panel.DOAnchorPosX(-200, 0.5f).SetEase(Ease.OutQuad))
        .AppendInterval(1.5f)
        .Append(panel.DOAnchorPosX(200, 0.5f).SetEase(Ease.InQuad))
        .AppendCallback(() => Destroy(gameObject));
    }
}