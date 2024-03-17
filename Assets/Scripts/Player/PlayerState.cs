using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 플레이어의 현재 상태를 보관하는 클래스
/// 최대 체력, 현재 체력, 공격력, 식물마법 언락 유무 등을 여기서 관리함
/// </summary>
public class PlayerState : MonoBehaviour
{
    // 컴포넌트 참조
    private PlayerStateUI stateUI;

    // states
    [SerializeField] private int maxHP;
    [SerializeField] private int currentHP;
    [SerializeField] private int maxSeed;
    public int currentSeed;
    [SerializeField] private int attackDmg;
    [SerializeField] private int seedRechargeTime;
    [SerializeField] private int[] magicLevel;    // 0이면 안배움, 1이면 기본, 2이면 업그레이드 상태

    private Timer seedTimer = null;

    private void Start()
    {
        Init();
    }

    // states getter
    public int AttackDmg { get { return attackDmg; } }

    public void Init(/*int maxHP, int attackDmg, bool[] plantMagicUnlock ...*/)
    {
        // HP, 공격력 등의 값 초기화하기
        stateUI = PlayerStateUI.Instance;
        currentHP = maxHP;
        currentSeed = maxSeed;
        for(int i = 0; i < maxHP; i++)
        {
            stateUI.AddHPUI();
        }
        for(int i=0; i<maxSeed; i++)
        {
            stateUI.AddSeedUI();
        }
    }

    public void Heal(int amount) 
    {
        stateUI.Heal(amount);
        while(amount > 0)
        {
            if (currentHP >= maxHP) return;
            currentHP++;
            amount--;
        }
    }
    public void TakeDamage(int amount) 
    {
        stateUI.TakeDamage(amount);
        while (amount > 0)
        {
            if (currentHP <= 1)
            {
                currentHP--;
                RespawnManager.Instance.Respawn();
                return;
            }
            currentHP--;
            amount--;
        }
    }

    public void ConsumeSeed(int amount)
    {
        Debug.Assert(currentSeed >= amount);
        currentSeed -= amount;
        stateUI.ConsumeSeed(amount);
        if(seedTimer == null)
        {
            seedTimer = Timer.StartTimer();
        }
    }

    private void Update()
    {
        if(seedTimer != null && seedTimer.duration >= seedRechargeTime)
        {
            RechargeSeed();
            if (currentSeed < maxSeed)
            {
                seedTimer.Reset();
            }
            else
            {
                seedTimer = null;
            }
        }
    }

    private void RechargeSeed()
    {
        currentSeed++;
        stateUI.RechargeSeed(1);
    }

    public void UpgradePlantMagic(SkillCode magicCode) { } // 획득 및 업그레이드
}
