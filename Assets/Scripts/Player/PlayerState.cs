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

    // property
    public int MaxHP { get { return _maxHP; } }
    public float CurrentHP { get { return _currentHP; } }

    // states
    [SerializeField] private int _maxHP;
    [SerializeField] private float _currentHP;

    // events
    public delegate void HpEvent(float currentValue);
    public HpEvent OnHpChanged;

    private void Start()
    {
        Init();
    }

    public void Init(/*int maxHP, int attackDmg, bool[] plantMagicUnlock ...*/)
    {
        // HP, 공격력 등의 값 초기화하기
        stateUI = PlayerStateUI.Instance;
        _currentHP = _maxHP;
    }

    // 소숫점 단위로 회복
    public void Heal(float amount)
    {
        if (amount <= 0) return;

        Debug.Log($"체력 회복: {amount}");
        _currentHP = Mathf.Min(_currentHP + amount, _maxHP);
        OnHpChanged?.Invoke(_currentHP);
    }

    // 정수 단위로 데미지
    public void TakeDamage(int amount) 
    {
        if (amount <= 0) return;

        Debug.Log("피해 입음 : " + amount);
        _currentHP = Mathf.Max(_currentHP - amount, 0);
        OnHpChanged?.Invoke(_currentHP);

        if (_currentHP <= 0)
        {
            if (RespawnManager.Instance != null)
                RespawnManager.Instance.Respawn();
            else
                Debug.LogWarning("RespawnManager가 씬에 존재하지 않음");
        }
    }
}
