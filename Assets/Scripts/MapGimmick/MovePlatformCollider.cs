using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlatformCollider : MonoBehaviour
{
    private MovePlatform platform;
    private void Start()
    {
        platform = GetComponentInParent<MovePlatform>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(IsOnTopOfPlatform(collision))
        {
            if (platform != null)
            {
                platform.HandleChildTriggerEnter(collision);
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (platform != null)
        {
            platform.HandleChildTriggerExit(collision);
        }
    }

    bool IsOnTopOfPlatform(Collision2D collision)
    {
        float platformTopY = transform.position.y +  GetComponent<Collider2D>().bounds.size.y / 2;

        float objectBottomY = collision.collider.bounds.min.y;

        return objectBottomY > platformTopY;
    }

}
