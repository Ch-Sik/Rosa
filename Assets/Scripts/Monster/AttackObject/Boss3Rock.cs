using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Boss3Rock : ProjectileBase
{
    protected override void HandleProjMoveOnHit(ProjectileWallHitOption option)
    {
        CameraShake.ShakeCamera(CameraShakePreset.Large, true);
        base.HandleProjMoveOnHit(option);
    }
}
