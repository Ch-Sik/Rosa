using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class InteractiveObject : MonoBehaviour
{
    public bool showGizmos = false;

    public bool canUse = true;

    Collider2D col;
    public UnityEvent function;
    public GameObject interactiveKeyUI;

    private void Start()
    {
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
        if (!canUse)
            return;

        PlayerRef.Instance.controller.ResetInteraction();
        PlayerRef.Instance.controller.SetInteraction(() => function.Invoke());
    }

    private void OnActive() 
    {
        interactiveKeyUI.SetActive(true);
    }

    public void OnInactive()
    {
        interactiveKeyUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canUse)
            return;

        if (collision.tag != "Player")
            return;

        OnActive();
        SetEvent();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!canUse)
            return;

        if (collision.tag != "Player")
            return;

        OnInactive();
        RemoveEvent();
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;
        Gizmos.color = Color.green;
    }
}
