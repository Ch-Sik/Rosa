using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterShockwave : MonoBehaviour
{
    [SerializeField]
    private new Rigidbody2D rigidbody;
    [SerializeField]
    private new Collider2D collider;
    [SerializeField, Tooltip("충격파의 앞쪽이 나아가는 속도. backSpeed보다 빠르다면 충격파가 시간이 지날수록 좌우로 넓어짐")]
    private float frontSpeed = 2f;
    [SerializeField, Tooltip("충격파의 뒤쪽이 나아가는 속도. frontSpeed보다 빠르다면 충격파가 시간이 지날수록 좌우로 좁아짐")]
    private float backSpeed = 1f;
    
    private bool stuckAtWall = false;
    
    public void Init(LR dir)
    {
        if(rigidbody != null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
        }
        rigidbody.velocity = dir.toVector2() * (frontSpeed + backSpeed) / 2;
    }

    private void Update()
    {
        if (stuckAtWall == false)   // 벽에 가로막히지 않은 경우 좌우 사이즈 서서히 커지기
        {
            float newXscale = transform.localScale.x + (frontSpeed - backSpeed) * Time.deltaTime;
            transform.localScale = new Vector3(newXscale, transform.localScale.y);
        }
        else    // 벽에 가로막힌 경우 좌우 사이즈 서서히 줄어들기
        {
            float newXscale = transform.localScale.x - backSpeed * Time.deltaTime;
            if(newXscale < 0.1f)
            {
                Destroy(gameObject);
                return;
            }
            transform.localScale = new Vector3(newXscale, transform.localScale.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("PlayerAttack"))
        {
            return;
        }
        if(collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            stuckAtWall = true;
            rigidbody.velocity = rigidbody.velocity.toLR().toVector2() * backSpeed / 2;
        }
    }
}
