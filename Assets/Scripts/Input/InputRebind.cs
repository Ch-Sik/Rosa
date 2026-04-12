using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputRebind : MonoBehaviour
{
    public GameObject InputRebindPopup;
    public InputActionAsset inputActionAsset;
    public InputKeyDisplayHandler inputKeyDisplayHandler;
    
    private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

    private string bindingBackupJson = "";
    public static InputRebind Instance = null; 

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        LoadBinding();
        OptionUI.Instance.OnOptionUiOpen += BackupInputBinding;
    }

    private void BackupInputBinding()
    {
        bindingBackupJson = inputActionAsset.SaveBindingOverridesAsJson();
        Debug.Log("[InputRebind] 바인딩맵 백업");
        OptionUI.Instance.OnOptionUiClose += HandleRollbackChanges;
    }

    private void HandleRollbackChanges(bool saveOnClose)
    {
        OptionUI.Instance.OnOptionUiClose -= HandleRollbackChanges;
        if (saveOnClose)
            return;
        RollbackInputBindingChanges();
    }

    public void RollbackInputBindingChanges()
    {
        inputActionAsset.LoadBindingOverridesFromJson(bindingBackupJson);
        UpdateAllBindingDisplay();
        Debug.Log("[InputRebind] 바인딩맵 롤백");
    }

    private void SaveBinding()
    {
        string json = inputActionAsset.SaveBindingOverridesAsJson();
        SaveLoadManager.Instance.SaveInputBinding(json);
    }

    private void LoadBinding()
    {
        string json = SaveLoadManager.Instance.LoadInputBinding();
        if(json == null || json.Length == 0)
            return;
        inputActionAsset.LoadBindingOverridesFromJson(json);
        UpdateAllBindingDisplay();
    }

    public void ResetToDefault()
    {
        inputActionAsset.RemoveAllBindingOverrides();
        UpdateAllBindingDisplay();
        SaveBinding();
    }

    private void UpdateAllBindingDisplay()
    {
        string[] inputs = {"up", "down", "left", "right", "Jump", "Dash", "Attack", "SuperJump"};

        foreach (string input in inputs)
        {
            (InputAction actionToRebind, int index) = FindInputAction(input);
            string keyName = InputActionRebindingExtensions.GetBindingDisplayString(actionToRebind, index);
            inputKeyDisplayHandler.UpdateLabel(input, keyName);
        }
    }

    public void StartInteractiveRebind(string inputActionName)
    {
        (InputAction actionToRebind, int index) = FindInputAction(inputActionName);
        
        // 1. 액션 비활성화
        EventSystem.current.sendNavigationEvents = false;
        actionToRebind.Disable();

        // 2. 리바인딩 작업 설정
        _rebindOperation = actionToRebind.PerformInteractiveRebinding(index)
            // 특정 키(예: ESC)를 누르면 취소되게 설정 가능
            .WithCancelingThrough("<Keyboard>/escape") 
            .WithControlsExcluding("<Mouse>/leftButton")
            .WithControlsExcluding("<Mouse>/rightButton")
            .OnComplete(operation => 
            {
                Debug.Log($"Rebind Complete: {actionToRebind.bindings[index].effectivePath}");
                CleanUp(actionToRebind, true);
                
                // 위쪽 방향키는 OnInteract와 묶여있으니 별도로 추가적인 처리 필요
                if (inputActionName == "up")
                {
                    (var moveAction, int upIndex) = FindInputAction("up");
                    string newPath = moveAction.bindings[upIndex].overridePath;
                    
                    (var interactAction, int index) = FindInputAction("Interact");
                    interactAction.ApplyBindingOverride(index, newPath);
                }
                
                // UI에 업데이트된 입력 바인딩 표시
                string keyName = InputActionRebindingExtensions.GetBindingDisplayString(actionToRebind, index);
                inputKeyDisplayHandler.UpdateLabel(inputActionName, keyName);
            })
            .OnCancel(operation => 
            {
                Debug.Log("Rebind Canceled");
                CleanUp(actionToRebind, false);
            });

        // 3. 입력 대기 시작
        InputRebindPopup.SetActive(true);
        _rebindOperation.Start();

    }

    private void CleanUp(InputAction action, bool save)
    {
        InputRebindPopup.SetActive(false);
        EventSystem.current.sendNavigationEvents = true;
        
        _rebindOperation?.Dispose();
        _rebindOperation = null;
        action.Enable();
        
        if(save)
            SaveBinding();
    }

    public string GetInputControl(string inputActionName)
    {
        (var inputAction, int index) = FindInputAction(inputActionName);
        return InputActionRebindingExtensions.GetBindingDisplayString(inputAction, index);
    }

    private (InputAction, int) FindInputAction(string actionName)
    {
        switch (actionName)
        {
            case "up":
            case "down":
            case "left":
            case "right":
                // 1. 해당 액션의 바인딩 중 'compositePartName'(예: "Up", "Down")과 일치하는 인덱스 찾기
                var moveAction = inputActionAsset.FindAction("Move");
                
                for (int i = 0; i < moveAction.bindings.Count; i++)
                {
                    if (moveAction.bindings[i].name.Equals(actionName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return (moveAction, i);
                    }
                }
                
                Debug.LogError($"[InputRebind] 해당하는 액션을 찾을 수 없음: {actionName}");
                return (null, 0);
            case "Jump":
                return (inputActionAsset.FindAction("Jump"), 0);
            case "Dash":
                return (inputActionAsset.FindAction("Dash"), 0);
            case "Attack":
                return (inputActionAsset.FindAction("Attack"), 0);
            case "SuperJump":
                return (inputActionAsset.FindAction("Mushroom"), 0);
            case "Interact":
                return (inputActionAsset.FindAction("Interact"), 0);
            default:
                var foundAction = inputActionAsset.FindAction("Interact");
                if(foundAction == null)
                    Debug.LogError($"[InputRebind] 해당하는 액션을 찾을 수 없음: {actionName}");
                return (foundAction, 0);
                return (null, 0);
        }
    }
}
