using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 필드 상에 떨어져있는 or 몬스터를 잡아서 떨어지는 아이템들 (= 반짝이)
/// </summary>
public class FieldItem : MonoBehaviour
{
    [SerializeField] private Collider2D col;
    [SerializeField] private SO_Item item;
    [Min(1)]
    [SerializeField] private int quantity = 1;

    public void OnInteraction()
    {
        // 25.08.08) 아이템 획득 UI가 특수 아이템획득 시에만 표시되도록 변경
        if(item.rarity != ItemRarity.normal)
            ItemToastMessage.Instance.AddItem(item, quantity);

        InventoryController.Instance.AddItem(item.code, quantity);
        Disappear();
    }

    private void Disappear()
    {
        if(col)
            col.enabled = false;
        DOVirtual.DelayedCall(3f, () =>
        {
            if(gameObject)
                Destroy(gameObject);
        });
    }
}
