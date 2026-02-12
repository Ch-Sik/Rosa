using System;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

    public delegate void CommunicationEvent(int id);
    public CommunicationEvent OnCommunicationStart;
    public CommunicationEvent OnCommunicationFinish;

    // 25.04.22) 대화 ID 보관하도록 수정
    int communicationID;

    //캐릭터 스프라이트 입력 그룹
    public List<CharacterEmotion> characterDatas = new List<CharacterEmotion>();
    //캐릭터 스프라이트 매치 그룹
    [ShowInInspector] 
    public Dictionary<CommunicationTarget, CharacterEmotion> characters = new Dictionary<CommunicationTarget, CharacterEmotion>();
    [ShowInInspector, InfoBox("프로젝트 내에 존재하는 모든 대화 데이터.\nStart 시에 알아서 스캔해서 가져오므로 수동 설정할 필요 없음.")] 
    public Dictionary<int, CommunicationSO> communicationDatas = new Dictionary<int, CommunicationSO>();

    public string folderName = "Dialogue";                          //폴더이름 수식
    public float endDelay = 1.5f;                                   //종료 딜레이
    public CommunicationUI UI;                                      //UI관리
    public CommunicationTextLanguage language;                                   //게임 언어 수식
    public int curIndex = 0;                                               //전역으로 사용할 반복자
    public bool isTalking = false;                                  //말하는 중인지 파악
    public bool isCommunicating = false;                            //대화 중인지 파악
    public CommunicationTarget curTarget;                           //현재 대화중인 대상

    Tween moveTween;                                                //스킵을 위한 트윈포인터

    List<CommunicationData> data = new List<CommunicationData>();   //communication data 수식

    // 25.04.19 추가
    // 캐릭터 움직이게 하기 위한 참조
    [ShowInInspector] public Dictionary<CommunicationTarget, NpcMovement> npcMovements = new Dictionary<CommunicationTarget, NpcMovement>();

    //CSV 파싱 뜰 데이터
    public List<Dictionary<string, object>> CSV = new List<Dictionary<string, object>>();
    public List<string> textData = new List<string>();                     //CSV 파싱 후 텍스트 데이터만 받음
    private int _textCount;                                                  //text의 Count 파악
    private bool _isDoingSkipSequence = false;


    private void Start()
    {
        //시작과 동시에 Dictionary에 데이터 정리
        LoadCommunicationAsDict();
        GetCharacterSpriteDatas();
    }

    public void ReadyForCommunication()
    {
        SetInputStateToCommunicationMode();
    }

    private void SetInputStateToCommunicationMode()
    {
        Debug.Log("[CommunicationManager] SetInputStateToCommunicationMode");
        InputManager.Instance.SetMoveInputState(PlayerMoveState.NO_MOVE);
        InputManager.Instance.SetUiInputState(UiState.DIALOG);
    }

    private void ResetInputState()
    {
        Debug.Log("[CommunicationManager] ResetInputState");
        InputManager.Instance.SetMoveInputState(PlayerMoveState.DEFAULT);
        InputManager.Instance.SetUiInputState(UiState.IN_GAME);
    }

    private void Update()
    {
        //커뮤니케이션 중이고,
        if (!isCommunicating)
            return;

        //텍스팅 중이라면,
        if (!isTalking)
            return;

        // ESC 키로 스킵
        if (Input.GetKeyDown(KeyCode.Escape))
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
                UI.EarlyDone(data[curIndex].text);
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
        if (curIndex + 1 >= data.Count)
            return true;

        if (data[curIndex + 1].type == CommunicationType.TargetText ||
            data[curIndex + 1].type == CommunicationType.PlayerText)
            return false;

        return true;
    }

    //모든 데이터를 리셋한다.
    public void ResetDatas()
    {
        data = null;
        curTarget = CommunicationTarget.None;
        isTalking = false;
        curIndex = 0;
        CSV.Clear();
        textData.Clear();
        _textCount = 0;
    }

    [Button]
    //Communication 시작 전 작업
    public void StartCommunication(int ID)
    {
        WaitAndStartCommunication(ID).Forget();
    }

    private async UniTaskVoid WaitAndStartCommunication(int ID)
    {
        // 대화가 겹칠 경우, 앞선 대화의 EndCommunication에서 '대화 모드'가 해제되어버리므로
        // 다음 대화를 시작하기 전에 다시 세팅해줘야함.
        ReadyForCommunication();
        
        if (!communicationDatas.ContainsKey(ID))
        {
            communicationID = -1;
            return;
        }

        communicationID = ID;
        CommunicationSO data = communicationDatas[communicationID];

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
        _textCount = GetTextCount();

        //유효성 테스트에 false가 나오면 커뮤니케이션은 실행되지 않는다.
        if (!Validation())
            return;

        //기본 UI의 제거
        //커뮤니케이션 UI의 생성
        float time = UI.Initialize();

        // NPC가 자동으로 플레이어 바라보는 기능 비활성화
        NpcLookatPlayer.EnableGlobally = false;

        //시작
        await UniTask.WaitForSeconds(time);
        StartCommunicationInternal();
    }

    //커뮤니케이션을 실행시킨다.
    private void StartCommunicationInternal()
    {
        // 25.04.22) 이벤트 관리 추가
        // 25.06.05) 이벤트 수행 타이밍을 대화를 위해 지정된 위치로 이동하기 시작하는 시점에서 
        // 이동 후 실제 대화를 시작하는 타이밍으로 이동
        OnCommunicationStart?.Invoke(communicationID);

        isCommunicating = true;
        Debug.Log($"대화 {communicationID} 시작");
        Communication();
    }

    //커뮤니케이션을 종료시킨다.
    public void EndCommunication()
    {
        Debug.Log($"대화 {communicationID} 종료");

        // 카메라 리셋
        // ProCamera2D.Instance.CenterOnTargets();
        ProCamera2D.Instance.FollowHorizontal = true;
        ProCamera2D.Instance.FollowVertical = true;

        //ResetDatas();
        UI.EndAnimation();
        //기존 UI의 생성

        // NPC가 자동으로 플레이어 바라보는 기능 복구
        NpcLookatPlayer.EnableGlobally = true;
        //조작시작
        ResetInputState();

        OnCommunicationFinish?.Invoke(communicationID);
        isCommunicating = false;
    }

    // 25.06.05) 함수 설명용 주석 수정
    // Scriptable Object에 기록된 '대화 데이터' 하나하나를 처리하는 커뮤니케이션 함수
    // Target text 또는 Player text가 아니라면 Communication - 임의의 함수 - Next - Communication 식으로 
    // 스택이 계속 쌓이는 문제가 있긴 한데...
    // Target text 또는 Player text가 나오면 리턴되면서 스택 해소되므로 
    // 걍 냅두기로 함.
    public void Communication()
    {
        if (data == null)
        {
            Debug.LogError("[CommunicationManager] data is null");
            return;
        }
        
        // 24.12.22) CommunicationType이 None이면 무시하고 다음으로 넘김
        while (curIndex < data.Count && data[curIndex].type == CommunicationType.None)
            curIndex++;

        //끝 판독
        if (curIndex >= data.Count)
        {
            EndCommunication();
            return;
        }

        HandleCurCommunication(data[curIndex]);
    }

    void HandleCurCommunication(CommunicationData curData)
    {
        CommunicationTarget target = curData.target;
        
        switch (curData.type)
        {
            case CommunicationType.None: /* 아무것도 안함 */ break;
            case CommunicationType.Show: Show(target, curData.location); break;
            case CommunicationType.Hide: Hide(target); break;
            case CommunicationType.SetEmotion: SetEmotion(target, curData.emotion); break;
            case CommunicationType.TargetText: TargetText(target, curData.text); break;
            case CommunicationType.PlayerText: TargetText(CommunicationTarget.Player, curData.text); break;
            case CommunicationType.MoveCameraTo: MoveCameraTo(curData.position); break;
            case CommunicationType.ReturnCameraToPlayer: ReturnCameraToPlayer(); break;
            // 25.05.13) CommunicationManager에서 임의 함수를 호출할 수 있는 기능 삭제
            //case CommunicationType.Function: Function(data[i].function); break;
            case CommunicationType.Function_DO_NOT_USE: 
                Debug.LogError("CommunicationType.Function 사용 금지!");
                throw new NotImplementedException();
                break;
            case CommunicationType.Delay: DelayAndGoNext(curData.delay); break;
            case CommunicationType.Sfx: Sfx(curData.sfx); break;
            case CommunicationType.Flag: SetFlag(curData.key, curData.flagValue); break;
            case CommunicationType.HideAll: HideAll(); break;
            case CommunicationType.MoveRoom: 
                MoveRoomAndSave(curData.room, curData.position).Forget();
                break;
            case CommunicationType.WalkTo: WalkTo(target, curData.position); break;
            case CommunicationType.UnlockPlayerAction: UnlockPlayerAction(curData.key); break;
            case CommunicationType.DisappearNPC: DisappearNPC(target); break;
            case CommunicationType.ActivavteObjectWithTag: ActivateChildrenOfObjectWithTag(curData.key); break;
        }
    }

    #region CommunicationFunction

    public async UniTaskVoid Skip()
    {
        if (_isDoingSkipSequence)
        {
            Debug.Log("[CommunicationManager] 스킵 중복 실행 차단됨");
            return;
        }
        _isDoingSkipSequence = true;
        
        // 26.02.07 페이드 아웃 연출 추가
        FadeoutPanel.Fadeout();

        await UniTask.WaitForSeconds(0.5f);
        
        CommunicationData moveRoom = null;
        // 25.10.12) 스킵시에도 방 이동이나 능력 획득은 정상적으로 되게 수정
        while (++curIndex < data.Count)
        {
            switch (data[curIndex].type)
            {
                // moveRoom을 먼저해버리면 이동한 방에서의 다음 대화 시작이 현재 대화의 종료보다 먼저 시작되어서 문제 발생
                // moveRoom은 나중에 하도록 잠시 데이터 보관
                case CommunicationType.MoveRoom:
                    moveRoom = data[curIndex];
                    break;
                case CommunicationType.UnlockPlayerAction:
                case CommunicationType.Flag:
                case CommunicationType.DisappearNPC:
                case CommunicationType.ActivavteObjectWithTag:
                    HandleCurCommunication(data[curIndex]);
                    break;
                case CommunicationType.WalkTo:
                    var curData = data[curIndex];
                    SkipWalkTo(curData.target, curData.position);
                    break;
            }
        }
        
        EndCommunication();
        await UniTask.WaitForSeconds(1.0f);
        isTalking = false;
        
        // 방 이동이 있을 경우 이동된 방에서 다음 대화 자동진행될 것을 고려, await 생략
        if (moveRoom != null)
        {
            MoveRoomAndSave(moveRoom.room, moveRoom.position).Forget();
        }
        else
        {
            // 방 이동이 있을 경우 FadeIn 두번 호출되어 너무 일찍 화면 표시되는 것 방지,
            // 방 이동이 없을 경우에만 FadeIn 호출
            FadeoutPanel.FadeIn();
        }
        
        _isDoingSkipSequence = false;
    }

    //Show 처리
    public void Show(CommunicationTarget target, CommunicationLocation location)
    {
        //보여주는 시간을 리턴받고,
        float time = UI.ShowTarget(target, location);
        //딜레이를 제공한다.
        DelayAndGoNext(time);
    }

    //Hide 처리
    public void Hide(CommunicationTarget target)
    {
        //사라지게 하는 시간을 리턴받고,
        float time = UI.HideTarget(target);
        //딜레이를 제공한다.
        // 26.01.13) Hide 직후 Show 시에 트윈 끝단이 겹쳐서
        // Image 컴포넌트가 inactive되는 문제로 딜레이에 0.1초 추가
        DelayAndGoNext(time + 0.1f);
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
        DelayAndGoNext(delay);
    }

    //ResetCamera 처리
    public void ReturnCameraToPlayer()
    {
        // ProCamera2D.Instance.CenterOnTargets();
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
    public void DelayAndGoNext(float time)
    {
        Invoke("Next", time);
    }

    //Sfx 처리
    public void Sfx(AudioClip clip)
    { 

    }

    public void SetFlag(string key, int value)
    {
        FlagManager.Instance.SetFlag(key, value);
        Next();
    }

    //24.12.22) 한꺼번에 숨기기 추가
    public void HideAll()
    {
        //사라지게 하는 시간을 리턴받고,
        float time = UI.HideAll();
        //딜레이를 제공한다.
        DelayAndGoNext(time);
    }

    public async UniTaskVoid MoveRoomAndSave(SORoom room, Vector3 pos)
    {
        await MoveRoom(room, pos);
        SaveLoadManager.Instance.SavePlayData();
        // Next();
    }

    //룸의 특정 위치로 이동
    public async UniTask MoveRoom(SORoom room, Vector3 pos)
    {
        await MapManager.Instance.Enter(room, pos);
    }

    // 25.04.19) NPC가 특정 위치까지 걷기
    // 25.04.21) 플레이어도 동일한 방식으로 움직일 수 있도록 추가
    public void WalkTo(CommunicationTarget targetCharacter, Vector2 pos)
    {
        float t = 0;        // 걷는데 필요한 예상 시간
        if (targetCharacter == CommunicationTarget.Player)
        {
            float moveDist = pos.x - PlayerRef.Instance.transform.position.x;
            t = moveDist / PlayerRef.Instance.movement.MoveSpeed;
            PlayerRef.Instance.movement.isMovingByScript = true;
            PlayerRef.Instance.movement.Walk(Vector2.right * (moveDist > 0 ? 1 : -1));
            PlayerRef.Instance.animation.anim.SetBool("isWalking", true);
            DOVirtual.DelayedCall(t, () =>
            {
                PlayerRef.Instance.movement.isMovingByScript = false;
                PlayerRef.Instance.movement.Walk(Vector2.zero);
                PlayerRef.Instance.animation.anim.SetBool("isWalking", false);
            });
        }
        else
        {
            t = npcMovements[targetCharacter].MoveTo(pos.x);
        }
        DelayAndGoNext(t);
    }

    public void SkipWalkTo(CommunicationTarget targetCharacter, Vector2 pos)
    {
        if (targetCharacter == CommunicationTarget.Player)
        {
            PlayerRef.Instance.transform.position = pos;
        }
        else
        {
            npcMovements[targetCharacter].MoveTo(pos.x);
        }
    }

    public void DisappearNPC(CommunicationTarget targetCharacter)
    {
        npcMovements[targetCharacter].Disappear();
        Next();
    }

    // 25.05.13) 플레이어 액션 해금 기능 추가
    public void UnlockPlayerAction(string actionToUnlock)
    {
        switch(actionToUnlock)
        {
            case "Dash":
                PlayerRef.Instance.movement.EnableDash();
                break;
            case "MushJump":
                PlayerRef.Instance.movement.EnableMushJump();
                break;
            case "Gliding":
                PlayerRef.Instance.movement.EnableGliding();
                break;
            default:
                Debug.LogError("CommunicationManager.UnlockPlayerAction) 잘못된 키값 들어옴");
                break;
        }
        Next();
    }

    // 26.01.13) 챕터3 연출용으로 추가
    public void ActivateChildrenOfObjectWithTag(string key)
    {
        var objects = GameObject.FindGameObjectsWithTag(key);
        foreach (var obj in objects)
        {
            for(int i=0; i<obj.transform.childCount; i++)
            {
                obj.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        Next();
    }

    //다음 커뮤니케이션 실행
    public void Next()
    {
        curIndex++;
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
        {
            try
            {
                communicationDatas.Add(arr[i].ID, arr[i]);
            }
            catch {
                Debug.LogError("CommunicationManager: 딕셔너리에 대화 데이터 입력 실패. 아마도 중복된 키값(ID) 때문\n"
                    + $"ID: {arr[i].ID}, textFileName: {arr[i].textFileName}");
            }
        }

        Debug.Log("CommunicationManager: 대화 정보 로드 완료");
    }

    //List 형태로 관리되고 있는 데이터를 Dictionary형태로 전환함
    public void GetCharacterSpriteDatas()
    {
        for (int i = 0; i < characterDatas.Count; i++)
            if (!characters.ContainsKey(characterDatas[i].target))
                characters.Add(characterDatas[i].target, characterDatas[i].DeepCopy());
        Debug.Log("CommunicationManager: 캐릭터 초상화 정보 로드 완료");
    }

    //현재 설정된 언어를 추적하여 언어에 따른 키를 반환함.
    public string GetKey()
    {
        switch (language)
        {
            case CommunicationTextLanguage.KOR: return "KOR";
            case CommunicationTextLanguage.ENG: return "ENG";
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

        if (index == _textCount)
        {
            return true;
        }
        else
        {
            Debug.Log("[Communication Error] CSV 파일과 CommuncationSO의 매칭을 실패했습니다.");
            Debug.Log("현재 언어 : " + language + " Data Text : " + _textCount + "CSV Text : " + textData.Count);
            isCommunicating = false;
            return false;
        }
    }
    #endregion
}

public enum CommunicationTextLanguage
{
    KOR,
    ENG
}