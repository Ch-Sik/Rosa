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
        int monsterBodyLayer = LayerMask.NameToLayer("Monster");
        int monsterAttackLayer = LayerMask.NameToLayer("MonsterAttack");

        // 26.07.08) 피격 소스가 Default 등 몬스터 외 레이어일 때 해당 레이어 전체를 전역으로 충돌 무시하면
        // 같은 레이어에 있는 트리거(대화 존 등)와의 접촉이 끊겼다가 무적 해제 시 재생성되면서
        // OnTriggerEnter2D가 재발화하는 문제(최종보스전 인트로 대사 반복 버그)가 있어
        // 몬스터 관련 레이어에 한해서만 충돌 무시하도록 제한.
        // 피해 자체는 _ignoreDamage가 막아주므로 그 외 레이어는 충돌 무시가 필요 없음.
        bool ignoreSourceLayer = originalLayer == monsterBodyLayer || originalLayer == monsterAttackLayer;

        Physics2D.IgnoreLayerCollision(monsterBodyLayer, collisionLayer, true); // 투사체 등에 맞았어도 몬스터 지나갈 수 있도록 수정
        if (ignoreSourceLayer)
            Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, true);
        _ignoreDamage = true;

        await UniTask.WaitForSeconds(delay);

        // 무적 해제
        Physics2D.IgnoreLayerCollision(monsterBodyLayer, collisionLayer, false);
        if (ignoreSourceLayer)
            Physics2D.IgnoreLayerCollision(originalLayer, collisionLayer, false);
        _ignoreDamage = false;
    }
}
