using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAttackItemOnDisappear : MonoBehaviour
{
    [SerializeField] private GameObject attackItemPrefab;
    [SerializeField] private Transform attackItemSpawnPos;
    [SerializeField] private float attackItemSpawnProbability = 1;
    [SerializeField] private float attackItemPopVertical = 150f;
    [SerializeField] private float attackItemPopHorizontal = 100f;
    
    [SerializeField] private ProjectileBase projectileComponent; 
    // Start is called before the first frame update
    private void Start()
    {
        projectileComponent.OnDisappear += SpawnAttackItem;
    }
    
    private void SpawnAttackItem()
    {
        if (!attackItemPrefab)
            return;

        float random = Random.Range(0f, 1f);
        if (random > attackItemSpawnProbability)
            return;
        
        var instance = Instantiate(attackItemPrefab, attackItemSpawnPos.position, Quaternion.identity);
        
        // 공격 아이템에 살짝 튀어오르는 연출
        var popVector = new Vector2(Random.Range(-attackItemPopHorizontal, attackItemPopHorizontal), 
            attackItemPopVertical);
        instance.GetComponent<Rigidbody2D>().AddForce(popVector);
    }
}
