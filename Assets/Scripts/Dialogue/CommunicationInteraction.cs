using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class CommunicationInteraction : MonoBehaviour
{
    public float delayAfterWalk = 0.3f;
    public CommunicationDecision decision;

    [SerializeField] private Transform communicationStartTransform;
    private bool isPlayerMoving = false;
    [HideInInspector] public int ID;

    private void Start()
    {
        communicationStartTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandlePlayerMove();
    }

    private void HandlePlayerMove()
    {
        if (!isPlayerMoving) return;

        if(communicationStartTransform == null)
        {
            DOVirtual.DelayedCall(delayAfterWalk, StartCommunication);
            return;
        }

        if (Mathf.Abs(PlayerRef.Instance.transform.position.x - communicationStartTransform.position.x) > 0.2f)
        {
            int direction = PlayerRef.Instance.transform.position.x - communicationStartTransform.position.x < 0 ? 1 : -1;
            PlayerRef.Instance.movement.Walk(Vector2.one * direction);
            PlayerRef.Instance.animation.anim.SetBool("isWalking", true);
        }
        else
        {
            PlayerRef.Instance.animation.anim.SetBool("isWalking", false);
            PlayerRef.Instance.movement.Walk(Vector2.zero);
            PlayerRef.Instance.movement.LookAt2D(transform.position);
            isPlayerMoving = false;

            // Invoke("StartCommunication", delay);
            DOVirtual.DelayedCall(delayAfterWalk, StartCommunication);
        }
    }

    [Button]
    public void ReadyForCommunication()
    {
        ID = decision.GetID();

        // ID가 애초에 없음(0 이하)로 설정된 경우 리턴
        if (ID <= 0)
        {
            Debug.Log($"설정된 대화 데이터가 없음 (ID: {ID})");
            return;
        }

        // ID에 해당하는 대화 데이터가 없는 경우 리턴
        if (!CommunicationManager.Instance.HaveCommunicationID(ID))
        {
            Debug.LogError($"ID {ID}에 해당하는 대화 데이터가 없음");
            return;
        }

        isPlayerMoving = true;

        InputManager.Instance.SetMoveInputState(PlayerMoveState.NO_MOVE);
        InputManager.Instance.SetUiInputState(UiState.DIALOG);
    }

    public void StartCommunication()
    {
        //ID에 따라 수행
        CommunicationManager.Instance.StartCommunication(ID);
    }
}

[Serializable]
public class CommunicationDecision
{
    // 첫 대화일 때 사용될 대화 ID
    public int initialID = -1;
    // 이후 반복대화일 때 사용될 대화 ID
    public int iterativeID = -1;
    public List<CommunicationDecisionNode> flagedID = new List<CommunicationDecisionNode>();

    public int GetID()
    {
        if (initialID > 0)      // '첫 대화' ID가 존재한다면 첫 대화인지 판단
        {
            if (GetIterativeCommunicationFlagValue() == 0)
            {
                SetIterativeCommunicationFlagValue(1);
                if (initialID >= 0)
                    return initialID;
            }
        }

        // 첫 대화가 아니고 조건 대화 플래그 섰을 때
        foreach (var flagID in flagedID)
        {
            int id = flagID.IsAvailable();

            if (id != -1)
                return id;
        }

        // 모두 해당 안되면 반복 대화 ID 리턴
        return iterativeID;
    }

    public int GetIterativeCommunicationFlagValue()
    {
        string key = GetFlagKeyString();
        return FlagManager.Instance.GetFlag(key);
    }

    public void SetIterativeCommunicationFlagValue(int value = 1)
    {
        string key = GetFlagKeyString();
        FlagManager.Instance.SetFlag(key, value);
    }

    string GetFlagKeyString()
    {
        return "IterativeCommunication" + initialID.ToString();
    }
}

[Serializable]
public class CommunicationDecisionNode
{
    public int IsAvailable()
    {
        //일회용 대화이고, 이미 소진되었다면,
        if (isOnce && isUsed)
            return -1;

        for (int i = 0; i < requireFlags.Count; i++)
        {
            if (FlagManager.Instance.GetFlag(requireFlags[i].Key) != requireFlags[i].Value)
                return -1;
        }

        isUsed = true;
        return ID;
    }

    public bool isOnce = true;
    bool isUsed = false;
    public List<KeyValuePair<string, int>> requireFlags = new List<KeyValuePair<string, int>>();
    public int ID;
}