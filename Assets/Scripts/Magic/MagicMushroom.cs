using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMushroom : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Collider2D trigger;
    [Space(10)]
    [SerializeField] float jumpPower; // 점프력

    private void Start()
    {
        MapManager.Instance.OnNextRoomLoaded += DestroyMushroom;
    }

    // Destroy는 즉시 삭제하는 것, Disappear는 사라지는 연출 후에 사라지는 것.
    public void Disappear()
    {
        trigger.enabled = false;
        if (anim != null)
        {
            // TODO: 버섯 사라지는 연출 적용
        }
        Invoke("DestroyMushroom", 0.5f);
    }

    void DestroyMushroom()
    {
        MapManager.Instance.OnNextRoomLoaded -= DestroyMushroom;
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            // if (PlayerRef.Instance.movement.isGrounded)
                PlayerRef.Instance.movement.MushJump();
        }
        
        if(collision.gameObject.CompareTag("Cube"))
        {
            int dir;
            if (gameObject.transform.position.x - collision.transform.position.x > 0) dir = -1;
            else dir = 1;
            collision.gameObject.GetComponent<G_Cube>().MushJump(dir);
        }
        
    }

    public void DoDestroy()
    {
        // TODO: 파괴 연출 추가
        GetComponent<Collider2D>().enabled = false;
        Destroy(transform.parent.gameObject, 1f);
    }
}
