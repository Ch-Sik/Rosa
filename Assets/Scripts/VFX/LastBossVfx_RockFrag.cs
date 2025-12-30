using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class LastBossVfx_RockFrag : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fragPrefab;
    [SerializeField] private List<Sprite> fragSprites;
    [SerializeField] private int fragCount;
    [SerializeField] private Vector2 areaCenter;
    [SerializeField] private Vector2 areaSize;
    [SerializeField] private bool drawGizmos;
    [SerializeField] private float riseDuration = 1f;
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private float hoverTravel = 0.4f;      // 위아래로 둥실거리는 거리
    [SerializeField] private float groundY = -10f;

    private List<SpriteRenderer> _instanceList = new();
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < fragCount; i++)
        {
            var instance = Instantiate(fragPrefab);
            instance.sprite = fragSprites[Random.Range(0, fragSprites.Count)];
            instance.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            instance.transform.position = new Vector3(
                Random.Range(areaCenter.x - areaSize.x, areaCenter.x + areaSize.x),
                Random.Range(areaCenter.y - areaSize.y, areaCenter.y + areaSize.y),
                0);
            _instanceList.Add(instance);
        }
        RiseFrags();
    }
    
    public void RiseFrags()
    {
        foreach (var frag in _instanceList)
        {
            if (frag == null) continue;

            Transform t = frag.transform;

            // 1. 기존 트윈 제거 (낙하 중이거나 다른 트윈이 있다면 즉시 중단)
            t.DOKill(true);

            // 2. 자연스러움을 위한 랜덤 값 설정
            float randomDelay = Random.Range(0f, 0.4f);
            float randomDuration = riseDuration * Random.Range(0.8f, 1.2f);
            
            // 3. 목표 높이 설정 (기준 높이 + 약간의 오차)
            float targetY = Random.Range(areaCenter.y - areaSize.y, areaCenter.y + areaSize.y);

            // 4. 시퀀스 생성: 바닥 -> 상승 -> 둥둥거림(Loop)
            Sequence seq = DOTween.Sequence();
            
            // 단계 A: 현재 위치에서 목표 높이까지 상승
            seq.Append(t.DOLocalMoveY(targetY, randomDuration).SetEase(Ease.OutQuad));
            
            // 단계 B: 둥둥거리는 루프 (위아래로 움직임)
            // MoveY를 통해 현재 위치 기준 위아래로 움직이게 설정
            seq.Append(t.DOLocalMoveY(targetY + hoverTravel, 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo));

            // 딜레이 적용 후 재생
            seq.SetDelay(randomDelay);
            seq.SetLink(frag.gameObject); // 오브젝트 파괴 시 트윈도 같이 파괴되도록 안전장치
            seq.SetTarget(frag.transform);
        }
    }

    public void FallFrags()
    {
        foreach (var frag in _instanceList)
        {
            if (frag == null) continue;

            Transform t = frag.transform;

            // 1. 부유(Rise) 트윈 즉시 중단
            t.DOKill();

            // 2. 바닥으로 낙하
            // 현재 높이에서 바닥(groundY)까지 떨어짐
            t.DOLocalMoveY(groundY, fallDuration)
                .SetEase(Ease.OutBounce) // 쿵 하고 떨어지는 느낌 (취향에 따라 InQuad 등으로 변경 가능)
                .SetLink(frag.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(areaCenter, areaSize * 2);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(-10, groundY, 0), new Vector3(10, groundY, 0));
    }
}
