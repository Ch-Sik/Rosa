using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class InteractiveObject : MonoBehaviour
{
    public bool showGizmos = false;

    [FormerlySerializedAs("canUse")]
    public bool canInteract = true;
    [Tooltip("유효거리 내로 다가오면 자동으로 상호작용 진행 여부")]
    public bool autoInteract = false;

    Collider2D col;
    public UnityEvent function;
    public GameObject interactiveKeyUI;

    private void Start()
    {
        interactiveKeyUI?.SetActive(false);

        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void ActivateInteraction()
    {
        this.col.enabled = true;
    }

    public void InactiveInteraction()
    {
        this.col.enabled = false;
    }

    public void RemoveEvent()
    {
        PlayerRef.Instance.controller.ResetInteraction();
    }

    public void SetEvent()
    {
        if (!canInteract)
            return;

        PlayerRef.Instance.controller.ResetInteraction();
        PlayerRef.Instance.controller.SetInteraction(() => function.Invoke());
    }

    private void OnActive() 
    {
        interactiveKeyUI?.SetActive(true);
    }

    public void OnInactive()
    {
        interactiveKeyUI?.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canInteract)
            return;

        if (collision.tag != "Player")
            return;

        OnActive();
        if (autoInteract)
        {
            // auto interact라면 function 바로 실행
            function.Invoke();
        }
        else
        {
            // 아니면 플레이어 '상호작용' 입력 이벤트에 function 예약
            SetEvent();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!canInteract)
            return;

        if (collision.tag != "Player")
            return;

        OnInactive();
        if (autoInteract)
        {
            // Do nothing
        }
        else
        {
            RemoveEvent();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;
        Gizmos.color = Color.green;
    }
}
