using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity에서 제공하는 Input System를 사용하기 편하게 InputState로 추상화.
/// 현재의 InputState에 해당하는 InputActionMap들만 활성화하고 나머지는 비활성화함.
/// 이를 통해 Player를 참조하지 않고도, UI 상태에서 Player input을 block할 수 있음.
/// </summary>
public class InputManager : MonoBehaviour
{
    // 싱글톤
    private static InputManager _instance;
    public static InputManager Instance => _instance;

    // InputAction Asset
    public InputActionAsset _inputAsset;

    // Input ActionMaps
    // 움직임 관련
    public InputActionMap AM_MoveDefault;
    public InputActionMap AM_MoveClimb;
    
    // UI 조작 관련
    public InputActionMap AM_UiInGame;
    public InputActionMap AM_UiDialogue;
    public InputActionMap AM_UiMenu;
    
    // 상태
    [SerializeField, ReadOnly] public PlayerMoveState _moveState = PlayerMoveState.DEFAULT;
    [SerializeField, ReadOnly] public PlayerActionState _actionState = PlayerActionState.DEFAULT;
    [SerializeField, ReadOnly] private UiState _uiState = UiState.IN_GAME;

    private void Awake()
    {
        _instance = this;
        InitInput();
    }

    private void InitInput()
    {
        // Find Action Maps

        // 플레이어 움직임 관련 Action Map
        {
            AM_MoveDefault = _inputAsset.FindActionMap("MoveDefault");
            AM_MoveClimb = _inputAsset.FindActionMap("Climb");
            //AM_MoveSuperDashReady = _inputAsset.FindActionMap("SuperDashReady");
            //AM_MoveSuperDash = _inputAsset.FindActionMap("SuperDash");
        }
        // 일시정지, 메뉴, 상점 등 UI 관련 Action Map
        {
            AM_UiInGame = _inputAsset.FindActionMap("InGame");
            AM_UiDialogue = _inputAsset.FindActionMap("Dialogue");
            AM_UiMenu = _inputAsset.FindActionMap("Menu");
        }

        // TODO: 타이틀 화면 추가하면 기본적으로 활성화된 액션맵을 바꾸기
        // 모두 비활성화하고 Grounded, DefaultAction, InGame만 활성화
        _inputAsset.Disable();
        AM_MoveDefault.Enable();
        //AM_ActionDefault.Enable();
        AM_UiInGame.Enable();
    }

    // moveState에 해당하는 action map을 제외하고 모두 비활성화
    public void SetMoveInputState(PlayerMoveState newMoveState)
    {
        if(newMoveState != _moveState)
        {
            switch (_moveState)
            {
                case PlayerMoveState.DEFAULT:
                    AM_MoveDefault.Disable();
                    break;
                case PlayerMoveState.CLIMBING:
                    AM_MoveClimb.Disable();
                    break;
                case PlayerMoveState.NO_MOVE:
                    break;
                // 아무것도 안함
            }
        }
        switch(newMoveState)
        {
            case PlayerMoveState.DEFAULT:
                AM_MoveDefault.Enable();
                break;
            case PlayerMoveState.CLIMBING:
                AM_MoveClimb.Enable();
                break;
            case PlayerMoveState.NO_MOVE:
                // 아무것도 안함
                break;
            default:
                Debug.LogError("잘못된 Move State 변환 요청");
                break;
        }
        _moveState = newMoveState;
    }

    /*
    public void SetActionInputState(PlayerActionState newActionState)
    {
        if(newActionState != _actionState)
        {
            switch(_actionState)
            {
                case PlayerActionState.DEFAULT:
                    AM_ActionDefault.Disable();
                    break;
                case PlayerActionState.MAGIC_READY:
                    AM_ActionMagicReady.Disable();
                    break;
                case PlayerActionState.DISABLED:
                    AM_ActionDisabled.Disable();
                    break;
            }
        }
        switch(newActionState)
        {
            case PlayerActionState.DEFAULT:
                AM_ActionDefault.Enable();
                break;
            case PlayerActionState.MAGIC_READY:
                AM_ActionMagicReady.Enable();
                break;
            case PlayerActionState.DISABLED:
                AM_ActionDisabled.Enable();
                break;
            default:
                Debug.LogError("잘못된 Action State 변환 요청");
                break;
        }
        _actionState = newActionState;
    }
    */

    public void SetUiInputState(UiState newUiState)
    {
        if(newUiState != _uiState)
        {
            switch(_uiState)
            {
                case UiState.IN_GAME:
                    AM_UiInGame.Disable();
                    break;
                case UiState.DIALOG:
                    AM_UiDialogue.Disable();
                    break;
                case UiState.MENU:
                    AM_UiMenu.Disable();
                    break;
            }
        }
        switch(newUiState)
        {
            case UiState.IN_GAME:
                AM_UiInGame.Enable();
                break;
            case UiState.DIALOG:
                AM_UiDialogue.Enable();
                break;
            case UiState.MENU:
                AM_UiMenu.Enable();
                break;
            default:
                Debug.LogError("잘못된 UI State 변환 요청");
                break;
        }
        _uiState = newUiState;
    }
    
    private void OnDestroy()
    {
        if(_instance == this)
            _instance = null;
    }
}

