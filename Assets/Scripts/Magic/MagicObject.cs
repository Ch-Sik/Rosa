using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicObject : MonoBehaviour
{
    Coroutine destroyCoroutine;

    public virtual void Init(Vector2 magicPos, float lifeTime, TilemapGroup tilemapGroup)
    {
#if !UNITY_STANDALONE
        endTime = Time.time + lifeTime;
#endif
        StartCoroutine(DestroyByLifetime(lifeTime));
    }

    private IEnumerator DestroyByLifetime(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        // TODO: 식물마법 사라지기 전 연출 추가
        Destroy(gameObject);
    }

#if !UNITY_STANDALONE
    [SerializeField, ReadOnly]
    private float leftTime;
    private float endTime;

    private void Update()
    {
        // 디버그용
        leftTime = endTime - Time.time;
    }
#endif
}
