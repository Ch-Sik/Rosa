using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Task_GA_Tackle_Crocodile : Task_GA_Tackle
{
    [Title("공격 아이템 스폰 관련")]
    [SerializeField] private GameObject attackItemPrefab;
    [SerializeField] private Transform attackItemSpawnPos;
    [SerializeField] private float attackItemSpawnProbability = 0.3f;
    [SerializeField] private float attackItemPopVertical = 150f;
    [SerializeField] private float attackItemPopHorizontal = 100f;
    [SerializeField] private float attackItemSpawnDelay = 0.1f;
    
    protected override void OnActiveBegin()
    {
        base.OnActiveBegin();
        
        if(Random.Range(0, 1f) <= attackItemSpawnProbability)
            StartCoroutine(SpawnAttackItem());
    }

    private IEnumerator SpawnAttackItem()
    {
        if (!attackItemPrefab)
            yield break;
        
        yield return new WaitForSeconds(attackItemSpawnDelay);
        var instance = Instantiate(attackItemPrefab, attackItemSpawnPos.position, Quaternion.identity);
        
        // 공격 아이템에 살짝 튀어오르는 연출
        var popVector = new Vector2(Random.Range(-attackItemPopHorizontal, attackItemPopHorizontal), 
                                    attackItemPopVertical);
        instance.GetComponent<Rigidbody2D>().AddForce(popVector);
    }
}
