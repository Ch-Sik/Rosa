using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastBossIvy : MonoBehaviour
{
    [SerializeField] LastBossIvySegment segmentPrefab;
    [SerializeField] float growHeight;
    [SerializeField] float growPerSec;
    [SerializeField] float unitPerSegment;
    [SerializeField] float lifetime;
    [Title("사인파 형태 조정")]
    [SerializeField] float frequency;
    [SerializeField] float amplitude;
    [Title("사운드")]
    [SerializeField] private SFXPlayer disappearSfx;

    List<LastBossIvySegment> segments = new List<LastBossIvySegment>();

    // Start is called before the first frame update
    void Start()
    {
        int segmentCount = Mathf.CeilToInt(growHeight / unitPerSegment);
        for (int i = 0; i < segmentCount; i++)
        {
            var instance = Instantiate(segmentPrefab, transform);
            instance.gameObject.SetActive(false);
            segments.Add(instance);
        }
    }

    [Button]
    public void Reset()
    {
        StopAllCoroutines();
        foreach(var o in segments)
        {
            o.gameObject.SetActive(false);
        }
    }

    [Button]
    public void StartGrow()
    {
        StartCoroutine(co_Grow());
        Invoke("Disappear", lifetime);

        IEnumerator co_Grow()
        {
            float secPerSegment = (1 / growPerSec) * (unitPerSegment);
            float vias = Random.Range(0, 2 * Mathf.PI);

            for (int i = 0; i < segments.Count; i++)
            {
                // 각 알맹이의 위치 계산
                float growth = i * unitPerSegment;
                float x = transform.position.x + Mathf.Sin(growth * frequency + vias) * amplitude;
                float y = transform.position.y + growth;
                // 알맹이 위치 지정 & 활성
                segments[i].gameObject.SetActive(true);
                segments[i].transform.position = new Vector3(x, y, transform.position.z);
                // 딜레이 부여
                yield return new WaitForSeconds(secPerSegment);
            }
        }
    }

    public void Disappear()
    {
        disappearSfx?.PlaySfx();
        
        // 덩굴 자라는 도중이었다면 해당 코루틴 중단
        StopAllCoroutines();
        foreach(var seg in segments)
            seg.Disappear();
        Destroy(gameObject, 0.5f);
        // 덩굴 자라는 도중에 삭제 호출되면 Invoke로 인해 Disappear 두번째 호출되는 것 방지
        CancelInvoke();
    }
}
