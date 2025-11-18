using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class WalkToInteraction : MonoBehaviour
{
    public float delayAfterWalk = 0.3f;

    [FormerlySerializedAs("communicationStartTransform")] 
    [SerializeField] private Transform WalkToTransform;
    private bool isPlayerMoving = false;
    [SerializeField] private bool lookThisAfterWalk = true;
    [ShowIf("@!lookThisAfterWalk")]
    [SerializeField] private LR afterLookAtDir;

    private void Start()
    {
        if(WalkToTransform)
            WalkToTransform.gameObject.SetActive(false);
    }

    [Button]
    public void StartWalking()
    {
        isPlayerMoving = true;
        OnStartWalk();
    }

    protected virtual void OnStartWalk() { }

    private void Update()
    {
        HandlePlayerMove();
    }

    private void HandlePlayerMove()
    {
        if (!isPlayerMoving) return;

        if(WalkToTransform == null)
        {
            DOVirtual.DelayedCall(delayAfterWalk, OnAfterWalk);
            isPlayerMoving = false;
            return;
        }

        if (Mathf.Abs(PlayerRef.Instance.transform.position.x - WalkToTransform.position.x) > 0.2f)
        {
            int direction = PlayerRef.Instance.transform.position.x - WalkToTransform.position.x < 0 ? 1 : -1;
            PlayerRef.Instance.movement.Walk(Vector2.one * direction);
            PlayerRef.Instance.animation.anim.SetBool("isWalking", true);
        }
        else
        {
            PlayerRef.Instance.animation.anim.SetBool("isWalking", false);
            PlayerRef.Instance.movement.Walk(Vector2.zero);
            if(lookThisAfterWalk)
                PlayerRef.Instance.movement.LookAt2D(transform.position);
            else
                PlayerRef.Instance.movement.LookAt2DLocal(afterLookAtDir.toVector2());
            isPlayerMoving = false;

            // Invoke("StartCommunication", delay);
            DOVirtual.DelayedCall(delayAfterWalk, OnAfterWalk);
        }
    }

    protected virtual void OnAfterWalk() { }
}