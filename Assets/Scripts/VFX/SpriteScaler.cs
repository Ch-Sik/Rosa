using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteScaler : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Transform targetTransform;

    // Update is called once per frame
    void Update()
    {
        if (!spriteRenderer || !targetTransform) return;
        spriteRenderer.size = targetTransform.localScale;
    }
}
