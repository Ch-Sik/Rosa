using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Boss3Rock : ProjectileBase
{
    [SerializeField] private LayerMask mushroomLayer;

    [Tooltip("버섯과 충돌하여 튕겨났을 때 되돌아가서 맞아야 할 위치")]
    [ReadOnly] public Vector3 returnPosition;

    [Tooltip("플레이어가 '튕겨내기'를 성공시켰을 때 데미지 입힐 컴포넌트")]
    [ReadOnly] public MonsterDamageReceiver damageReceiver;

    [Tooltip("플레이어가 '튕겨내기'를 성공시켰을 때 투사체가 튕겨내어져 돌아가는 시간")]
    [SerializeField] private float returnTime = 1.8f;

    // Start is called before the first frame update
    void Start()
    {
        // Debug.Log(LayerMask.GetMask("Ground", "Cube", "PlayerGrab"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
