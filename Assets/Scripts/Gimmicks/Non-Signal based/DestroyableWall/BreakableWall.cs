using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO: 파괴된 벽 저장기능 추가하기
/// </summary>
public class BreakableWall : DamageReceiver
{
    // 충돌 판정
    [SerializeField] new Collider2D collider;
    // 무너지지 않은 상태의 벽
    [SerializeField] GameObject go_wall;
    // 벽 파괴 후 활성화되는 잔해
    [SerializeField] GameObject[] go_debries;

    [Tooltip("벽 체력")]
    [SerializeField] int maxHP;

    [Tooltip("벽 파괴 후 오브젝트 완전히 비활성화될 때까지 걸리는 시간")]
    [SerializeField] float disableTime;

    [Tooltip("고정 데미지 1을 사용할지 아니면 유저 공격력을 사용할지")]
    [SerializeField] bool fixDamageTo1;

    [Tooltip("스프라이트 교체용")]
    [SerializeField] Sprite[] sprites;

    int HP;
    int curSpriteIndex;

    private void Start()
    {
        HP = maxHP;
        curSpriteIndex = 0;
    }

    public override bool GetHitt(int damage, float attackAngle)
    {
        if(fixDamageTo1)
            HP -= 1;
        else
            HP -= damage;

        if (HP <= 0)
        {
            BreakWall();
        }
        else
        {
            int nextSprite = Mathf.FloorToInt((maxHP - HP) / (float)maxHP * sprites.Length);
            go_wall.GetComponent<SpriteRenderer>().sprite = sprites[nextSprite];
        }
        return true;
    }

    private void BreakWall()
    {
        collider.enabled = false;
        go_wall.SetActive(false);
        foreach (var deb in go_debries)
        {
            deb.SetActive(true);
        }
        StartCoroutine(Disable());

        IEnumerator Disable()
        {
            yield return new WaitForSeconds(disableTime);
            gameObject.SetActive(false);
        }
    }
}
