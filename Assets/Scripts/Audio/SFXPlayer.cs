using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    [InfoBox("enabled 될 때 자동으로 효과음하나를 재생하는 컴포넌트.\nAudioManager의 볼륨에 영향받음.")]

    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioResource audioResource;
    [FormerlySerializedAs("playOnAwake")]
    [SerializeField] protected bool playOnEnable = false;
    [SerializeField] protected bool loop = false;
    [SerializeField] protected float loopStartDelay = 0f;

    AudioManager audioManager;
    private float _volume; 


    // Start is called before the first frame update
    protected virtual void Start()
    {
        audioManager = AudioManager.Instance;
        audioManager.OnAudioVolumeChanged += OnVolumeChanged;
        UpdateVolume(audioManager.GetSoundVolume(AudioType.SFX));
        
        if(audioSource != null )
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    protected void OnDestroy()
    {
        audioManager = AudioManager.Instance;
        audioManager.OnAudioVolumeChanged -= OnVolumeChanged;
    }

    void OnVolumeChanged(AudioType type, float value)
    {
        if (type != AudioType.SFX) return;
        UpdateVolume(value);
    }

    void UpdateVolume(float value)
    {
        audioSource.volume = value;
    }

    private void OnEnable()
    {
        if (!playOnEnable)
            return;

        if (loopStartDelay > 0)
            PlaySfxWithDelay(loopStartDelay).Forget();
        else
            PlaySfx();
    }

    public async UniTaskVoid PlaySfxWithDelay(float delay)
    {
        await UniTask.WaitForSeconds(delay);
        PlaySfx();
    }
    
    public void PlaySfx()
    {
        if (loop)
        {
            audioSource.clip = audioResource;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(audioResource, audioResource.volume);
        }
    }

    public void StopSfx()
    {
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
    }
}
