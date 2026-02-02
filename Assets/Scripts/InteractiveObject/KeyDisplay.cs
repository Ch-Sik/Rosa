using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeyDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private string inputActionName;
    [SerializeField] private Sprite shortKeySprite, longKeySprite;
    
    // Start is called before the first frame update
    void Start()
    {
        string control = InputRebind.Instance.GetInputControl(inputActionName);
        switch (control)
        {
            case "Up":
                control = "↑";
                break;
            case "Down":
                control = "↓";
                break;
            case "Left":
                control = "←";
                break;
            case "Right":
                control = "→";
                break;
        }
        background.sprite = control.Length > 1 ? longKeySprite : shortKeySprite;
        keyLabel.text = control;
    }
}
