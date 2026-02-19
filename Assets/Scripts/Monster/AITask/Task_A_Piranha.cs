using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Panda;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class Task_A_Piranha : MonoBehaviour
{
    [SerializeField] private GameObject piranhaVisual;
    [SerializeField] private ParticleSystem[] waterSplashParticles;
    
    [SerializeField] float breachHeight = 3f;     // 튀어오르는 높이
    [SerializeField] float breachUpTime = 1f;
    [SerializeField] float breachDownTime = 0.7f;
    [SerializeField] float timeBetweenBreach = 1.0f;
    [SerializeField] float startDelay = 0;       // 딜레이를 줘서 다른 피라냐들과 같이 파도타기 구현 가능
    
    [SerializeField] private SFXPlayer breachStartSfx;
    [SerializeField] private SFXPlayer breachEndSfx;
    
    private float _originHeight;

    private Rigidbody2D _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _originHeight = transform.position.y;
        
        StartTween().Forget();
    }

    private async UniTaskVoid StartTween()
    {
        await UniTask.WaitForSeconds(startDelay);
        DOTween.Sequence()
            .AppendCallback(() => { OnBreachStart(); })
            .Append(
                _rigidbody.DOMoveY(transform.position.y + breachHeight, breachUpTime)
                    .SetEase(Ease.OutCubic))
            .Insert(0, transform.DORotate(new Vector3(0, 0, 0), 0.2f))
            .Append(
                _rigidbody.DOMoveY(_originHeight, breachDownTime)
                    .SetEase(Ease.InQuad))
            .Insert(breachUpTime, transform.DORotate(new Vector3(0, 0, 180), 0.2f))
            .AppendCallback(() => { OnBreachEnd(); })
            .AppendInterval(timeBetweenBreach)
            .SetLoops(-1);
    }
    
    private void OnBreachStart()
    {
        piranhaVisual.SetActive(true);
        breachStartSfx?.PlaySfx();
        foreach(var p in waterSplashParticles)
        {
            if(p.isPlaying)
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            p.Play();
        }
    }

    private void OnBreachEnd()
    {
        piranhaVisual.SetActive(false);
        breachEndSfx?.PlaySfx();
        foreach(var p in waterSplashParticles)
        {
            if(p.isPlaying)
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            p.Play();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if(Application.isPlaying)
        {
            Vector3 position = transform.position;
            position.y = _originHeight;
            Gizmos.DrawLine(position, position + new Vector3(0, breachHeight, 0));
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, breachHeight, 0));
        }
    }
}
