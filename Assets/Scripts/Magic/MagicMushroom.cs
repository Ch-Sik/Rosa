using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class MagicMushroom : MonoBehaviour
{
    [SerializeField] float jumpPower; // 점프력
    [Space(10)]
    [SerializeField] Collider2D trigger;
    [SerializeField] AnimancerComponent animancer;
    [SerializeField] AnimationClip spawnAnim;
    [SerializeField] AnimationClip disappearAnim;

    private void Start()
    {
        MapManager.Instance.OnNextRoomLoaded += DestroyMushroom;
        if (animancer && spawnAnim)
            animancer.Play(spawnAnim);
    }

    // 사라지는 연출 후에 삭제
    public void Disappear()
    {
        trigger.enabled = false;
        if (animancer && disappearAnim)
            animancer.Play(disappearAnim);
        Invoke("DestroyMushroom", 0.5f);
    }

    // 버섯 즉시 삭제
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
}
