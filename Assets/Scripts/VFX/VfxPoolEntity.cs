using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class VfxPoolEntity : MonoBehaviour
{
    [InfoBox("스폰된 VFX가 일정시간 후 알아서 오브젝트 풀로 돌아가는 것 담당하는 컴포넌트")]
    [SerializeField] int maxPoolSize = 5;
    [SerializeField] float minReleaseTime = 1f;

    public int MaxPoolSize { get => maxPoolSize; }
    IObjectPool<VfxPoolEntity> _poolToReturn;
    float _releaseTime;
    private bool _initialized = false;

    public void Init(IObjectPool<VfxPoolEntity> pool)
    {
        _poolToReturn = pool;
        UpdateReleaseTime();
    }
    
    private void UpdateReleaseTime()
    {
        _releaseTime = minReleaseTime;

        // 파티클 시스템 가져와서 적당한 releaseTime 설정
        // 전제 조건
        // 1. Emission over time이 아닌, Burst로만 구성된 파티클 효과임을 가정
        // 2. lifeTime이 커브로 주어졌을 때, 두 키프레임 사이의 evaluate 결과가
        //    두 키프레임에서의 evaluate 결과보다 큰 케이스까지는 고려하지 않음. (귀찮)
        var allParticleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (var p in allParticleSystems)
        {   
            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[p.emission.burstCount];
            p.emission.GetBursts(bursts);
            
            // 26.01.27) burst 없이 rate over time만 있는 파티클 예외 처리
            if (bursts.Length <= 0)
                continue;
            
            float lastBurstTime = bursts.Max((e) => { return e.time; });
            float particleLifetime = 1f;
            switch (p.main.startLifetime.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    particleLifetime = p.main.startLifetime.constant;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    particleLifetime = p.main.startLifetime.constantMax;
                    break;
                case ParticleSystemCurveMode.Curve:
                    particleLifetime = p.main.startLifetime.curve.keys.Max((k) => { return k.value; });
                    break;
                case ParticleSystemCurveMode.TwoCurves:
                    particleLifetime = p.main.startLifetime.curveMax.keys.Max((k) => { return k.value; });
                    break;
            }

            _releaseTime = Mathf.Max(_releaseTime, lastBurstTime + particleLifetime);
        }

        _initialized = true;
        Debug.Log($"[VfxPoolEntity] {gameObject.name}의 releaseTime을 {_releaseTime}으로 설정");
    }

    public void OnGet()
    {
        DelayAndRelease(_releaseTime);
    }

    private async UniTaskVoid DelayAndRelease(float delay)
    {
        await UniTask.WaitForSeconds(delay);
        _poolToReturn.Release(this);
    }
}
