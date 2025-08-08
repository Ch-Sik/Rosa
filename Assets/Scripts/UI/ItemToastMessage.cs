using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemToastMessage : MonoBehaviour
{
    private static ItemToastMessage _instance;
    public static ItemToastMessage Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ItemToastMessage>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<ItemToastMessage>();
                }
            }
            return _instance;
        }
    }

    public RectTransform panel;
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemQuantity;

    [Button]
    public void AddItem(SO_Item item, int quantity)
    {
        itemImage.sprite = item.sprite;
        itemName.text = item.itemName;
        itemQuantity.text = $"x{quantity}";

        Sequence seq = DOTween.Sequence()
        .Append(panel.DOAnchorPosX(0, 0.2f).SetEase(Ease.InSine))
        .AppendInterval(1f)
        .Append(panel.DOAnchorPosX(400, 0.2f).SetEase(Ease.OutSine));
    }
}
