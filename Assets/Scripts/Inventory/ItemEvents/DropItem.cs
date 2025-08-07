using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 필드 상에 떨어져있는 or 몬스터를 잡아서 떨어지는 아이템들 (= 반짝이)
/// </summary>
public class DropItem : MonoBehaviour
{
    public SO_Item item;
    [HideInInspector] public ItemCode code;
    [Min(1)] public int quantity = 1;

    public void AddItem()
    {
        ItemToastMessage.Instance.AddItem(item, quantity);

        Destroy(gameObject);
    }
}
