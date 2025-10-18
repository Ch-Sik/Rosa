using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using DG.Tweening;
using Com.LuisPedroFonseca.ProCamera2D;

public class BossRoomManager : MonoBehaviour
{
    [SerializeField] 
    Blackboard bossBlackboard;

    [SerializeField, FoldoutGroup("보스전 BGM 관련")] 
    AudioClip[] bgmClip;           // 각 페이즈 별 브금 클립
    [SerializeField, FoldoutGroup("보스전 BGM 관련")] 
    float bgmReturnDelay = 3f;     // 보스 사망 후 기본 BGM으로 돌아올 때까지의 딜레이

    [SerializeField, FoldoutGroup("보스전 시작 대화")] 
    bool useIntroCommunication;
    [SerializeField, FoldoutGroup("보스전 시작 대화"), ShowIf("useIntroCommunication")] 
    PandaBehaviour bossAI;
    [SerializeField, FoldoutGroup("보스전 시작 대화"), ShowIf("useIntroCommunication")]
    [Tooltip("임의의 대화가 끝났을 때 그 대화가 이 값과 같다면 보스를 활성화함.")]
    int IntroCommunication_ID;
    [Tooltip("대화 끝난 후 몇 초 후에 보스가 움직이도록 할 것인지")]
    [SerializeField, FoldoutGroup("보스전 시작 대화"), ShowIf("useIntroCommunication")]
    float IntroCommunication_BossActivateDelay;

    [SerializeField, FoldoutGroup("보스전 끝 대화")]
    bool useOutroCommunication;
    [SerializeField, FoldoutGroup("보스전 끝 대화"), ShowIf("useOutroCommunication")]
    int OutroCommunication_ID;
    [Tooltip("보스 사망 후 몇 초 후에 대화가 자동으로 뜨게 할 것인지")]
    [SerializeField, FoldoutGroup("보스전 끝 대화"), ShowIf("useOutroCommunication")]
    float OutroCommunication_StartDelay;

    [SerializeField, FoldoutGroup("보스방 벽 관련")]
    bool requireWallActivation;
    [SerializeField, FoldoutGroup("보스방 벽 관련"), ShowIf("requireWallActivation")]
    bool disableWallRenderer;       
    [SerializeField, FoldoutGroup("보스방 벽 관련"), ShowIf("requireWallActivation")]
    GameObject bossRoomWall;

    [SerializeField, FoldoutGroup("보스방 카메라 관련")]
    ProCamera2DTriggerBoundaries bossRoomCameraTrigger;
    [SerializeField, FoldoutGroup("보스방 카메라 관련")]
    ProCamera2DTriggerBoundaries resetCameraTrigger;

    [SerializeField, FoldoutGroup("보스방 탈출 방지 관련")]
    RoomManager roomManager;

    BGMPlayer bgmPlayer;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(bossBlackboard != null);
        bossBlackboard.OnBlackboardUpdated += OnBossBlackboardUpdated;

        if (bossRoomCameraTrigger)
            bossRoomCameraTrigger.gameObject.SetActive(false);
        if (resetCameraTrigger)
            resetCameraTrigger.gameObject.SetActive(false);
        if (roomManager)
            roomManager.InactiveTriggers();     // 플레이어 보스방에서 탈출 방지

        if(useIntroCommunication)
        {
            Debug.Assert(bossAI != null);
            bossAI.enabled = false;
            CommunicationManager.Instance.OnCommunicationFinish += OnIntroCommunicationFinish;
        }
    }

    void OnIntroCommunicationFinish(int communicationID)
    {
        if(communicationID == IntroCommunication_ID)
        {
            if (bossRoomCameraTrigger)
                bossRoomCameraTrigger.gameObject.SetActive(true);
            DOVirtual.DelayedCall(IntroCommunication_BossActivateDelay, ActivateBoss);
            CommunicationManager.Instance.OnCommunicationFinish -= OnIntroCommunicationFinish;
        }
    }

    void ActivateBoss()
    {
        bossAI.enabled = true;
        if (requireWallActivation)
        {
            bossRoomWall.SetActive(true);
            // 투명벽
            if (disableWallRenderer)
            {
                var previews = bossRoomWall.GetComponentsInChildren<SpriteRenderer>();
                foreach (var preview in previews)
                {
                    preview.enabled = false;
                }
            }
        }
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
        // 브금 재생
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
        // SORoom에 정의된 각 방의 기본 BGM으로 복구
        DOVirtual.DelayedCall(bgmReturnDelay, () => { bgmPlayer.PlayRoomBGM(); });

        // 카메라 해방
        if (bossRoomCameraTrigger)
            bossRoomCameraTrigger.gameObject.SetActive(false);
        if(resetCameraTrigger)
            resetCameraTrigger.gameObject.SetActive(true);

        // 보스 사망 후 대화
        if (useOutroCommunication)
        {
            DOVirtual.DelayedCall(OutroCommunication_StartDelay, () =>
            {
                CommunicationManager.Instance.ReadyForCommunication();
                CommunicationManager.Instance.StartCommunication(OutroCommunication_ID);
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
        CommunicationManager.Instance.OnCommunicationFinish -= OnIntroCommunicationFinish;
    }
}
