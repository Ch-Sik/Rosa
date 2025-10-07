using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;
/// <summary>
/// 플레이어 애니메이션을 담당
/// </summary>
public class PlayerAnimation : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;
    public PlayerController playerControl;
    public PlayerMovement playerMove;
    public PlayerRef playerRef;
    public apPortrait portrait;
    public float minAngle;
    public float maxAngle;

    private Vector3 _aimPosition;
    public enum AnimState {Idle, Walk, Attack, Jump, Climb, Fall }

    
    public AnimState state = AnimState.Idle;
    bool isDoingAttack = false;

    Coroutine blinkCoroutine;
    float reactionBrightness = 0.75f;
    private void Start()
    {
        GetComponents();
    }

    void GetComponents()
    {
        playerRef = PlayerRef.Instance;
        rb = playerRef.rb;
        //anim = playerRef.anim;
        playerControl = playerRef.controller;
        playerMove = playerRef.movement;
        if(portrait == null)
            portrait = GetComponentInChildren<apPortrait>();
    }

    private void FixedUpdate()
    {
        UpdateVariables();
        UpdateAnimParameters();
        // 25.08.16) 마우스 방향 바라보기 삭제
        // UpdateHeadDir();
    }

    private void UpdateVariables()
    {
        // isDoingAttack = playerRef.combat.isDoingAttack;
    }

    public void UpdateAnimParameters()
    {
        if (PlayerRef.Instance.movement.isMovingByScript) return;

        anim.SetBool("isGrounded", playerMove.isGrounded);
        anim.SetBool("isWalking", 
            playerControl.currentMoveState == PlayerMoveState.DEFAULT 
            && !(isDoingAttack && playerMove.noMoveOnAttack)
            && Mathf.Abs(playerMove.moveVector.x) > 0.1f);
        //anim.SetBool("isJumping", 
        //    playerControl.currentMoveState == PlayerMoveState.DEFAULT 
        //    && !playerMove.isGrounded 
        //    && !playerMove.isDoingHooking);

        anim.SetBool("isClimbing", 
            playerControl.currentMoveState == PlayerMoveState.CLIMBING);
        anim.SetBool("isClimbEnd",
            playerMove.isDoingLedgeClimb);
        anim.SetBool("isInputClimbX", 
            playerControl.currentMoveState == PlayerMoveState.CLIMBING 
            && (playerMove.facingDirection.toVector2().x == -playerMove.moveVector.x));
        //anim.SetBool("isMagicReady", 
        //    playerControl.currentActionState == PlayerActionState.MAGIC_READY);
        //anim.SetBool("isClimbOver", ((playerControl.currentMoveState == PlayerMoveState.CLIMBING && playerMove.isWallClimbingTop) ? true : false));
        anim.SetFloat("xVel", rb.velocity.x);
        anim.SetFloat("yVel", rb.velocity.y);
        anim.SetFloat("spriteDir", transform.localScale.x);
        anim.SetBool("isHoldingCube", playerRef.grabCube.isHoldingCube);
        anim.SetBool("isGliding", playerMove.isGliding);
        anim.SetBool("isSuperDashing", playerMove.isDoingSuperDash);
        // 25.05.20) 대시 애니메이션 추가
        anim.SetBool("isDashing", playerMove.isDashing);
    }

    public void SetAttackAnimTrigger()
    {
        anim.SetTrigger("Attack");
    }

    Rect leftHalfScreen = new Rect(0, 0, 0.5f, 1f);
    Rect rightHalfScreen = new Rect(0.5f, 0, 0.5f, 1f);
    Rect wholeScreen = new Rect(0, 0, 1f, 1f);

    private Vector2 headDirection = new Vector2(0, 0);
    private Vector2 eyeDirection = new Vector2(0, 0);
    void UpdateHeadDir()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 viewportPosition = Camera.main.ScreenToViewportPoint(mousePosition);

        if(gameObject.transform.localScale.x == -1)
        {
            // Viewport 좌표를 (-1, -1)에서 (1, 1) 범위로 매핑
            headDirection = MapToRange(viewportPosition, rightHalfScreen.min, rightHalfScreen.max, new Vector2(-1, -1), new Vector2(1, 1));
            eyeDirection = MapToRange(viewportPosition, wholeScreen.min, wholeScreen.max, new Vector2(-1, -1), new Vector2(1, 1));
        }
        else
        {
            // Viewport 좌표를 (-1, -1)에서 (1, 1) 범위로 매핑
            headDirection = MapToRange(viewportPosition, leftHalfScreen.min, leftHalfScreen.max, new Vector2(1, -1), new Vector2(-1, 1));
            eyeDirection = MapToRange(viewportPosition, wholeScreen.min, wholeScreen.max, new Vector2(1, -1), new Vector2(-1, 1));
        }

        portrait.SetControlParamVector2("Head Direction", headDirection);
        portrait.SetControlParamVector2("Eye Direction", eyeDirection);
    }

    Vector2 MapToRange(Vector3 value, Vector3 fromMin, Vector3 fromMax, Vector2 toMin, Vector2 toMax)
    {
        Vector2 result;
        result.x = Mathf.Lerp(toMin.x, toMax.x, Mathf.InverseLerp(fromMin.x, fromMax.x, value.x));
        result.y = Mathf.Lerp(toMin.y, toMax.y, Mathf.InverseLerp(fromMin.y, fromMax.y, value.y));
        return result;
    }

    public void BlinkEffect()
    {
        if (portrait == null)
            return;         // 아직 애니메이션이 적용되지 않은 녀석들 예외 처리

        // 딱히 추가로 수정할 필요 없어보여서 수치들을 하드코딩했는데 필요하다면 필드값으로 빼낼 것
        const float timePerBlink = 0.2f;
        const int blinkCount = 8;
        Color blinkColor = Color.gray * reactionBrightness;
        blinkColor.a = 1;

        blinkCoroutine = StartCoroutine(DoBlink());

        IEnumerator DoBlink()
        {
            for (int i = 0; i < blinkCount; i++)
            {
                portrait.SetMeshColorAll(blinkColor);
                yield return new WaitForSeconds(timePerBlink / 2);
                portrait.ResetMeshMaterialToBatchAll();
                yield return new WaitForSeconds(timePerBlink / 2);
            }
            blinkCoroutine = null;
        }
    }

    public void SetJumpTrigger()
    {
        anim.SetTrigger("isJumping");
    }

    public void SetTrigger(string name)
    {
        anim.SetTrigger(name);
        
    }

    public void ResetTrigger(string name)
    {
        anim.ResetTrigger(name);
    }
}
