using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 자기 위치에 Vfx 오브젝트를 스폰하는 단순 유틸 컴포넌트
public class VfxPlayer : MonoBehaviour
{
    public VfxPoolEntity vfxPrefab;

    public void PlayVfx()
    {
        if (vfxPrefab == null)
        {
            Debug.LogWarning("[VfxPlayer] vfxPrefab is null]");
            return;
        }
        VfxManager.Instance.SpawnVfxObject(vfxPrefab, transform.position);
    }
}
