using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputKeyDisplay : MonoBehaviour
{
    [SerializeField] private string inputActionName;
    [SerializeField] private TMP_Text textComponent;

    public string InputActionName => inputActionName;
    
    // Update is called once per frame
    public void UpdateText(string text)
    {
        textComponent.text = text;
    }
}
