using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiButtonSfx : MonoBehaviour
{
    public AudioResource audioResource;
    private Button _button;

    // Start is called before the first frame update
    void Start()
    {
        if (!_button)
            _button = GetComponent<Button>();
        if (_button)
            _button.onClick.AddListener(PlayButtonSfx);
    }

    // EventTrigger를 사용하여 버튼 수동으로 구현한 케이스 때문에 public으로 노출
    public void PlayButtonSfx()
    {
        UiSoundPlayer.Instance.PlayClip(audioResource, audioResource.volume);
    }
}
