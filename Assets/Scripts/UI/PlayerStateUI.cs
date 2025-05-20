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
    [SerializeField] Sprite filled;
    [SerializeField] Sprite empty;
    [SerializeField] List<Image> hearts = new List<Image>();
    int curHp = 0;

    private void Awake()
    {
        _instance = this;
    }

    public void AddHPUI()
    {
        GameObject h = Instantiate(heart, heartContainer.transform);
        hearts.Add(h.GetComponent<Image>());
        curHp++;
    }

    public void Heal(int amount) 
    {
        while(amount > 0)
        {
            if (curHp >= hearts.Count) return;
            hearts[curHp].sprite = filled;
            curHp++;
            amount--;
        }
        
        
    }
    public void TakeDamage(int amount)
    {
        while (amount > 0)
        {
            if (curHp <= 0) return;
            curHp--;
            hearts[curHp].sprite = empty;
            amount--;
        }
    }

    //// TODO: 아이콘과 스프라이트 기반으로 기능 재구현
    //public void UpdateSelectedMagic(SO_Magic selectedMagic)
    //{
    //    text_selectedMagic.text = selectedMagic.skillName;
    //}
}
