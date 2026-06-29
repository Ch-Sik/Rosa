using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class BGMPlayer : SingletonBehaviour<BGMPlayer>
{
    [InfoBox("BGM 재생용 컴포넌트.\nAudioManager의 볼륨에 영향받음.")]

    // fade 효과를 위해 오디오 소스를 2개 사용
    [SerializeField] AudioSource audioSourceA;
    [SerializeField] AudioSource audioSourceB;

    [SerializeField] AudioResource startBGM;
    [SerializeField] bool playAutomatically;
    [SerializeField] float fadeDuration = 0.3f;

    [SerializeField, ReadOnly] AudioResource currentPlayingClip;
    public AudioClip CurrentPlayingClip { get { return currentPlayingClip; } }

    AudioManager audioManager;
    Sequence fadeSeq = null;
    bool readyToFade = true;
    float bgmVolume = 1;

    protected override void Awake()
    {
        base.Awake();
        // 기존에 다른 BGMPlayer가 있어서 싱글톤 등록에 실패한 경우
        if(Instance != this)
            return;
        audioManager = AudioManager.Instance;
        audioManager.OnAudioVolumeChanged += OnVolumeChanged;
    }

    protected void OnDestroy()
    {
        audioManager = AudioManager.Instance;
        audioManager.OnAudioVolumeChanged -= OnVolumeChanged;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(audioSourceA != null);
        Debug.Assert(audioSourceB != null);

        if(playAutomatically)
        {
            if (startBGM != null)
                PlayBGM(startBGM);
            else
                Debug.LogError("시작 BGM이 설정되어있지 않음");
        }

        // MapManager.Instance.OnNextRoomLoaded += PlayRoomBGM;
    }

    void OnVolumeChanged(AudioType type, float value)
    {
        if (type != AudioType.BGM) return;
        bgmVolume = value;
        SetAudioSourceAVolume();
    }

    private void SetAudioSourceAVolume()
    {
        float curClipVolume = currentPlayingClip ? currentPlayingClip.volume : 1;
        audioSourceA.volume = bgmVolume * curClipVolume;
    }

    [Button("브금 전환 테스트")]
    public void PlayBGM(AudioResource newClip)
    {
        currentPlayingClip = newClip;
        if (audioSourceA.isPlaying)
        {
            // 26.03.05) newClip.audioClip == null인 경우 추가
            // 그 경우에는 기존 브금을 끄는 형태로 동작
            if (newClip.audioClip == null || audioSourceA.clip.name != newClip.audioClip.name)
                SwitchBGM(newClip);
            // else
            //     Debug.LogWarning("이미 재생중인 BGM임!");
        }
        else
        {
            audioSourceA.clip = newClip;
            audioSourceA.volume = bgmVolume * currentPlayingClip.volume;
            audioSourceA.Play();
        }
    }

    private void SwitchBGM(AudioResource newClip)
    {
        Debug.Log($"브금 전환: {audioSourceA.clip.name} → {newClip}");

        if(!readyToFade)
        {
            Debug.LogError("이미 BGM 전환 수행중임! 기다렸다가 다시 시도하세요");
            return;
        }
        readyToFade = false;

        // audioSourceB 미리 준비시키기
        audioSourceB.clip = newClip;
        audioSourceB.volume = 0;
        audioSourceB.loop = true;

        // 페이드 수행
        fadeSeq = DOTween.Sequence()
            .Append(audioSourceA.DOFade(0, fadeDuration))
            .AppendCallback(() =>
            {
                audioSourceA.Stop();
                if(audioSourceB.clip)
                    audioSourceB.Play();
            })
            .Append(audioSourceB.DOFade(bgmVolume * currentPlayingClip.volume, fadeDuration))
            .OnComplete(() => {
                // 다음 페이드 수행가능하다고 표시
                readyToFade = true;
                // 두 AudioSource의 참조를 교환
                (audioSourceA, audioSourceB) = (audioSourceB, audioSourceA);
            });
    }

    // SORoom에 정의된 각 방의 기본 BGM 재생
    public void PlayRoomBGM()
    {
        if(MapManager.Instance == null)
        {
            Debug.LogError("MapManager instance is Null!");
            return;
        }
        // 같은 브금이 재생중일 때의 예외 처리는 PlayBGM 내부에서 이루어짐
        var roomBGM = MapManager.Instance.CurrentRoom.defaultBGM;
        if (roomBGM)
            PlayBGM(roomBGM);
    }

    public void SetLoop(bool value)
    {
        if (readyToFade)
            audioSourceA.loop = value;
        else
            audioSourceB.loop = value;
    }
}
