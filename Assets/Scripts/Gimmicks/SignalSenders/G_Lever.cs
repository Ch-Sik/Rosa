using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G_Lever : GimmickSignalSender
{
    public bool isOnce = true;
    [SerializeField] Transform leverHandle;
    [SerializeField] private SFXPlayer sfxPlayer;

    /// <summary> 레버가 작동하는 도중인지 여부 </summary>
    public bool isInteracting = false;      
    [SerializeField] private InteractiveObject interactiveObject;

    public override void Init(GimmickSignalSenderState state)
    {
        if (GetState() != state)
        {
            SetState(state);
            switch (state)
            {
                case GimmickSignalSenderState.Activated: // Active
                    ToggleOnLever();
                    break;
                case GimmickSignalSenderState.Inactivated: // InActive
                    ToggleOffLever();
                    break;
                default:
                    Debug.LogError("잘못된 enum value");
                    return;
            }
        }
        // ImmediateSendSignal();
    }

    private void ToggleOnLever()
    {
        isInteractable = true;

        if (isOnce)
        {
            interactiveObject.canInteract = false;
            interactiveObject.OnInactive();
        }

        leverHandle.DORotate(new Vector3(0, 0, -90), 0f, RotateMode.LocalAxisAdd).SetRelative(true);
    }

    private void ToggleOffLever()
    {
        isInteractable = false;

        leverHandle.DORotate(new Vector3(0, 0, +90), 0f, RotateMode.LocalAxisAdd).SetRelative(true);
    }

    [Button]
    public void LeverAction()
    {
        // 이미 레버 사용중인 경우 중복 사용 걸러냄
        if (isInteracting)
            return;

        Debug.Log("레버 사용됨");

        // 현재 비활성화 상태인경우 -> 활성화함
        if (GetState() == GimmickSignalSenderState.Inactivated)
        {
            SetState(GimmickSignalSenderState.Activated);
            ActivateSignal(true);
        }
        // 현재 활성화 상태인 경우 -> 비활성화함
        else
        {
            SetState(GimmickSignalSenderState.Inactivated);
            ActivateSignal(false);
        }
    }
    
    public void ActivateSignal(bool value)
    {
        if (value)
            StartCoroutine(ActivateLever());
        else
            StartCoroutine(InactivateLever());

        if (value == true && isOnce)
        {
            isInteractable = false;
            interactiveObject.canInteract = false;
            interactiveObject.OnInactive();
        }
    }

    private IEnumerator ActivateLever()
    {
        sfxPlayer.PlaySfx();
        DOTween.Sequence()
            .AppendCallback(() => isInteracting = true)
            .Append(leverHandle.DORotate(new Vector3(0, 0, -90), 0.4f, RotateMode.LocalAxisAdd).SetRelative(true))
            .AppendCallback(() =>
            {
                isInteracting = false;
                SendSignal();
            });
        yield return 0;
    }

    private IEnumerator InactivateLever()
    {
        DOTween.Sequence()
            .AppendCallback(() => isInteracting = true)
            .Append(leverHandle.DORotate(new Vector3(0, 0, +90), 0.4f, RotateMode.LocalAxisAdd).SetRelative(true))
            .AppendCallback(() =>
            {
                isInteracting = false;
                SendSignal();
            });
        yield return 0;
    }
}
