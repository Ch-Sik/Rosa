using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MonsterLaser : MonoBehaviour
{
    [Title("게임오브젝트/컴포넌트 레퍼런스")] 
    [SerializeField, Tooltip("레이저 공격 이펙트 묶음")]
    private GameObject beamRoot;
    [SerializeField, Tooltip("레이저 공격 이펙트의 시작부분")]
    private GameObject beamStart;
    [SerializeField, Tooltip("레이저 공격 이펙트의 길쭉한 부분")]
    private GameObject beamMid;
    [SerializeField, Tooltip("레이저 공격 이펙트의 끝부분")]
    private GameObject beamEnd;
    [SerializeField, Tooltip("레이저 공격 판정")]
    private Collider2D laserCollider;
    [SerializeField, Tooltip("레이저 공격 처리 컴포넌트")]
    private MonsterDamageInflictor damageInflictor;
    [SerializeField]
    private Animator animator;

    [Title("레이저 형태 관련 파라미터")]
    [SerializeField, Range(0.01f, 20.0f)]
    private float laserWidth = 0.5f;
    [SerializeField, Tooltip("레이저가 지형에 막히지 않았을 경우의 최대 길이")]
    private float laserMaxLength = 20.0f;
    [SerializeField, Tooltip("빔이 막힐 지형을 나타내는 레이어들 선택")]
    LayerMask terrainLayers;
    [SerializeField, Tooltip("레이저 뻗어나가는 속도")]
    private float laserSpeed = 1f;

    private bool collideWithTerrain;
    private float laserLength;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(beamMid != null, "레이저 이펙트의 길쭉한 부분은 필수로 할당되어야 함!");
        if(laserCollider == null)
        {
            laserCollider = GetComponentInChildren<Collider2D>();
            Debug.Assert(laserCollider != null);
        }
    }

    public void Initalize(Vector2 dir)
    {
        // 레이저 방향에 맞게 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle -= 90;   // 스프라이트가 오른쪽 기준이 아니라 위쪽 기준인 것 고려
        beamRoot.transform.eulerAngles = new Vector3(0, 0, angle);

        // 필요한 오브젝트/컴포넌트만 활성화하고 나머지는 비활성화
        gameObject.SetActive(true);
        beamStart?.SetActive(true);
        beamMid.SetActive(false);
        beamEnd?.SetActive(false);
        laserCollider.enabled = false;

        // 플레이어가 피할 수 있게 생성 시점에 조준 수행
        // raycast 수행 후 레이저 길이 조절
        RaycastHit2D hit;
        hit = Physics2D.Raycast(transform.position, dir, laserMaxLength, terrainLayers);
        if(hit.collider == null)
        {
            collideWithTerrain = false;
            laserLength = laserMaxLength;
        }
        else
        {
            collideWithTerrain = true;
            laserLength = hit.distance;
        }
        SetLaserSize(laserWidth, 0);

        // 만약에 레이저 발사 방향 미리보기 필요하다면 여기에다가 구현하기
    }

    public void Activate(int damage)
    {
        // 오브젝트/컴포넌트 모두 활성화
        beamMid.SetActive(true);
        beamEnd?.SetActive(true);

        beamMid.transform.DOScale(new Vector3(laserWidth, laserLength, 1), laserLength / laserSpeed);
        beamMid.transform.DOLocalMoveY(laserLength / 2, laserLength / laserSpeed);
        
        // 데미지 값 설정하고 콜라이더 활성화하기
        damageInflictor.damage = damage;
        laserCollider.enabled = true;
    }

    private void SetLaserSize(float width, float length)
    {
        if(beamStart != null)
        {
            beamStart.transform.localScale = new Vector3(width, width, 1);
        }
        beamMid.transform.localPosition = new Vector3(0, length / 2, 0);
        beamMid.transform.localScale = new Vector3(width, length, 0);
    }

    public void Terminate()
    {
        // TODO: 레이저 비활성화되기 전에 자연스러운 느낌 나도록 트윈 적용
        animator.SetTrigger("Disappear");
        beamMid.transform.DOScale(new Vector3(0, laserLength, 1), 0.1f);
        beamEnd.transform.DOScale(new Vector3(0, 0, 1), 0.1f);
        
        // 불필요한 오브젝트 비활성화
        // beamStart?.SetActive(false);
        // beamMid.SetActive(false);
        // beamEnd?.SetActive(false);
        // gameObject.SetActive(false);

        // 레이저 공격 판정 비활성화
        laserCollider.enabled = false;
        
        Destroy(gameObject, 1f);
    }
}
