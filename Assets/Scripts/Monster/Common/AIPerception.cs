using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적을 감지. 시각/후각/청각 등등에 해당
/// 몬스터 최상단 오브젝트의 자식클래스에 부착할 것. (자손 x)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AIPerception : MonoBehaviour
{
    [SerializeField] Blackboard blackboard;

    protected virtual void Awake()
    {
        if (blackboard == null)
        {
            blackboard = transform.parent.GetComponent<Blackboard>();
            if (blackboard == null)
            {
                Debug.LogError($"{transform.parent.name}: Blackboard를 찾을 수 없음!");
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            blackboard.Set("Enemy", col.gameObject);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            blackboard.Set<GameObject>("Enemy", null);
        }
    }
}
