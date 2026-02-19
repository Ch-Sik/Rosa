using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BreachPlatformSFX : MonoBehaviour
{
    [SerializeField] private MovePlatform movePlatform;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioResource breachStart;
    [SerializeField] private AudioResource breachEnd;
    
    // Start is called before the first frame update
    void Start()
    {
        movePlatform.OnBreachStart += PlayBreachStartSfx;
        movePlatform.OnBreachEnd += PlayBreachEndSfx;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayBreachStartSfx()
    {
        audioSource.PlayOneShot(breachStart, breachStart.volume);
    }

    void PlayBreachEndSfx()
    {
        audioSource.PlayOneShot(breachEnd, breachEnd.volume);
    }
}
