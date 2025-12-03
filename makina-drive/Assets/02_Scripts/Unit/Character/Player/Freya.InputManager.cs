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
        else
        {
            _stateMachine.ChangeState(Triggers.attackConplete);
        }
    }
}
