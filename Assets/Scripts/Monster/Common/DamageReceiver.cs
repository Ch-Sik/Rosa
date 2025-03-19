using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    /// <summary>
    /// 리턴값 true면 공격이 제대로 들어갔다는 것, false면 공격이 제대로 안먹혔다는 것(무적 상태 등)
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="attackAngle"></param>
    /// <returns></returns>
    public virtual bool GetHitt(int damage, float attackAngle)
    {
        Debug.LogWarning("virtual method NOT OVERRIDED!\n");
        return false;
    }
}
