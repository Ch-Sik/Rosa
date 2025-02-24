using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 커뮤니케이션을 담당하는 싱글턴 함수부
/// 
/// SetDatas를 통해 실행 가능
/// 
/// *** 경고 ***
/// MoveToPosition, ReturnToPosition, Sfx, Function에 대한 테스팅 필요,
/// 입력에 대한 통일 필요
/// </summary>

public class CommunicationManager : MonoBehaviour
{
    private static CommunicationManager instance;
    public static CommunicationManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //캐릭터 스프라이트 입력 그룹
    public List<CharacterEmotion> characterDatas = new List<CharacterEmotion>();
    //캐릭터 스프라이트 매치 그룹
    [ShowInInspector] public Dictionary<CommunicationTarget, CharacterEmotion> characters = new Dictionary<CommunicationTarget, CharacterEmotion>();
    [ShowInInspector] public Dictionary<int, CommunicationSO> communicationDatas = new Dictionary<int, CommunicationSO>();

    public string folderName = "Dialogue";                          //폴더이름 수식
    public float endDelay = 1.5f;                                   //종료 딜레이
    public CommunicationUI UI;                                      //UI관리
    public TestLanguage language;                                   //게임 언어 수식
    public int i = 0;                                               //전역으로 사용할 반복자
    public bool isTalking = false;                                  //말하는 중인지 파악
    public bool isCommunicating = false;                            //대화 중인지 파악
    public CommunicationTarget curTarget;                           //현재 대화중인 대상

    Tween moveTween;                                                //스킵을 위한 트윈포인터

    List<CommunicationData> data = new List<CommunicationData>();   //communication data 수식

    //CSV 파싱 뜰 데이터
    public List<Dictionary<string, object>> CSV = new List<Dictionary<string, object>>();
    public List<string> textData = new List<string>();                     //CSV 파싱 후 텍스트 데이터만 받음
    int textCount;                                                  //text의 Count 파악


    private void Start()
    {
        //시작과 동시에 Dictionary에 데이터 정리
        LoadCommunicationAsDict();
        GetCharacterSpriteDatas();
    }

    private void Update()
    {
        //커뮤니케이션 중이고,
        if (!isCommunicating)
            return;

        //텍스팅 중이라면,
        if (!isTalking)
            return;

        //F를 입력받으면, 스킵한다.
        if (Input.GetKeyDown(KeyCode.F))
        {
            Skip();
            return;
        }

        //그렇지 않고, 아무키나 입력받으면, 
        if (Input.anyKeyDown)
        {
            //만약 UI에서 텍스팅 중이라면, 빠르게 종료
            if (UI.isTalking)
            {
                UI.EarlyDone(data[i].text);
            }
            //만약 UI에서 텍스팅 중이 아니라면, 다음 커뮤니케이션을 살펴서 UI를 관리하고, 다음 커뮤니케이션으로 이동한다.
            else
            {
                UI.EarlyDone("", FlexibleTextingHelper());
                isTalking = false;
                Next();
            }
        }
    }

    //현재 대화 중일 때, 다음 턴에도 대화가 예상된다면 UI관리 Boolean형 데이터를 전달한다.
    public bool FlexibleTextingHelper()
    {
        if (i + 1 > data.Count)
            return true;

        if (data[i + 1].type == CommunicationType.TargetText ||
            data[i + 1].type == CommunicationType.PlayerText)
            return false;

        return true;
    }

    //모든 데이터를 리셋한다.
    public void ResetDatas()
    {
        data = null;
        isCommunicating = false;
        curTarget = CommunicationTarget.None;
        isTalking = false;
        i = 0;
        CSV.Clear();
        textData.Clear();
        textCount = 0;
    }

    private void ResetPlayerState()
    {
        InputManager.Instance.SetMoveInputState(PlayerMoveState.DEFAULT);
        InputManager.Instance.SetUiInputState(UiState.IN_GAME);
    }

    [Button]
    //Communication 시작 전 작업
    public bool StartCommunication(int ID)
    {
        if (!communicationDatas.ContainsKey(ID))
            return false;

        CommunicationSO data = communicationDatas[ID];

        //데이터 리셋
        ResetDatas();

        //커뮤니케이션 데이터 추출
        this.data = new List<CommunicationData>(data.data);

        //CSV 파싱 및 텍스트 파일 추출
        CSV = CSVReader.Read(folderName, data.textFileName);
        string key = GetKey();
        for (int i = 0; i < CSV.Count; i++)
            textData.Add(CSV[i][key].ToString());
        //Data에 있는 파일의 출력 개수 파악
        textCount = GetTextCount();

        //유효성 테스트에 false가 나오면 커뮤니케이션은 실행되지 않는다.
        if (!Validation())
            return false;

        //조작중단
        //기본 UI의 제거
        //커뮤니케이션 UI의 생성
        float time = UI.StartAnimation();

        //시작
        Invoke("StartCommunication", time);

        return true;
    }

    //커뮤니케이션을 실행시킨다.
    private void StartCommunication()
    {
        isCommunicating = true;
        Communication();
    }

