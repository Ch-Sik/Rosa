using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CommunicationZone : MonoBehaviour
{
    public bool showGizmos = false;

    public float delayAfterWalk = 0.3f;
    public CommunicationDecision decision;
    [HideInInspector] public int ID = -1;

    [SerializeField] private Transform communicationStartTransform;
    private bool isPlayerMoving = false;

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    private void Update()
    {
        HandlePlayerMove();
    }

    private void HandlePlayerMove()
    {
        if (!isPlayerMoving) return;

        if (communicationStartTransform == null)
        {
            DOVirtual.DelayedCall(delayAfterWalk, StartCommunication);
            return;
        }

        // 25.04.22) communicationStartTransform이 없어도 동작하도록 수정
        if (communicationStartTransform != null 
            && Mathf.Abs(PlayerRef.Instance.transform.position.x - communicationStartTransform.position.x) > 0.2f)
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

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        ReadyForCommunication();
    }

    private void ReadyForCommunication()
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

        InputManager.Instance.SetMoveInputState(PlayerMoveState.NO_MOVE);
        InputManager.Instance.SetUiInputState(UiState.DIALOG);
    }

    private void StartCommunication()
    {
        CommunicationManager.Instance.StartCommunication(ID);
    }
}
