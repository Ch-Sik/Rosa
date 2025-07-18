using Com.LuisPedroFonseca.ProCamera2D;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickSignalConnector : MonoBehaviour
{
    [SerializeField] bool useSave;
    [SerializeField] string saveKey;
    [Tooltip("SignalConnector에서 시네마틱 사용이 ON이더라도, 각 SignalReceiver에서 시네마틱 사용이 설정되어있어야 함")]
    [SerializeField] bool useCinematic = true;

    public List<GimmickSignalReceiver> gimmicks = new List<GimmickSignalReceiver>();
    public List<GimmickSignalSender> signals = new List<GimmickSignalSender>();

    [ReadOnly] public bool isActive = false;
    // true에서 true로 업데이트 될 때 이벤트 발생하는 것을 방지하기 위해,
    // 기존 isActive값 보관
    private bool curIsActive = false;
    ProCamera2DCinematics cinematicsComponent;

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

        if(useCinematic)
        {
            cinematicsComponent = Camera.main.GetComponent<ProCamera2DCinematics>();
        }

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

        // 25.06.29) 플래그 키값 주어지지 않았을 때에도 작동은 되도록 내결함성 개선
        if (saveKey != null && saveKey.Length > 0)
        {
            // 25.05.27) 세이브/로드를 위해 플래그 연동 추가
            FlagManager.Instance.SetFlag(saveKey, isActive ? 1 : 0);
        }
        else
        {
            Debug.LogWarning("기믹에 세이브를 위한 키가 할당되어있지 않음! 작동한 기믹이 제대로 저장되지 않을 수 있음");
        }

        if (isActive) OnAct();
        else OffAct();
    }

    [Button("Force Gimmicks ON")]
    private void OnAct()
    {
        for (int i = 0; i < gimmicks.Count; i++)
            gimmicks[i].OnAct();
        if (useCinematic)
            ShowCinematic();
    }

    [Button("Force Gimmicks OFF")]
    private void OffAct()
    {
        for(int i = 0; i < gimmicks.Count; i++)
            gimmicks[i].OffAct();
    }

    private void ShowCinematic()
    {
        if(cinematicsComponent == null)
        {
            Debug.LogError("ProCamera2DCinematics 컴포넌트를 찾을 수 없음!");
            return;
        }

        // 기존 시네마틱 타겟이 있다면 클리어
        cinematicsComponent.CinematicTargets.Clear();

        // 설정된 SignalReceiver중에 시네마틱 On으로 되어있는 녀석들을 추가
        foreach(var signalReceiver in gimmicks)
        {
            if(signalReceiver.useCinematic)
                cinematicsComponent.AddCinematicTarget(
                    signalReceiver.transform,
                    signalReceiver.cinematicsSetting.easeInDur, 
                    signalReceiver.cinematicsSetting.holdDur,
                    signalReceiver.cinematicsSetting.zoomAmount);
        }

        // 시네마틱 연출 실행
        cinematicsComponent.Play();
        Debug.Log("시네마틱 연출 실행됨");
    }

    // 25.07.18)
    // 불필요한 함수들 주석 처리
    //public void ImmediateSignal()
    //{
    //    for (int i = 0; i < signals.Count; i++)
    //        if (!signals[i].isInteractable)
    //        {
    //            isActive = false;
    //            ImmediateChangeState();
    //            return;
    //        }

    //    isActive = true;
    //    ImmediateChangeState();
    //    return;
    //}

    //public void ImmediateChangeState()
    //{
    //    if (curIsActive == isActive)
    //        return;

    //    curIsActive = isActive;

    //    if (isActive) ImmediateOnAct();
    //    else ImmediateOffAct();
    //}

    //public void ImmediateOnAct()
    //{
    //    for (int i = 0; i < gimmicks.Count; i++)
    //        gimmicks[i].ImmediateOnAct();
    //}

    //public void ImmediateOffAct()
    //{
    //    for (int i = 0; i < gimmicks.Count; i++)
    //        gimmicks[i].ImmediateOffAct();
    //}
}
