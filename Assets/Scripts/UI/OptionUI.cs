using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public static OptionUI Instance { get; private set; }

    public GameObject UI;
    public CanvasGroup uiGroup;
    public float fadeDuration = 0.2f;

    public OptionSetting defaultOption = new OptionSetting();
    public OptionSetting savedOption; //저장된 옵션
    public OptionSetting currentOption; //현재 적용된 옵션

    public TextChoiceButtonController Window;
    public TextChoiceButtonController Resolution;
    public ScrollbarUI MasterVolume;
    public ScrollbarUI BGM;
    public ScrollbarUI SFX;

    public Vector2Int[] resolutions = new Vector2Int[3];
    public FullScreenMode[] screenModes = new FullScreenMode[2];

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Load();
    }

    #region Environment
    public void ApplyAndClose()
    {
        Save();
        Close();
    }

    public void ResetAndClose()
    {
        currentOption = savedOption.MakeCopy();
        SetByCurrentOption();
        Close();
    }

    public void ToDefaultOption()
    {
        currentOption = defaultOption.MakeCopy();
        SetByCurrentOption();
    }

    public void SetByCurrentOption()
    {
        SetWindow();
        SetResolution();
        SetMasterVolume();
        SetBGM();
        SetSFX();
        SetScreenEnvironmentByCurrentOption();
        SetSoundEnvironmentByCurrentOption();
    }

    public void SetScreenEnvironmentByCurrentOption()
    {
        Vector2Int resolution = resolutions[Resolution.index];
        FullScreenMode mode = screenModes[Window.index];
        Screen.SetResolution(resolution.x, resolution.y, mode);
    }

    public void SetSoundEnvironmentByCurrentOption()
    {
        
    }

    #endregion

    #region Save/Load

    public void Save()
    {
        SaveLoadManager.Instance.SaveOptionData(currentOption);
        savedOption = currentOption.MakeCopy();
    }

    public void Load()
    {
        if (SaveLoadManager.Instance == null)
            return;

        OptionSetting loadedOption = SaveLoadManager.Instance.LoadOptionData();
        if (loadedOption != null)
            savedOption = loadedOption.MakeCopy();
        else
            savedOption = defaultOption.MakeCopy();
        currentOption = savedOption.MakeCopy();

        SetByCurrentOption();
    }

    #endregion

    #region Display
    public void SetWindow() { Window.Choice(currentOption.window); }

    public void SetResolution() { Resolution.Choice(currentOption.resolution); }

    #endregion

    #region Sound

    public void SetMasterVolume() { MasterVolume.SetValue(currentOption.vol); }

    public void SetBGM() { BGM.SetValue(currentOption.bgm); }

    public void SetSFX() { SFX.SetValue(currentOption.sfx); }

    #endregion

    #region UI Set

    public void Open()
    {
        Load();
        OpenInternal().Forget();
    }

    private async UniTaskVoid OpenInternal()
    {
        UI.SetActive(true);
        uiGroup.DOFade(1, fadeDuration).SetUpdate(true);
    }

    public void Close()
    {
        CloseInternal().Forget();
    }

    private async UniTaskVoid CloseInternal()
    {
        await uiGroup.DOFade(0, fadeDuration).SetUpdate(true)
                    .AsyncWaitForCompletion();
        await UniTask.SwitchToMainThread();
        UI.SetActive(false); 
    }

    public void OpenClose()
    {
        if (UI.activeSelf)
            Close();
        else
            Open();
    }
    #endregion
    
    #region Key setting

    public void OnKeySettingChangeButtonClick(string inputAction)
    {
        var inputRebinder = GetComponent<InputRebind>();
        if (inputRebinder == null)
        {
            Debug.LogError("InputRebinder is null");
            return;
        }
        
        inputRebinder.StartInteractiveRebind(inputAction);
    }
    
    #endregion
    
    #region Event
    public void OnChoiceButtonChanged(TextChoiceButtonController cont)
    {
        if (cont == Window)
            currentOption.window = Window.index;
        else if (cont == Resolution)
            currentOption.resolution = Resolution.index;

        SetScreenEnvironmentByCurrentOption();
    }

    public void OnScrollChanged(ScrollbarUI scro)
    {
        if (scro == MasterVolume)
            currentOption.vol = scro.GetValue();
        else if (scro == BGM)
            currentOption.bgm = scro.GetValue();
        else if (scro == SFX)
            currentOption.sfx = scro.GetValue();

        SetSoundEnvironmentByCurrentOption();
    }
    #endregion
}