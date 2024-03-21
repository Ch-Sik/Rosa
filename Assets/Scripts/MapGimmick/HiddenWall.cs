using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HiddenWall : MonoBehaviour
{
    [ReadOnly]
    public bool revealed = false;

    [SerializeField]
    private float fadeTime = 0.5f;
    [SerializeField]
    public int fadeStep = 10;

    private Tilemap tilemap;
    private BoundsInt hiddenWallArea;

    Coroutine fade = null;
    WaitForSeconds fadeTick = null;       // 코루틴 최적화

    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        Collider2D col = GetComponent<Collider2D>();

        // 타일맵 범위 계산
        Vector3Int boundsMin = Vector3Int.FloorToInt(col.bounds.min);
        Vector3Int boundsMax = Vector3Int.CeilToInt(col.bounds.max + new Vector3(0, 0, 1));
        // ↑↑ (boundsMax-boundsMin)의 xyz중 하나이상이 0이면 allPositionsWithin이 동작하지 않음. 
        hiddenWallArea = new BoundsInt(boundsMin, boundsMax - boundsMin);

        fadeTick = new WaitForSeconds(fadeTime / fadeStep);

        foreach (Vector3Int point in hiddenWallArea.allPositionsWithin)
        {
            // 타일 컬러 고정 플래그 해제. 타일 컬러 변경을 위해 필요.
            tilemap.SetTileFlags(point, TileFlags.None);
        }
    }

    // private void OnTriggerEnter2D(Collider2D collision)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!collision.collider.CompareTag("Player"))
        { return; }

        if(fade != null)
        {
            StopCoroutine(fade);
        }
        fade = StartCoroutine(FadeOut());

        IEnumerator FadeOut()
        {
            revealed = true;
            TilemapCollider2D col = GetComponent<TilemapCollider2D>();
            Debug.Log("숨겨진 방 나타남");

            col.enabled = false;
            for (int i=fadeStep; i>= 0; i--)
            {
                foreach (Vector3Int point in hiddenWallArea.allPositionsWithin)
                {
                    tilemap.SetColor(point, new Color(1, 1, 1, i / (float)fadeStep));
                }
                yield return fadeTick;
            }
            GetComponentInParent<TilemapGroup>().RemoveTilemapFromList(GetComponent<Tilemap>());
            Destroy(gameObject);
        }
    }
}
