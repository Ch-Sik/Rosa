using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

// TODO: 투사체 회전 옵션 만들기
public class MonsterProjectile : MonoBehaviour
{
    [SerializeField, Tooltip("중력 영향 여부")]
    private bool useGravity = false;
    [SerializeField, Tooltip("속도 계수. 중력 사용하지 않을 때에만 사용할 것")]
    private float speedScale = 1f;
    [SerializeField, Tooltip("발사 시의 회전값 옵션")]
    ProjectileRotationMode startRotationMode = ProjectileRotationMode.RandomRotation;
    [SerializeField, ShowIf("startRotationMode", Value = ProjectileRotationMode.RandomRotation), Tooltip("투사체 스폰 시의 랜덤 회전값. 0 이하면 0으로 취급")]
    private float randomRotationOnStart = 0;
    [SerializeField, Tooltip("투사체 랜덤 회전 속도 여부")]
    private bool useRandomAngularVelocity = false;
    [SerializeField, Tooltip("랜덤 회전 최대치")]
    private float randomRotationRange = 30f;

    [SerializeField, Tooltip("투사체와 충돌하여 가로막힐 레이어")]
    private LayerMask blockingLayers = 656384;          // 기본값: "Ground", "Cube", "PlayerGrab"
    [SerializeField, Tooltip("버섯 파괴 가능?")]
    protected bool canDestroyMushroom = false;
    [SerializeField, Tooltip("투사체가 벽에 닿았을 때 행동 설정")]
    protected ProjectileWallHitOption onWallHit = ProjectileWallHitOption.Destroy;

    [SerializeField, Tooltip("수명 사용")]
    private bool useLifetime = false;
    [SerializeField, ShowIf("useLifetime")]
    private float lifetime;

    [SerializeField]
    protected new Rigidbody2D rigidbody;
    [SerializeField]
    public Collider2D[] colliders;
    [SerializeField]
    private Animator animator;

    public virtual void InitProjectile(Vector2 direction)
    {
        // 필요 컴포넌트 설정
        if (rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
            Debug.Assert(rigidbody != null, $"{gameObject.name}: Rigidbody2D 레퍼런스가 설정되어있지 않음");
        }
        if (useGravity)
            rigidbody.isKinematic = false;
        else
            rigidbody.isKinematic = true;
        if (colliders == null || colliders.Length == 0)
        {
            colliders = GetComponents<Collider2D>();
            Debug.Assert(colliders != null && colliders.Length > 0, $"{gameObject.name}: Collider2D 레퍼런스가 설정되어있지 않음");
        }

        // 만약 콜라이더 꺼져있다면 활성화
        foreach (var c in colliders)
            c.enabled = true;

        // 기본 속도 설정
        rigidbody.velocity = direction * speedScale;
        // Debug.Log($"투사체 속도:{direction * speedScale}");

        // 기본 회전값 설정
        switch (startRotationMode)
        {
            case ProjectileRotationMode.DefaultFixed:
                // 아무것도 하지 않음
                break;
            case ProjectileRotationMode.RandomRotation:
                if (randomRotationOnStart > 0)
                {
                    transform.rotation = Quaternion.Euler(0, 0, Random.Range(-randomRotationOnStart, randomRotationOnStart));
                }
                break;
            case ProjectileRotationMode.UseVelocityDir:
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                break;
            default:
                Debug.LogWarning("투사체 발사시의 회전값 옵션이 올바르지 않은 것 같음");
                break;
        }

        // 회전값 설정
        if (useRandomAngularVelocity)
        {
            rigidbody.angularVelocity = Random.Range(-randomRotationRange, randomRotationRange);
        }

        // 수명 옵션 사용시 수명 설정
        if(useLifetime)
        {
            Invoke("OnLifetimeEnd", lifetime);
        }
    }

    void OnLifetimeEnd()
    {
        Disappear(1f);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        // 벽에 닿았다면
        if((1 << collider.gameObject.layer & blockingLayers) != 0)
        {
            switch(onWallHit)
            {
                case ProjectileWallHitOption.Ignore:
                    // Do nothing
                    break;
                case ProjectileWallHitOption.Disable:
                    rigidbody.velocity = Vector2.zero;
                    foreach (var c in colliders)
                        c.enabled = false;
                    StartCoroutine(DisableWithDelay());
                    IEnumerator DisableWithDelay()
                    {
                        yield return new WaitForSeconds(1f);
                        gameObject.SetActive(false);
                    }
                    break;
                case ProjectileWallHitOption.Destroy:
                    rigidbody.velocity = Vector2.zero;
                    foreach (var c in colliders)
                        c.enabled = false;
                    Disappear(1f);
                    break;
                case ProjectileWallHitOption.Stop:
                    rigidbody.velocity = Vector2.zero;
                    break;
                case ProjectileWallHitOption.Reflect:
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, rigidbody.velocity, 10f, blockingLayers);
                    if (hit.collider != null)
                    {
                        // 벽에 부딪혔을 때 반사 수행
                        Vector2 normal = hit.normal;
                        Vector2 reflected = Vector2.Reflect(rigidbody.velocity, normal);
                        rigidbody.velocity = reflected;
                    }
                    break;
                default:
                    Debug.LogError("잘못된 ProjectileWallHitOption!");
                    break;
            }
        }

        if(collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("몬스터 투사체 플레이어와 접촉");
            rigidbody.velocity = Vector2.zero;
            foreach (var c in colliders)
                c.enabled = false;
            Disappear(1f);
        }

        if(canDestroyMushroom && (collider.tag == "Mushroom"))
        {
            Debug.Log("버섯 파괴 시전");
            collider.GetComponent<MagicMushroom>().DoDestroy();
        }
    }

    public void Disappear(float delay)
    {
        if(animator != null)
        {
            animator.SetTrigger("disappear");
        }
        Invoke("DoDestroy", delay);
    }

    void DoDestroy()
    {
        Destroy(gameObject);
        CancelInvoke();
    }
}
