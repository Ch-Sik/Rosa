using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryEffect : MonoBehaviour
{
    [SerializeField] float _healAmount;
    [SerializeField] private Collider2D col;

    public void OnGetBerry()
    {
        Debug.Log("베리 습득");
        PlayerRef.Instance.state.Heal(_healAmount);
        // 2번 획득 가능한 거 방지
        if (col)
            col.enabled = false;
    }
}
