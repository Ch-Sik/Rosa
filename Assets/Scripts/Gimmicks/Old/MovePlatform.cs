using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using System.Net.Http.Headers;

public class MovePlatform : MonoBehaviour
{
    enum MovePlatformType
    {
        OnOff,
        Once,
        Reverse,
        EndPoint
    }

    [Header("선택")]
    [SerializeField] MovePlatformType type;
    public float waitDelay = 0.0f;          //웨이포인트에서 대기할 것인지? 

    [Space]
    public bool showGizmos = false;
    public List<Transform> points;
    public float speed = 2f;
    private int currentIndex = 0;
    public bool isArrive = false;

    public bool onWait = false;
    private Coroutine waitCor;

    public bool isReverse = false;
    public bool curReverseState = false;
    private bool canMove = false;

    [Button]
    public void OnAct()
    {
        switch (type)
        {
            case MovePlatformType.OnOff:
                canMove = true;
                break;
            case MovePlatformType.Once:
                canMove = true;
                break;
            case MovePlatformType.Reverse:
                Reverse();
                break;
            case MovePlatformType.EndPoint:
                if (!isReverse)
                    return;

                canMove = true;
                isArrive = false;
                isReverse = false;
                break;
        }
    }

    [Button]
    public void OffAct()
    {
        switch (type)
        {
            case MovePlatformType.OnOff:
                canMove = false;
                break;
            case MovePlatformType.Once:
                canMove = true;
                break;
            case MovePlatformType.Reverse:
                Reverse();
                break;
            case MovePlatformType.EndPoint:
                if (isReverse)
                    return;

                canMove = true;
                isArrive = false;
                isReverse = true;
                break;
        }
    }

    private void Start()
    {
        transform.position = points[0].position;

        if (type == MovePlatformType.OnOff ||
            type == MovePlatformType.Once ||
            type == MovePlatformType.EndPoint)
            canMove = false;
        else
            canMove = true;

        if (type == MovePlatformType.EndPoint)
            isReverse = true;
    }

    void Update()
    {
        if (!canMove) return;
        if (points.Count == 0) return;
        if (type == MovePlatformType.Once && isArrive) return;
        if (type == MovePlatformType.EndPoint && isArrive) return;
        if (onWait) return;

        if (curReverseState != isReverse)
        {
            curReverseState = isReverse;

            if (isReverse)
                currentIndex = (currentIndex - 1) > -1 ? currentIndex - 1 : points.Count - 1;
            else
                currentIndex = (currentIndex + 1) % points.Count;
        }

        Transform targetPoint = points[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            if (isReverse)
                currentIndex = (currentIndex - 1) > -1 ? currentIndex - 1 : points.Count - 1;
            else
                currentIndex = (currentIndex + 1) % points.Count;

            // 일회성인지 파악
            if (type == MovePlatformType.Once && currentIndex == 0 && !isReverse)
                isArrive = true;
            if (type == MovePlatformType.Once && currentIndex == points.Count - 1 && isReverse)
                isArrive = true;

            if (type == MovePlatformType.EndPoint && currentIndex == 0 && !isReverse)
                isArrive = true;
            if (type == MovePlatformType.EndPoint && currentIndex == points.Count - 1 && isReverse)
                isArrive = true;

            // 도착 지연
            if (waitDelay <= 0.0f)
                return;
            if (waitCor != null)
                StopCoroutine(waitCor);
            waitCor = StartCoroutine(Wait());
        }
    }

    public void Reverse()
    {
        if (type == MovePlatformType.Once)
            return;

        isReverse = !isReverse;
    }

    IEnumerator Wait()
    {
        onWait = true;
        yield return new WaitForSeconds(waitDelay);
        onWait = false;
    }  

    public void HandleChildTriggerEnter(Transform other)
    {
        other.SetParent(transform);
    }

    public void HandleChildTriggerExit(Transform other)
    {
        if(Application.isPlaying)
        {
            if (other.parent == transform)
            {
                other.SetParent(null);
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        for (int i = 0; i < points.Count; i++)
        {
            int j = i + 1 >= points.Count ? 0 : i + 1;
            Gizmos.DrawLine(points[i].position, points[j].position);
        }
    }
}