using DG.Tweening;
using Panda;
using System.Collections;
using System.Collections.Generic;
using AnyPortrait;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class Task_A_Boss3PheonixFlyUp : Task_A_Base
{
    [Tooltip("화면 안쪽에서 날아올 때 지면 아래 몇미터를 향해 날아올건지")]
    [SerializeField] float downOffset = 15;
    [SerializeField] private SortingGroup sortingGroup;
    [SerializeField] private int orderInLayerWhenFlying = 0;

    Rigidbody2D _rigidbody;
    MonsterDamageInflictor _bodyDamageComponent;
    GameObject _target;
    private int _originalOrderInLayer;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _bodyDamageComponent = GetComponent<MonsterDamageInflictor>();
        _originalOrderInLayer = sortingGroup.sortingOrder;
    }

    [Task]
    void PheonixFlyUp()
    {
        ExecuteAttack();
    }

    protected override void OnActiveBegin()
    {
        // 적(플레이어) 위치 정보 가져오기
        Vector3 targetPosition = transform.position;
        if (blackboard.TryGet(BBK.Enemy, out _target))
        {
            targetPosition.x = _target.transform.position.x;
        }
        else
        {
            Debug.LogError("적(플레이어)를 찾을 수 없음!");
        }

        DOTween.Sequence()
            // 1. 위로 올라가기
            .Append(_rigidbody.DOMoveY(transform.position.y + 20f, 1.5f))
            // 2. 공격 판정 끄고, 저 멀리서 나타나기
            .AppendCallback(() =>
            {
                _bodyDamageComponent.attackEnabled = false;
                transform.position = targetPosition + Vector3.forward * 20;
                sortingGroup.sortingOrder = orderInLayerWhenFlying;
            })
            // 3. 지면 아래로 날아오기
            // 여기 Transform으로 하는 게 맞는가? Lifecycle 주기가 좀 다른데;;
            .Append(transform.DOMove(targetPosition + Vector3.down * downOffset, 3f))
            .OnComplete(() =>
            {
                sortingGroup.sortingOrder = _originalOrderInLayer;
            });
        // 이후는 PheonixRise에서 계속
    }
}
