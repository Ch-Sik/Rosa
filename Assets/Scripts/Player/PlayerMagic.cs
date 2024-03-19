using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public struct TerrainCastHit
{
    public Vector2 worldPos;
    public Vector3Int cellPos;
}



/// <summary>
/// 식물 마법 시전을 담당하는 컴포넌트
/// 시전 미리보기는 CursorFairy.cs에서 담당하며 여기에서는 상관하지 않음.
/// </summary>
public class PlayerMagic : MonoBehaviour
{
    [Header("사용 가능한 마법 목록")]
    [SerializeField]
    private SO_Magic[] magicList;

    [Space(10), Header("시전 관련")]
    [SerializeField, Tooltip("지형 경계면과 마우스가 얼마나 떨어져있어도 시전 가능한 것으로 판단할지?")]
    private float castDist = 1.5f;

    [Space(10), Header("UI")]
    [SerializeField]
    private PlayerStateUI playerStateUI;

    [Header("정보")]
    [SerializeField, ReadOnly]
    private SO_Magic selectedMagic;
    [SerializeField, ReadOnly]
    private bool isPreviewOn = false;
    [SerializeField, ReadOnly]
    private Vector3? magicPos;          // null이라면 현재 조준하고 있는 위치가 유효하지 않다는 뜻
    [SerializeField, ReadOnly]
    private int targetTerrainType;      // 0-바닥, 1-왼쪽을 보는 벽, 2-오른쪽을 보는 벽, 3-천장
    [SerializeField, ReadOnly]
    private LR wallLR;                  // 벽일 경우, 왼쪽을 보는 벽인지, 오른쪽을 보는 벽인지

    /*
    [Space(10), Header("스폰된 오브젝트")]
    [SerializeField, ReadOnly]
    private GameObject[] spawnedObject = new GameObject[8];
    */

    private InputManager inputInstance;
    private InputAction aimInput;
    private int layerMagicAble = 0;
    private int selectedMagicIndex = 0;

    private const string layerStringGround = "Ground";
    private const string layerStringTmp = "TmpTerrain";
    private const string layerStringCurtain = "HiddenRoom";

    // 4방향 검사용 상수들
    private int[] cTerrainFlag = { 0x01, 0x06, 0x06, 0x08 };
    private Vector2[] cDirections = { Vector2.up, Vector2.left, Vector2.right, Vector2.down };
    private const int GROUND = 0;
    private const int LWALL = 1;
    private const int RWALL = 2;
    private const int CEIL = 3;

    [SerializeField, ReadOnly]
    private TilemapGroup targetTileGroup;


    private void Start()
    {
        Init();
    }

    private void Init()
    {
        // 상수 설정
        layerMagicAble |= LayerMask.GetMask(layerStringGround);
        layerMagicAble  |= LayerMask.GetMask(layerStringTmp);
        layerMagicAble  |= LayerMask.GetMask(layerStringCurtain);

        // 파라미터 정상 설정되어있는지 검사
        Debug.Assert(magicList.Length != 0, "식물 마법이 하나도 설정되어있지 않음!");

        // 필드 초기화
        selectedMagic = magicList[selectedMagicIndex];
        if (inputInstance == null)
            inputInstance = InputManager.Instance;
        aimInput = inputInstance._inputAsset.FindActionMap("MagicReady").FindAction("Aim");
        playerStateUI = PlayerStateUI.Instance;
    }

    private void Update()
    {
        if (isPreviewOn)
            UpdatePreview();
    }

    /// <summary>
    /// 마법 선택 키를 눌렀을 때, 마법 교체
    /// </summary>
    public void SelectMagic()
    {
        selectedMagicIndex = (++selectedMagicIndex) % magicList.Length;
        selectedMagic = magicList[selectedMagicIndex];
        if(playerStateUI != null)
            playerStateUI.UpdateSelectedMagic(selectedMagic);
    }

    /// <summary>
    /// 마법 시전 키를 처음 눌렀을 때.
    /// 현재 선택된 마법이 유효한지 검사하고, 미리보기 켜기.
    /// </summary>
    public void ReadyMagic()
    {
        // TODO: PlayerState와 연동하여 현재 선택된 마법이 유효한지 확인하기
        inputInstance.SetActionInputState(PlayerActionState.MAGIC_READY);
        Debug.Log("식물 마법 시전 준비");

        ShowPreview();
    }

    /// <summary>
    /// Prepare Cast 상태에서 클릭했을 경우 미리보기로 보여준 위치에 실제로 마법을 시전하기.
    /// </summary>
    public void ExecuteMagic()
    {
        if (magicPos == null)
        {
            Debug.Log("식물마법 시전 실패: 잘못된 위치 지정");
            CancelMagic();
            return;
        }

        DoMagic();
        Debug.Log($"식물마법 시전\n 종류: {selectedMagic.name}\n 지형타입: {targetTerrainType}");
        HidePreview();

        inputInstance.SetActionInputState(PlayerActionState.DEFAULT);
    }

