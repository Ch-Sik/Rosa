using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LastBossAoe : MonoBehaviour
{
    [SerializeField] private Collider2D col;
    [SerializeField] private Animator anim;
    [SerializeField] private float activeDuration;
    [SerializeField] private ParticleSystem[] particles;
    
    private int _stateNameHash;
    private Sequence _colorTween = null;
    
    private void Start()
    {
        col.enabled = false;
        _stateNameHash = Animator.StringToHash("State");
        foreach (var p in particles)
        {
            var shape = p.shape;
            shape.radius *= transform.localScale.x;
        }
    }
    
    public void Activate()
    {
        ActivateInternal().Forget();
    }

    private async UniTaskVoid ActivateInternal()
    {
        col.enabled = true;
        anim.SetInteger(_stateNameHash, 1);
        foreach(var p in particles)
            p.Play();
        
        await UniTask.WaitForSeconds(activeDuration);
        anim.SetInteger(_stateNameHash, 0);
        col.enabled = false;
        await UniTask.WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public void CancelAOE()
    {
        anim.SetInteger(_stateNameHash, -1);
    }
}
