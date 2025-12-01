using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour
{
    [SerializeField] float defaultInvincibleTime = 2f;

    public bool IsInvincible => _ignoreDamage;
    
    private bool _ignoreDamage = false;
    private PlayerRef _playerRef;

    public void Start()
    {
        _playerRef = PlayerRef.Instance;
    }

    public void GetDamage(GameObject source, int damage)
    {
        GetDamage(source, damage, defaultInvincibleTime);
    }

    public void GetDamage(GameObject source, int damage, float ignoreDur)
    {
        if (_ignoreDamage) return;
        
        GetDamageInternal(damage);
        GetKnockbackInternal(source);
        
        StartCoroutine(SetInvincibleAndIgnoreCollision(source.layer, ignoreDur));
    }

    public void GetDamageAndRespawn(int damage)
    {
        // _ignoreDamage 무시함
        bool isDead = GetDamageInternal(damage);
        // 어차피 리스폰할거니 넉백 필요 없음
        
        if(!isDead)
            RespawnHandler.Instance.Respawn();
    }
    
    public void GetDamageIgnoreInvincible(GameObject source, int damage)
    {
        GetDamageInternal(damage);
        GetKnockbackInternal(source);
    }

    private bool GetDamageInternal(int damage)
    {
        _playerRef.animation.BlinkEffect();
        _playerRef.animation.SetTrigger("Hit");
        CameraShake.ShakeCamera(CameraShakePreset.PlayerHit);
        return _playerRef.state.TakeDamage(damage);
    }

    private void GetKnockbackInternal(GameObject source)
    {
        Vector2 knockbackOrigin = new Vector2(source.transform.position.x,
            source.transform.position.y - (source.transform.localScale.y / 2));
        _playerRef.movement.Knockback((Vector2)(transform.position) - knockbackOrigin);
    }

    IEnumerator SetInvincibleAndIgnoreCollision(int originalLayer, float delay)
    {
        // 무적 플래그 ON & 충돌 무시 설정 (몬스터와 피격 시 몬스터 통과하여 지나갈 수 있게)
        int collisionLayer = gameObject.layer;
        Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, true);
        _ignoreDamage = true;
        
        yield return new WaitForSeconds(delay);
        
        // 무적 해제
        Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, false);
        _ignoreDamage = false;
    }
}
