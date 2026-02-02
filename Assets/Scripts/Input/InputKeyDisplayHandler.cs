using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputKeyDisplayHandler : MonoBehaviour
{
    public List<InputKeyDisplay> keyLabelList = new();
    public Button button;

    public void Start()
    {
        UpdateAllLabels();
    }

    public void UpdateAllLabels()
    {
        
    }
    
    public void UpdateLabel(string actionName, string bindedKey)
    {
        foreach (var keyLabel in keyLabelList)
        {
            if(keyLabel.InputActionName == actionName)
                keyLabel.UpdateText(bindedKey);
        }
        // 버튼 선택 해제
        EventSystem.current.SetSelectedGameObject(null); 
    }
}
