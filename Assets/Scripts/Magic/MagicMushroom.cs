using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Cysharp.Threading.Tasks;

public class MagicMushroom : MonoBehaviour
{
    [SerializeField] float jumpPower; // 점프력
    [SerializeField] private float lifeTime;
    [Space(10)] [SerializeField] Collider2D trigger;
    [SerializeField] AnimancerComponent animancer;
    [SerializeField] AnimationClip spawnAnim;
    [SerializeField] AnimationClip disappearAnim;
    [Space(10)] [SerializeField] VfxPoolEntity spawnVfx;
    [SerializeField] VfxPoolEntity disappearVfx;

    private bool _disappeared = false;
    
    private void Start()
    {
        MapManager.Instance.OnNextRoomLoaded += DestroyMushroom;
        if (animancer && spawnAnim)
            animancer.Play(spawnAnim);
        if (spawnVfx)
            VfxManager.Instance.SpawnVfxObject(spawnVfx, transform.position);
        ReserveDisappear(lifeTime);
    }

    private async UniTaskVoid ReserveDisappear(float time)
    {
        await UniTask.WaitForSeconds(time);
        Disappear();
    }

// 사라지는 연출 후에 삭제
    public void Disappear()
    {
        // 소멸 시퀀스 중복 실행 방지
        if (_disappeared) return;
        _disappeared = true;
        
        trigger.enabled = false;

        if (animancer && disappearAnim)
            animancer.Play(disappearAnim);
        if (disappearVfx)
            VfxManager.Instance.SpawnVfxObject(disappearVfx, transform.position);

        Invoke("DestroyMushroom", 0.5f);
    }

    // 버섯 즉시 삭제
    void DestroyMushroom()
    {
        MapManager.Instance.OnNextRoomLoaded -= DestroyMushroom;
        if(gameObject)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        MapManager.Instance.OnNextRoomLoaded -= DestroyMushroom;
    }

    // Disable되는 상황은 부모 오브젝트(설치된 플랫폼)이 사라지는 케이스밖에 없다고 전제
    private void OnDisable()
    {
        gameObject.transform.SetParent(null);
        gameObject.SetActive(true); // 사라지는 모습 보여줘야하니까 다시 활성화
        Disappear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerRef.Instance.movement.MushJump();
            if (spawnVfx)
                VfxManager.Instance.SpawnVfxObject(spawnVfx, transform.position);
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
