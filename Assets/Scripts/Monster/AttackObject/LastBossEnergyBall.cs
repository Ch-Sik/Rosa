using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastBossEnergyBall : ProjectileBase
{
    [SerializeField] int damageToPlayer = 1;
    [SerializeField] int damageToBoss = 1;
    [Tooltip("진행 방향 벡터와 대상을 향하는 벡터의 각도가 이 값을 넘으면 유도 중단")]
    [SerializeField] float follingLimitAngle;
    [SerializeField] float rotateSpeed;
    [SerializeField, ReadOnly] bool isFollowing = false; // 현재 유도 활성화중인지
    [SerializeField, ReadOnly] bool damageToBossEnabled = false;
    [SerializeField, ReadOnly] GameObject target;       // 유도될 대상

    public Action OnDestroyAction;

    
    // Start is called before the first frame update
    void Start()
    {
        isFollowing = false;
    }

    public void SetTarget(GameObject target)
    {
        this.target = target;
        if(target != null)
        {
            isFollowing = true;
        }
    }

    public override void InitProjectile(Vector2 direction)
    {
        base.InitProjectile(direction);

        isFollowing = true;
        Invoke("EnableDamageToBoss", 5f);        
        // 에너지볼 생성 시점에는 보스와 겹쳐있는점 고려, 발사 후 일정 시간 후에 보스에게 데미지 활성화하도록 함.
    }

    void EnableDamageToBoss()
    {
        damageToBossEnabled = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isFollowing) return;

        Vector2 curAngle = rigidbody.velocity.normalized;
        Vector2 toTarget = (target.transform.position - transform.position).normalized;
        float dot = Vector2.Dot(curAngle, toTarget);
        if (dot < Mathf.Cos(follingLimitAngle * Mathf.Deg2Rad))
        {
            isFollowing =false;
            return;
        }
        else
        {
            Vector2 rotated = Vector3.RotateTowards(curAngle, toTarget, rotateSpeed * Time.fixedDeltaTime, 999f);
            rigidbody.velocity = rotated * rigidbody.velocity.magnitude;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어에게 닿으면 플레이어에게 데미지
        base.OnTriggerEnter2D(collision);

        if (damageToBossEnabled)
        {
            // 몬스터(최종보스)에게 닿으면 그쪽에 데미지
            if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
            {
                Debug.Log("에너지볼 보스와 접촉");
                rigidbody.velocity = Vector2.zero;

                var damageComponent = collision.gameObject.GetComponent<MonsterDamageReceiver>();
                damageComponent.GetHitt(damageToBoss, 0);       // 최종보스는 넉백 안받으므로 angle 설정 안해줘도 됨.
                foreach(var col in colliders)
                {
                    col.enabled = false;
                }
                Disappear();                  // 사라질 연출 고려 1초 대기
            }
        }
    }

    public void OnDestroy()
    {
        if (OnDestroyAction != null)
            OnDestroyAction();
    }
}
