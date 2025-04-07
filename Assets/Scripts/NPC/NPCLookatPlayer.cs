using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLookatPlayer : MonoBehaviour
{
    // 대화 도중 등 LookAt이 작동하지 않아야 될 상황에서는 끄기
    public static bool EnableGlobally = true;

    // anyportrait로 제작된 NPC 비주얼은 모두 기본 왼쪽을 보고 있음
    private LR _lookingDir = LR.LEFT;

    private readonly Vector3 flipScale = new Vector3(-1, 1, 1);

    // Update is called once per frame
    void Update()
    {
        if (!EnableGlobally) return;

        // 플레이어가 왼쪽에 있을 경우
        if(PlayerRef.Instance.transform.position.x < transform.position.x)
        {
            if (_lookingDir.isRIGHT())
            {
                Flip();
                _lookingDir = LR.LEFT;
            }
        }
        else
        {
            if (_lookingDir.isLEFT())
            {
                Flip();
                _lookingDir = LR.RIGHT;
            }
        }
    }

    void Flip()
    {
        transform.localScale = Vector3.Scale(transform.localScale, flipScale);
    }
}
