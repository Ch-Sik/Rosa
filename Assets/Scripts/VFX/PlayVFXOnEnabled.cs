using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayVFXOnEnabled : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] string animStateName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        animator.Play(animStateName, -1, 0);
    }
}
