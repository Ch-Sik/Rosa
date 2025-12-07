using Sirenix.OdinInspector;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class G_WaterLaser : GimmickSignalReceiver
{
    public bool showGizmos = true;
    public Transform point;
    public SpriteRenderer laserSprite;
    public LayerMask playerLayerMask;
    public LayerMask obstaclesLayerMask;
    public float startDelay = 0.0f;
    [FormerlySerializedAs("laserMaxLength")] 
    public float activeLength;
    public float inactiveLength;
    /// 전체 레이저 On/Off 사이클의 동작 여부
    public bool isActivate = true;
    public float onTime = 1.0f;
    public float offTime = 1.0f;        //OFF Time이 0일 시 무한
    
    [SerializeField] private ParticleSystem[] onReadyParticles;
    [SerializeField] private ParticleSystem[] onActiveParticles;
    [SerializeField] private Animator topAnim;
    [SerializeField] private Animator midAnim;
    [SerializeField] private Animator botAnim;

    /// 실제 데미지를 가하는 레이저가 동작하고 있는지 여부
    [SerializeField, ReadOnly] private bool lasing = false;
    /// 레이저의 실제 길이
    [SerializeField, ReadOnly] private float length = 0.00f;

    private RaycastHit _hit;

    public bool overrideInvincibleDuration = false;
    [ShowIf("overrideInvincibleDuration")]
    public float ignoreDuration = 2f;

    private CancellationTokenSource _cts = new();

    private void Start()
    {
        Invoke(nameof(ActivateLaser), startDelay);
    }

    private void Update()
    {
        if (lasing)
        {
            var detected = Detect();
            if (detected)
            {
                DoDamageIfItsPlayer(detected);
            }
        }
        
        var spriteSize = new Vector2(laserSprite.size.x, length);
        laserSprite.size = spriteSize;
        topAnim.transform.localPosition = Vector3.up * length;
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

        if (!overrideInvincibleDuration)
            go.GetComponent<PlayerDamageReceiver>().GetDamage(gameObject, 1);
        else
            go.GetComponent<PlayerDamageReceiver>().GetDamage(gameObject, 1, ignoreDuration);
    }

    [Button]
    public void ActivateLaser()
    {
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
            
            await UniTask.WaitForSeconds(offTime - 0.2f, cancellationToken: ctk);
            
            // 레이저 발사 시작
            SetAnimatorsState(1);
            DOTween.To(() => length,
                (x) => { length = x; }, activeLength, 0.1f);
            foreach(var p in onReadyParticles)
                p.Stop();
            foreach(var p in onActiveParticles)
                p.Play();
            
            await UniTask.WaitForSeconds(0.1f, cancellationToken: ctk);
            
            // 레이저 활성화
            lasing = true;
            SetAnimatorsState(2);
            
            await UniTask.WaitForSeconds(onTime, cancellationToken: ctk);
            
            // 레이저 발사 종료 
            SetAnimatorsState(1);
            DOTween.To(() => length,
                (x) => { length = x; }, inactiveLength, 0.1f);
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
        topAnim.SetInteger(hash, state);
        midAnim.SetInteger(hash, state);
        botAnim.SetInteger(hash, state);
    }

    private void OnDestroy()
    {
        InactivateLaser();
    }
}
