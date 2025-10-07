using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItemOnDestroy : MonoBehaviour
{
    [SerializeField]
    GameObject itemPrefab;

    void OnDestroy ()
    {
        GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        
        // 위로 뿅 튀어오르는 연출
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (!rb)
            return;
        rb.AddForce(Vector2.up, ForceMode2D.Impulse);
    }
}
