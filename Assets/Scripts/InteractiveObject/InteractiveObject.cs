using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [Tooltip("제자리에서 쿨타임만 차면 반복 상호작용 허용 여부")]
    public bool allowRepeatInteract = true;
    [HideIf("autoInteract")]
    public float coolDown = 0f;

    public UnityEvent function;
    public GameObject interactiveKeyUI;

    Collider2D col;
    private float lastInterationTime = -10000f;

    private void Start()
    {
        if (interactiveKeyUI != null)
            interactiveKeyUI.SetActive(false);

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
        PlayerRef.Instance.controller.SetInteraction(OnInteration);
    }

    private void OnInteration()
    {
        if(Time.time - lastInterationTime < coolDown)
        {
            Debug.Log("쿨타임이라서 상호작용할 수 없음");
            return;
        }

        function.Invoke();
        lastInterationTime = Time.time;
        OnInactive();
        if(allowRepeatInteract)
            DOVirtual.DelayedCall(coolDown, () => { OnActive(); });
    }

    private void OnActive() 
    {
        if (!autoInteract && interactiveKeyUI != null)
            interactiveKeyUI.SetActive(true);
    }

    public void OnInactive()
    {
        RemoveEvent();
        if(interactiveKeyUI != null)
            interactiveKeyUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canInteract)
            return;

        if (collision.tag != "Player")
            return;

        OnActive();
        if (autoInteract && !RespawnHandler.Instance.IsDoingRespawn)
            function.Invoke();
        else
            // autoInteract가 아니면 플레이어 '상호작용' 입력 이벤트에 function 예약
            SetEvent();
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

    [Button("상호작용 테스트")]
    private void TestInteraction()
    {
        function.Invoke();
    }
}
