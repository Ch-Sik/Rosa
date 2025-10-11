using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G_Draft : GimmickSignalReceiver
{
    public bool isActivated = false;
    public float risingPower = 3.0f;
    public GameObject[] particles;
    public Animator[] fanAnimators;

    private void Start()
    {
        ToggleParticles(isActivated);
        ToggleSpriteAnimation(isActivated);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isActivated)
            return;
        if (!collision.CompareTag("Player"))
            return;

        PlayerRef.Instance.movement.Rising(risingPower);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isActivated)
            return;
        if (!collision.CompareTag("Player"))
            return;

        PlayerRef.Instance.movement.CancleRising();
    }

    public override void OnAct()
    {
        isActivated = true;
        ToggleParticles(true);
        ToggleSpriteAnimation(true);
    }

    public override void OffAct()
    {
        isActivated = false;
        ToggleParticles(false);
        ToggleSpriteAnimation(false);
    }

    private void ToggleParticles(bool value)
    {
        if(value == true)
            for(int i=0; i<particles.Length; i++)
            {
                particles[i].GetComponent<ParticleSystem>().Play();
            }
        else
            for(int i=0; i<particles.Length; i++)
            {
                particles[i].GetComponent<ParticleSystem>().Stop();
            }
    }

    private void ToggleSpriteAnimation(bool value)
    {
        foreach(var anim in fanAnimators)
        {
            anim.SetBool("running", value);
        }
    }

    public override void ImmediateOnAct()
    {
        isActivated = true;
        ToggleParticles(true);
        ToggleSpriteAnimation(true);
    }

    public override void ImmediateOffAct()
    {
        isActivated = false;
        ToggleParticles(false);
        ToggleSpriteAnimation(false);
    }
}
