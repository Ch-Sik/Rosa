using UnityEngine;

/// <summary>
/// 오디오 클립 한번에 바꿀 수 있게 한 차례 추상화
/// </summary>
[CreateAssetMenu(fileName = "AudioResource", menuName = "Audio/AudioResource")]
public class AudioResource : ScriptableObject
{
    public AudioClip audioClip;
    public float volume = 1f;
    
    public static implicit operator AudioClip(AudioResource audioResource)
    {
        return audioResource.audioClip;
    }
}
