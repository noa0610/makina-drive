using System;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class Freya
{
    public override void Pause()
    {
        GetComponent<PlayerInput>().DeactivateInput();
    }

    public override void Play()
    {
        GetComponent<PlayerInput>().ActivateInput();
    }

    private void OnMove(InputValue value) => OnMove(value.Get<Vector2>());
    public void OnMove(Vector2 dir)
    {
        if (dir != Vector2.zero)
        {
            Direction = dir.normalized;
            AttackDirection = dir;
            _stateMachine.ChangeState(Triggers.moveInput);
        }

        MoveDirection = dir.normalized;

        if (dir.x != 0)
        {
            _animator.SetTrigger("toWalk");
        }

        if (dir == Vector2.zero)
        {
            _stateMachine.ChangeState(Triggers.moveCancel);
        }
    }

    private void OnFire(InputValue value) => OnFire(value.isPressed);
    public void OnFire(bool isPressed)
    {
        if (isPressed)
        {
            _stateMachine.ChangeState(Triggers.attackInput);
        }
    }


    private void OnS_Attack(InputValue value) => OnS_Attack(value.isPressed);
    public void OnS_Attack(bool isPressed)
    {
        if (isPressed)
        {
            _stateMachine.ChangeState(Triggers.TestShoot);
        }
    }

    private void OnDodge(InputValue value) => OnDodge(value.isPressed);
    public void OnDodge(bool isPressed)
    {
        if (isPressed)
        {
            Debug.Log("stamina: " + statusManager.ReadValue(Status.Stamina));
            if(statusManager.ReadValue(Status.Stamina) < 20f)
            {
                // スタミナ不足で回避できない
                return;
            }
            if(IsMatchingState(States.dodge))
            {
                // すでに回避中
                return;
            }
            _stateMachine.ChangeState(Triggers.dodgeInput);
            statusManager.AddValue(Status.Stamina, -20f);
        }
    }

    private void OnDash(InputValue value) => OnDash(value.isPressed);
    public void OnDash(bool isPressed)
    {
        if (isPressed)
        {
            _inputDash = true;
            _stateMachine.ChangeState(Triggers.dashInput);
        }
        else
        {
            _inputDash = false;
            _stateMachine.ChangeState(Triggers.dashCancel);
        }
    }

    private void OnJump(InputValue value) => OnJump(value.isPressed);
    public void OnJump(bool isPressed)
    {
        if (isPressed)
        {
            _stateMachine.ChangeState(Triggers.jumpInput);
        }
    }
}
