using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using Sirenix.OdinInspector;

/// <summary>
/// <br>Howitzer는 곡사포라는 뜻</br>
/// blackboard에 기록되어있는 enemy의 위치값을 중심으로 
/// x축 방향으로 일정한 간격이 되게 곡사형 투사체를 발사함
/// </summary>

// TODO: 지면 높이와 상관없이 작동하도록 스크립트 수정하기
public class Task_A_MultipleMortar : Task_A_Base
{
    [Header("공격 관련")]
    [SerializeField, Tooltip("투사체 프리팹들. 0번 기준으로 궤도가 계산되므로 가급적 물리적인 성질은 통일할 것.")]
    protected GameObject[] projectilePrefabs;
    [SerializeField, Tooltip("투사체 갯수")]
    protected int projectileCount;
    [SerializeField, Tooltip("투사체가 발사되는 위치")]
    protected Transform muzzle;
    [SerializeField, Tooltip("(지면으로부터 기준) 투사체가 얼마나 높게 올라갔다 내려와야하는지. 반드시 muzzle의 높이보다 높아야 함.")]
    protected float projectileMaxHeight = 3f;
    [SerializeField, Tooltip("투사체가 지면에 도달할 때를 기준으로, x축 방향으로 간격")]
    protected float targetInterval = 1f;

    [SerializeField, ReadOnly]
    protected float groundCoordY; // world space 상에서의 지면 y 좌표

    [SerializeField, ReadOnly]
    protected Vector2[] targetLocations;
    [SerializeField, ReadOnly]
    protected Vector2[] launchVectors;

    private bool computedValuesFlag = false;    // 복잡한 계산은 한번만 하려고
    private float projectileEta;
    private float launchVectorY;

    [Title("debug")]
    [SerializeField] private Vector2 fireVector;
    private float projectileGravityScale;   // 투사체의 중력가속도 절댓값

    private void Start()
    {
        if (blackboard == null)
        {
            blackboard = GetComponent<Blackboard>();
            Debug.Assert(blackboard != null, $"{gameObject.name}: Blackboard를 찾을 수 없음");
        }
        Debug.Assert(projectilePrefabs.Length != 0, $"{gameObject.name}: 투사체 프리팹이 설정되어있지 않음");

        targetLocations = new Vector2[projectileCount];
        launchVectors = new Vector2[projectileCount];
    }

    [Task]
    private void MultipleMortarAttack(int projectileCount = -1)
    {
        if(projectileCount > 0)
        {
            this.projectileCount = projectileCount;
        }
        ExecuteAttack();
    }

    // 공격 패턴을 구체적으로 지정하지 않고 대충 Attack()으로 뭉뚱그려 작성된 BT 스크립트 호환용
    [Task]
    private void Attack()
    {
        ExecuteAttack();
    }

    protected override void OnActiveBegin()
    {
        if(!computedValuesFlag)
        {
            ComputeReusableValues();
            computedValuesFlag = true;
        }
        // 적(플레이어) 위치 파악 & 좌표 산정
        GetTargetLocations();
        // 각도 및 파워 계산
        GetLaunchVectors();

        // 공격 시전
        LaunchProjectiles();
    }

    private void ComputeReusableValues()
    {
        // 지면의 높이 측정
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, Vector2.down,
                                            10f, LayerMask.GetMask("Ground"));
        groundCoordY = hit.point.y;

        // 곡사에 필요한 계산 미리 해놓기
        float projectileTopPos = this.projectileMaxHeight + groundCoordY;
        float projectileGravityScale = -Physics2D.gravity.y * projectilePrefabs[0].GetComponent<Rigidbody2D>().gravityScale;
        // ↓ 중력가속도 식 두번 적분하고 t=... 꼴로 정리한 거
        float timeForUp = Mathf.Sqrt(2 * (projectileTopPos - muzzle.position.y) / projectileGravityScale);
        float timeForDown = Mathf.Sqrt(2 * (projectileTopPos - groundCoordY) / projectileGravityScale);
        projectileEta = timeForUp + timeForDown;
        launchVectorY = projectileGravityScale* timeForUp;
    }

    protected void GetTargetLocations()
    {
        float centerXcoord = GetTargetCenterXcoord();
        // 투사체 갯수가 짝수일 경우 0.5간격만큼 우로 이동 (즉, 플레이어 가만히 있으면 안맞음)
        if(projectileCount % 2 == 0)
        {
            centerXcoord += 0.5f * targetInterval;
        }
        for(int i=0; i<projectileCount; i++)
        {
            targetLocations[i] = new Vector2(
                centerXcoord + (i - (projectileCount / 2)) * targetInterval, 
                groundCoordY);
        }
    }

    protected virtual float GetTargetCenterXcoord()
    {
        GameObject enemy;
        blackboard.TryGet(BBK.Enemy, out enemy);
        return enemy.transform.position.x;
    }

    private void GetLaunchVectors()
    {
        for(int i=0; i<projectileCount; i++)
        {
            launchVectors[i] = new Vector2(
                (targetLocations[i].x - muzzle.position.x) / projectileEta, 
                launchVectorY);
        }
    }

    private void LaunchProjectiles()
    {
        for (int i = 0; i < projectileCount; i++)
        {
            GameObject projectile;

            int randomIndex = Random.Range(0, projectilePrefabs.Length);
            projectile = Instantiate(projectilePrefabs[randomIndex], muzzle.position, Quaternion.identity);

            projectile.GetComponent<ProjectileBase>().InitProjectile(launchVectors[i]);
        }
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            for (int i = 0; i < targetLocations.Length; i++)
            {
                Gizmos.DrawSphere(targetLocations[i], 0.5f);
            }
        }
    }
}
