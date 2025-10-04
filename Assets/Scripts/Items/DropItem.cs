using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 필드 상에 떨어져있는 or 몬스터를 잡아서 떨어지는 아이템들 (= 반짝이)
/// </summary>
public class DropItem : MonoBehaviour
{
    public SO_Item item;
    [Min(1)] public int quantity = 1;

    public void OnInteraction()
    {
        // 25.08.08) 아이템 획득 UI가 특수 아이템획득 시에만 표시되도록 변경
        if(item.rarity != ItemRarity.normal)
            ItemToastMessage.Instance.AddItem(item, quantity);

        InventoryController.Instance.AddItem(item.code, quantity);

        Destroy(gameObject);
    }

    // 낙하하다가 지면에 닿으면 그대로 멈추기. 
    // rigidbody & non-trigger collider가 부착되지 않은 npc형 dropItem에는 해당 안됨.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if(rb != null)
            rb.isKinematic = true;
    }
}
