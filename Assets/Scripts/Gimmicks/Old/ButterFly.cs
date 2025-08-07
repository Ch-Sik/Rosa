using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 나비의 웨이포인트들과 데이터들을 다루기 위한 프리팹 스크립트입니다.
/// </summary>

public class Butterfly : MonoBehaviour
{
    public bool isCaged = false;                    //나비장에 갇혀있는 경우 플레이어 공격과 상호작용할 수 없음.

    public bool showGizmo = true;                       //기즈모 표기 유무
    public Color gizmoColor = Color.red;                //라인 구분을 위한 기즈모 컬러 세팅
    [ShowInInspector] bool onWayTracking = false;       //waytacking을 하고 있는가?

    [ShowInInspector] private Vector2[] waypoints;      //연결된 웨이포인트 점의 집합
    [ShowInInspector] private float distance;           //웨이포인트 점을 따라 갈 때의 거리

    [ShowInInspector] private GameObject direction;     //디렉션 표시

    private Sequence tracking;                          //트래킹 시퀀스
    public Transform waypointsTransform;                //웨이포인트점들의 부모 오브젝트

    Transform riderTF;                                  //현재 나비를 탄 대상의 트랜스폼

    public InteractiveObject interactiveObject;

    public Vector2 startPoint;
    public Vector2 endPoint;
    public float angle;

    float savedGravity = 0f;

    //시작과 동시에 waypoint를 연산하고, 거리데이터를 산출한다.
    private void Start()
    {
        SetData();
        InitPosition();
        InitDirection();
        SetInteractive();
    }

    public void InitDirection()
    {
        //이상한 버그가 있어서 direction을 인스펙터 드랍으로 못 얻음.. 아마도 내 생각엔, 프리팹이 2곳에 엮여서 그런듯.
        direction = transform.parent.transform.GetChild(1).gameObject;

        startPoint = waypoints[0];
        endPoint = waypoints[waypoints.Length - 1];

        Vector3 dir = new Vector2(endPoint.x - startPoint.x, endPoint.y - startPoint.y).normalized;
        // 직사각형의 회전 각도 계산
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        //현재 Transform에 적용
        direction.transform.position = startPoint;
        direction.transform.rotation = Quaternion.Euler(0, 0, angle);

        //숨기기
        if(isCaged)
            HideDirection();
    }
    public void ShowDirection() { direction.SetActive(true); }
    public void HideDirection() { direction.SetActive(false); }

    //나비가 날기위한 waypoint와 거리 데이터를 산출한다.
    public void SetData()
    {
        PathsToVector3Array();
        DistanceCalcultor();
    }

    public void SetInteractive()
    {
        if (isCaged)
            interactiveObject.canInteract = false;
    }

    private void InitPosition()
    {
        if(!isCaged)
            transform.position = waypoints[0];
    }

    public void RideButterFly()
    {
        Debug.Log("ButterFlyAct");
        if (PlayerRef.Instance.combat.isRidingButterfly)
            return;

        // PlayerRef.Instance.combat.StopAttack();
        ButterFlyAct(PlayerRef.Instance.transform);
    }

    public void Release()
    {
        interactiveObject.canInteract = true;

    }

    //나비가 날게 되는 핵심 코드
    public void ButterFlyAct(Transform player)
    {
        if (onWayTracking)
            return;

        tracking = DOTween.Sequence()
        //DOPath 이전에 기본적 세팅 //tracking 여부를 true로 설정하고, 플레이어를 나비에 태운다.
        .AppendCallback(() =>
        {
            PlayerRef.Instance.combat.isRidingButterfly = true;
            onWayTracking = true;
            Ride(player);
            HideDirection();
        })
        //플레이어의 위치를 나비의 위치로 0.3초 동안 이동시킨다.
        .Append(player.GetComponent<Rigidbody2D>().DOMove(transform.position, 0.3f))
        //나비의 자식으로 플레이어가 있기에, 나비를 이동시키며 플레이어를 동시에 이동시킨다. 이때 나비는 waypoints를 순차적으로 방문한다.
        .Append(transform.GetComponent<Rigidbody2D>().DOPath(waypoints, distance / 10))  // 속도 버프
        .Join(player.GetComponent<Rigidbody2D>().DOPath(waypoints, distance / 10))
        //DOPath 완료후 세팅 // tracking 여부를 false로 설정하고, 트래킹을 초기화 하며, 플레이어를 내리게한다.
        .AppendCallback(() =>
        {
            ResetTracking();
            ShowDirection();
        });
    }

    //나비의 위치를 멈추게 한다.
    public void StopTracking()
    {
        tracking.Pause();
    }

    //트래킹을 취소시킨다.
    public void ResetTracking()
    {
        StopTracking();
        UnRide();
        transform.position = waypoints[0];
        onWayTracking = false;
        PlayerRef.Instance.combat.isRidingButterfly = false;
    }

    //나비에 탈 때, 
    public void Ride(Transform playerTF)
    {
        // 나비 탑승 중 떨림을 막기 위해 나비 탑승중에는 카메라 업데이트 모드를 LateUpdate로 바꿈 
        //Camera.main.GetComponent<ProCamera2D>().UpdateType = Com.LuisPedroFonseca.ProCamera2D.UpdateType.LateUpdate;

        riderTF = playerTF;                                         //탑승자 데이터를 저장한다.
        //playerTF.SetParent(transform);                            //플레이어를 자식으로 설정해준다.
        transform.GetComponent<Collider2D>().enabled = false;   //나비와의 재충돌을 대비해 콜라이더를 끈다.

        playerTF.GetComponent<Collider2D>().enabled = false;

        savedGravity = PlayerRef.Instance.rb.gravityScale;
        PlayerRef.Instance.rb.gravityScale = 0;
        // 낙하 or 상승 중일 떄 velocityY가 남아있는 것을 고려하여 속도를 초기화시킨다.
        PlayerRef.Instance.rb.velocity = Vector2.zero;
    }

    //나비에 내릴 때, (이름은 추후 생각해보기)
    public void UnRide()
    {
        // 나비에서 내린 후 카메라 업데이트 모드를 원상복구
        //Camera.main.GetComponent<ProCamera2D>().UpdateType = Com.LuisPedroFonseca.ProCamera2D.UpdateType.FixedUpdate;

        //riderTF.SetParent(null);                                  //탑승자를 내린다.

        riderTF.GetComponent<Collider2D>().enabled = true;
        transform.GetComponent<Collider2D>().enabled = true;    //나비의 재활성화


        PlayerRef.Instance.rb.gravityScale = savedGravity;
    }

    //자식데이터를 토대로 Vector3배열을 만든다.
    private void PathsToVector3Array()
    {
        int count = waypointsTransform.childCount;          //Waypoint 자식의 개수

        waypoints = new Vector2[count];

        for (int i = 0; i < count; i++)                 //자식을 순회하며 waypoint를 설정해준다.
            waypoints[i] = waypointsTransform.GetChild(i).position;
    }

    //waypoints의 각점을 비교하여 최종 거리를 산출한다.
    private void DistanceCalcultor()
    {
        distance = 0;
        for (int i = 0; i < waypoints.Length - 1; i++)
            distance += Vector3.Distance(waypoints[i], waypoints[i + 1]);
    }


    private void OnDrawGizmos()
    {
        if (!showGizmo)
            return;

        if (onWayTracking)
            return;

        SetData();

        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(transform.position, waypoints[0]);

            Gizmos.color = gizmoColor;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
            }
        }
    }
}