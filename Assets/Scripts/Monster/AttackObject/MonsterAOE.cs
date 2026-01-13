using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <br>Attack Of Range. 몬스터의 범위 공격 판정을 관리하는 스크립트</br>
/// 처음 스폰되었을 때에는 공격판정이 없고 공격 범위 표시의 역할만 하다가,
/// ExecuteAttack이 호출되면 비로소 스프라이트가 바뀌면서 공격판정이 생김
/// </summary>
public class MonsterAOE : MonoBehaviour
{
    [SerializeField]
    private new Collider2D collider;
    [SerializeField, Tooltip("공격이 완료/취소되었을 때 참이면 오브젝트 삭제, 거짓이면 오브젝트 비활성화")]
    private bool destroyOnAttackEnd = true;

    // 위 것들은 에러 방지용으로 남겨뒀음. 240906 시연회 끝나면 아래만 남기고 위는 삭제할 것.
    [SerializeField] private GameObject startupSprite;
    [SerializeField] private GameObject activatedSprite;
    [SerializeField] private Animator animator;

    public void Init()
    {
        gameObject.SetActive(true);
        if(collider == null)
        {
            collider = GetComponent<Collider2D>();
            Debug.Assert(collider != null, $"{gameObject.name}: Collider2D를 찾을 수 없음");
        }
        collider.enabled = false;
        if(startupSprite != null)
            startupSprite.SetActive(true);
        if(activatedSprite != null)
            activatedSprite.SetActive(false);
    }

    public void ExecuteAttack()
    {
        // Debug.Log("범위 공격 수행");
        collider.enabled = true;

        if (animator != null)
        {
            // Animator 리셋
            animator.Rebind();
            animator.Update(0f);
            // 애니메이션 수행
            animator.SetTrigger("Activate");
        }
        else
        {
            if (startupSprite != null)
                startupSprite.SetActive(false);
            if (activatedSprite != null)
                activatedSprite.SetActive(true);
        }

        if (destroyOnAttackEnd)
            Destroy(gameObject, 1f);
        else
            StartCoroutine(SetActiveWithDelay(false, 1f));
    }

    public void CancelAttack()
    {
        // Debug.Log("공격 취소");
        if (destroyOnAttackEnd)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private IEnumerator SetActiveWithDelay(bool value, float delay)
    {
        yield return new WaitForSeconds( delay );
        gameObject.SetActive( value );
    }
}
