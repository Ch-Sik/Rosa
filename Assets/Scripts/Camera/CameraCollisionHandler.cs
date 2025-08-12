using Com.LuisPedroFonseca.ProCamera2D;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    public bool showGizmos = false;
    public float minDistance = 0.5f;
    public float maxDistance = 10f;
    public float moveSmoothness = 0.1f;

    public ProCamera2D proCameraComponent;

    void Awake()
    {
        MapManager.Instance.OnNextRoomLoaded += AdjustCameraPosition;
    }

    [Button]
    public void AdjustCameraPosition()
    {
        Debug.Log("AdjustCameraPosition 호출됨");

        // 일단 카메라를 플레이어 위치로 옮김
        proCameraComponent.MoveCameraInstantlyToPosition(PlayerRef.Instance.transform.position);

        Vector3 moveDir = Vector3.zero;
        int hitCount = 0;

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right};
        Vector2 cameraSize = proCameraComponent.ScreenSizeInWorldCoordinates;
        float[] distances = { cameraSize.y /2, cameraSize.y /2, cameraSize.x /2 , cameraSize.x /2 };

        for(int i = 0; i<directions.Length; i++)
        {
            Vector2 origin = transform.position;
            Vector3 dir = directions[i];
            float dist = distances[i];

            if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, LayerMask.GetMask("Boundary")))
            {
                // 카메라 사이즈만큼 여유공간을 확보하기 위해 얼마나 움직여야 하는지 계산
                float moveDist = dist - hit.distance + 0.01f;
                moveDir += -dir * moveDist;
                Debug.Log($"{dir} 방향으로 {dist}만큼 공간이 필요한데 {hit.distance}만큼밖에 공간이 없음. 반대편으로 {moveDist} 만큼 움직임 필요");
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            Vector3 targetPosition = transform.position + moveDir;
            ProCamera2D.Instance.MoveCameraInstantlyToPosition(targetPosition);
            Debug.Log("AdjustCameraPosition: 걸리는 콜라이더가 있으므로 움직임 수행");
        }
    }
}
