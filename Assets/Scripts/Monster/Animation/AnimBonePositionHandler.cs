using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using AnyPortrait;

public class AnimBonePositionHandler : MonoBehaviour
{
    [SerializeField]
    private apPortrait targetPortrait;
    [SerializeField]
    private Blackboard blackboard;

    [Title("옵션")]

    [Tooltip("애니메이션에 Aim 위치 사용")]
    public bool useAimMotion;
    [ShowIf("useAimMotion"), Tooltip("공격 도중에는 Aim 고정")]
    [SerializeField] private bool stopAimWhenAttack;
    [ShowIf("useAimMotion"), Tooltip("Aim 위치를 반영할 Portrait의 Bone 이름")]
    [SerializeField] private string enemyAimBoneName;
    [ShowIf("useAimMotion"), Tooltip("Aim 따라가기 속도")]
    [SerializeField] private float aimFollowSpeed = 0.3f;

    private Vector2 _currentAimPos;
    private bool _trackingEnemy;


    // Start is called before the first frame update
    void Start()
    {
        if(targetPortrait == null)
        {
            targetPortrait = GetComponent<apPortrait>();
            Debug.Assert(targetPortrait != null);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!useAimMotion) return;
        
        bool updateAimPosition = true;

        // 에임을 업데이트해도 되는지 우선 체크
        if (stopAimWhenAttack)
        {
            int attackState;
            blackboard.TryGet(BBK.AttackState, out attackState);
            // 공격 진행중일 때 조준 따라가기 멈춤 (후딜은 포함 안됨)
            if (attackState == 1 || attackState == 2) updateAimPosition = false;    
        }
        if (!updateAimPosition) return;
            
        // 에임을 업데이트해도 된다고 판단되면 에임 위치 정보 가져오기
        GameObject enemy;
        blackboard.TryGet(BBK.Enemy, out enemy);
        if (!enemy)
        {
            _trackingEnemy = false;
            return;
        }

        if (!_trackingEnemy)
        {
            // 플레이어를 인식한 첫 프레임에는 Aim 본을 즉시 이동
            JumpAim(enemy.transform.position);
        }
        else
        {
            TweenAim(enemy.transform.position);
        }
        _trackingEnemy = true;
    }

    private void JumpAim(Vector3 targetPos)
    {
        _currentAimPos = targetPos;
        targetPortrait.SetBonePosition(enemyAimBoneName, _currentAimPos, Space.World);
    }

    private void TweenAim(Vector3 targetPos)
    {
        // Aim 본을 천천히 이동
        _currentAimPos = Vector2.MoveTowards(_currentAimPos, targetPos, aimFollowSpeed);
        targetPortrait.SetBonePosition(enemyAimBoneName, _currentAimPos, Space.World);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_currentAimPos, 0.2f);
        }
    }
}
