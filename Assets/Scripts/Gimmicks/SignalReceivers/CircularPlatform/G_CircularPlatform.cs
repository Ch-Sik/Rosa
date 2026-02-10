using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G_CircularPlatform : GimmickSignalReceiver
{
    public bool showGizmos = true;
    public float speed = 2.0f;
    [SerializeField] private float time = 0;
    [SerializeField] private float totalDistance = 0;

    public Transform movePlatform;
    public Transform parentObject;
    [ShowInInspector] private List<Vector3> originWaypoints = new List<Vector3>();
    [ShowInInspector] private List<Vector3> nearWaypoints;
    public int lastPoint = 0;

    private Tween tween;

    private void Start()
    {
        for(int i = 0; i < parentObject.childCount; i++)
            originWaypoints.Add(parentObject.GetChild(i).transform.position);

        nearWaypoints = new List<Vector3>(originWaypoints);

        totalDistance += Vector2.Distance(originWaypoints[originWaypoints.Count - 1], originWaypoints[0]);
        for (int i = 0; i < originWaypoints.Count - 1; i++)
            totalDistance += Vector2.Distance(originWaypoints[i], originWaypoints[i + 1]);

        time = (totalDistance) / (speed);

        StartCoroutine(WarmUp());
    }

    public IEnumerator WarmUp()
    {
        Move();
        yield return new WaitForSeconds(0.5f);
        Move(true);
        yield return new WaitForSeconds(0.5f);
        Move();
        yield return new WaitForSeconds(0.5f);
        Move(true);
        yield return new WaitForSeconds(0.5f);
        Move();
    }


    private List<Vector3> GetNearWaypoint(bool isInverted)
    {
        int index = lastPoint;

        if (!isInverted)
            index--;

        if (index >= nearWaypoints.Count || index <= -1)
            index = 0;

        List<Vector3> firstPart = nearWaypoints.GetRange(0, index);
        List<Vector3> secondPart = nearWaypoints.GetRange(index, nearWaypoints.Count - index);

        nearWaypoints.Clear();

        nearWaypoints.AddRange(secondPart);
        nearWaypoints.AddRange(firstPart);

        //점 6개일 떄, 7개로 판정, abc abca

        return nearWaypoints;
    }

    private void Move(bool isInverted = false)
    {
        if (tween != null)
            tween.Kill();

        tween = movePlatform
             .DOPath(GetNearWaypoint(isInverted).ToArray(), time, PathType.Linear)
//             .DOPath(originWaypoints.ToArray(), time, PathType.Linear)
                            .SetOptions(true)                                       //닫힌 경계로 설정
                            .SetLoops(-1)                                           //무한한 반복 설정
                            .SetInverted(isInverted)                                //역방향 설정
                            .OnWaypointChange((point) =>
                            {
                                lastPoint = point;
                            });
    }

    [Button]
    public override void OffAct()
    {
        Move();
    }

    [Button]
    public override void OnAct()
    {
        Move(true);
    }

    public override void ImmediateOnAct()
    {
        Move();
    }

    public override void ImmediateOffAct()
    {
        Move(true);
    }
}