    private void DoMagic()
    {
        PlayerRef.Instance.State.ConsumeSeed(1, selectedMagic.seedRechargeTime);

        GameObject magicInstance;
        if (selectedMagic.prefab == null)
        {
            Debug.LogError($"{selectedMagic.name}: 식물마법 프리팹이 지정되지 않음!");
            return;
        }

        // 회전 계산 및 식물 오브젝트 소환
        if (selectedMagic.castType == MagicCastType.EVERYWHERE)
        {
            // 천장-벽-바닥 아무데나 설치가능한 경우
            // '지형과 수직인 방향'을 가르키기 위해 '회전'을 사용
            float rotation;
            switch (targetTerrainType)
            {
                case GROUND: rotation = 0f; break;
                case LWALL: rotation = 90f; break;
                case RWALL: rotation = 270f; break;
                default: //case CEIL:
                    rotation = 180f; break;
            }
            magicInstance = Instantiate(selectedMagic.prefab, (Vector3)magicPos, Quaternion.Euler(0, 0, rotation));
        }
        else
        {
            // 그 외의 경우 '지형과 수직인 방향'을 가르키기 위해 '좌우 대칭'을 사용
            magicInstance = Instantiate(selectedMagic.prefab, (Vector3)magicPos, Quaternion.identity);
            // wall_facing_left는 기본 형태, wall_facing_right가 반전된 형태
            if (targetTerrainType == RWALL)
            {
                magicInstance.transform.localScale = Vector3.Scale(magicInstance.transform.localScale, new Vector3(-1, 1, 1));
            }
        }

        // 움직이는 플랫폼 대비, 식물마법을 대상 타일맵그룹의 자식오브젝트로 설정
        magicInstance.transform.SetParent(targetTileGroup.transform);

        // 식물 마법의 Init까지 수행
        magicInstance.GetComponentInChildren<MagicObject>().Init((Vector2)magicPos, selectedMagic.lifeTime, targetTileGroup);

        /*
        // 오브젝트 풀 관리: 동시에 유지 가능한 오브젝트는 최대 1개
        if (spawnedObject[(int)selectedMagic.skillCode] != null)
        {
            // TODO: 기존에 설치된 식물이 자연스럽게 사라지는 것 연출
            Destroy(spawnedObject[(int)selectedMagic.skillCode], 1f);
        }
        spawnedObject[(int)selectedMagic.skillCode] = magicInstance;
        */
    }

    /// <summary>
    /// Prepare Cast 상태에서 우클릭했을 경우 마법 시전을 취소하기
    /// </summary>
    public void CancelMagic()
    {
        inputInstance.SetActionInputState(PlayerActionState.DEFAULT);
        Debug.LogWarning("식물마법 취소");
        HidePreview();
    }

    /// <summary> 미리보기 ON </summary>
    [ContextMenu("Set Preview ON")]
    private void ShowPreview()
    {
        isPreviewOn = true;
        CursorFairy.Instance.SetMagicMode(true);
    }

    /// <summary> 미리보기 OFF </summary>
    [ContextMenu("Set Preview OFF")]
    private void HidePreview()
    {
        isPreviewOn = false;
        //previewObject.SetActive(false);
        CursorFairy.Instance.SetMagicMode(false);
    }

    /// <summary> 미리보기 Update </summary>
    private void UpdatePreview()
    {
        if(PlayerRef.Instance.State.currentSeed < 1)
        {
            magicPos = null;
            CursorFairy.Instance.SetMagicPreview(false, Vector3.zero);

            return;
        }
        CheckIfValidPosition();
    }

