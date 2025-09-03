using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 체력 등 GamePlay 타임동안 항상 보이는 UI 담당
/// </summary>
public class PlayerStateUI : MonoBehaviour
{
    // 싱글턴
    private static PlayerStateUI _instance = null;
    public static PlayerStateUI Instance { get { return _instance; } }

    // 25.05.16) 선택된 마법 관련 UI 삭제
    // [SerializeField] TMP_Text text_selectedMagic;
    [SerializeField] GameObject heart;
    [SerializeField] GameObject heartContainer;
    [SerializeField] List<HeartIcon> heartUiList = new List<HeartIcon>();
    float curHp = 0;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        PlayerRef.Instance.state.OnHpChanged += OnHpChanged;

        Initialize();
    }

    [Button("강제 초기화 진행")]
    private void Initialize()
    {
        Debug.Log($"PlayerStateUI 초기화 수행");
        // 필드값 설정
        curHp = PlayerRef.Instance.state.CurrentHP;

        // UI에 필요한 만큼 하트 아이콘 생성
        for (int i = 0; i < PlayerRef.Instance.state.MaxHP; i++)
        {
            GameObject h = Instantiate(heart, heartContainer.transform);
            heartUiList.Add(h.GetComponent<HeartIcon>());
            curHp++;
        }
    }

    [Button("HP 게이지 테스트")]
    public void OnHpChanged(float newHP)
    {
        for(int i=0; i<heartUiList.Count; i++)
        {
            heartUiList[i].ChangeHeartValue(Mathf.Min(newHP, 1.0f));
            newHP -= 1.0f;
        }
    }

    //// TODO: 아이콘과 스프라이트 기반으로 기능 재구현
    //public void UpdateSelectedMagic(SO_Magic selectedMagic)
    //{
    //    text_selectedMagic.text = selectedMagic.skillName;
    //}
}
