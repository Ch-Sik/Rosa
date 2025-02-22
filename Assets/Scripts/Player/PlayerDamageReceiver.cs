using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour
{
    [SerializeField] float defaultNoDmgTime = 2f;
    public bool ignoreDamage = false;

    PlayerRef playerRef;
    bool isJustEndedIgnoreTime = false; // 해당 트리거 켜져있는 동안 플레이어는 '밟기' 수행할 수 없음.

    public void Start()
    {
        playerRef = PlayerRef.Instance;
    }

    public void GetDamage(GameObject target, int damage, bool isDamageFromMonsterBody = false)
    {
        if (ignoreDamage) return;
        
        // if(!isJustEndedIgnoreTime && isDamageFromMonsterBody && target.transform.position.y < transform.position.y - 0.73f)
        // {
        //     Debug.Log("몬스터가 플레이어보다 아래에 있음 = 플레이어가 밟은 상황, 데미지 무시");
        //     return;
        // }

        Debug.Log("플레이어가 다음 대상에게 피격됨:" + target.name);

        playerRef.animation.BlinkEffect();
        playerRef.animation.SetTrigger("Hit");

        playerRef.movement.Knockback(new Vector2(target.transform.position.x,
                    target.transform.position.y - (target.transform.localScale.y / 2)));
        playerRef.state.TakeDamage(damage);

        int originalLayer = target.layer;
        int collisionLayer = gameObject.layer;

        // 현재 게임 오브젝트와 충돌한 오브젝트의 충돌을 무시
        Debug.Log("플레이어 무적 시작");
        Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, true);

        // 일정 시간 후 충돌 무시 해제
        StartCoroutine(RestoreCollision(originalLayer, collisionLayer, defaultNoDmgTime));
    }

    public void GetDamage(GameObject target, int damage, float ignoreDur, bool isDamageFromMonsterBody = false)
    {
        if(isDamageFromMonsterBody && target.transform.position.y < transform.position.y - 0.73f)
        {
            Debug.Log("몬스터가 플레이어보다 아래에 있음 = 플레이어가 밟은 상황, 데미지 무시");
            return;
        }

        Debug.Log($"다음으로부터 피격: {target.name}");

        playerRef.animation.BlinkEffect();
        playerRef.animation.SetTrigger("Hit");

        playerRef.movement.Knockback(new Vector2(target.transform.position.x,
                    target.transform.position.y - (target.transform.localScale.y / 2)));
        playerRef.state.TakeDamage(damage);

        int originalLayer = target.layer;
        int collisionLayer = gameObject.layer;

        // 현재 게임 오브젝트와 충돌한 오브젝트의 충돌을 무시
        Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, true);

        // 일정 시간 후 충돌 무시 해제
        StartCoroutine(RestoreCollision(originalLayer, collisionLayer, ignoreDur));
    }

    IEnumerator RestoreCollision(int originalLayer, int collisionLayer, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 충돌 무시 해제
        Debug.Log("플레이어 무적 종료");
        Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, false);

        // 트리거 설정
        isJustEndedIgnoreTime = true;
        yield return new WaitForFixedUpdate();  // 확실하게 FixedUpdate 한번이 끝날 떄까지 기다림
        isJustEndedIgnoreTime = false;
    }

    public void SetNoDmgForSeconds(float duration)
    {
        ignoreDamage = true;
        StartCoroutine(Co_RestoreInvincible());
        IEnumerator Co_RestoreInvincible()
        {
            yield return new WaitForSeconds(duration);
            ignoreDamage = false;
        }

    }
}
