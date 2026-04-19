using System;
using Sirenix.OdinInspector;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

// NOTE: Transform.Scale은 애니메이팅에 사용
public class G_Laser : GimmickSignalReceiver
{
    [Title("레퍼런스")]
    public Transform point;
    public SpriteRenderer laserSprite;
    public LayerMask playerLayerMask;
    public LayerMask obstaclesLayerMask;
    [SerializeField] private ParticleSystem[] onReadyParticles;
    [SerializeField] private ParticleSystem[] onActiveParticles;
    [SerializeField] private Animator topAnim;
    [SerializeField] private Animator midAnim;
    [SerializeField] private Animator botAnim;
    [SerializeField] private SFXPlayer sfxPlayerActivate;
    [SerializeField] private SFXPlayer sfxPlayerLoop;
    
    [Title("타이밍 & 길이")]
    public float startDelay = 0.0f;
    public float onTime = 1.0f;
    [Tooltip("끄고 켜지는 사이시간을 고려, offTime은 0.2초 이상이어야 하며, 그 이하일 경우 무한 지속으로 판정")]
    public float offTime = 1.0f;        //OFF Time이 0일 시 무한
    public bool autoLaserLength = false;
    [FormerlySerializedAs("laserMaxLength")] 
    public float activeLength;
    public float inactiveLength;
    [SerializeField] private bool shutByTerrain = false;
    
    [Title("데미지")]
    [SerializeField] private int damage = 1;
    [SerializeField] private bool respawnOnDamage = false;
    
    [Title("디버깅")]
    public bool showGizmos = true;
    /// 전체 레이저 On/Off 사이클의 동작 여부
    public bool isActivate = true;
    /// 실제 데미지를 가하는 레이저가 동작하고 있는지 여부
    [SerializeField, ReadOnly] private bool lasing = false;
    /// 레이저의 실제 길이
    [SerializeField, ReadOnly] private float curLength = 0.00f;

    private float _curLaserLength;
    private RaycastHit _hit;
    private CancellationTokenSource _cts = new();

    private void Start()
    {
        Invoke(nameof(ActivateLaser), startDelay);

        if (onTime <= 0)
        {
            Debug.LogWarning("[G_WaterLaser] onTime은 0보다 커야 함. 임의로 0.1로 설정");
            onTime = 0.1f;
        }

        if (autoLaserLength)
            UpdateLaserMaxLength();
    }

    private void UpdateLaserMaxLength()
    {
        RaycastHit2D hit = Physics2D.Raycast(point.position, point.up, 
            100f, obstaclesLayerMask);
        if (hit.collider && ((1 << hit.collider.gameObject.layer) & obstaclesLayerMask) != 0)
        {
            activeLength = hit.distance;
        }
    }

    private void Update()
    {
        if (lasing)
        {
            if(shutByTerrain)
                UpdateLaserLength();
            
            var detected = Detect();
            if (detected)
            {
                DoDamageIfItsPlayer(detected);
            }
        }
        
        var spriteSize = new Vector2(laserSprite.size.x, curLength);
        laserSprite.size = spriteSize;
        topAnim.transform.localPosition = Vector3.up * curLength;
    }
    
    private void UpdateLaserLength()
    {
        RaycastHit2D hit = Physics2D.Raycast(point.position, point.up, 
            100f, obstaclesLayerMask);
        if (hit.collider && ((1 << hit.collider.gameObject.layer) & obstaclesLayerMask) != 0)
        {
            activeLength = hit.distance;
        }
    }

    public GameObject Detect()
    {
        RaycastHit2D hit = Physics2D.Raycast(point.position, point.up, 
                                            activeLength, playerLayerMask | obstaclesLayerMask);
        
        if (hit.collider && hit.collider.CompareTag("Player"))
        {
            return hit.collider.gameObject;
        }
        return null;
    }
    
    private void DoDamageIfItsPlayer(GameObject go)
    {
        if (!go.CompareTag("Player")) return;

        if (respawnOnDamage)
            go.GetComponent<PlayerDamageReceiver>().GetDamageAndRespawn(damage);
        else
            go.GetComponent<PlayerDamageReceiver>().GetDamage(gameObject, damage);
    }

    [Button]
    public void ActivateLaser()
    {
        if(shutByTerrain)
            UpdateLaserLength();

        isActivate = true;
        
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        Laser(_cts.Token).Forget();
    }

    [Button]
    public void InactivateLaser()
    {
        lasing = false;
        isActivate = false;
        
        _cts.Cancel();
    }

    private async UniTaskVoid Laser(CancellationToken ctk)
    {
        while (true)
        {
            // 비활성화 상태
            lasing = false;
            SetAnimatorsState(0);
            
            if(offTime > 0.2f)
                await UniTask.WaitForSeconds(offTime - 0.2f, cancellationToken: ctk);
            
            // 레이저 발사 시작
            SetAnimatorsState(1);
            DOTween.To(() => curLength,
                (x) => { curLength = x; }, activeLength, 0.1f);
            foreach(var p in onReadyParticles)
                p.Stop();
            foreach(var p in onActiveParticles)
                p.Play();
            
            await UniTask.WaitForSeconds(0.1f, cancellationToken: ctk);
            
            // 레이저 활성화
            lasing = true;
            SetAnimatorsState(2);
            sfxPlayerActivate?.PlaySfx();
            sfxPlayerLoop?.PlaySfx();
            
            await UniTask.WaitForSeconds(onTime, cancellationToken: ctk);
            
            // 무한 지속일 경우, 캔슬될 때까지 무한지속
            if (offTime <= 0.2f)
            {
                await UniTask.WaitUntil(() => false, cancellationToken: ctk);
                return;
            }
            
            // 레이저 발사 종료 
            SetAnimatorsState(1);
            sfxPlayerLoop?.StopSfx();
            DOTween.To(() => curLength,
                (x) => { curLength = x; }, inactiveLength, 0.1f);
            foreach(var p in onReadyParticles)
                p.Play();
            foreach(var p in onActiveParticles)
                p.Stop();
            await UniTask.WaitForSeconds(0.1f, cancellationToken: ctk);
        }
    }

    public override void OffAct()
    {
        ActivateLaser();
    }

    public override void OnAct()
    {
        InactivateLaser();
    }

    public override void ImmediateOffAct()
    {
        ActivateLaser();
    }

    public override void ImmediateOnAct()
    {
        InactivateLaser();
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(point.position, 0.3f);
        Debug.DrawRay(point.position, point.up * activeLength, Color.red);
    }

    private void SetAnimatorsState(int state)
    {
        var hash = Animator.StringToHash("State");
        topAnim?.SetInteger(hash, state);
        midAnim?.SetInteger(hash, state);
        botAnim?.SetInteger(hash, state);
    }

    private void OnDestroy()
    {
        InactivateLaser();
    }
}
