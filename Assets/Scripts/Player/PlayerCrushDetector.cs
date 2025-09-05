using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 압사당하는지를 체크해주는 컴포넌트. 
/// 압사시에는 리스폰되도록 하는 것을 기본 동작으로 함.
/// </summary>
public class PlayerCrushDetector : MonoBehaviour
{
    [SerializeField] int damageOnCrush = 1;

    private bool isCollidingLeft = false;
    private bool isCollidingRight = false;
    private bool isCollidingTop = false;
    private bool isCollidingBottom = false;

    void FixedUpdate()
    {
        CheckForCrush();
        ResetCollisionFlags();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 수평 충돌 확인
            if (Mathf.Abs(contact.normal.y) < 0.1f)
            {
                if (contact.normal.x > 0.9f) // 오른쪽 면
                {
                    isCollidingRight = true;
                }
                if (contact.normal.x < -0.9f) // 왼쪽 면
                {
                    isCollidingLeft = true;
                }
            }

            // 수직 충돌 확인
            if (Mathf.Abs(contact.normal.x) < 0.1f)
            {
                if (contact.normal.y > 0.9f) // 위쪽 면
                {
                    isCollidingTop = true;
                }
                if (contact.normal.y < -0.9f) // 아래쪽 면
                {
                    isCollidingBottom = true;
                }
            }
        }
    }

    private void CheckForCrush()
    {
        bool isCrushedHorizontally = isCollidingLeft && isCollidingRight;
        bool isCrushedVertically = isCollidingTop && isCollidingBottom;

        if (isCrushedHorizontally || isCrushedVertically)
        {
            DieByCrush();
        }
    }

    private void ResetCollisionFlags()
    {
        isCollidingLeft = false;
        isCollidingRight = false;
        isCollidingTop = false;
        isCollidingBottom = false;
    }

    private void DieByCrush()
    {
        Debug.Log("플레이어 압사");
        // 압사 리스폰 + 사망 리스폰으로 리스폰이 2번 연속 발생하는 것을 방지하기 위해 플레이어 현재 체력 검사
        if(PlayerRef.Instance.state.CurrentHP > damageOnCrush)
        { 
            RespawnManager.Instance.Respawn();
        }
        PlayerRef.Instance.state.TakeDamage(damageOnCrush);
    }
}
