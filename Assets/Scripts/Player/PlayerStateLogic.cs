using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerState의 여러 상황에서의 상태를 획득하고,
/// 상태에 맞는 논리를 반환한다.
/// </summary>

public class PlayerStateLogic : MonoBehaviour
{
    //Logic에 사용될 데이터 PreSet;
    public StateLogic hit;
    public StateLogic interaction;
    public StateLogic grounded;
    public StateLogic midair;
    public StateLogic climbing;
    public StateLogic butterfly;
    //현재의 데이터
    [ShowInInspector] private StateLogic logic = new StateLogic();
    //마법 준비중인가? 
    public bool isReadyforMagic = false;
    //맞았는가?
    public bool onHit = false;

    Rigidbody2D rigid;
    float gravityScale = 0.0f;

    private void Start()
    {
        rigid = PlayerRef.Instance.Move.GetComponent<Rigidbody2D>();
        gravityScale = rigid.gravityScale;
    }

    private void Update()
    {
        StateFinder();
    }

    public State GetState() { return logic.state; }
    public bool CanJump() { return logic.canJump; }
    public bool CanHorizontalMove() { return logic.canHorizontalMove; }
    public bool CanVerticalMove() {  return logic.canVerticalMove; }
    public bool UseGrivity() { return logic.useGravity; }
    public bool CanAttack() { return logic.canAttack; }
    public bool UseMagic() { return logic.useMagic; }

    //상태를 추적하여 상태를 변환하는 함수
    private void StateFinder()
    {
        State curState = State.None;
        /*
        if(UIOpen)
        {
            ChangeState(State.Interaction);
            return;
        }
        */
        /*
        if(맞는다면!)
        {
            ChangeState(State.HIT);
            return;
        }
        */
        if (PlayerRef.Instance.combat.isFly)
        {
            ChangeState(State.BUTTERFLY);
            return;
        }

        switch (PlayerRef.Instance.Controller.MoveState)
        {
            case PlayerMoveState.WALK:
            case PlayerMoveState.GROUNDED:
                curState = State.GROUNDED;
                break;
            case PlayerMoveState.MIDAIR:
                curState = State.MIDAIR;
                break;
            case PlayerMoveState.CLIMBING:
                curState = State.CLIMBING;
                break;
        }

        ChangeState(curState);
    }

    //스테이트를 전환하는 함수 단 1회 실행됌.
    private void ChangeState(State state)
    {
        if (logic.state == state)
            return;

        // Debug.Log("State 전환 : " + state);
        switch (state)
        {
            case State.INTERACTION:
                logic.Copy(interaction);
                //여기에 이벤트를 넣을 수 있음!
                break;
            case State.GROUNDED:
                logic.Copy(grounded);
                break;
            case State.MIDAIR:
                logic.Copy(midair);
                break;
            case State.CLIMBING:
                logic.Copy(climbing);
                break;
            case State.BUTTERFLY:
                logic.Copy(butterfly);
                break;
            case State.HIT:
                logic.Copy(hit);
                break;
        }

        Vector2 vec = PlayerRef.Instance.Move.moveVector;

        if (!CanHorizontalMove())
            vec.x = 0;
        if (!CanVerticalMove())
            vec.y = 0;

        /*
        if(CanJump())
            점프상태 관리
        if(UseMagic())
            마법사용불가, 마법레디 취소
        */
        if (CanAttack())
            PlayerRef.Instance.combat.canAttack = true;
        else
            PlayerRef.Instance.combat.canAttack = false;

        if (UseGrivity())
            rigid.gravityScale = gravityScale;
        else
            rigid.gravityScale = 0;

        PlayerRef.Instance.Move.moveVector = vec;
    }
}

public enum State
{
    None,
    INTERACTION,
    GROUNDED,
    MIDAIR,
    CLIMBING,
    BUTTERFLY,
    HIT
}

[Serializable]
public class StateLogic
{
    //특정 상태
    public State state;

    //특정 상태에서의 로직들
    public bool canJump = true;
    public bool canHorizontalMove = true;
    public bool canVerticalMove = true;
    public bool useGravity = true;
    public bool canAttack = true;
    public bool useMagic = true;

    public void Copy(StateLogic logic)
    {
        this.state = logic.state;
        this.canJump = logic.canJump;
        this.canHorizontalMove = logic.canHorizontalMove;
        this.canVerticalMove = logic.canVerticalMove;
        this.useGravity = logic.useGravity;
        this.canAttack = logic.canAttack;
        this.useMagic = logic.useMagic;
    }
}
