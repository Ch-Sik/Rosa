using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastBossCrossBladeCluster : MonoBehaviour
{
    [SerializeField] float clusterLifetime = 3f;
    [SerializeField] Animator animator = null;
    [SerializeField] ProjectileBase projRight;
    [SerializeField] ProjectileBase projUp;
    [SerializeField] ProjectileBase projLeft;
    [SerializeField] ProjectileBase projDown;

    private void Start()
    {
        // TODO: 나타나는 애니메이션 추가
        if(animator != null)
        {
            animator.SetTrigger("appear");
        }
    }

    public void LaunchProjectiles(float projSpeed)
    {
        // 클러스터 본체는 자탄 발사 후 사라져야 하니 자탄들은 자식에서 떼어냄
        projRight.transform.parent = null;
        projUp.transform.parent = null;
        projLeft.transform.parent = null;
        projDown.transform.parent = null;

        // 자탄 발사
        projRight.InitProjectile(Vector2.right * projSpeed);
        projUp.InitProjectile(Vector2.up * projSpeed);
        projLeft.InitProjectile(Vector2.left * projSpeed);
        projDown.InitProjectile(Vector2.down * projSpeed);

        // 클러스터 본체는 소멸
        Disappear(clusterLifetime);
    }

    public void Disappear(float delay)
    {
        // TODO: 사라지는 애니메이션 추가
        if (animator != null)
        {
            animator.SetTrigger("disappear");
        }
        Invoke("DoDestroy", delay);
    }

    void DoDestroy()
    {
        Destroy(gameObject);
        CancelInvoke();     // DoDestroy 두번 호출되는 것 방지
    }

    public void DisappearAllClusterImmediatly()
    {
        // 자탄 삭제
        if(projRight != null)
            projRight.Disappear();
        if (projUp != null)
            projUp.Disappear();
        if (projLeft != null)
            projLeft.Disappear();
        if (projDown != null)
            projDown.Disappear();
        // 자기 자신도 삭제
        Disappear(1f);
    }
}
