using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CommunicationInteraction : WalkToInteraction
{
    public CommunicationDecision decision;
    [HideInInspector] public int ID;

    protected override void OnStartWalk()
    {
        ReadyForCommunication();
    }

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

        
        CommunicationManager.Instance.ReadyForCommunication();
    }
    
    protected override void OnAfterWalk()
    {
        //ID에 따라 수행
        CommunicationManager.Instance.StartCommunication(ID);
    }
}
