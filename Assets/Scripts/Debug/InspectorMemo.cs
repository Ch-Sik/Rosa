using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectorMemo : MonoBehaviour
{
    #if UNITY_EDITOR
    [TextArea(3, 10)]
    public string MEMO;
    #endif
}