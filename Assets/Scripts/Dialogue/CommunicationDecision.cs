using System.Collections.Generic;
using System;

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
