using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Tilemaps;
using DG.Tweening;


/// <summary>
/// 플레이어의 움직임 담당.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // 이동 관련 파라미터
    [FoldoutGroup("좌우 이동 관련")]
    [Tooltip("플레이어 이동 속도")]
    [SerializeField] float moveSpeed = 1f;
    public float MoveSpeed { get { return moveSpeed; } }

    [FoldoutGroup("좌우 이동 관련")]
    [Tooltip("시작 시 플레이어 스프라이트의 방향")]
    [SerializeField] LR spriteDirection = LR.RIGHT;

    [FoldoutGroup("좌우 이동 관련")]
    [Tooltip("공격 중에 좌우 이동 불가능하게 설정")]
    public bool noMoveOnAttack;

    [FoldoutGroup("좌우 이동 관련")]
    [Tooltip("공격과 동시에 이동이 가능할 경우, 그 때의 이동 속도 조정")]
    [SerializeField] float attackSpeedCoef = 0.5f;

    // 점프 관련 파라미터
    [FoldoutGroup("점프 관련")]
    [Tooltip("플레이어 점프 파워")]
    [SerializeField] float jumpPower = 10f;

    [FoldoutGroup("점프 관련")]
    [Tooltip("플레이어 하향 점프 파워")]
    [SerializeField] float jumpDownPower = 1f;

    [FoldoutGroup("점프 관련")]
    [Tooltip("최소 상승 시간")]
    [SerializeField] float minJumpUpDuration = 0.1f;

    [FoldoutGroup("점프 관련")]
    [Tooltip("하향 점프 플렛폼 적용 시간")]
    [SerializeField] float downJumpPlatformDuration = 0.1f;


    // 벽이동 관련 파라미터
    [FoldoutGroup("벽이동 관련")]
    [Tooltip("벽 기어오르기 활성화")]
    [SerializeField] public bool wallClimbEnabled = true;

    [FoldoutGroup("벽이동 관련")]
    [Tooltip("기어오르기 속도")]
    [SerializeField] float climbUpSpeed;

    [FoldoutGroup("벽이동 관련")]
    [Tooltip("기어내려가기 속도")]
    [SerializeField] float climbDownSpeed;

    [FoldoutGroup("벽이동 관련")]
    [Tooltip("벽 점프 활성화")]
    [SerializeField] bool wallJumpEnabled;

    [FoldoutGroup("벽이동 관련")]
    [Tooltip("벽 점프 시 속도")]
    [SerializeField] Vector2 wallJumpPower;

    [FoldoutGroup("벽이동 관련")]
    [Tooltip("벽 점프 최소시간")]
    [SerializeField] float minWallJumpDuration;

    [FoldoutGroup("벽이동 관련")]
    [Tooltip("반대를 보고 벽에 메달리는 시간")]
    [SerializeField] float maxClimbTime;

    [FoldoutGroup("벽이동 관련")]
    [Tooltip("벽오르기 root motion 대체")]
    public Vector3 ClimbEndOffset = new Vector3(0.85f, 0.744f);

    [FoldoutGroup("벽이동 관련")]
    [Tooltip("벽오르기 root motion 대체")]
    public float ledgeClimbStartTime = 0.1f, ledgeClimbUpTime = 0.1f, 
                    ledgeClimbForwardTime = 0.1f, ledgeClimbEndTime = 0.1f;

    // 대시 관련
    [FoldoutGroup("대시 관련")]
    [Tooltip("대시 활성화 여부")]
    [SerializeField, ReadOnly] bool dashEnabled = false;

    [FoldoutGroup("대시 관련")]
    [Tooltip("대시 속도")]
    [SerializeField] float dashSpeed = 5f;

    [FoldoutGroup("대시 관련")]
    [Tooltip("대시 유지 시간")]
    [SerializeField] float dashDuration = 0.3f;

    [FoldoutGroup("대시 관련")]
    [Tooltip("대시 쿨타임")]
    [SerializeField] float dashCooldown = 1.2f;

    [FoldoutGroup("대시 관련")]
    [Tooltip("디버그용: 대시 쿨타임 남은 시간")]
    [SerializeField, ReadOnly] float dashLeftCooldown;

    [FoldoutGroup("대시 관련")]
    [SerializeField] VfxPlayer dashVfx;

    // 버섯점프 관련
    [FoldoutGroup("버섯 점프 관련")]
    [Tooltip("버섯 점프 활성화 여부")]
    [SerializeField, ReadOnly] bool mushJumpEnabled = false;

    [FoldoutGroup("버섯 점프 관련")]
    [Tooltip("버섯을 설치할 때 플레이어 앞으로 얼마나 떨어지게 설치할건지")]
    [SerializeField] float mushroomOffset = 1.0f;

    [FoldoutGroup("버섯 점프 관련")]
    [SerializeField] GameObject mushroomPrefab;

    [FoldoutGroup("버섯 점프 관련")]
    [SerializeField, ReadOnly] GameObject mushroomInstance = null;

    [FoldoutGroup("버섯 점프 관련")]
    [Tooltip("플레이어 버섯 점프 이동 속도")]
    [SerializeField] float mushJumpMoveSpeed = 7f;

    [FoldoutGroup("버섯 점프 관련")]
    [Tooltip("플레이어 기존 이동 속도 저장")]
    float defaultMoveSpeed;

    [FoldoutGroup("버섯 점프 관련")]
    [Tooltip("플레이어 버섯 점프 파워")]
    [SerializeField] float mushJumpPower = 20f;

    [FoldoutGroup("버섯 점프 관련")]
    [SerializeField] VfxPlayer mushJumpVfx;

    // 공격 관련
    [FoldoutGroup("공격 관련")]
    [Tooltip("공격 활성화 여부")]
    [SerializeField, ReadOnly] bool attackEnabled = false;

    [FoldoutGroup("공격 관련")]
    [Tooltip("공격 투사체 프리팹")]
    [SerializeField] GameObject attackPrefab;

    [FoldoutGroup("공격 관련")]
    [Tooltip("공격 투사체 발사 위치")]
    [SerializeField] Transform attackMuzzle;


    // 슈퍼대시 관련
    //[FoldoutGroup("슈퍼대쉬(오이) 관련")]
    //[Tooltip("오이대쉬 속도")]
    //[SerializeField] float superDashSpeed = 3f;

    // 넉백 관련
    [FoldoutGroup("넉백 관련")]
    [Tooltip("넉백 계수")]
    [SerializeField] float knockbackStrength = 1f;

    [FoldoutGroup("큐브 관련")]
    [SerializeField] float grabSpeedCoef = 0.5f;

    // 활강 관련
    [FoldoutGroup("활강 관련")]
    [Tooltip("활강 활성화 여부")]
    [SerializeField, ReadOnly] bool glidingEnabled = false;

    [FoldoutGroup("활강 관련")] 
    [SerializeField] float glidingGravityScale = 0.1f;



    // 플래그
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isMovingByScript = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isGrounded = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isJumpingUp = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isFacingWall = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isWallClimbing = false;      // 벽에 붙어서 이동중
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isWallJumpReady = false;      // 벽에 붙어서 이동중
    // NOTE: 벽오르기가 애니메이션 기반이라 연속 애니메이션 재생을 막기 위해
    // '턱 도달'과 '턱오르기 진행중'의 개념을 분리함
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isOnLedge = false;           // 벽에 붙어서 이동중 '턱' 도달
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isDoingLedgeClimb = false;   // '턱 오르기' 진행중
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isWallJumping = false;       // 벽 점프 중인지
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isDoingMagic = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isKnockbacked = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isSlidingOnWall = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isDoingHooking = false;      // 후크액션을 수행하고 있는지
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isHitHookingTarget = false;  // 후크액션중 후크목표에 도달했는지
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isDoingAttack = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isDoingSuperDash = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isNotMoveable = false;   // 종합적으로 고려하여 플레이어가 움직일 수 있는지 없는지
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isGrabCube = false;          //큐브를 잡고 있는지
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isGliding = false;
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isMushJumping = false;       // 버섯점프를 하고 있는지
    [FoldoutGroup("플래그")]
    [ReadOnly] public bool isDashing = false;       // 대시 중인지

    // 범위 지정
    [FoldoutGroup("벽 감지 범위")]
    public Vector2 detectWallTop = new Vector2(0.0f, 0.5f);
    [FoldoutGroup("벽 감지 범위")]
    public Vector2 detectWallBot = new Vector2(0.6f, -0.7f);
    [FoldoutGroup("벽 감지 범위")]
    public Vector2 detectWallEndTop = new Vector2(0.0f, 0.5f);
    [FoldoutGroup("벽 감지 범위")]
    public Vector2 detectWallEndBot = new Vector2(0.6f, 0.0f);

    // 필드값 중 일부는 디버그용으로 Inspector에 노출
    [FoldoutGroup("Debug")]
    [VerticalGroup("Debug/Vertical")]
    [BoxGroup("Debug/Vertical/Component")]
    [ReadOnly, SerializeField] Rigidbody2D rb;

    [BoxGroup("Debug/Vertical/Component")]
    [ReadOnly, SerializeField] BoxCollider2D col;

    [BoxGroup("Debug/Vertical/Component")]
    [ReadOnly, SerializeField] PlayerRef playerRef;

    [BoxGroup("Debug/Vertical/Component")]
    [ReadOnly, SerializeField] PlayerController playerControl;

    [BoxGroup("Debug/Vertical/General")]
    [ReadOnly, SerializeField] public GameObject platformBelow = null;

    [BoxGroup("Debug/Vertical/General")]
    [Tooltip("현재 매달려있는 담쟁이")]
    [ReadOnly, SerializeField] public GameObject hangingIvy = null;

    [BoxGroup("Debug/Vertical/General")]
    [ReadOnly, SerializeField] public Vector2 moveVector;

    [BoxGroup("Debug/Vertical/General")]
    [ReadOnly, SerializeField] public LR facingDirection;                 // 플레이어 바라보는 방향

    // 타이머 (non-serializable)
    private Timer jumpTimer;                 // 최소 점프 시간을 위한 타이머
    private Timer jumpBufferTimer;           // 점프 선입력 타이머
    private Timer climbTimer;                // 반대방향 입력후 벽에 메달림 유지 타이머
    private Timer dashCooldownTimer;

    // 기타 변수
    float originGravityScale;               // 대시 할 때 잠깐 중력 적용받지 않도록 하면서 원본 중력값 보관

    // 상수
    LayerMask groundLayer;      // NameToLayer가 constructor에서 호출 불가능하여 InitFields에서 초기화
    LayerMask climbableLayer;
    float gravityScale = 2.8f;

    #region 초기화
    private void Start()
    {
        GetComponents();
        InitFields();
        LoadFlags();
    }


    void GetComponents()
    {
        playerRef = PlayerRef.Instance;
        rb = playerRef.rb;
        col = playerRef.col;
        playerControl = playerRef.controller;
    }

    void InitFields()
    {
        //aimLine = GameObject.Find("AimLine");
        facingDirection = spriteDirection;
        groundLayer = LayerMask.NameToLayer("Ground");
        climbableLayer = LayerMask.NameToLayer("Climbable");
        originGravityScale = rb.gravityScale;
        defaultMoveSpeed = moveSpeed;
    }

    void LoadFlags()
    {
        dashEnabled = FlagManager.Instance?.GetFlag("dashEnabled") == 1 ? true : false;
        mushJumpEnabled = FlagManager.Instance?.GetFlag("mushJumpEnabled") == 1 ? true : false;
        glidingEnabled = FlagManager.Instance?.GetFlag("glidingEnabled") == 1 ? true : false;
        attackEnabled = FlagManager.Instance?.GetFlag("attackEnabled") == 1 ? true : false;
    }
    #endregion

    #region 업데이트

    // =====
    // Update and set flags
    // =====

    void FixedUpdate()
    {
        UpdateFlags();
        UpdateMovement();
    }

    void UpdateFlags()
    {
        isFacingWall = DetectWall();

        if (isFacingWall)
        {
            if (playerControl.currentMoveState == PlayerMoveState.CLIMBING)
            {
                isOnLedge = CheckWallEnd();
            }
            else
            {
                isOnLedge = false;
            }

            isSlidingOnWall = false;
            if (isOnLedge && !isDoingLedgeClimb)
            {
                StartLedgeClimb();
            }
        }
        else
        {
            isOnLedge = false;
            if (playerControl.currentMoveState == PlayerMoveState.CLIMBING && !isWallJumpReady)
            {
                if (!isDoingLedgeClimb)
                    UnstickFromWall();
            }
            // isWallClimbingTop = false;
        }
        // isDoingAttack = playerRef.combat.isDoingAttack;
        isNotMoveable = isDoingLedgeClimb || isKnockbacked || isWallJumping || isDashing;
    }

    /// <summary>
    /// Walk와 Climb 등에서 설정한 moveVector를 실제로 플레이어 움직임에 반영
    /// </summary>
    void UpdateMovement()
    {
        if (isNotMoveable) return;

        // CLIMBING 상태면 벽오르기
        if (playerControl.currentMoveState == PlayerMoveState.CLIMBING)
        {
            if (playerControl.currentMoveState == PlayerMoveState.CLIMBING)
            {
                // 기어 올라가는 속도와 내려오는 속도를 구분
                rb.velocity = new Vector2(0,
                    moveVector.y * (moveVector.y > 0 ? climbUpSpeed : climbDownSpeed));
                if (isFacingWall == false)
                {
                    // TODO: 이 부분을 벽 위 지면에 올라가는 것으로 대체
                    //UnstickFromWall();
                }
            }
        }
        // DEFAULT 상태라면 좌우 이동
        else
        {
            float xVelocity = moveVector.x * moveSpeed;

            // 큐브를 옮기는 동안은 이동속도를 조정
            if (isGrabCube)
            {
                xVelocity *= grabSpeedCoef;
                //속도에 따라 값의 조정
                if (xVelocity == 0)
                    playerControl.currentCubeActionState = PlayerCubeActionState.GRAB;
                else
                {
                    if (IsFacingDirection(xVelocity))
                        playerControl.currentCubeActionState = PlayerCubeActionState.PUSH;
                    else
                        playerControl.currentCubeActionState = PlayerCubeActionState.PULL;
                }
            }
            else
                playerControl.currentCubeActionState = PlayerCubeActionState.DEFAULT;

            // 공격 중에도 이동속도 조정
            if (isDoingAttack)
                xVelocity *= attackSpeedCoef;

            rb.velocity = new Vector2(xVelocity, rb.velocity.y);

            // 글라이딩 중 또는 공격중에는 좌우 반전 수행하지 않음
            if(!isGliding && !isDoingAttack)
                LookAt2DLocal(moveVector);
        }
    }
    #endregion

    #region 걷기 관련

    /// <summary>
    /// 지상과 공중에서 x축 방향으로의 이동. 
    /// 여기서는 조작 입력값만 받고 실제 속도 업데이트는 UpdateMovement에서 이루어짐.
    /// </summary>
    /// <param name="inputVector">x,y 모두 -1~+1 사이의 값인 입력 벡터</param>
    internal void Walk(Vector2 inputVector)
    {
        moveVector = new Vector2(inputVector.x, 0);
    }
    #endregion

    #region 점프 관련
    internal void OnJump(bool pressedDown)
    {
        //큐브를 옮기는 중이라면, 큐브를 놓아버림
        if (isGrabCube)
            PlayerRef.Instance.grabCube.UnGrab();

        if (!isGrounded)
        {
            ReserveJump();       // 공중에서는 점프 선입력
        }
        else
        {
            if (pressedDown && (platformBelow?.CompareTag("Platform") == true))
            {
                // Debug.Log(platformBelow?.tag);
                JumpDown();      // 하향 점프
            }
            else
            {
                isJumpingUp = true;
                JumpUp();        // 상향 점프
            }
        }
    }

    internal void JumpUp()
    {
        if (isNotMoveable) return;

        rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        jumpTimer = Timer.StartTimer();
        playerRef.animation.SetJumpTrigger();
    }

    /// <summary>
    /// 숏점프와 롱점프 구분을 위해 조기 점프종료하는 함수
    /// </summary>
    internal void FinishJumpUp()
    {
        // early return
        if (isNotMoveable) return;
        if (!isJumpingUp) return;

        isJumpingUp = false;
        // 최소 점프 시간에 도달했는지 체크
        if (jumpTimer.duration > minJumpUpDuration)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y < 0 ? rb.velocity.y : 0);
        }
        else
            StartCoroutine(ReserveFinishJumpUp());

        IEnumerator ReserveFinishJumpUp()
        {
            // Debug.Log("Too short jump duration");
            yield return new WaitForSeconds(minJumpUpDuration - jumpTimer.duration);
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y < 0 ? rb.velocity.y : 0);
        }
    }

    /// <summary>
    /// 하향 점프. 콜라이더를 트리거로 변환하여 아래의 플랫폼을 통과할 수 있게 함.
    /// </summary>
    internal void JumpDown()
    {
        StartCoroutine(ApplyDownJumpCollider());
        rb.velocity = new Vector2(rb.velocity.x, jumpDownPower);

        IEnumerator ApplyDownJumpCollider()
        {
            PlatformEffector2D platEffector = platformBelow.GetComponent<PlatformEffector2D>();
            platEffector.rotationalOffset = 180f;
            yield return new WaitForSeconds(downJumpPlatformDuration);
            platEffector.rotationalOffset = 0;
        }
    }

    /// <summary>
    /// OnTriggerExit2D에 의해 호출됨. 플랫폼을 완전히 통과하면 트리거 해제
    /// </summary>
    void FinishJumpDown()
    {
        col.isTrigger = false;      // 지형 통과 불가능
    }

    /// <summary>
    /// 점프 선입력 받기. 선입력 받은 점프의 처리는 OnLanded에서 수행함.
    /// </summary>
    internal void ReserveJump()
    {
        jumpBufferTimer = Timer.StartTimer();
    }

    /// <summary>
    /// 벽에서의 점프. 벽점프는 소점프/대점프를 구분하지 않음.
    /// </summary>
    internal void WallJump()
    {
        if (isNotMoveable) return;

        // 플래그 설정
        isWallJumping = true;
        isWallJumpReady = false;

        // 벽으로부터 떨어지고 상태 바꾸기
        UnstickFromWall();
        playerControl.SetMoveState(PlayerMoveState.DEFAULT);

        if (!isSlidingOnWall)
        {
            float xDirection = facingDirection.isRIGHT() ? -1 : 1;  // 보고 있는 방향의 반대방향으로 점프
            rb.velocity = new Vector2(wallJumpPower.x * xDirection, wallJumpPower.y);
            Flip();
        }
        else
        {
            float xDirection = facingDirection.isRIGHT() ? 1 : -1;  // 보고 있는 방향으로 점프
            rb.velocity = new Vector2(wallJumpPower.x * xDirection, wallJumpPower.y);
        }

        // 애니메이션 트리거 발동
        playerRef.animation.SetJumpTrigger();


        StartCoroutine(ReserveFinishWallJump());

        IEnumerator ReserveFinishWallJump()
        {
            // minWallJumpDuration 동안 wallJumping 플래그를 통해 x축 방향 입력을 무시함. (Walk 함수 참고)
            yield return new WaitForSeconds(minWallJumpDuration);
            isWallJumping = false;
        }
    }

   
    #endregion

    #region 벽 오르기 관련

    /// <summary>
    /// 벽면에서의 y축 방향으로의 이동.
    /// 여기서는 조작 입력값만 받고 실제 속도 업데이트는 UpdateMovement에서 이루어짐.
    /// </summary>
    /// <param name="inputVector"></param>
    internal void Climb(Vector2 inputVector)
    {
        //큐브를 옮기는 중엔 벽타기 불가능
        if (isGrabCube)
            return;
        // 턱오르기 도중에 벽타기 애니메이션 나오는 경우 방지
        if (isDoingLedgeClimb)
            return;
        if (!wallClimbEnabled)
            return;

        // Debug.Log("Climbing");

        moveVector = inputVector;       // 벽 점프 등을 위해 x축 방향 필터링하지 않음.
        if (moveVector.x != 0)
        {
            if (!DetectWall())
            {
                Debug.Log("start Timer");
                climbTimer = Timer.StartTimer();
                isWallJumpReady = true;
            }
        }
        else
        {
            if (!DetectWall())
            {
                // TODO: 지면으로 올라가기 구현
                //Debug.Log("AAA");
                //UnstickFromWall();
            }
        }
    }

    internal void StopClimb(Vector2 inputVector)
    {
        if (isNotMoveable) return;
        if (moveVector.x != 0 && !isOnLedge)
        {
            if (!DetectWall() && climbTimer != null)
            {
                Debug.Log("TimerCheck: " + climbTimer.duration);
                if (climbTimer.duration > maxClimbTime)
                {
                    UnstickFromWall();
                    Flip();
                    isSlidingOnWall = true;
                    isWallJumpReady = false;
                }
            }
        }
        moveVector = inputVector;
        isWallJumpReady = false;
    }

    /// <summary>
    /// 벽 오르기 상태로 전환
    /// </summary>
    internal void StickToWall(GameObject ivy)
    {
        //큐브를 옮기는 중엔 벽에 붙기 불가능
        if (isGrabCube)
            return;
        // 넉백 중에도 벽에 붙기 불가능
        if (isKnockbacked)
            return;
        Debug.Assert(ivy != null);

        isWallClimbing = true;
        playerControl.SetMoveState(PlayerMoveState.CLIMBING);
        // 액션 state를 no_action으로 변경: 공격 및 새로운 마법 시전 불가능
        // playerControl.SetActionState(PlayerActionState.DISABLED);
        // 기존에 마법을 준비중이었으면 그것을 취소
        //if (playerRef.magic.isMagicMode)
        //{
        //    playerRef.magic.CancelMagic();
        //}
        rb.gravityScale = 0;
        hangingIvy = ivy;
        transform.parent = ivy.transform;
    }

    /// <summary>
    /// 벽 오르기 해제.
    /// </summary>
    internal void UnstickFromWall()
    {
        isWallClimbing = false;
        playerControl.SetMoveState(PlayerMoveState.DEFAULT);
        // playerControl.SetActionState(PlayerActionState.DEFAULT);
        rb.gravityScale = this.gravityScale;

        hangingIvy = null;
        transform.parent = null;
    }

    // 절벽 위에 도달했을 때 isOnLedge = true와 함께 호출됨.
    public void StartLedgeClimb()
    {
        // Debug.Log("LedgeClimbStart");
        isDoingLedgeClimb = true;
        isWallClimbing = false;

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        col.enabled = false;

        // TODO: 카메라가 fixedUpdate에 따라 움직이므로 해당 부분 fixedUpdate에 맞게 움직이도록 해야 버벅거리지 않음.
        DOTween.Sequence().AppendInterval(ledgeClimbStartTime)
            .Append(transform.DOLocalMoveY(transform.localPosition.y + ClimbEndOffset.y, ledgeClimbUpTime))
            .Append(transform.DOLocalMoveX(transform.localPosition.x + ClimbEndOffset.x, ledgeClimbForwardTime))    
                    // 담쟁이가 좌우반전되어있어서 ClimbOffset.x는 좌우 방향 신경쓸 필요 없음
            .AppendInterval(ledgeClimbEndTime)
            .AppendCallback(() => OnLedgeClimbEnd());
    }

    public void OnLedgeClimbEnd()
    {
        // Debug.Log("LedgeClimbEnd");

        hangingIvy = null;
        transform.parent = null;
        isDoingLedgeClimb = false;
        moveVector = Vector2.zero;

        rb.gravityScale = this.gravityScale;
        rb.isKinematic = false;
        col.enabled = true;

        playerControl.SetMoveState(PlayerMoveState.DEFAULT);
        // playerControl.SetActionState(PlayerActionState.DEFAULT);
    }

    /// <summary>
    /// 하향 점프 시 플랫폼을 온전히 빠져나오면 FinishJumpDown을 호출
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer.Equals(groundLayer))
        {
            FinishJumpDown();
        }
    }

    /// <summary>
    /// 덩굴과 충돌하였을 때 벽오르기 상태로 전환
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(isDashing)
        {
            Debug.Log("충돌로 일한 대시 취소");
            EndDash();
        }

        // Debug.Log($"{collision.gameObject}:{LayerMask.LayerToName(collision.gameObject.layer)}");

        //if (isDoingSuperDash)
        //{
        //    isDoingSuperDash = false;
        //    CancelSuperDashAfterLaunch();
        //}
        //else
        //{
            // 벽에 설치된 덩굴에 충돌하였을 경우
            if (collision.gameObject.layer.Equals(climbableLayer))
            {
                if (isFacingWall && !isSlidingOnWall)
                {
                    StickToWall(collision.gameObject);
                    //playerAnim.SetTrigger("ClimbTrigger");
                }
            }
        //}
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 벽에 설치된 덩굴에 충돌하였을 경우
        if (collision.gameObject.layer.Equals(climbableLayer))
        {
            if (isFacingWall && !isSlidingOnWall)
                StickToWall(collision.gameObject);
        }
    }

    bool DetectWall()
    {
        if ((moveVector.x == -1 && facingDirection == LR.RIGHT) || (moveVector.x == 1 && facingDirection == LR.LEFT)) return false;
        Vector2 pointTop = (Vector2)transform.position
                            + Vector2.Scale(detectWallTop, new Vector2(facingDirection.isRIGHT() ? 1 : -1, 1));
        Vector2 pointBot = (Vector2)transform.position
                            + Vector2.Scale(detectWallBot, new Vector2(facingDirection.isRIGHT() ? 1 : -1, 1));
        Collider2D[] hitWall = Physics2D.OverlapAreaAll(pointTop, pointBot);
        // OverlapArea가 clibableLayer로 지정하였을 때 항상 null을 리턴하는 문제가 있어 아래와 같이 구현함.
        foreach (var i in hitWall)
        {
            if (i.gameObject.layer == climbableLayer)
                return true;
        }
        return false;
    }

    bool CheckWallEnd()
    {
        Vector2 pointTop = (Vector2)transform.position
                            + Vector2.Scale(detectWallEndTop, new Vector2(facingDirection.isRIGHT() ? 1 : -1, 1));
        Vector2 pointBot = (Vector2)transform.position
                            + Vector2.Scale(detectWallEndBot, new Vector2(facingDirection.isRIGHT() ? 1 : -1, 1));
        Collider2D[] hitWall = Physics2D.OverlapAreaAll(pointTop, pointBot);
        foreach (var i in hitWall)
        {
            if (i.gameObject.layer == climbableLayer
                || i.gameObject.layer == groundLayer)
                return false;
        }
        return true;
    }
    public void Falling()
    {
        isSlidingOnWall = true;
        UnstickFromWall();
    }

    #endregion

    #region 버섯점프 관련

    /// <summary>
    /// 버섯에서의 점프. 버섯점프는 소점프/대점프를 구분하지 않음.
    /// </summary>
    public void MushJump()
    {
        if(!mushJumpEnabled)
        {
            Debug.Log("버섯 점프 미습득");
            return;
        }

        // 대시 중이라면 대시 종료
        if (isDashing)
        {
            EndDash();
            playerRef.animation.UpdateAnimParameters(); // 애니메이션 플래그 강제 업데이트
        }

        //큐브를 옮기는 중이라면, 큐브를 놓아버림
        if (isGrabCube)
            PlayerRef.Instance.grabCube.UnGrab();

        isJumpingUp = true;
        isMushJumping = true;
        moveSpeed = mushJumpMoveSpeed;
        rb.velocity = new Vector2(rb.velocity.x, mushJumpPower);
        playerRef.animation.SetJumpTrigger();
        // 25.05.23) 강조선이펙트 추가
        mushJumpVfx.PlayVFX();
    }

    [Button, FoldoutGroup("버섯 점프 관련")]
    public void EnableMushJump()
    {
        Debug.Log("버섯 점프 습득!");
        FlagManager.Instance?.SetFlag("mushJumpEnabled", 1);
        mushJumpEnabled = true;
    }
    #endregion

    #region 활강 관련
    internal void StartGliding()
    {
        if (!glidingEnabled)
        {
            Debug.Log("활강 미습득");
            return;
        }
        if (isGrounded) return;
        if (isWallJumping) return;
        if (isJumpingUp) FinishJumpUp();

        rb.velocity = new Vector2(rb.velocity.x, 0);
        isGliding = true;
        rb.gravityScale = glidingGravityScale;
    }

    internal void CancleGliding()
    {
        isGliding = false;
        rb.gravityScale = originGravityScale;
    }

    public void Rising(float risingPower)
    {
        if (isGliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, risingPower);
        }
    }

    public void CancleRising()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    [Button, FoldoutGroup("활강 관련")]
    public void EnableGliding()
    {
        Debug.Log("활강 습득!");
        FlagManager.Instance?.SetFlag("glidingEnabled", 1);
        glidingEnabled = true;
    }
    #endregion

    #region 대시 관련

    public void Dash()
    {
        if (!dashEnabled)
        {
            Debug.Log("대시 미습득");
            return;
        }

        if(isDashing || (dashCooldownTimer != null && dashCooldownTimer.duration < dashCooldown))
        {
            Debug.Log("대시 쿨타임 중");
            return;
        }
        DOTween.Sequence().OnStart(() =>
        {
            dashCooldownTimer = Timer.StartTimer();
            isDashing = true;

            Debug.Log("Dash");

            rb.gravityScale = 0;
            rb.velocity = Vector2.right * (facingDirection.isLEFT() ? -1 : 1) * dashSpeed;

            // 25.05.23) 강조선이펙트 추가
            dashVfx.PlayVFX();
        }).AppendInterval(dashDuration)
        .AppendCallback(() =>
        {
            EndDash();
        });
    }

    private void EndDash()
    {
        if (!isDashing) return;
        isDashing = false;

        if (isGliding)
            rb.gravityScale = glidingGravityScale;
        else
            rb.gravityScale = originGravityScale;
        rb.velocity = Vector2.zero;
    }

    [Button, FoldoutGroup("대시 관련")]
    public void EnableDash()
    {
        Debug.Log("대시 습득!");
        FlagManager.Instance?.SetFlag("dashEnabled", 1);
        dashEnabled = true;
    }

    #endregion

    #region 버섯 설치 관련

    public void MakeMushroom()
    {
        if(!mushJumpEnabled)
        {
            Debug.Log("버섯 점프 미습득");
            return;
        }
        if(isDashing)
        {
            Debug.Log("대시중에는 버섯 설치 불가");
            return;
        }

        // 플레이어 정면 방향으로 offset만큼 이동한 포인트.
        Vector2 frontPosition = (Vector2)(transform.position) + facingDirection.toVector2() * mushroomOffset;
        Collider2D overlapTest = Physics2D.OverlapCircle(frontPosition, 0.1f, LayerMask.GetMask("Ground"));
        if(overlapTest != null)
        {
            Debug.Log("전방이 지형으로 막혀있음. 버섯 설치 실패");
            return;
        }
        RaycastHit2D rayhit 
            = Physics2D.Raycast(frontPosition, Vector2.down, 1.0f, LayerMask.GetMask("Ground", "Platform"));
        if(rayhit.collider == null)
        {
            Debug.Log("전방에 바닥이 감지되지 않음. 버섯 설치 실패");
            return;
        }
        
        // 기존에 설치된 녀석이 있다면 삭제
        if(mushroomInstance != null)
        {
            // TODO: 삭제 연출 넣기
            mushroomInstance.GetComponent<MagicMushroom>().Disappear();
        }

        // 설치 수행
        mushroomInstance = Instantiate(mushroomPrefab, rayhit.point, Quaternion.identity);
    }

    #endregion

    #region 공격 관련
    public void TryAttack()
    {
        if (isGliding)
        {
            Debug.Log("활강 중에는 공격을 할 수 없음!");
            return;
        }
        if (!attackEnabled)
        {
            Debug.Log("공격 미습득");
            return;
        }
        // 현재 '공격 아이템' 가지고 있는 갯수 체크
        if (InventoryController.Instance == null)
        {
            Debug.LogError("InventoryController 인스턴스가 존재하지 않아 공격 아이템 소지 여부를 확인할 수 없음");
            return;
        }
        // 만약 가지고 있지 않다면 '공격 실패' 수행
        if (InventoryController.Instance.GetQuantity(ItemCode.AttackItem) <= 0)
        {
            FailAttack();
        }
        // 가지고 있다면 '진짜 공격' 수행하고 공격 아이템 1개 소모
        else
        {
            ExecuteAttack();
            InventoryController.Instance.RemoveItem(ItemCode.AttackItem, 1);
        }
    }

    private void FailAttack()
    {
        // TODO: 공격 실패하는 연출 추가
        Debug.Log("공격 아이템 갯수가 모자라서 공격 실패");
    }

    private void ExecuteAttack()
    {
        // 현재 바라보는 방향에 따라 투사체 발사 방향 결정
        Vector2 attackDir = transform.localScale.toLR().toVector2();
        // 투사체 생성 및 발사
        GameObject attackInstance = Instantiate(attackPrefab, attackMuzzle.position, Quaternion.identity);
        attackInstance.GetComponent<ProjectileBase>().InitProjectile(attackDir);

        Debug.Log("투사체 생성 및 발사 수행");
    }

    [Button, FoldoutGroup("공격 관련")]
    void EnableAttack()
    {
        FlagManager.Instance?.SetFlag("glidingEnabled", 1);
        attackEnabled = true;
    }
    #endregion

    #region 그라운드 체크. PlayerGroundCheck.cs에서 참조
    public void SetIsGrounded(GameObject belowObject)
    {
        //해당 값이 0일 경우, 플랫폼에 평행하게 진입할 경우 바로 리턴되는 아래와 같은 문제가 있었음.
        //1. 이동하면서 더블점프하면 점프가 안 되는 버그
        //2. A방향으로 더블점프를 착지 전에 -A 방향으로 전환 시 점프 리셋이 안 됌.
        if (rb.velocity.y > 0.1f) return; // 1-way platform의 groundcheck 방지
        if (playerControl.currentMoveState == PlayerMoveState.CLIMBING) return; // climb 중 groundcheck 방지
       
        if (!isGrounded)
        {
            // 막 착지했을 때
            OnLanded();
        }
        isGrounded = true;
        platformBelow = belowObject;
        // playerControl.ChangeMoveState(PlayerMoveState.GROUNDED);
    }

    public void SetIsNotGrounded()
    {
        isGrounded = false;
        platformBelow = null;
        // state가 기본 상태일 경우에만 state를 수정하지 않음.
        if (playerControl.currentMoveState == PlayerMoveState.DEFAULT)
        {
            // playerControl.ChangeMoveState(PlayerMoveState.MIDAIR);
            // playerRef.animation.ResetTrigger("Grounded");
        }
    }

    /// <summary>
    /// 그라운드 체크 관리는 PlayerController.cs에서 수행하고 OnLanded를 호출해줌.
    /// </summary>
    internal void OnLanded()
    {
        // playerRef.animation.SetTrigger("Grounded");
        if (isSlidingOnWall)
        {
            isSlidingOnWall = false;
        }
        if (jumpBufferTimer?.duration < 0.3) // 최근 O.3초 이내에 점프 선입력이 있었을 경우
        {
            JumpUp();
            Debug.Log("선입력으로 인한 점프");
        }
        if (isGliding)
        {
            CancleGliding();
        }
        if(isMushJumping)
        {
            isMushJumping = false;
            isJumpingUp = false;
            moveSpeed = defaultMoveSpeed;
        }
    }
    #endregion


    public void LookAt2D(Vector2 worldPoint)
    {
        float delta = worldPoint.x - transform.position.x;
        if((facingDirection.isLEFT() && delta > 0) 
            || (facingDirection.isRIGHT() && delta < 0))
        {
            Flip();
        }
    }

    public void LookAt2DLocal(Vector2 localPoint)
    {
        if ((facingDirection.isLEFT() && localPoint.x > 0)
            || (facingDirection.isRIGHT() && localPoint.x < 0))
        {
            Flip();
        }
    }

    public bool IsFacingDirection(float x)
    { 
        if ((facingDirection.isLEFT() && x > 0) ||
            (facingDirection.isRIGHT() && x < 0))
            return false;
        return true;
    }

    public void Flip()
    {
        if (isGrabCube)
            return;

        // 플레이어가 바라보는 방향을 전환
        if (facingDirection.isLEFT()) facingDirection = LR.RIGHT;
        else facingDirection = LR.LEFT;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
        //aimLine.transform.localScale = theScale;
    }

    public void Knockback(Vector2 normalizedDir, float customKnockbackPow)
    {
        DoKnockback(normalizedDir.normalized * customKnockbackPow);
    }

    public void Knockback(Vector2 normalizedDir)
    {
        DoKnockback(normalizedDir.normalized * knockbackStrength);
    }

    private void DoKnockback(Vector2 knockbackVector)
    {
        // 만약 담쟁이에 매달린 상태라면
        if (playerControl.currentMoveState == PlayerMoveState.CLIMBING)
        {
            // 벽에서 떨어짐
            UnstickFromWall();
            // 넉백 방향 강제 수정
            knockbackVector.x = Mathf.Abs(knockbackVector.x) * (facingDirection.isRIGHT() ? -1 : 1);
        }
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackVector * knockbackStrength, ForceMode2D.Impulse);

        StartCoroutine(Knockback());

        IEnumerator Knockback()
        {
            playerControl.SetMoveState(PlayerMoveState.NO_MOVE);
            LookAt2DLocal(-knockbackVector);     // 넉백되는 방향의 반대편 바라보기
            // 피격 애니메이션 관련은 PlayerDamageReceiver.GetDamage()로 옮김
            // 보스 패턴 등에서 '밀쳐내기'를 하면서도 데미지는 없는 경우가 있기 때문.
            // playerRef.animation.SetTrigger("Hit");
            isKnockbacked = true;
            yield return new WaitForSeconds(0.3f);

            playerControl.SetMoveState(PlayerMoveState.DEFAULT);
            isKnockbacked = false;
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 detectPointTop = (Vector2)transform.position
                            + Vector2.Scale(detectWallTop, new Vector2(facingDirection.isRIGHT() ? 1 : -1, 1));
        Vector2 detectPointBot = (Vector2)transform.position
                            + Vector2.Scale(detectWallBot, new Vector2(facingDirection.isRIGHT() ? 1 : -1, 1));
        Vector2 detectPointTopWall = (Vector2)transform.position
                            + Vector2.Scale(detectWallEndTop, new Vector2(facingDirection.isRIGHT() ? 1 : -1, 1));
        Vector2 detectPointBotWall = (Vector2)transform.position
                            + Vector2.Scale(detectWallEndBot, new Vector2(facingDirection.isRIGHT() ? 1 : -1, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((detectPointTop + detectPointBot) / 2, detectPointTop - detectPointBot);
        Gizmos.DrawWireCube((detectPointTopWall + detectPointBotWall) / 2, detectPointTopWall - detectPointBotWall);
    }
}