    //커뮤니케이션을 종료시킨다.
    public void EndCommunication()
    {
        ReturnCameraToPlayer();
        ResetDatas();
        UI.EndAnimation();
        //기존 UI의 생성
        //조작시작
        ResetPlayerState();
    }

    //계속해서 무한 while하는 커뮤니케이션 함수
    public void Communication()
    {
        // 24.12.22) CommunicationType이 None이면 무시하고 다음으로 넘김
        while (i < data.Count && data[i].type == CommunicationType.None)
            i++;

        //끝 판독
        if (i >= data.Count)
        {
            EndCommunication();
            return;
        }

        //타겟 리셋 및 타겟 추적
        CommunicationTarget target = CommunicationTarget.None;
        if (data[i].type == CommunicationType.TargetText ||
            data[i].type == CommunicationType.Show ||
            data[i].type == CommunicationType.Hide ||
            data[i].type == CommunicationType.SetEmotion)
        {
            target = data[i].target;
        }

        //커뮤니케이션 타입에 따른 함수에 파라미터 전달
        switch (data[i].type)
        {
            case CommunicationType.Show: Show(target, data[i].location); return;
            case CommunicationType.Hide: Hide(target); return;
            case CommunicationType.SetEmotion: SetEmotion(target, data[i].emotion); return;
            case CommunicationType.TargetText: TargetText(target, data[i].text); return;
            case CommunicationType.PlayerText: TargetText(CommunicationTarget.Player, data[i].text); return;
            case CommunicationType.MoveCameraTo: MoveCameraTo(data[i].position); return;
            case CommunicationType.ReturnCameraToPlayer: ReturnCameraToPlayer(); return;
            case CommunicationType.Function: Function(data[i].function); return;
            case CommunicationType.Delay: Delay(data[i].delay); return;
            case CommunicationType.Sfx: Sfx(data[i].sfx); return;
            case CommunicationType.Flag: SetFlag(data[i].flag); return;
            case CommunicationType.HideAll: HideAll(); return;
            case CommunicationType.MoveRoom: MoveRoom(data[i].room, data[i].roomPosition); return;
        }
    }

    #region CommunicationFunction
    //F를 입력받아 스킵할 때의 함수
    public void Skip()
    {
        EndCommunication();
        isTalking = false;
        isCommunicating = false;
    }

    //Show 처리
    public void Show(CommunicationTarget target, CommunicationLocation location)
    {
        //보여주는 시간을 리턴받고,
        float time = UI.ShowTarget(target, location);
        //딜레이를 제공한다.
        Delay(time);
    }

    //Hide 처리
    public void Hide(CommunicationTarget target)
    {
        //사라지게 하는 시간을 리턴받고,
        float time = UI.HideTarget(target);
        //딜레이를 제공한다.
        Delay(time);
    }

    //SetEmotion 처리
    public void SetEmotion(CommunicationTarget target, Emotion emotion)
    {
        //표정을 변경하고,
        UI.SetEmotion(target, emotion);
        //다음 커뮤니케이션을 실행시킨다.
        Next();
    }

    //TargetText 처리
    public void TargetText(CommunicationTarget target, string text)
    {
        //대화 시작으로 설정
        isTalking = true;
        //Target을 추적해서, Texting 효과를 준다.
        UI.Texting(target, text);
    }

    //MoveCameraTo 처리
    public void MoveCameraTo(Vector2 pos)
    {
        //거리 비례 딜레이를 받을 변수
        float delay = 0.0f;
        //카메라 수식
        Camera camera = Camera.main;
        //거리 계산
        float distance = Vector2.Distance(pos, camera.transform.position);
        //거리에 따른 지연 계산
        delay = distance * 0.025f;
        //카메라 이동
        moveTween = camera.transform.DOMove(new Vector3(pos.x, pos.y, camera.transform.position.z), delay)
                                    .OnStart(() =>
                                    {
                                        ProCamera2D.Instance.FollowHorizontal = false;
                                        ProCamera2D.Instance.FollowVertical = false;
                                    });
        //딜레이
        Delay(delay);
    }

    //ResetCamera 처리
    public void ReturnCameraToPlayer()
    {
        ProCamera2D.Instance.CenterOnTargets();
        ProCamera2D.Instance.FollowHorizontal = true;
        ProCamera2D.Instance.FollowVertical = true;

        Next();
    }

    //Function 처리
    public void Function(UnityEvent func)
    {
        //함수 이벤트가 있다면, 실행
        if (func != null)
            func.Invoke();

        Next();
    }

    //딜레이 후에 다음 커뮤니케이션 실행
    public void Delay(float time)
    {
        Invoke("Next", time);
    }

    //Sfx 처리
    public void Sfx(AudioClip clip)
    { 

    }

    public void SetFlag(Flag flag)
    {
        FlagManager.Instance.SetFlag(flag);
    }

    //24.12.22) 한꺼번에 숨기기 추가
    public void HideAll()
    {
        //사라지게 하는 시간을 리턴받고,
        float time = UI.HideAll();
        //딜레이를 제공한다.
        Delay(time);
    }

