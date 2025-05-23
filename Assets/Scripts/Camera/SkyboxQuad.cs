using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxQuad : MonoBehaviour
{
    [SerializeField]
    float distToCamera = 1000f;

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = Vector3.forward * distToCamera;

        UpdateQuadRatio();
    }

    void UpdateQuadRatio()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float textureAspect = 16f / 9f;

        // 카메라 FOV 기반 화면 높이 계산
        float viewHeight = 2f * distToCamera * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float quadWidth, quadHeight;

        if (screenAspect > textureAspect)
        {
            // 화면이 더 넓음 → 높이에 맞춰 width를 늘리고 좌우 크롭
            quadHeight = viewHeight;
            quadWidth = viewHeight * textureAspect;
        }
        else
        {
            // 화면이 더 좁음 → 넓이에 맞춰 height를 늘리고 위아래 크롭
            float viewWidth = viewHeight * screenAspect;
            quadWidth = viewWidth;
            quadHeight = viewWidth / textureAspect;
        }

        transform.position = Camera.main.transform.position + Camera.main.transform.forward * distToCamera;
        transform.rotation = Camera.main.transform.rotation;
        transform.localScale = new Vector3(quadWidth, quadHeight, 1f);
    }
}
