using System;
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
    public static OptionUI Instance => _instance;
    private static OptionUI _instance = null;

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

    public Action OnOptionUiOpen;
    /// true면 저장하고 닫기, false면 취소하고 닫기
    public Action<bool> OnOptionUiClose;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
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
        OnOptionUiClose?.Invoke(true);
    }

    public void ResetAndClose()
    {
        currentOption = savedOption.MakeCopy();
        ApplyCurrentOption();
        Close();
        OnOptionUiClose?.Invoke(false);
    }

    public void ToDefaultOption()
    {
        currentOption = defaultOption.MakeCopy();
        ApplyCurrentOption();

    }

    public void ApplyCurrentOption()
    {
        // 옵션창 UI에 옵션 반영
        SetWindow();
        SetResolution();
        SetMasterVolume();
        SetBgmSlider();
        SetSfxSlider();
        // 옵션 반영
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
        AudioManager.Instance.SetSoundVolume(AudioType.BGM, currentOption.bgm);
        AudioManager.Instance.SetSoundVolume(AudioType.SFX, currentOption.sfx);
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

        ApplyCurrentOption();
    }

    #endregion

    #region Display
    public void SetWindow() { Window.Choice(currentOption.window); }

    public void SetResolution() { Resolution.Choice(currentOption.resolution); }

    #endregion

    #region Sound

    public void SetMasterVolume() { MasterVolume.SetValue(currentOption.vol); }

    public void SetBgmSlider() { BGM.SetValue(currentOption.bgm); }

    public void SetSfxSlider() { SFX.SetValue(currentOption.sfx); }

    #endregion

    #region UI Set

    public void Open()
    {
        Load();
        OpenInternal().Forget();
        OnOptionUiOpen?.Invoke();
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
        // if (scro == MasterVolume)
        // {
        //     currentOption.vol = scro.GetValue();
        // }
        if (scro == BGM)
        {
            currentOption.bgm = scro.GetValue();
            AudioManager.Instance.SetSoundVolume(AudioType.BGM, currentOption.bgm);
        }
        else if (scro == SFX)
        {
            currentOption.sfx = scro.GetValue();
            AudioManager.Instance.SetSoundVolume(AudioType.SFX, currentOption.sfx);
        }

    }
    #endregion
}