using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class BGMPlayer : MonoBehaviour
{
    [InfoBox("BGM 재생용 컴포넌트.\nAudioManager의 볼륨에 영향받음.")]

    // fade 효과를 위해 오디오 소스를 2개 사용
    [SerializeField] AudioSource audioSourceA;
    [SerializeField] AudioSource audioSourceB;

    [SerializeField] AudioClip startBGMclip;
    [SerializeField] bool playAutomatically;
    [SerializeField] float fadeDuration = 0.3f;

    [SerializeField, ReadOnly] AudioClip currentPlayingClip;
    public AudioClip CurrentPlayingClip { get { return currentPlayingClip; } }

    AudioManager audioManager;
    Sequence fadeSeq = null;
    bool readyToFade = true;
    float bgmVolume = 1;

    private void Awake()
    {
        audioManager = AudioManager.Instance;
        audioManager.OnAudioVolumeChanged += OnVolumeChanged;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(audioSourceA != null);
        Debug.Assert(audioSourceB != null);

        if(playAutomatically)
        {
            if (startBGMclip != null)
                PlayBGM(startBGMclip);
            else
                Debug.LogError("시작 BGM이 설정되어있지 않음");
        }

        MapManager.Instance.OnNextRoomLoaded += PlayRoomBGM;
    }

    void OnVolumeChanged(AudioType type, float value)
    {
        if (type != AudioType.BGM) return;
        bgmVolume = value;
        audioSourceA.volume = bgmVolume;
    }

    [Button("브금 전환 테스트")]
    public void PlayBGM(AudioClip newClip)
    {
        currentPlayingClip = newClip;
        if (audioSourceA.isPlaying)
        {
            if (audioSourceA.clip != newClip)
                SwitchBGM(newClip);
            else
                Debug.LogWarning("이미 재생중인 BGM임!");
        }
        else
        {
            audioSourceA.clip = newClip;
            audioSourceA.Play();
        }
    }

    private void SwitchBGM(AudioClip newClip)
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
                audioSourceB.Play();
            })
            .Append(audioSourceB.DOFade(bgmVolume, fadeDuration))
            .OnComplete(() => {
                // 다음 페이드 수행가능하다고 표시
                readyToFade = true;
                // 두 AudioSource의 참조를 교환
                AudioSource temp = audioSourceA;
                audioSourceA = audioSourceB;
                audioSourceB = temp;
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
        AudioClip roomBGM = MapManager.Instance.CurrentRoom.defaultBGM;
        if (roomBGM)
            PlayBGM(roomBGM);
    }
}
