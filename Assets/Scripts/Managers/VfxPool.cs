using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class VfxPool : Object
{
    private VfxPoolEntity prefab;
    private IObjectPool<VfxPoolEntity> pool;
    private Transform vfxParent;
    public VfxPool(VfxPoolEntity target, int maxPoolCount, Transform vfxParent)
    {
        prefab = target;
        this.vfxParent = vfxParent;

        pool = new ObjectPool<VfxPoolEntity>(
                CreatePooledItem,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                true,
                1,
                maxPoolCount
            );
    }

    public VfxPoolEntity Get()
    {
        return pool.Get();
    }

    public void Release(VfxPoolEntity target)
    {
        pool.Release(target);
    }

    public void Clear()
    {
        pool.Clear();
    }

    VfxPoolEntity CreatePooledItem()
    {
        VfxPoolEntity newPoolItem = Instantiate(prefab);
        newPoolItem.transform.SetParent(vfxParent);
        newPoolItem.SetPoolToRelease(pool);
        newPoolItem.gameObject.SetActive(false);
        return newPoolItem;
    }

    void OnTakeFromPool(VfxPoolEntity item)
    {
        item.gameObject.SetActive(true);
    }

    void OnReturnedToPool(VfxPoolEntity item)
    {
        item.gameObject.SetActive(false);
    }

    void OnDestroyPoolObject(VfxPoolEntity item)
    {
        if (item != null) 
            Destroy(item.gameObject);
    }
}
