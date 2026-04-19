using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 플레이어의 현재 상태를 보관하는 클래스
/// 최대 체력, 현재 체력, 공격력, 식물마법 언락 유무 등을 여기서 관리함
/// </summary>
public class PlayerState : MonoBehaviour
{
    // 컴포넌트 참조
    private PlayerStateUI stateUI;

    // property
    public int MaxHP { get { return maxHp; } }
    public float CurrentHP { get { return currentHp; } }

    // states
    [FormerlySerializedAs("_maxHP")] 
    [SerializeField] private int maxHp;
    [SerializeField] private float currentHp;
    [SerializeField] private bool respawnOnDie;

    // events
    public delegate void HpEvent(float currentValue, bool allowSfx);
    public HpEvent OnHpChanged;

    private void Start()
    {
        Init();
    }

    public void Init(/*int maxHP, int attackDmg, bool[] plantMagicUnlock ...*/)
    {
        // HP, 공격력 등의 값 초기화하기
        stateUI = PlayerStateUI.Instance;
        currentHp = maxHp;
        OnHpChanged?.Invoke(currentHp, false);
    }

    // 소숫점 단위로 회복
    public void Heal(float amount)
    {
        if (amount <= 0) return;

        currentHp = Mathf.Min(currentHp + amount, maxHp);
        OnHpChanged?.Invoke(currentHp, true);
    }
    
    /// <returns>사망 여부</returns>
    public bool TakeDamage(int amount) 
    {
        if (amount <= 0) return false;

        currentHp = Mathf.Max(currentHp - amount, 0);
        OnHpChanged?.Invoke(currentHp, true);

        if (currentHp <= 0)
        {
            OnDie();
            return true;
        }

        return false;
    }

    private void OnDie()
    {
        if (respawnOnDie)
        {
            Heal(2);
            if (RespawnHandler.Instance != null)
                RespawnHandler.Instance.Respawn();
            else
                Debug.LogError("RespawnManager가 씬에 존재하지 않음");
            return;
        }
        else
        {
            GameManager.Instance.GameOver();
        }
    }
}
