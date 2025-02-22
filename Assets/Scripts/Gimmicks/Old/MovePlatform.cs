using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using System.Net.Http.Headers;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;

public class MovePlatform : MonoBehaviour
{
    enum MovePlatformType
    {
        OnOff,
        Once,
        Reverse,
        EndPoint,
        Bounce
    }

    [FoldoutGroup("선택"), SerializeField, Tooltip("Cinematic은 OnOff와 Reverse에서만 동작함")]
    [ShowIf("ShowCinematicOption")]bool useAutoCinematic = true;
    [FoldoutGroup("선택"), SerializeField] MovePlatformType type;
    public float waitDelay = 0.0f;          //웨이포인트에서 대기할 것인지? 
    public float startDelay = 0.0f;         //시작 딜레이 시간

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

    [Space]
    [Tooltip("Bounce 전용 옵션"), ShowIf("ShowBounceOption")]
    [SerializeField, ShowIf("ShowBounceOption")] float breachHeight = 3f;     // 튀어오르는 높이
    [SerializeField, ShowIf("ShowBounceOption")] float breachUpTime = 1f;
    [SerializeField, ShowIf("ShowBounceOption")] float breachDownTime = 0.7f;
    [SerializeField, ShowIf("ShowBounceOption")] float timeBetweenDown = 1.0f;
    [SerializeField, ShowIf("ShowBounceOption")] float timeBetweenBreach = 1.0f;

    GameObject cam;
    ProCamera2DCinematics cinematics;
    public List<cinematicsSetting> cinematicsSettings = new List<cinematicsSetting>();

    bool ShowCinematicOption()
    {
        switch (type)
        {
            case MovePlatformType.OnOff:
            case MovePlatformType.Reverse:
                return true;
            case MovePlatformType.Once:
            case MovePlatformType.EndPoint:
                return false;
        }

        return false;
    }

    public void ImmediateOnAct()
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

    public void ImmediateOffAct()
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
    bool ShowBounceOption()
    {
        if(type == MovePlatformType.Bounce) return true;
        else return false;
    }

    [Button]
    public void OnAct()
    {
        switch (type)
        {
            case MovePlatformType.OnOff:
                canMove = true;
                if (useAutoCinematic)
                    Cinematic();
                break;
            case MovePlatformType.Once:
                canMove = true;
                break;
            case MovePlatformType.Reverse:
                Reverse();
                if (useAutoCinematic)
                    Cinematic();
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
        cam = Camera.main.gameObject;
        cinematics = cam.GetComponent<ProCamera2DCinematics>();

        transform.position = points[0].position;

        if (type == MovePlatformType.OnOff ||
            type == MovePlatformType.Once ||
            type == MovePlatformType.EndPoint)
            canMove = false;
        else
        {
            if (waitCor != null)
                StopCoroutine(waitCor);
            waitCor = StartCoroutine(Wait(startDelay));
            canMove = true;
        }
            

        if (type == MovePlatformType.EndPoint)
            isReverse = true;

        if (type == MovePlatformType.Bounce)
            Bounce();
    }

    public void Cinematic()
    {
        if (cinematics != null)
        {
            while (cinematics.CinematicTargets.Count > 0)
            {
                cinematics.CinematicTargets.RemoveAt(0);
            }

            for (int i = 0; i < cinematicsSettings.Count; i++)
            {
                cinematics.AddCinematicTarget(transform, cinematicsSettings[i].easeInDur, cinematicsSettings[i].holdDur, cinematicsSettings[i].zoomAmount);
            }

            cinematics.Play();
            //추후에 이벤트 취급으로 하여 플레이어의 조작을 막을 필요가 있음
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        if (points.Count == 0) return;
        if (type == MovePlatformType.Once && isArrive) return;
        if (type == MovePlatformType.EndPoint && isArrive) return;
        if (type == MovePlatformType.Bounce) return;
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
            transform.position = targetPoint.position;
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

    IEnumerator Wait(float delay)
    {
        onWait = true;
        yield return new WaitForSeconds(delay);
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

    public void Bounce()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        float originHeight = transform.position.y;
        DOTween.Sequence().AppendInterval(startDelay).AppendCallback(
            () =>
            {
                DOTween.Sequence()
                .Append(
                    rigidbody.DOMoveY(transform.position.y + breachHeight, breachUpTime)
                        .SetEase(Ease.OutCubic))
                .Insert(0, transform.DORotate(new Vector3(0, 0, 0), 0.2f))
                .AppendInterval(timeBetweenDown)
                .Append(
                    rigidbody.DOMoveY(originHeight, breachDownTime)
                        .SetEase(Ease.InQuad))
                .Insert(breachUpTime, transform.DORotate(new Vector3(0, 0, 0), 0.2f))
                .AppendInterval(timeBetweenBreach)
                .SetLoops(-1);
            }
        );
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