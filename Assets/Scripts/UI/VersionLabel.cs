using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    void Start()
    {
        text.text = Application.version;
    }
}
