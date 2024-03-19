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

    [SerializeField] TMP_Text text_selectedMagic;
    [SerializeField] GameObject heartPrefab;
    [SerializeField] GameObject heartContainer;
    [SerializeField] GameObject seedPrefab;
    [SerializeField] GameObject seedContainer;
    [SerializeField] Sprite filled;
    [SerializeField] Sprite empty;

    [SerializeField] List<Image> hearts = new List<Image>();
    [SerializeField] List<Image> seeds = new List<Image>();
    int curHp = 0;
    int curSeed = 0;

    private void Awake()
    {
        _instance = this;
    }

    public void AddHPUI()
    {
        GameObject h = Instantiate(heartPrefab, heartContainer.transform);
        hearts.Add(h.GetComponent<Image>());
        curHp++;
    }

    public void AddSeedUI()
    {
        GameObject s = Instantiate(seedPrefab, seedContainer.transform);
        seeds.Add(s.GetComponent<Image>());
        curSeed++;
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

    public void ConsumeSeed(int index, float rechargeTime)
    {
        seeds[index].fillAmount = 0f;
        StartCoroutine(UpdateSeedUI());

        IEnumerator UpdateSeedUI()
        {
            Debug.Log($"{index}번 식물마법 재충전 시작");
            int step = 5;
            float timePerStep = rechargeTime / step;
            var wait = new WaitForSeconds(timePerStep);

            for(int i=0; i<step; i++)
            {
                seeds[index].fillAmount = (float)1 / step * i;
                yield return wait;
            }
        }
    }

    public void RechargeSeed(int index)
    {
        Debug.Log($"{index}번 씨앗 준비됨");
        seeds[index].fillAmount = 1f;
    }

    // TODO: 아이콘과 스프라이트 기반으로 기능 재구현
    public void UpdateSelectedMagic(SO_Magic selectedMagic)
    {
        text_selectedMagic.text = selectedMagic.skillName;
    }
}
