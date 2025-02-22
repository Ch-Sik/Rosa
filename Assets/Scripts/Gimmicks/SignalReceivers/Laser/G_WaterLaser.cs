using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G_WaterLaser : GimmickSignalReceiver
{
    public bool showGizmos = true;
    public Transform point;
    public SpriteRenderer sp;
    public LayerMask playerLayerMask;
    public LayerMask obstaclesLayerMask;
    public float startDelay = 0.0f;
    public float laserMaxLength;
    public bool isActivate = true;      //작동 여부 Activate일 때만 사이클이 작동
    public float onTime = 1.0f;
    public float offTime = 1.0f;        //OFF Time이 0일 시 무한

    public bool lasing = false;
    public float length = 0.00f;

    public Transform show1;
    public Transform show2;
    private SpriteRenderer sr1;
    private SpriteRenderer sr2;

    RaycastHit hit;
    Coroutine cor;
    Sequence seq;

    public bool isUseIgnoreDuration = false;
    public float ignoreDuration = 2f;

    private void Start()
    {
        sr1 = show1.GetComponent<SpriteRenderer>();
        sr2 = show2.GetComponent<SpriteRenderer>();

        Invoke("ActivateLaser", startDelay);
    }

    private void Update()
    {
        Vector2 size = new Vector2(sp.size.x, length);

        sp.size = size;
        sr1.size = size;
        sr2.size = size;

        Detect();

        if (!lasing)
        {
            sp.enabled = false;
            return;
        }
        sp.enabled = true;
    }

    [Button]
    public void ActivateLaser()
    {
        isActivate = true;
        if (cor != null)
            StopCoroutine(cor);
        cor = StartCoroutine(Laser());
    }

    [Button]
    public void InactivateLaser()
    {
        isActivate = false;
        lasing = false;
        StopCoroutine(cor);
        seq.Kill();
    }

    IEnumerator Laser()
    {
        lasing = true;
        yield return new WaitForSeconds(onTime);

        seq = DOTween.Sequence()
        .AppendCallback(() =>
        {
            show1.localPosition = new Vector3(0.5f, 0, 0);
            show2.localPosition = new Vector3(-0.5f, 0, 0);
        })
        .Append(show1.DOLocalMoveX(0, offTime).SetEase(Ease.Linear))
        .Join(show2.DOLocalMoveX(0, offTime).SetEase(Ease.Linear))
        .Join(sr1.DOFade(100f / 255f, offTime))
        .Join(sr2.DOFade(100f / 255f, offTime))
        .AppendCallback(() =>
        {
            sr1.DOFade(0, 0);
            sr2.DOFade(0, 0);
        })
        .OnKill(() =>
        {
            sr1.DOFade(0, 0);
            sr2.DOFade(0, 0);
        });

        lasing = false;
        yield return new WaitForSeconds(offTime);

        if (cor != null)
            StopCoroutine(cor);
        cor = StartCoroutine(Laser());
    }

    public void Detect()
    {
        RaycastHit2D hit = Physics2D.Raycast(point.position, point.up, laserMaxLength, obstaclesLayerMask);
        if (hit.collider != null && (obstaclesLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
        {
            if (lasing)
            {
                // 레이캐스트가 어떤 콜라이더와 충돌했을 때
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("플레이어 충돌");
                    //hit.collider.GetComponent<PlayerDamageReceiver>().GetDamage(gameObject, 1);
                    DoDamageIfItsPlayer(hit.collider.gameObject);
                    //RespawnManager.Instance?.Respawn(); //WaterLaser는 리스폰 시키지 않음
                }
                if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Cube"))
                {
                    //Debug.Log("벽과 충돌");
                }
            }
        }

        // Debug.Log(hit.collider != null ? hit.collider.gameObject.name : "");

        if (hit.collider == null)
            length = laserMaxLength;
        else
            length = Vector2.Distance(point.position, hit.point);
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
        Debug.DrawRay(point.position, point.up * laserMaxLength, Color.red);
    }

    private void DoDamageIfItsPlayer(GameObject go)
    {
        if (go.CompareTag("Player"))
        {
            bool isMonsterBody = gameObject.layer.Equals(LayerMask.NameToLayer("Monster"));

            // Debug.Log("damaged");
            if (!isUseIgnoreDuration)
            {
                go.GetComponent<PlayerDamageReceiver>().GetDamage(gameObject, 1, isMonsterBody);
            }
            else
            {
                go.GetComponent<PlayerDamageReceiver>().GetDamage(gameObject, 1, ignoreDuration, isMonsterBody);
            }

        }
    }
}
