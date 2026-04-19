using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartIcon : MonoBehaviour
{
    [SerializeField] Image imageComponent;
    [SerializeField] private AudioResource heartRegenSound;

    [Button("하트 아이콘 수동 조작")]
    public void ChangeHeartValue(float value, bool allowSfx)
    {
        if(allowSfx)
        {
            if (imageComponent.fillAmount < 1.0f && value >= 1.0f)
            {
                if (UiSoundPlayer.Instance)
                {
                    UiSoundPlayer.Instance.PlayClip(heartRegenSound, heartRegenSound.volume);
                }
            }
        }
        imageComponent.fillAmount = value;
    }
}
