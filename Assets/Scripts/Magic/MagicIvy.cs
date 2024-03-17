using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MagicIvy : MagicObject
{
    [SerializeField]
    private EdgeCollider2D edgeCol;     // 0번 점이 위, 1번 점이 아래
    [SerializeField]
    private Vector3Int originCellPos;   // 담쟁이가 설치된 타일 위치
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private SpriteRenderer spriteUp;
    [SerializeField]
    private SpriteRenderer spriteDown;
    [SerializeField]
    private ParticleSystem particle1Up;
    [SerializeField]
    private ParticleSystem particle2Up;
    [SerializeField]
    private ParticleSystem particle1Down;
    [SerializeField]
    private ParticleSystem particle2Down;
    [SerializeField]
    private AudioClip[] sfx;

    [SerializeField, Tooltip("덩굴이 성장할 때의 틱 속도")]
    private float tickTime = 0.1f;
    [SerializeField, Tooltip("덩굴 스프라이트가 벽과 떨어진 정도 (만약 음수면 벽을 파고들어감)")]
    private float offset;

    private TilemapGroup targetTilemapGroup;
    private LR wallDirection;   // 현재 담쟁이가 자라는 벽이 바라보는 방향
    private int upLength = 0;
    private int downLength = 0;
    private WaitForSeconds tick;

    public override void Init(Vector2 magicPos, float lifeTime, TilemapGroup tilemapGroup)
    {
        base.Init(magicPos, lifeTime, tilemapGroup);

        // 필드 값 세팅
        if(edgeCol == null) edgeCol = GetComponent<EdgeCollider2D>();
        targetTilemapGroup = tilemapGroup;

        // 오른쪽 벽에 부딪혔는지 왼쪽 벽에 부딪혔는지 판단
        // 0.5의 오프셋을 주어서 오른쪽 셀 검사
        originCellPos = targetTilemapGroup.WorldToCell(magicPos + new Vector2(0.5f, 0));
        TileData originCellData = targetTilemapGroup.GetTileData(originCellPos);
        if (originCellData != null && originCellData.isSubstance)
        {
            wallDirection = LR.LEFT;
            Debug.Log("왼쪽 보는 벽에 담쟁이 설치");
        }
        else
        {
            wallDirection = LR.RIGHT;
            Debug.Log("오른쪽 보는 벽에 담쟁이 설치");
        }

        // y축 방향 위치 보정
        magicPos.y = Mathf.Round(magicPos.y - 0.5f) + 0.5f;
        transform.position = magicPos;
        originCellPos = targetTilemapGroup.WorldToCell(magicPos + new Vector2(0.5f, 0));

        // 최적화
        tick = new WaitForSeconds(tickTime);

        // 초기화가 끝나면 코루틴 실행
        StartCoroutine(GrowVineUpway());
        StartCoroutine(GrowVineDownway());
    }

    IEnumerator GrowVineUpway()
    {
        Vector2[] points;

        while(true)
        {
            // 한칸씩 위쪽으로 가면서 타일 검사
            upLength++;
            Vector3 centerPos = transform.position + upLength * Vector3.up;
            Vector3 innerPos = centerPos - (Vector3)(0.5f * wallDirection.toVector2());
            Vector3 outerPos = centerPos + (Vector3)(0.5f * wallDirection.toVector2());

            Vector3Int innerCellPos = targetTilemapGroup.WorldToCell(innerPos);
            Vector3Int outerCellPos = targetTilemapGroup.WorldToCell(outerPos);

            TileData innerData = targetTilemapGroup.GetTileData(innerCellPos);
            TileData outerData = targetTilemapGroup.GetTileData(outerCellPos);

            bool isInnerTileMagicAllowed = innerData != null && innerData.isPlantable;
            bool isOuterTileNonSubstance = !outerData || !outerData.isSubstance;

            if (isInnerTileMagicAllowed && isOuterTileNonSubstance)
            {
                points = edgeCol.points;
                points[0] = new Vector2(-offset, upLength);     // 0번 포인트가 위쪽, 1번 포인트가 아래쪽
                edgeCol.points = points;
                // 스프라이트 업데이트 (콜라이더와 동기화)
                spriteUp.size = new Vector2(spriteUp.size.x, upLength);

                // 파티클 효과
                if (particle1Up != null)
                {
                    particle1Up.gameObject.transform.position = transform.position + Vector3.up * upLength;
                    particle1Up.Emit(1);
                    if(particle2Up != null)
                        particle2Up.Emit(5);
                }

                // 사운드 효과
                PlaySFX();
            }
            else
            {
                upLength--;
                break;
            }
            yield return tick;
        }

        // 마지막으로 끝까지 성장
        points = edgeCol.points;
        points[0] = new Vector2(-offset, upLength + 0.5f);
        edgeCol.points = points;
        // 스프라이트 업데이트 (콜라이더와 동기화)
        spriteUp.size = new Vector2(spriteUp.size.x, upLength + 0.5f);
    }

    IEnumerator GrowVineDownway()
    {
        Vector2[] points;

        // 한칸씩 위쪽으로 가면서 프로브
        while (true)
        {
            // 한칸씩 아래쪽으로 가면서 타일 검사
            downLength--;
            Vector3 centerPos = transform.position + downLength * Vector3.up;
            Debug.Log(centerPos);
            Vector3 innerPos = centerPos - (Vector3)(0.5f * wallDirection.toVector2());
            Vector3 outerPos = centerPos + (Vector3)(0.5f * wallDirection.toVector2());

            Vector3Int innerCellPos = targetTilemapGroup.WorldToCell(innerPos);
            Vector3Int outerCellPos = targetTilemapGroup.WorldToCell(outerPos);

            TileData innerData = targetTilemapGroup.GetTileData(innerCellPos);
            TileData outerData = targetTilemapGroup.GetTileData(outerCellPos);

            bool isInnerTileMagicAllowed = innerData != null && innerData.isPlantable;
            bool isOuterTileNonSubstance = !outerData || !outerData.isSubstance;

            if (isInnerTileMagicAllowed && isOuterTileNonSubstance)
            {
                points = edgeCol.points;
                points[1] = new Vector2(-offset, downLength);     // 0번 포인트가 위쪽, 1번 포인트가 아래쪽
                edgeCol.points = points;
                // 스프라이트 업데이트 (콜라이더와 동기화)
                spriteDown.size = new Vector2(spriteDown.size.x, -downLength);

                // 파티클 효과
                if(particle1Down != null)
                {
                    particle1Down.gameObject.transform.position = transform.position + Vector3.up * downLength;
                    particle1Down.Emit(1);
                    if(particle2Down != null)
                        particle2Down.Emit(5);
                }

                // 사운드 효과
                PlaySFX();
            }
            else
            {
                downLength++;
                break;
            }
            yield return tick;
        }

        // 마지막으로 끝까지 성장
        points = edgeCol.points;
        points[1] = new Vector2(-offset, downLength - 0.5f);
        edgeCol.points = points;
        // 스프라이트 업데이트 (콜라이더와 동기화)
        spriteDown.size = new Vector2(spriteDown.size.x, -(downLength - 0.5f));
    }

    private void PlaySFX()
    {
        if (audioSource == null || sfx == null || sfx.Length == 0)
            return;
        audioSource.PlayOneShot(sfx[Random.Range(0,sfx.Length)]);
    }
    
    // TODO: Destroy/OnDestroy 대신 별도의 함수를 사용하여 '사라지는 연출' 구현
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