    private void CheckIfValidPosition()
    {
        // 마우스 위치 가져오기
        Vector2 mouseScreenPos2 = aimInput.ReadValue<Vector2>();
        float zDistance = transform.position.z - Camera.main.transform.position.z; // 플레이어와 카메라의 z좌표 차이
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos2.x, mouseScreenPos2.y, zDistance));

        // 식물 마법 종류에 따라 캐스팅 방향 판단하기
        int castFlag = 0;
        MagicCastType castType = selectedMagic.castType;
        switch (castType)
        {
            case MagicCastType.GROUND_ONLY:
                castFlag = cTerrainFlag[0]; break;
            case MagicCastType.WALL_ONLY:
                castFlag = cTerrainFlag[1]; break;
            case MagicCastType.CEIL_ONLY:
                castFlag = cTerrainFlag[3]; break;
            default:        // case MagicCastType.EVERYWHERE:
                castFlag = cTerrainFlag[0] | cTerrainFlag[1] | cTerrainFlag[3]; break;
        }

        // 마우스가 지형 안쪽인지 바깥쪽인지 파악하고 마우스 위치로부터 가장 가까운 캐스팅 위치 가져오기
        Collider2D col = Physics2D.OverlapPoint(mouseWorldPosition, layerMagicAble);
        TerrainCastHit? terrainHit = null;
        if (col != null)
        {
            terrainHit = FindTerrainPointFromInside(mouseWorldPosition, castFlag);
        }
        else
        {
            terrainHit = FindTerrainPointFromOutside(mouseWorldPosition, castFlag);
        }

        // 성공적으로 지형 위치를 가져왔다면 해당 위치에 프리뷰 표시하기
        if (terrainHit != null)
        {
            magicPos = ((TerrainCastHit)terrainHit).worldPos;
            //previewObject.SetActive(true);
            //previewObject.transform.position = (Vector3)magicPos;
            CursorFairy.Instance.SetMagicPreview(true, (Vector3)magicPos);
        }
        // 아니면 시전 불가능하다고 표시하기
        else
        {
            magicPos = null;
            // previewObject.SetActive(false);
            CursorFairy.Instance.SetMagicPreview(false, Vector3.zero);
        }
    }

    private TerrainCastHit? FindTerrainPointFromInside(Vector2 mousePosition, int castFlag)
    {
        // 가장 '마우스에 가까운' 지점을 찾을 때 쓰는 임시 변수. 최적화를 위해 sqr를 사용.
        float minDistanceSqr = float.MaxValue, tmpDistanceSqr;

        // overlap test & raycast용 임시 변수
        Collider2D overlapResult;                      
        RaycastHit2D raycastResult;

        // 결과 저장용
        TerrainCastHit result = new TerrainCastHit();

        // 4방향으로 검사
        for (int i = 0; i < 4; i++)
        {
            // 해당 방향으로 지형 검사를 해야 하는지 플래그 확인
            if ((castFlag & 1<<i) != 0)
            {
                // 지형 바깥쪽 방향으로 castDist만큼 뻗어도 지형 안쪽일 경우 '너무' 안쪽인 것으로 판정
                overlapResult = Physics2D.OverlapPoint(mousePosition + cDirections[i], layerMagicAble);
                if (overlapResult == null)
                {
                    // 지형 바깥쪽 지점에서 안쪽 방향으로 raycast
                    raycastResult = Physics2D.Raycast(mousePosition + cDirections[i], -cDirections[i], castDist, layerMagicAble);
                    tmpDistanceSqr = (raycastResult.point - mousePosition).SqrMagnitude();
                    if (raycastResult.collider != null && tmpDistanceSqr < minDistanceSqr)
                    {
                        // 마지막으로 식물 설치가능한 지형인지 파악
                        TilemapGroup targetTilemapGroup = raycastResult.collider.GetComponentInParent<TilemapGroup>();
                        Vector3Int tmpCellPos = targetTilemapGroup.WorldToCell(raycastResult.point - 0.1f * cDirections[i]);  // 움직이는 플랫폼을 고려하여 FloorToInt 대신 WorldToCell 사용
                        TileData tileData = targetTilemapGroup.GetTileData(tmpCellPos);
                        
                        if (tileData.isPlantable)
                        {
                            minDistanceSqr = tmpDistanceSqr;
                            targetTerrainType = i;
                            result.worldPos = raycastResult.point;
                            result.cellPos = tmpCellPos;
                            targetTileGroup = raycastResult.collider.GetComponentInParent<TilemapGroup>();
                        }
                    }
                }
            }
        }

        if (minDistanceSqr <= castDist) 
            return result;
        else 
            return null;
    }

    private TerrainCastHit? FindTerrainPointFromOutside(Vector2 mousePosition, int castFlag)
    {
        // 가장 '마우스에 가까운' 지점을 찾을 때 쓰는 임시 변수.
        float minDistance = float.MaxValue;

        // overlap test & raycast용 임시 변수
        RaycastHit2D raycastResult;

        // 결과 저장용
        TerrainCastHit result = new TerrainCastHit();

        // 4방향으로 검사
        for (int i = 0; i < 4; i++)
        {
            // 해당 방향으로 지형 검사를 해야 하는지 플래그 확인
            if ((castFlag & 1<<i) != 0)
            {
                // 레이캐스팅 수행
                raycastResult = Physics2D.Raycast(mousePosition, -cDirections[i], castDist, layerMagicAble);
                if (raycastResult.collider != null && raycastResult.distance < minDistance)
                {
                    // 마지막으로 식물 설치가능한 지형인지 파악
                    TilemapGroup targetTilemapGroup = raycastResult.collider.GetComponentInParent<TilemapGroup>();
                    Vector3Int tmpCellPos = targetTilemapGroup.WorldToCell(raycastResult.point - 0.1f * cDirections[i]);  // 움직이는 플랫폼을 고려하여 FloorToInt 대신 WorldToCell 사용
                    TileData tileData = targetTilemapGroup.GetTileData(tmpCellPos);

                    if (tileData.isPlantable)
                    {
                        minDistance = raycastResult.distance;
                        targetTerrainType = i;
                        result.worldPos = raycastResult.point;
                        result.cellPos = tmpCellPos;
                        targetTileGroup = raycastResult.collider.GetComponentInParent<TilemapGroup>();
                    }
                }
            }
        }
        if (minDistance < castDist)
            return result;
        else
            return null;
    }
}
