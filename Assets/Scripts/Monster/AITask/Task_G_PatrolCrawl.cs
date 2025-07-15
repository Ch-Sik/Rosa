using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using Sirenix.OdinInspector;

/// <summary>
/// 지면/벽/천장 등 지형의 경계선을 따라 움직이는 Patrol을 수행하는 스크립트
/// </summary>
public class Task_G_PatrolCrawl : Task_Base
{
    public enum CrawlOption { Rotate, TurnBack };

    [SerializeField]
    private Blackboard blackboard;
    [SerializeField]
    private new Rigidbody2D rigidbody;

    [SerializeField, Tooltip("진행방향을 가로막는 벽을 만났을 때의 행동")]
    private CrawlOption OnMeetWall = CrawlOption.Rotate;
    [SerializeField, ShowIf("OnMeetWall", CrawlOption.Rotate), Tooltip("위쪽 방향으로 90도 회전할 때의 중심점")]
    private Transform bottomCenter;
    [SerializeField, Tooltip("진행방향을 가로막는 낭떠러지를 만났을 때의 행동")]
    private CrawlOption OnMeetCliff = CrawlOption.Rotate;
    [SerializeField, ShowIf("OnMeetCliff", CrawlOption.Rotate), Tooltip("아래쪽 방향으로 90도 회전할 때의 중심점")]
    private Transform topCenter;

    [SerializeField, Tooltip("이동 속도")]
    private float patrolSpeed = 1.0f;
    [SerializeField, ShowIf("@OnMeetWall == CrawlOption.Rotate || OnMeetCliff == CrawlOption.Rotate"), Tooltip("90도 회전할 때 걸리는 시간")]
    private float rotateTime = 0.5f;
    [SerializeField, Tooltip("평지 기준으로 생각했을 때 움직이는 방향.\nRIGHT면 시계방향, LEFT면 반시계방향으로 지형을 따라 움직임")]
    private LR moveDir = LR.RIGHT;

    // 지면에 있을 경우 moveDir가 가리키는 방향과 동일.
    // 천장에 붙어있을 경우 moveDir가 가리키는 방향과 정반대.
    [SerializeField, ReadOnly, Tooltip("몬스터의 현재 진행 방향")]
    private Vector2 forwardVector;
    private bool isDoingRotate = false;

    // Start is called before the first frame update
    void Start()
    {
        // 컴포넌트 누락된 거 있으면 설정
        if (blackboard == null)
        {
            blackboard = GetComponent<Blackboard>();
            Debug.Assert(blackboard != null, $"{gameObject.name}: Blackboard를 찾을 수 없음!");
        }
        if (rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
            Debug.Assert(rigidbody != null, $"{gameObject.name}: Rigidbody2D를 찾을 수 없음!");
        }

        // rigidbody 설정
        rigidbody.isKinematic = true;

        // 시작하자마자 방향 설정
        forwardVector = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z) * moveDir.toVector2();
        if (GetCurrentDir() != moveDir)
        {
            Flip();
        }
    }

    [Task]
    private void Patrol()
    {
        // 회전중이라면 회전 끝날 때까지 다른 행동 하지 않음 (피격 리액션 포함)
        if (isDoingRotate) return;

        // 피격시 행동 중단
        bool isHitt;
        blackboard.TryGet(BBK.isHitt, out isHitt);
        if (isHitt) {
            rigidbody.velocity = Vector2.zero;  // 제자리에 잠깐 멈춤
            ThisTask.Fail();
            return;
        }

        // 벽에 닿았을 경우
        bool isStuckAtWall;
        blackboard.TryGet(BBK.StuckAtWall, out isStuckAtWall);
        if(isStuckAtWall)
        {
            if (OnMeetWall == CrawlOption.Rotate)
            {
                StopMove();
                Rotate(topCenter.position, moveDir.isRIGHT() ? 90 : -90);
                return;
            }
            else
            {
                StopMove();
                TurnBack();
                return;
            }
        }

        // 낭떠러지에 닿았을 경우
        bool isStuckAtCliff;
        blackboard.TryGet(BBK.StuckAtCliff, out isStuckAtCliff);
        if (isStuckAtCliff)
        {
            if (OnMeetCliff == CrawlOption.Rotate)
            {
                StopMove();
                Rotate(bottomCenter.position, moveDir.isRIGHT() ? -90 : 90);
                return;
            }
            else
            {
                StopMove();
                TurnBack();
                return;
            }
        }
        
        // 앞에 걸리는 거 없다면 전진
        Move();
    }

    // 회전은 코루틴으로 작동하기 때문에 '피격시 행동 정지'가 작동하지 않음.
    // 근데 구현 난이도에 비해서 중요하지 않은 것 같아서 넘어감. (할로우 나이트도 동일)
    private void Rotate(Vector3 pivot, float angle)
    {
        isDoingRotate = true;
        float currentAngle = transform.localRotation.eulerAngles.z;
        float targetAngle = currentAngle + angle;

        float rotatePerSecond = angle / rotateTime;
        

        StartCoroutine(DoRotate90());

        IEnumerator DoRotate90()
        {
            //Debug.LogWarning("회전 시작");
            float sumAngle = 0;
            while(Mathf.Abs(sumAngle) < 90)
            {
                float rotatePerDeltaTime = rotatePerSecond * Time.deltaTime;
                transform.RotateAround(pivot, Vector3.forward, rotatePerDeltaTime);
                sumAngle += rotatePerDeltaTime;
                yield return 0;
            }
            //Debug.LogWarning("while loop end, set angle to targetAngle");
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
            forwardVector = moveDir.isRIGHT()? transform.right : -transform.right;
            isDoingRotate = false;
            yield return 0;
        }
    }

    private void TurnBack()
    {
        Flip();
        forwardVector = -forwardVector;
        moveDir = moveDir == LR.RIGHT ? LR.LEFT : LR.RIGHT;
    }

    private void Move()
    {
        rigidbody.velocity = forwardVector * patrolSpeed;
    }

    private void StopMove()
    {
        rigidbody.velocity = Vector2.zero;
    }
}
