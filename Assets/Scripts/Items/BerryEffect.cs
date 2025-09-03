using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryEffect : MonoBehaviour
{
    [SerializeField] float _healAmount;

    public void OnGetBerry()
    {
        Debug.Log("베리 습득");
        PlayerRef.Instance.state.Heal(_healAmount);
    }
}
