using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartIcon : MonoBehaviour
{
    [SerializeField] Image imageComponent;

    [Button("하트 아이콘 수동 조작")]
    public void ChangeHeartValue(float value)
    {
        imageComponent.fillAmount = value;
    }
}
