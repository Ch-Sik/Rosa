using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorFairy : MonoBehaviour
{
    public static CursorFairy Instance;

    [SerializeField] private ParticleSystem particle;
    private ParticleSystem.MainModule mainParticle;
    private Color originColor;

    [SerializeField, ReadOnly] private bool isTargetingTiles = false;
    [SerializeField, ReadOnly] private Vector3 mouseScreenPosition;
    [SerializeField, ReadOnly] private Vector3 mouseWorldPosition;
    [SerializeField, ReadOnly] private Vector3 magicPreviewPosition;

    InputAction defaultAim;
    InputAction magicAim;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        // player 프리팹에 묶여있다가 게임 시작하면 player 자식에서 빠져나옴.
        transform.parent = Camera.main.transform;

        if(particle != null)
        {
            mainParticle = particle.main;
            originColor = mainParticle.startColor.color;
        }
        defaultAim = InputManager.Instance.AM_ActionDefault.FindAction("Aim");
        magicAim = InputManager.Instance.AM_ActionMagicReady.FindAction("Aim");
        defaultAim.performed += GetMousePosition;
        magicAim.performed += GetMousePosition;
    }

    void GetMousePosition(InputAction.CallbackContext context)
    {
        mouseScreenPosition = context.ReadValue<Vector2>();
        // z=0인 xy평면으로 좌표 계산
        mouseWorldPosition = ScreenToWorldPosition(mouseScreenPosition);

        // 마우스 위치 설정
        if (!isTargetingTiles)
        {
            transform.position = mouseWorldPosition;
        }
    }

    Vector3 ScreenToWorldPosition(Vector3 screenPos)
    {
        screenPos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetMagicMode(bool value)
    {
        if(value == true)
        {
            isTargetingTiles = false;
            mainParticle.startColor = Color.red;
        }
        else
        {
            isTargetingTiles = false;
            mainParticle.startColor = originColor;
        }
    }

    public void SetMagicPreview(bool isValidPos, Vector3 targetPosition)
    {
        magicPreviewPosition = targetPosition;
        if(isValidPos)
        {
            isTargetingTiles = true;
            mainParticle.startColor = Color.green;

            // 커서 위치 설정
            transform.position = magicPreviewPosition;
        }
        else
        {
            isTargetingTiles = false;
            mainParticle.startColor = Color.red;

            // 커서 위치 설정
            transform.position = ScreenToWorldPosition(magicAim.ReadValue<Vector2>());
        }
    }
}
