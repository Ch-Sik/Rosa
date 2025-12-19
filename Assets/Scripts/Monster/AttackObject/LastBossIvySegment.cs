using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastBossIvySegment : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator anim;
    
    void Start()
    {
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
    }

    public void Disappear()
    {
        if(anim != null)
            anim.SetTrigger("Disappear");
    }
}
