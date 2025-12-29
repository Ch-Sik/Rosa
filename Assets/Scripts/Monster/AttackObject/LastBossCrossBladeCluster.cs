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
    [SerializeField] private ParticleSystem particleRight;
    [SerializeField] private ParticleSystem particleUp;
    [SerializeField] private ParticleSystem particleLeft;
    [SerializeField] private ParticleSystem particleDown;

    private void Start()
    {
        SetProjParticleEmission(false);
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
        

        // 자탄의 파티클 활성화
        SetProjParticleEmission(true);

        // 클러스터 본체는 소멸
        Disappear(clusterLifetime);
    }

    private void SetProjParticleEmission(bool value)
    {
        var particleRightEmission = particleRight.emission;
        particleRightEmission.enabled = value;
        var particleUpEmission = particleUp.emission;
        particleUpEmission.enabled = value;
        var particleLeftEmission = particleLeft.emission;
        particleLeftEmission.enabled = value;
        var particleDownEmission = particleDown.emission;
        particleDownEmission.enabled = value;
    }

    public void Disappear(float delay)
    {
        // TODO: 사라지는 애니메이션 추가
        if (animator != null)
        {
            animator.SetTrigger("Disappear");
        }
        Destroy(gameObject, delay);
    }

    public void DisappearAllClusterImmediatly()
    {
        Debug.Log("[LastBossCrossBladeCluster] DisappearAllClusterImmediatly");
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
