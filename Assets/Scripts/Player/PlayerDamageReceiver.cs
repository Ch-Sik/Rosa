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

    public void GetDamage(GameObject target, int damage)
    {
        GetDamage(target, damage, defaultNoDmgTime);
    }

    public void GetDamage(GameObject target, int damage, float ignoreDur)
    {
        if (ignoreDamage) return;

        Debug.Log("플레이어 피격 from:" + target.name);

        playerRef.animation.BlinkEffect();
        playerRef.animation.SetTrigger("Hit");
        CameraShake.ShakeCamera(CameraShakePreset.PlayerHit);

        Vector2 knockbackOrigin = new Vector2(target.transform.position.x,
                    target.transform.position.y - (target.transform.localScale.y / 2));
        playerRef.movement.Knockback((Vector2)(transform.position) - knockbackOrigin);
        playerRef.state.TakeDamage(damage);

        StartCoroutine(IgnoreCollisionForAWhile(target.layer, ignoreDur));
    }

    IEnumerator IgnoreCollisionForAWhile(int originalLayer, float delay)
    {
        int collisionLayer = gameObject.layer;
        Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, true);

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
