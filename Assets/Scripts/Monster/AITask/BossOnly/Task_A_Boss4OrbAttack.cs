using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Panda;

public class Task_A_Boss4OrbAttack : Task_A_Base
{
    [Tooltip("투사체 프리팹")]
    public GameObject orbPrefab;

    [Tooltip("투사체가 초기에 위치할 곳")]
    public Transform[] muzzles;

    [Tooltip("투사체 속도")]
    public float projectileSpeed = 3f;

    [Tooltip("발사할 투사체 3개")]
    GameObject[] Orbs;

    [SerializeField] private SFXPlayer launchSfx;

    [Task]
    void OrbAttack()
    {
        ExecuteAttack();
    }

    protected override void OnStartupBegin()
    {
        Orbs = new GameObject[3];
        // 투사체는 풀링된다고 가정. 초기화 필요
        for(int i=0; i<3; i++)
        {
            Orbs[i] = Instantiate(orbPrefab);
            Orbs[i].transform.position = transform.position;
            Orbs[i].SetActive(true);
            Orbs[i].GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }

        // 투사체 3개를 제 위치로 이동시키기
        var seq = DOTween.Sequence();
        for(int i=0; i<Orbs.Length; i++)
        {
            seq.Insert(0, Orbs[i].transform.DOMove(muzzles[i].position, startupDuration - 0.1f));
        }
    }


    protected override void OnActiveBegin()
    {
        GameObject enemy;
        blackboard.TryGet(BBK.Enemy, out enemy);
        Vector2 enemyPosition = enemy.transform.position;

        foreach(var orb in Orbs)
        {
            // 각 구체 별로 발사 벡터 계산 후 발사
            Vector2 launchDir = (enemyPosition - (Vector2)orb.transform.position).normalized;
            orb.GetComponent<ProjectileBase>().InitProjectile(launchDir * projectileSpeed);
        }
        launchSfx?.PlaySfx();
    }

    protected override void OnRecoveryBegin()
    {
        // // 발사된 투사체들 비활성화
        // foreach(var orb in Orbs)
        // {
        //     orb.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        //     orb.SetActive(false);
        // }
    }
}
