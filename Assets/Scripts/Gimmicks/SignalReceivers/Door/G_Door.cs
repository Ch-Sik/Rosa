using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Com.LuisPedroFonseca.ProCamera2D;

/*
[Serializable]
public struct cinematicsSetting
{
    public float easeInDur;     //줌인 시간
    public float holdDur;       //정지 시간
    public float zoomAmount;    //얼마나 줌인
}
*/

public class G_Door : GimmickSignalReceiver
{
    [SerializeField] private BoxCollider2D col;
    [SerializeField] private Transform doorSprite;
    [SerializeField] private float openTime;
    [SerializeField] private float openDelay = 1.5f;                                      //Procam2dCinem은 기본 1초의 Easing 타임을 ㅏㄱ짐.
    GameObject cam;
    ProCamera2DCinematics cinematics;
    public List<cinematicsSetting> cinematicsSettings = new List<cinematicsSetting>();

    private void Start()
    {
        cam = Camera.main.gameObject;
        cinematics = cam.GetComponent<ProCamera2DCinematics>();
        // col.enabled = true;
    }

    public void Init(bool activated)
    {
        if (activated)
        {
            col.enabled = false;
            doorSprite.localPosition = new Vector3(0, 2.5f, 0);
        }
    }

    public override void OnAct()
    {
        Open();
    }

    public void Open()
    {
        Cinematic();
        Sequence sq = DOTween.Sequence()
            .AppendInterval(openDelay)
            .Append(doorSprite.DOMoveY(1.5f, openTime * 0.6f).SetRelative(true))
            .AppendCallback(() =>
            {
                col.enabled = false;
            })
            .Append(doorSprite.DOMoveY(1f, openTime * 0.4f).SetRelative(true));
    }

    public void Cinematic()
    {
        if (cinematics != null)
        {
            while (cinematics.CinematicTargets.Count > 0)
            {
                cinematics.CinematicTargets.RemoveAt(0);
            }

            for (int i = 0; i < cinematicsSettings.Count; i++)
            {
                cinematics.AddCinematicTarget(transform, cinematicsSettings[i].easeInDur, cinematicsSettings[i].holdDur, cinematicsSettings[i].zoomAmount);
            }

            cinematics.Play();
            //추후에 이벤트 취급으로 하여 플레이어의 조작을 막을 필요가 있음
        }
    }

    public override void ImmediateOnAct()
    {
        doorSprite.DOMoveY(2.5f, 0f).SetRelative(true);
        col.enabled = false;
    }

    public override void OffAct()
    {
        if (!isOnAct) return;
    }



    public override void ImmediateOffAct()
    {
        if (!isOnAct) return;
    }
}
