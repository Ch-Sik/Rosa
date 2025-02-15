using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCCommunicationInteraction : MonoBehaviour
{
    public float delay = 1f;
    public CommunicationDecision decision;

    [SerializeField] private Transform communicationStartTransform;
    private bool isGo = false;
    [HideInInspector] public int ID;

    private void Start()
    {
        communicationStartTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isGo)
            return;

        if (Mathf.Abs(PlayerRef.Instance.transform.position.x - communicationStartTransform.position.x) > 0.3f)
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
            isGo = false;

            Invoke("ChooseCommunication", delay);
        }
    }

    [Button]
    public void StartCommunication()
    {
        ID = decision.GetID();

        //ID가 없는 경우 리턴
        if (!CommunicationManager.Instance.HaveCommunicationID(ID))
        {
            Debug.LogWarning("해당하는 ID의 대화가 없음");
            return;
        }

        isGo = true;

        InputManager.Instance.SetMoveInputState(PlayerMoveState.NO_MOVE);
        InputManager.Instance.SetUiInputState(UiState.DIALOG);
    }

    public void ChooseCommunication()
    {
        //ID에 따라 수행
        CommunicationManager.Instance.StartCommunication(ID);
    }
}

[Serializable]
public class CommunicationDecision
{
    //첫 대화인지 파악
    [HideInInspector] public bool isFirst = true;
    public int initialID = -1;
    public int iterativeID = -1;
    public List<CommunicationDecisionNode> flagedID = new List<CommunicationDecisionNode>();

    public int GetID()
    {
        //첫 대화
        if (isFirst)
        {
            isFirst = false;
            if (initialID >= 0)
                return initialID;
        }

        foreach (var flagID in flagedID)
        {
            int id = flagID.IsAvailable();

            if (id != -1)
                return id;
        }

        //반복 대화
        return iterativeID;
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

        for (int i = 0; i < flags.Count; i++)
        {
            if (FlagManager.Instance.GetFlag(flags[i].flag) != flags[i].value)
                return -1;
        }

        isUsed = true;
        return ID;
    }

    public bool isOnce = true;
    bool isUsed = false;
    public List<Flag> flags = new List<Flag>();
    public int ID;
}