using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GimmickSignalSender : MonoBehaviour
{
    public bool isInteractable = false;             // 플레이어가 조작 가능한지 여부
    private GimmickSignalConnector handler;
    [SerializeField] 
    private GimmickSignalSenderState state = GimmickSignalSenderState.Inactivated;     // 신호를 발생시키고 있는지 여부

    private void Awake()
    {
        tag = "Sender";
    }

    public abstract void Init(GimmickSignalSenderState state);

    //GimmickSignalHandler에 넣을 시, 이벤트 송신을 위한 Init 
    public void SetHandler(GimmickSignalConnector hander) { this.handler = hander; }

    //상태 변화 시 SendSignal을 통해 Signal 점검
    public void SendSignal()
    {
        if (handler != null)
        {
            handler.OnSignal();
            Debug.Log($"{gameObject.name}: 기믹 시그널 발생");
        }
        else
        {
            Debug.LogWarning("시그널 발생 조건은 만족하였으나, 시그널을 전달할 대상이 설정되지 않음");
        }
    }

    // 25.07.18)
    // 불필요한 함수 주석 처리
    //public void ImmediateSendSignal()
    //{
    //    Debug.Log("전송");
    //    if (handler != null)
    //        handler.ImmediateSignal();
    //}

    public GimmickSignalSenderState GetState()
    {
        return state;
    }

    public void SetState(GimmickSignalSenderState state)
    {
        this.state = state;
        //이외 상태에 따른 변경
    }
}
