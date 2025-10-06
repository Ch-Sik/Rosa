using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VfxPlayer : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] string animStateName;
    [SerializeField] AudioSource audioSource;

    public void PlayVFX()
    {
        if(animator)
            animator.Play(animStateName, -1, 0);
        if(audioSource)
            audioSource.Play();
    }
}
