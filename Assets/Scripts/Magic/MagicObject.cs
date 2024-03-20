using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicObject : MonoBehaviour
{
    Coroutine destroyCoroutine;
    SpriteRenderer[] sprites;

    public virtual void Init(Vector2 magicPos, float lifeTime, TilemapGroup tilemapGroup)
    {
#if UNITY_STANDALONE
        endTime = Time.time + lifeTime;
#endif
        sprites = GetComponentsInChildren<SpriteRenderer>();
        StartCoroutine(DestroyByLifetime(lifeTime));
    }

#if UNITY_STANDALONE
    [SerializeField, ReadOnly]
    private float leftTime;
    private float endTime;

    private void Update()
    {
        // 디버그용
        leftTime = endTime - Time.time;
    }
#endif

    private IEnumerator DestroyByLifetime(float lifeTime)
    {
        float destroyPreviewTime = 3f;
        float timeTilDestroyReady = lifeTime - destroyPreviewTime;


        Debug.Assert(lifeTime > destroyPreviewTime, "식물 유지 시간이 너무 짧음!");
        yield return new WaitForSeconds(timeTilDestroyReady);

        // 2초동안은 .25초 간격으로 깜빡거리고
        for(int i = 0; i<4; i++)
        {
            setSpritesColor(Color.gray);
            yield return new WaitForSeconds(0.25f);
            setSpritesColor(Color.white);
            yield return new WaitForSeconds(0.25f);
        }
        // 사라지기 1초 전에는 .125초 간격으로 깜빡거림
        for(int i=0; i<8; i++)
        {
            setSpritesColor(Color.gray);
            yield return new WaitForSeconds(0.125f);
            setSpritesColor(Color.white);
            yield return new WaitForSeconds(0.125f);
        }
        
        Destroy(gameObject);
    }

    private void setSpritesColor(Color color)
    {
        foreach(var sprite in sprites)
        {
            sprite.color = color;
        }
    }
}
