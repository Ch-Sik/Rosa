using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommunicationZone : MonoBehaviour
{
    public bool showGizmos = false;
    public CommunicationDecision decision;
    [HideInInspector] public int ID = -1;

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

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

        InputManager.Instance.SetMoveInputState(PlayerMoveState.NO_MOVE);
        InputManager.Instance.SetUiInputState(UiState.DIALOG);
        CommunicationManager.Instance.StartCommunication(ID);
    }
}
