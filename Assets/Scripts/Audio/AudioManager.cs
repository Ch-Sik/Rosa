using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class AudioManager : SingletonBehaviour<AudioManager>
{
    [InfoBox("SFXplayer, BGMplayer 등을 통해 전반적인 사운드 볼륨을 조정하는 컴포넌트." +
        "\n씬에 딱 하나면 있으면 됨. GameManager에다가 부착하면 될 듯" +
        "\n_volumes의 0번째가 BGM, 1번째가 SFX")]
    [InfoBox("아래 슬라이드를 조절하여 볼륨 초기값 조절\n(런타임 중에는 효과 없음. 그 아래 BGM 볼륨 수정 테스트 버튼을 사용할 것)")]
    [SerializeField, ReadOnly, Range(0, 1f)] float[] _volumes;

    public delegate void AudioVolumeChangeEvent(AudioType type, float value);
    public AudioVolumeChangeEvent OnAudioVolumeChanged;

    private void Awake()
    {
        base.Awake();
        if(_volumes == null || _volumes.Length != Enum.GetNames(typeof(AudioType)).Length)
            _volumes = new float[Enum.GetNames(typeof(AudioType)).Length];
    }

    private void Start()
    {
        // TODO: 저장된 설정값 읽어와서 볼륨 바꾸도록 수정
        for(int i=0; i< _volumes.Length; i++)
        {
            // 처음에 볼륨을 일괄 자기 자신으로 설정하도록 하여
            // OnAudioVolumeChanged를 참조하는 모든 오디오 관련 컴포넌트들의 볼륨 일괄 조정
            SetSoundVolume((AudioType)i, _volumes[i]);
        }
    }

    [Button("BGM 볼륨 수정 테스트")]
    private void SetSoundVolume(AudioType audioType, float value)
    {
        _volumes[(int)audioType] = value;
        if(OnAudioVolumeChanged != null)
            OnAudioVolumeChanged(audioType, value);
    }

    public float GetSoundVolume(AudioType audioType)
    {
        if ((int)audioType >= _volumes.Length)
        {
            return 0;
        }
        
        return _volumes[(int)audioType];
    }
}
