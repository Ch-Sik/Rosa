using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenBGM : MonoBehaviour
{
    [SerializeField] private AudioResource bgm;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!BGMPlayer.Instance)
        {
            Debug.LogError("[TitleScreenBGM] Cannot find BGMPlayer Instance");
            return;
        }
        BGMPlayer.Instance.PlayBGM(bgm);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
