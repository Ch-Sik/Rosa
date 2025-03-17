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
    [SerializeField, Tooltip("충격파 비주얼 담당하는 게임오브젝트. 방향에 따라 Flip 되어야 할 녀석")]
    GameObject vfx;
    [SerializeField, ReadOnly]
    LR _dir;
    
    private bool stuckAtWall = false;
    
    public void Init(LR dir)
    {
        _dir = dir;

        if(rigidbody != null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
        }
        rigidbody.velocity = _dir.toVector2() * (frontSpeed + backSpeed) / 2;

        // 비주얼 이펙트의 좌우 반전 처리. 기본 이펙트 방향은 오른쪽으로 가정.
        //if(_dir.isLEFT() && vfx != null)
        //{
        //    vfx.transform.localScale = Vector3.Scale(vfx.transform.localScale, new Vector3(-1, 1, 1));
        //}
        if(_dir.isLEFT())
        {
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }
    }

    private void Update()
    {
        if (stuckAtWall == false)   // 벽에 가로막히지 않은 경우 좌우 사이즈 서서히 커지기
        {
            float newXscale = transform.localScale.x + (frontSpeed - backSpeed) * Time.deltaTime * _dir.toFloat();
            transform.localScale = new Vector3(newXscale, transform.localScale.y);
        }
        else    // 벽에 가로막힌 경우 좌우 사이즈 서서히 줄어들기
        {
            float newXscale = transform.localScale.x - backSpeed * Time.deltaTime * _dir.toFloat();
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
