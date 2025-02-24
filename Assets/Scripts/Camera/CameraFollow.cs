using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 따라다니는 카메라
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // 싱글톤
    private static CameraFollow _instance;
    public static CameraFollow Instance { get { return _instance; } }
    private Camera _camera;
    private PlayerRef _player;

    private Vector3 zOffset = new Vector3(0, 0, -10);

    public bool allowCameraFollow = true;

    private void Start()
    {
        Init();
    }

    public void AllowCamFollow() { allowCameraFollow = true; }

    public void DisallowCamFollow() { allowCameraFollow = false; }

    public void Init()
    {
        _camera = GetComponent<Camera>();
        _player = PlayerRef.Instance;
    }

    private void Update()
    {
        if (!allowCameraFollow)
            return;

        transform.position = _player.transform.position + zOffset;
    }
}
