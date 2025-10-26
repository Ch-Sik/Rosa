using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DropItemVisual : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject spriteObject;
    [SerializeField] ParticleSystem particle;

    private Sequence _hovering;
    private const float HoveringLoopDuration = 2f;
    private const float HoveringTravel = 0.1f;

    // Start is called before the first frame update
    private void Start()
    {
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];

        // 위아래로 움직이는 트윈 추가
        spriteObject.transform.localPosition = new Vector3(0, -HoveringTravel / 2, 0);
        _hovering = DOTween.Sequence()
            .Append(spriteObject.transform.DOLocalMoveY( HoveringTravel / 2, HoveringLoopDuration / 2 ))
            .Append(spriteObject.transform.DOLocalMoveY( -HoveringTravel / 2, HoveringLoopDuration / 2 ))
            .SetLoops(-1);
        // 랜덤 시간 오프셋 추가
        _hovering.Goto(Random.Range(0, HoveringLoopDuration), true);
    }

    // 사라지는 연출
    // GameObject.Destroy는 DropItem.cs 등 다른 클래스에서 담당
    public void Disappear()
    {
        if(particle)
            particle.Stop();
        
        _hovering.Kill();
        DOTween.Sequence()
            .Append(spriteObject.transform.DOLocalMoveY(1f, 0.3f).SetEase(Ease.OutQuint))
            .Insert(0f, spriteObject.transform.DOLocalRotate(new Vector3(0, 720, 0), 0.3f, RotateMode.LocalAxisAdd))
            .Insert(0.1f, spriteRenderer.DOColor(new Color(1, 1, 1, 0), 0.2f).SetEase(Ease.OutCubic));
    }

    public void OnDestroy()
    {
        _hovering?.Kill();
    }
}
