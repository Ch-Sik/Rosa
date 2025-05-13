using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CommunicationData
{
    public CommunicationType type;
    [ShowIf("@type == CommunicationType.TargetText || type == CommunicationType.Show || type == CommunicationType.Hide || type == CommunicationType.SetEmotion || type == CommunicationType.WalkTo")]
    public CommunicationTarget target;
    [ShowIf("@type == CommunicationType.SetEmotion")]
    public Emotion emotion;
    [ShowIf("@type == CommunicationType.PlayerText || type == CommunicationType.TargetText"), ReadOnly]
    public string text;                 // 24.12.21.    가독성 개선 목적으로 인스펙터에 노출되게 변경
    [ShowIf("@type == CommunicationType.Show")]
    public CommunicationLocation location;
    [ShowIf("@type == CommunicationType.MoveCameraTo || type == CommunicationType.WalkTo || type == CommunicationType.MoveRoom")]
    public Vector2 position;
    //[ShowIf("@type == CommunicationType.Function")]
    //public UnityEvent function;
    [ShowIf("@type == CommunicationType.Delay")]
    public float delay;
    [ShowIf("@type == CommunicationType.Sfx")]
    public AudioClip sfx;
    [ShowIf("@type == CommunicationType.Flag || type == CommunicationType.UnlockPlayerAction")]
    public string key;
    [ShowIf("@type == CommunicationType.Flag")]
    public int flagValue;
    [ShowIf("@type == CommunicationType.MoveRoom")]
    public SORoom room;
    // 25.05.13) roomPosition의 역할을 position과 통합
    //[ShowIf("@type == CommunicationType.MoveRoom")]
    //public Vector2 roomPosition;

    CommunicationData()
    {
        type = CommunicationType.None;
        target = CommunicationTarget.None;
        emotion = Emotion.Normal;
        text = "";
        location = CommunicationLocation.Left;
        position = Vector2.zero;
        delay = 0;
        sfx = null;
        room = null;
        //roomPosition = Vector2.zero;
    }
}

public enum CommunicationType
{
    None,
    Show,                       //UI창에서 대상을 생성한다.
    Hide,                       //UI창에서 대상을 없앤다.
    SetEmotion,                 //대상의 이모션을 변경한다.
    TargetText,                 //대상의 채팅을 출력한다.
    PlayerText,                 //플레이어의 채팅을 출력한다.
    MoveCameraTo,               //특정 위치로 카메라를 이동시킨다.
    ReturnCameraToPlayer,       //카메라를 원위치 시킨다.
    // 25.05.13) Function 설계가 잘못되어 삭제하고자 하나,그러면 int로 저장된 SO의 CommunicationType 값들이
    // 하나씩 당겨지는 문제가 있어 일단 방치함. 
    Function_DO_NOT_USE,        //특정 함수를 작동시킨다.
    Delay,                      //커뮤니케이션에 딜레이를 준다.
    Sfx,                        //특정 소리를 발생시킨다.
    Flag,                       //플래그를 변경한다.
    HideAll,                    // 24.12.22) 화면 상에 보이는 모든 대상을 '동시에' 숨긴다.
    MoveRoom,                   //특정 룸으로 이동시킨다.
    WalkTo,                     // 25.04.19) 캐릭터를 설정한 x좌표까지 걷게 한다.
    UnlockPlayerAction,         // 25.05.12) 캐릭터의 특정 액션을 해금한다.
}

public enum CommunicationTarget
{
    None,
    Player,
    Healer,
    Healer_noName,
    Healer_boss,
    Salamander,
    Salamander_noName,
    Watchmaker,
    Watchmaker_noName,
    Bear,
    Crane,
    Wolf,
    A,
    B,
    C
}

public enum CommunicationLocation
{
    Left,
    Right
}

public enum Emotion
{
    Normal,
    Happy1,
    Happy2,
    Embarrassed1,
    Embarrassed2,
    Serious1,
    Serious2,
    Sad,
    Sigh,
    Mad,
    Special
}
