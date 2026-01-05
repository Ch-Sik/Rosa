using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePointInteraction : WalkToInteraction
{
    [SerializeField] private VfxPlayer vfxPlayer;
    
    protected override void OnAfterWalk()
    {
        PlayerRef.Instance.movement.SitOnChair();
        PlayerRef.Instance.state.Heal(5.0f);
        vfxPlayer?.PlayVfx();
        SaveLoadManager.Instance.SavePlayData();
        Debug.Log("[SavePointInteraction] Saved play data]");
    }
}
