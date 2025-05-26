using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GimmickSignalReceiver : MonoBehaviour
{
    protected bool isOnAct = false;

    private void Awake()
    {
        gameObject.tag = "Receiver";
    }

    //모든 Trigger가 On일 때 작동할 함수
    public abstract void OnAct();
    public abstract void ImmediateOnAct();
    //모든 Trigger가 Off일 때 작동할 함수
    public abstract void OffAct();
    public abstract void ImmediateOffAct();
}
