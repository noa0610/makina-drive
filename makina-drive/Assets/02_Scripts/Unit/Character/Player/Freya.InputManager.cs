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
            _isPressingFire = true;
            // _stateMachine.ChangeState(Triggers.attackInput);
        }
        else
        {
            // ボタンが離された
            if (_isPressingFire)
            {
                if (_isChargeCompleted)
                {
                    // チャージ完了していたらチャージ攻撃
                    _stateMachine.ChangeState(Triggers.chargeAttackInput); // ※Triggersに定義が必要
                }
                else
                {
                    // チャージ未完了なら通常攻撃
                    _stateMachine.ChangeState(Triggers.attackInput);
                }
            }
            // ボタンが離れてリセット
            _isPressingFire = false;
            ResetCharge();
        }
    }


    private void OnS_Attack(InputValue value) => OnS_Attack(value.isPressed);
    public void OnS_Attack(bool isPressed)
    {
        if (isPressed)
        {

        }
    }

    private void OnDodge(InputValue value) => OnDodge(value.isPressed);
    public void OnDodge(bool isPressed)
    {
        if (isPressed)
        {
            Debug.Log("stamina: " + statusManager.ReadValue(Status.Stamina));
            if (statusManager.ReadValue(Status.Stamina) < _dodgeStaminaLostAmount)
            {
                // スタミナ不足
                return;
            }
            if (IsMatchingState(States.dodge))
            {
                // すでに回避中
                return;
            }

            if (_stateMachine.ChangeState(Triggers.dodgeInput))
            {
                statusManager.AddValue(Status.Stamina, -_dodgeStaminaLostAmount);
                PlaySE(_DodgeSE.SEName, _DodgeSE.Volume);
            }
        }
    }

    private void OnDash(InputValue value) => OnDash(value.isPressed);
    public void OnDash(bool isPressed)
    {
        if (isPressed)
        {
            _recoveryStatus.SetLock(Status.Stamina, true);
            _inputDash = true;
            _stateMachine.ChangeState(Triggers.dashInput);
        }
        else
        {
            if (IsMatchingState(States.drivedash))
            {
                _recoveryStatus.SetLock(Status.Stamina, false);
            }
            _inputDash = false;
            _stateMachine.ChangeState(Triggers.dashCancel);
        }
    }

    private void OnJump(InputValue value) => OnJump(value.isPressed);
    public void OnJump(bool isPressed)
    {

        if (isPressed)
        {
            if (IsMatchingState(States.jumpfallAim))
            {
                _stateMachine.ChangeState(Triggers.jumpInputNext);
                _activeFallAimEffect.Stop();
                _activeFallAimEffect = null;
                return;
            }

            if (IsMatchingState(States.idle) || IsMatchingState(States.move))
            {

                if (statusManager.ReadValue(Status.Stamina) < _jumpStaminaLostAmount)
                {
                    // スタミナ不足
                    return;
                }
                
                if (_stateMachine.ChangeState(Triggers.jumpInput))
                {
                    _recoveryStatus.SetLock(Status.Stamina, true);
                    PlaySE(_JumpSE.SEName, _JumpSE.Volume);
                    statusManager.AddValue(Status.Stamina, -_jumpStaminaLostAmount);

                    if (EffectManager.instance != null)
                    _activeJumpEffect =EffectManager.instance.Play("JumpBooster",
                                                _jumpBoosterEffectPoint.transform.position,
                                                _jumpBoosterEffectPoint.transform,
                                                this.transform);
                }
            }
        }

    }

    private void OnSkillDisplay(InputValue value) => OnSkillDisplay(value.isPressed);
    public void OnSkillDisplay(bool isPressed)
    {
        if (isPressed)
        {
            TryOpenEnhanceUI();
        }
    }
}