    //룸의 특정 위치로 이동
    public void MoveRoom(SORoom room, Vector3 pos)
    {
        MapManager.Instance.OpenScene(room, pos);
        Next();
    }

    //다음 커뮤니케이션 실행
    public void Next()
    {
        i++;
        Communication();
    }
    #endregion

    #region DataControl
    public bool HaveCommunicationID(int ID)
    {
        return communicationDatas.ContainsKey(ID);
    }

    //Load Data
    public void LoadCommunicationAsDict()
    {
        communicationDatas = new Dictionary<int, CommunicationSO>();
        CommunicationSO[] arr = Resources.LoadAll<CommunicationSO>(folderName).ToArray();

        for (int i = 0; i < arr.Length; i++)
            communicationDatas.Add(arr[i].ID, arr[i]);
    }

    //List 형태로 관리되고 있는 데이터를 Dictionary형태로 전환함
    public void GetCharacterSpriteDatas()
    {
        for (int i = 0; i < characterDatas.Count; i++)
            if (!characters.ContainsKey(characterDatas[i].target))
                characters.Add(characterDatas[i].target, characterDatas[i].DeepCopy());
    }

    //현재 설정된 언어를 추적하여 언어에 따른 키를 반환함.
    public string GetKey()
    {
        switch (language)
        {
            case TestLanguage.KOR: return "KOR";
            case TestLanguage.ENG: return "ENG";
            default: return "ENG";
        }
    }

    //유효한 텍스트의 개수를 반환함.
    public int GetTextCount()
    {
        int count = 0;
        for (int i = 0; i < data.Count; i++)
            if (data[i].type == CommunicationType.TargetText || data[i].type == CommunicationType.PlayerText)
                count++;

        return count;
    }

    //텍스트의 개수와 커뮤니케이션에서 텍스트의 개수를 비교대조하고, 텍스트데이터를 설정해줌.
    public bool Validation()
    {
        int index = 0;
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].type == CommunicationType.TargetText ||
                data[i].type == CommunicationType.PlayerText)
            {
                data[i].text = textData[index];
                index++;
            }
        }

        if (index == textCount)
        {
            return true;
        }
        else
        {
            Debug.Log("[Communication Error] CSV 파일과 CommuncationSO의 매칭을 실패했습니다.");
            Debug.Log("현재 언어 : " + language + " Data Text : " + textCount + "CSV Text : " + textData.Count);
            isCommunicating = false;
            return false;
        }

        return false;
    }
    #endregion
}



[Serializable]
public class CommunicationData
{
    public CommunicationType type;
    [ShowIf("@type == CommunicationType.TargetText || type == CommunicationType.Show || type == CommunicationType.Hide || type == CommunicationType.SetEmotion")]
    public CommunicationTarget target;
    [ShowIf("@type == CommunicationType.SetEmotion")]
    public Emotion emotion;
    [ShowIf("@type == CommunicationType.PlayerText || type == CommunicationType.TargetText"), ReadOnly]
    public string text;                 // 24.12.21.    가독성 개선 목적으로 인스펙터에 노출되게 변경
    [ShowIf("@type == CommunicationType.Show")]
    public CommunicationLocation location;
    [ShowIf("@type == CommunicationType.MoveCameraTo")]
    public Vector2 position;
    [ShowIf("@type == CommunicationType.Function")]
    public UnityEvent function;
    [ShowIf("@type == CommunicationType.Delay")]
    public float delay;
    [ShowIf("@type == CommunicationType.Sfx")]
    public AudioClip sfx;
    [ShowIf("@type == CommunicationType.Flag")]
    public Flag flag;
    [ShowIf("@type == CommunicationType.MoveRoom")]
    public SORoom room;
    [ShowIf("@type == CommunicationType.MoveRoom")]
    public Vector2 roomPosition;

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
    Function,                   //특정 함수를 작동시킨다.
    Delay,                      //커뮤니케이션에 딜레이를 준다.
    Sfx,                        //특정 소리를 발생시킨다.
    Flag,                       //플래그를 변경한다.
    HideAll,                    // 24.12.22) 화면 상에 보이는 모든 대상을 '동시에' 숨긴다.
    MoveRoom,                   //특정 룸으로 이동시킨다.

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

[Serializable]
public class CharacterEmotion
{
    public CommunicationTarget target;
    public string Name;
    public Sprite Normal;
    public Sprite Happy;
    public Sprite Mad;
    public Sprite GetEmotionImage(Emotion emotion)
    {
        switch (emotion)
        {
            case Emotion.Normal: return Normal;
            case Emotion.Happy: return Happy;
            case Emotion.Mad: return Mad;
            default: return Normal;
        }
    }

    public CharacterEmotion DeepCopy()
    {
        return new CharacterEmotion()
        {
            Name = this.Name,
            target = this.target,
            Normal = this.Normal,
            Happy = this.Happy,
            Mad = this.Mad,
        };
    }
}

public enum Emotion
{
    Normal,
    Happy,
    Mad
}

public enum TestLanguage
{
    KOR,
    ENG
}