using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LastBossAoe : MonoBehaviour
{
    [SerializeField] private Collider2D col;
    [SerializeField] private Animator anim;
    [SerializeField] private float activeDuration;

    private int stateNameHash;
    
    private void Start()
    {
        col.enabled = false;
        stateNameHash = Animator.StringToHash("State");
    }
    
    public void Activate()
    {
        ActivateInternal().Forget();
    }

    private async UniTaskVoid ActivateInternal()
    {
        col.enabled = true;
        anim.SetInteger(stateNameHash, 1);
        await UniTask.WaitForSeconds(activeDuration);
        anim.SetInteger(stateNameHash, 0);
        col.enabled = false;
        await UniTask.WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
