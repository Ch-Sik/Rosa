using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickSignalConnector : MonoBehaviour
{
    [SerializeField] bool useSave;
    [SerializeField] string saveKey;

    public List<GimmickSignalReceiver> gimmicks = new List<GimmickSignalReceiver>();
    public List<GimmickSignalSender> signals = new List<GimmickSignalSender>();

    public bool isActive = false;
    // true에서 true로 업데이트 될 때 이벤트 발생하는 것을 방지하기 위해,
    // 기존 isActive값 보관
    private bool curIsActive = false;   

    public void Awake()
    {
        gameObject.tag = "Connector";
        Init();
    }

    private void Init()
    {
        // 필수 필드값 초기화
        for (int i = 0; i < signals.Count; i++)
            signals[i]?.SetHandler(this);

        if (useSave)
        {
            if (saveKey == null || saveKey.Length == 0)
                Debug.LogError($"{gameObject.name}: 세이브에 사용할 플래그 키값 미지정됨");

            // 이 타이밍에 FlagManager.instance가 초기화되어있어야 정상임
            if (FlagManager.Instance == null)
            {
                Debug.LogError("FlagManager가 초기화되어있지 않음!!!!");
            }
            // 플래그 읽어와서 1이라면 sender와 receiver 모두 '작동'시켜줘야 함
            int flag = FlagManager.Instance.GetFlag(saveKey);
            if (flag != 0)
            {
                foreach (var receiver in gimmicks)
                    receiver?.ImmediateOnAct();
                foreach (var sender in signals)
                    sender?.Init(GimmickSignalSenderState.Activated);
            }
            else
            {
                foreach (var receiver in gimmicks)
                    receiver?.ImmediateOffAct();
                foreach (var sender in signals)
                    sender?.Init(GimmickSignalSenderState.Inactivated);
            }
        }
        else
        {
            for (int i = 0; i < gimmicks.Count; i++)
                gimmicks[i]?.ImmediateOffAct();
            foreach (var sender in signals)
                sender?.Init(GimmickSignalSenderState.Inactivated);
        }
    }

    public bool OnSignal()
    {
        for (int i = 0; i < signals.Count; i++)
            if (signals[i].GetState() == GimmickSignalSenderState.Inactivated)
            {
                isActive = false;
                ChangeState();
                return false;
            }

        isActive = true;
        ChangeState();
        return true;
    }

    public void ChangeState()
    {
        if (curIsActive == isActive)
            return;

        curIsActive = isActive;

        // 25.05.27) 세이브/로드를 위해 플래그 연동 추가
        FlagManager.Instance.SetFlag(saveKey, isActive? 1 : 0);

        if (isActive) OnAct();
        else OffAct();
    }

    public void OnAct()
    {
        for (int i = 0; i < gimmicks.Count; i++)
            gimmicks[i].OnAct();
    }

    public void OffAct()
    {
        for(int i = 0; i < gimmicks.Count; i++)
            gimmicks[i].OffAct();
    }

    public void ImmediateSignal()
    {
        for (int i = 0; i < signals.Count; i++)
            if (!signals[i].isInteractable)
            {
                isActive = false;
                ImmediateChangeState();
            }

        isActive = true;
        ImmediateChangeState();
    }

    public void ImmediateChangeState()
    {
        if (curIsActive == isActive)
            return;

        curIsActive = isActive;

        if (isActive) ImmediateOnAct();
        else ImmediateOffAct();
    }

    public void ImmediateOnAct()
    {
        for (int i = 0; i < gimmicks.Count; i++)
            gimmicks[i].ImmediateOnAct();
    }

    public void ImmediateOffAct()
    {
        for (int i = 0; i < gimmicks.Count; i++)
            gimmicks[i].ImmediateOffAct();
    }
}
