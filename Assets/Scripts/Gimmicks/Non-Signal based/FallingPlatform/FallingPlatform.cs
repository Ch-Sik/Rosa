using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float lifeTime = 8f;
    public float moveSpeed = 1f;

    // 기즈모 그리기용
    private Vector3 endPoint;
    private Tween _tween;
    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
        _tween = gameObject.GetComponent<Rigidbody2D>().DOMoveY(transform.position.y - moveSpeed * lifeTime, lifeTime);

        endPoint = transform.position + Vector3.down * lifeTime * moveSpeed;
    }

    private void OnDestroy()
    {
        if(_tween != null)
            _tween.Kill();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if(Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position, endPoint);
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * moveSpeed * lifeTime);
        }
    }
}
