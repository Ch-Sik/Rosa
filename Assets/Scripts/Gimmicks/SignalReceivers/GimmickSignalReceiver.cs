using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GimmickSignalReceiver : MonoBehaviour
{
    protected bool isOnAct = false;
    public bool useCinematic = true;
    [ShowIf("useCinematic")] public cinematicsSetting cinematicsSetting;

    private void Awake()
    {
        gameObject.tag = "Receiver";
    }

    /// <summary>
    /// 모든 Trigger가 On일 때 작동할 함수
    /// </summary>
    public abstract void OnAct();
    /// <summary>
    /// 애니메이션이나 카메라 연출 등을 사용하지 않고 즉시 On 상태로 변환
    /// </summary>
    public abstract void ImmediateOnAct();
    /// <summary>
    /// 모든 Trigger가 Off일 때 작동할 함수
    /// </summary>
    public abstract void OffAct();
    /// <summary>
    /// 애니메이션이나 카메라 연출 등을 사용하지 않고 즉시 Off 상태로 변환
    /// </summary>
    public abstract void ImmediateOffAct();
}
