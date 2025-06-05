using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using DG.Tweening;

public class BossRoomManager : MonoBehaviour
{
    [SerializeField] Blackboard bossBlackboard;
    [SerializeField] AudioClip[] bgmClip;       // 각 페이즈 별 브금 클립

    [SerializeField, FoldoutGroup("보스전 시작 대화")] 
    bool doEngageCommunication;
    [SerializeField, FoldoutGroup("보스전 시작 대화"), ShowIf("doEngageCommunication")] 
    PandaBehaviour bossAI;
    [SerializeField, FoldoutGroup("보스전 시작 대화"), ShowIf("doEngageCommunication")]
    int engageCommunicationID;
    [Tooltip("대화 끝난 후 몇 초 후에 보스가 움직이도록 할 것인지")]
    [SerializeField, FoldoutGroup("보스전 시작 대화"), ShowIf("doEngageCommunication")]
    float engageCommunicationEndDelay;

    [SerializeField, FoldoutGroup("보스전 끝 대화")]
    bool doFinishCommunication;
    [SerializeField, FoldoutGroup("보스전 끝 대화"), ShowIf("doFinishCommunication")]
    int finishCommunicationID;
    [Tooltip("보스 사망 후 몇 초 후에 대화가 자동으로 뜨게 할 것인지")]
    [SerializeField, FoldoutGroup("보스전 끝 대화"), ShowIf("doFinishCommunication")]
    float finishCommunicationStartDelay;

    BGMPlayer bgmPlayer;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(bossBlackboard != null);
        bossBlackboard.OnBlackboardUpdated += OnBossBlackboardUpdated;

        if(doEngageCommunication)
        {
            Debug.Assert(bossAI != null);
            bossAI.enabled = false;
            CommunicationManager.Instance.OnCommunicationFinish += OnBossroomEnterCommunicationFinish;
        }
    }

    void OnBossroomEnterCommunicationFinish(int communicationID)
    {
        if(communicationID == engageCommunicationID)
        {
            DOVirtual.DelayedCall(engageCommunicationEndDelay, ActivateBoss);
            CommunicationManager.Instance.OnCommunicationFinish -= OnBossroomEnterCommunicationFinish;
        }
    }

    void ActivateBoss()
    {
        bossAI.enabled = true;
    }

    // 보스의 상태가 변화되었을 때 호출.
    void OnBossBlackboardUpdated(string key, object value)
    {
        switch(key)
        {
            case BBK.Enemy:
                if ((GameObject)value != null)
                {
                    OnPlayerEnteredBossRoom();
                }
                break;
            case BBK.CurrentPhase:
                OnBossPhaseChanged((int)value);
                break;
            case BBK.isDead:
                OnBossDead();
                break;
            default: 
                // 아무것도 안함
                break;
        }
    }

    void OnPlayerEnteredBossRoom()
    {
        if(bgmPlayer == null)
        {
            bgmPlayer = Camera.main.gameObject.GetComponentInChildren<BGMPlayer>();
        }
        bgmPlayer.PlayBGM(bgmClip[0]);
    }

    void OnBossPhaseChanged(int phase)
    {
        if(phase < bgmClip.Length && bgmClip[phase] != null)
        {
            bgmPlayer.PlayBGM(bgmClip[phase]);
        }
    }

    void OnBossDead()
    {
        bgmPlayer.PlayDefaultBGM();
        if(doFinishCommunication)
        {
            DOVirtual.DelayedCall(finishCommunicationStartDelay, () => {
                CommunicationManager.Instance.ReadyForCommunication();
                CommunicationManager.Instance.StartCommunication(finishCommunicationID);
            });
        }
    }

    [Button("테스트: 보스 즉시 사망")]
    void Test_KillBossImmediatly()
    {
        bossBlackboard.gameObject.GetComponent<MonsterState>().TakeDamage(999);
    }

    void OnDestroy()
    {
        // 혹시나 보스룸 들락날락했을 때
        // 이벤트 리스너 중복등록되거나 이미 destroy된 객체의 등록이 남아있는 경우 방지용
        CommunicationManager.Instance.OnCommunicationFinish -= OnBossroomEnterCommunicationFinish;
    }
}
