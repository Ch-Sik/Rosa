using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Task_GA_Tackle_WildBoar : Task_GA_Tackle
{
    [Title("이펙트 관련")] 
    [SerializeField] private VfxPoolEntity wallCrashParticle;
    [SerializeField] private Transform wallCrashParticleSpawnPos;
    
    [Title("공격 아이템 스폰 관련")]
    [SerializeField] private GameObject attackItemPrefab;

    [SerializeField] private Transform attackItemSpawnPos;
    [SerializeField] private float attackItemPopVertical = 1f;
    [SerializeField] private float attackItemPopHorizontal = 1f;
    [SerializeField] private float attackItemSpawnDelay = 0.1f;
    
    protected override void OnStunStarted()
    {
        SpawnWallCrashParticle();
        StartCoroutine(SpawnAttackItem());
    }

    private void SpawnWallCrashParticle()
    {
        VfxManager.Instance.SpawnVfxObject(wallCrashParticle, wallCrashParticleSpawnPos.position);
    }

    private IEnumerator SpawnAttackItem()
    {
        yield return new WaitForSeconds(attackItemSpawnDelay);
        var instance = Instantiate(attackItemPrefab, attackItemSpawnPos.position, Quaternion.identity);
        
        // 공격 아이템에 살짝 튀어오르는 연출
        var popVector = new Vector2(attackItemPopVertical,
            Random.Range(-attackItemPopHorizontal, attackItemPopHorizontal));
        instance.GetComponent<Rigidbody2D>().AddForce(popVector);
    }
}
