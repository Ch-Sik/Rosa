using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BerrySpriteController : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject spriteObject;

    Sequence hovering;
    const float hoveringLoopDuration = 2f;
    const float hoveringTravel = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];

        // 위아래로 움직이는 트윈 추가
        spriteObject.transform.localPosition = new Vector3(0, -hoveringTravel / 2, 0);
        hovering = DOTween.Sequence()
            .Append(spriteObject.transform.DOLocalMoveY( hoveringTravel / 2, hoveringLoopDuration / 2 ))
            .Append(spriteObject.transform.DOLocalMoveY( -hoveringTravel / 2, hoveringLoopDuration / 2 ))
            .SetLoops(-1);
        // 랜덤 시간 오프셋 추가
        hovering.Goto(Random.Range(0, hoveringLoopDuration), true);
    }

    public void Disappear()
    {
        // 사라지는 연출
        hovering.Kill();
        DOTween.Sequence()
            .Append(spriteObject.transform.DOLocalMoveY(1f, 0.3f).SetEase(Ease.OutQuint))
            .Insert(0f, spriteObject.transform.DOLocalRotate(new Vector3(0, 720, 0), 0.3f, RotateMode.LocalAxisAdd))
            .Insert(0.1f, spriteRenderer.DOColor(new Color(1, 1, 1, 0), 0.2f).SetEase(Ease.OutCubic))
            .OnComplete(() => { Destroy(gameObject); });
    }
}
