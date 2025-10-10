using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraShakePreset
{
    None,
    PlayerHit,
    Small,
    Large,
}

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    private ProCamera2DShake procamShakeComponent;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        procamShakeComponent = GetComponent<ProCamera2DShake>();
        if (procamShakeComponent == null)
            Debug.LogError("ProCamera2DShake 컴포넌트를 찾을 수 없음");
    }

    public static void ShakeCamera(CameraShakePreset preset, bool stopOtherShakes = false)
    {
        instance.ShakeCameraInternal(preset, stopOtherShakes);
    }

    private void ShakeCameraInternal(CameraShakePreset preset, bool stopOtherShakes)
    {
        if (stopOtherShakes)
            procamShakeComponent.StopShaking();
        string presetName = "";
        switch(preset)
        {
            case CameraShakePreset.PlayerHit:
                presetName = "PlayerHit";
                break;
            case CameraShakePreset.Small:
                presetName = "SmallExplosion";
                break;
            case CameraShakePreset.Large:
                presetName = "LargeExplosion";
                break;
        }
        procamShakeComponent.Shake(presetName);
    }
}
