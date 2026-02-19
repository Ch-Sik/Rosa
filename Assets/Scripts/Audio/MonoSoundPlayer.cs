using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSoundPlayer : SingletonBehaviour<MonoSoundPlayer>
{
    public AudioSource audioSource;
    private AudioManager audioManager;

    private Camera audioListener;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        audioManager = AudioManager.Instance;
        audioManager.OnAudioVolumeChanged += OnVolumeChanged;
        UpdateVolume(audioManager.GetSoundVolume(AudioType.UI));
    }

    void Update()
    {
        if (!audioListener)
        {
            audioListener = Camera.main;
        }
        else
        {
            // 메인 카메라에 부착된 AudioListener 따라가게 해서 일정한 볼륨&패닝 유지
            transform.position = audioListener.transform.position;
        }
    }
    
    void OnVolumeChanged(AudioType type, float value)
    {
        if (type != AudioType.UI) return;
        UpdateVolume(value);
    }
    
    void UpdateVolume(float value)
    {
        audioSource.volume = value;
    }

    public void PlayClip(AudioClip clip, float volume)
    {
        audioSource.PlayOneShot(clip, volume);
    }
}
