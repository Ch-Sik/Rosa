using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <br>Attack Of Range. 몬스터의 범위 공격 판정을 관리하는 스크립트</br>
/// 처음 스폰되었을 때에는 공격판정이 없고 공격 범위 표시의 역할만 하다가,
/// ExecuteAttack이 호출되면 비로소 스프라이트가 바뀌면서 공격판정이 생김
/// </summary>
public class SunkenFlowerAOE : MonsterAOE
{
    protected override void Start()
    {
        // Do nothing
    }
    
    
}
