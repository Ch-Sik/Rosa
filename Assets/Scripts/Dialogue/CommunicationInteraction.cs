using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CommunicationInteraction : MonoBehaviour
{
    public float delayAfterWalk = 0.3f;
    public CommunicationDecision decision;

    [SerializeField] private Transform communicationStartTransform;
    private bool isPlayerMoving = false;
    [HideInInspector] public int ID;

    private void Start()
    {
        communicationStartTransform.gameObject.SetActive(false);
    }

    [Button]
    public void ReadyForCommunication()
    {
        ID = decision.GetID();

        // ID가 애초에 없음(0 이하)로 설정된 경우 리턴
        if (ID <= 0)
        {
            Debug.Log($"설정된 대화 데이터가 없음 (ID: {ID})");
            return;
        }

        // ID에 해당하는 대화 데이터가 없는 경우 리턴
        if (!CommunicationManager.Instance.HaveCommunicationID(ID))
        {
            Debug.LogError($"ID {ID}에 해당하는 대화 데이터가 없음");
            return;
        }

        isPlayerMoving = true;
        CommunicationManager.Instance.ReadyForCommunication();
    }

    private void Update()
    {
        HandlePlayerMove();
    }

    private void HandlePlayerMove()
    {
        if (!isPlayerMoving) return;

        if(communicationStartTransform == null)
        {
            DOVirtual.DelayedCall(delayAfterWalk, StartCommunication);
            isPlayerMoving = false;
            return;
        }

        if (Mathf.Abs(PlayerRef.Instance.transform.position.x - communicationStartTransform.position.x) > 0.2f)
        {
            int direction = PlayerRef.Instance.transform.position.x - communicationStartTransform.position.x < 0 ? 1 : -1;
            PlayerRef.Instance.movement.Walk(Vector2.one * direction);
            PlayerRef.Instance.animation.anim.SetBool("isWalking", true);
        }
        else
        {
            PlayerRef.Instance.animation.anim.SetBool("isWalking", false);
            PlayerRef.Instance.movement.Walk(Vector2.zero);
            PlayerRef.Instance.movement.LookAt2D(transform.position);
            isPlayerMoving = false;

            // Invoke("StartCommunication", delay);
            DOVirtual.DelayedCall(delayAfterWalk, StartCommunication);
        }
    }

    public void StartCommunication()
    {
        //ID에 따라 수행
        CommunicationManager.Instance.StartCommunication(ID);
    }
}