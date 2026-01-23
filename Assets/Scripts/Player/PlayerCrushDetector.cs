using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    
    private bool _isDying = false;

    void FixedUpdate()
    {
        CheckForCrush();
        ResetCollisionFlags();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 플랫폼 오르는 도중에 압사판정되는 것 방지를 위해 Platform이 '아래 방향'일 때에만 압사 판정에 산입
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (!collision.gameObject.CompareTag("Platform"))
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
            }

            // 수직 충돌 확인
            if (Mathf.Abs(contact.normal.x) < 0.1f)
            {
                if (contact.normal.y > 0.9f) // 위쪽 면
                {
                    isCollidingTop = true;
                }

                if (!collision.gameObject.CompareTag("Platform"))
                {
                    if (contact.normal.y < -0.9f) // 아래쪽 면
                    {
                        isCollidingBottom = true;
                    }
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
        // 이미 리스폰중일 때에는 리턴
        if (RespawnHandler.Instance.IsDoingRespawn) return;
        
        // 압사 도중에 지형 뚫기 방지
        IgnoreCollisionForSeconds(1f).Forget();
        
        Debug.Log("플레이어 압사");
        PlayerRef.Instance.damageReceiver.GetDamageAndRespawn(damageOnCrush);
    }

    private async UniTaskVoid IgnoreCollisionForSeconds(float seconds)
    {
        var groundMask = LayerMask.GetMask("Ground");

        PlayerRef.Instance.rb.isKinematic = true;
        // PlayerRef.Instance.col.excludeLayers |= groundMask;
        await UniTask.WaitForSeconds(seconds);
        PlayerRef.Instance.rb.isKinematic = false;
        // PlayerRef.Instance.col.excludeLayers &= groundMask;
    }
}
