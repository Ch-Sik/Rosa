using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(ProCamera2D))]
public class CameraCollisionHandler : MonoBehaviour
{
    [SerializeField, ReadOnly]
    ProCamera2D proCamera2D;

    private void Start()
    {
        proCamera2D = GetComponent<ProCamera2D>();
        // 25.06.15) 
        // AdjustCameraInfluenceTemporarily 함수 비정상 작동, 임시로 코드 비활성화
        // 비정상 작동했던 상황 1-b에서 1-7로 넘어갈때
        // MapManager.Instance.OnNextRoomLoaded += AdjustCameraInfluenceTemporarily;
    }

    // 다른 방으로 넘어갔을 때 맵의 경계 콜라이더와 카메라가 겹쳐진 채로 시작하는 문제를 처리하는 함수
    void AdjustCameraInfluenceTemporarily()
    {
        StartCoroutine(c());

        IEnumerator c()
        {
            if(MapManager.Instance.currentRoomManager.roomCenterTransfrom == null)
                yield break;

            // 카메라를 일시적으로 맵 중앙으로 이동시킴
            Transform originalTarget = proCamera2D.CameraTargets[0].TargetTransform;
            proCamera2D.CameraTargets[0].TargetTransform = MapManager.Instance.currentRoomManager.roomCenterTransfrom;

            yield return new WaitForFixedUpdate(); // 최소 1 Fixed Frame 대기
            yield return new WaitForSeconds(0.05f); // 

            // 원래대로 돌아가면서 플레이어를 가리키되, 맵 경계 바깥은 비추지 않게 함.
            proCamera2D.CameraTargets[0].TargetTransform = originalTarget;
        }
    }
}
