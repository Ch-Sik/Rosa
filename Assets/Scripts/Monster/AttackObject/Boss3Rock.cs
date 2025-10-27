using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Boss3Rock : ProjectileBase
{
    protected override void HandleHitWall()
    {
        CameraShake.ShakeCamera(CameraShakePreset.Large, true);
        base.HandleHitWall();
    }
}
