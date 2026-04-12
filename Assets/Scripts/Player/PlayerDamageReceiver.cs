using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour
{
    [SerializeField] float defaultInvincibleTime = 2f;

    public bool IsInvincible => _ignoreDamage;
    
    private bool _ignoreDamage = false;
    private PlayerRef _playerRef;
    
    // 넉백 관련
    [Tooltip("넉백 계수")]
    [SerializeField] private float defaultKnockbackStrength = 1f;

    public Action OnDamaged;

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
        SetInvincibleAndIgnoreCollision(source.layer, ignoreDur).Forget();
    }

    public void GetKnockBack(GameObject source, float knockbackPow = -1)
    {
        if(knockbackPow < 0)
            knockbackPow = defaultKnockbackStrength;
        GetKnockbackInternal(source, knockbackPow);
    }
    

    public void GetDamageAndRespawn(int damage)
    {
        // 낙사 등의 상황 고려, _ignoreDamage 무시함
        // _ignoreDamage 대신 리스폰 시퀀스 중인지 여부로 무한 데미지 입는 상황 방지
        if (RespawnHandler.Instance.IsDoingRespawn) // 물대포, 레이저 등 강제 리스폰
            return;
        if(PlayerRef.Instance.state.CurrentHP <= 0) // 사망으로 인한 게임 리셋
            return;
        
        // isDead인 경우 리스폰은 GetDamageInternal -> TakeDamage -> OnDie가 처리
        bool isDead = GetDamageInternal(damage);
        
        // 그 외에 경우에는 여기서 수동으로 리스폰 처리
        if(!isDead)
            RespawnHandler.Instance.Respawn();
        
        // 어차피 리스폰할거니 넉백 필요 없음
    }
    
    public void GetDamageIgnoreInvincible(GameObject source, int damage)
    {
        GetDamageInternal(damage);
        GetKnockbackInternal(source, defaultKnockbackStrength);
    }

    private bool GetDamageInternal(int damage)
    {
        _playerRef.animation.BlinkEffect().Forget();
        _playerRef.animation.SetTrigger("Hit");
        CameraShake.ShakeCamera(CameraShakePreset.PlayerHit);
        OnDamaged?.Invoke();
        return _playerRef.state.TakeDamage(damage);
    }

    private void GetKnockbackInternal(GameObject source, float knockbackPow)
    {
        Vector2 knockbackOrigin = new Vector2(source.transform.position.x,
            source.transform.position.y - (source.transform.localScale.y / 2));
        _playerRef.movement.Knockback((Vector2)(transform.position) - knockbackOrigin, knockbackPow);
    }

    private async UniTaskVoid SetInvincibleAndIgnoreCollision(int originalLayer, float delay)
    {
        // 무적 플래그 ON & 충돌 무시 설정 (몬스터와 피격 시 몬스터 통과하여 지나갈 수 있게)
        int collisionLayer = gameObject.layer;
        Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, true);
        _ignoreDamage = true;
        
        await UniTask.WaitForSeconds(delay);
        
        // 무적 해제
        Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, false);
        _ignoreDamage = false;
    }
}
