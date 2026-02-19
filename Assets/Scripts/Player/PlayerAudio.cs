using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : SFXPlayer
{
    [InfoBox("AniPortrait에서 보낸 message 수신해서 플레이어 발소리, 점프 소리 등 재생")]
    
    [SerializeField] private AudioResource footstep;
    [SerializeField] private AudioResource climb;
    [SerializeField] private AudioResource jump;
    [SerializeField] private AudioResource dash;
    [SerializeField] private AudioResource mushJump;
    [SerializeField] private AudioResource glideOn;
    [SerializeField] private AudioResource glideLoop;
    [SerializeField] private AudioResource damaged;

    protected override void Start()
    {
        base.Start();
        var playerMove = GetComponent<PlayerMovement>();
        playerMove.OnJump += PlayJumpSound;
        playerMove.OnDash += PlayDashSound;
        playerMove.OnMushJump += PlayMushJumpSound;
        playerMove.OnGlideStart += PlayGlideSoundOn;
        playerMove.OnGlideStart += PlayGlideSoundOff;
        
        var damageReceiver = GetComponent<PlayerDamageReceiver>();
        damageReceiver.OnDamaged += PlayDamagedSound;
    }

    public void PlayFootstepSound()
    {
        audioSource.PlayOneShot(footstep, footstep.volume);
    }

    public void PlayClimbSound()
    {
        audioSource.PlayOneShot(climb,  climb.volume);
    }

    private void PlayJumpSound()
    {
        audioSource.PlayOneShot(jump,  jump.volume);
    }

    private void PlayDashSound()
    {
        audioSource.PlayOneShot(dash, dash.volume);
    }

    private void PlayMushJumpSound()
    {
        audioSource.PlayOneShot(mushJump, mushJump.volume);
    }

    private void PlayGlideSoundOn()
    {
        audioSource.PlayOneShot(glideOn, glideOn.volume);

        if (!glideLoop)
        {
            audioSource.clip = glideLoop;
            audioSource.volume = glideLoop.volume;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void PlayGlideSoundOff()
    {
        audioSource.Stop();
    }

    private void PlayDamagedSound()
    {
        audioSource.PlayOneShot(damaged, damaged.volume);
    }
}
