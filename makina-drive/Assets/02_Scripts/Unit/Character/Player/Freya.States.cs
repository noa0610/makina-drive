using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public partial class Freya
{
    private ShootCombo N_attack1;
    private ShootCombo N_attack2;
    private ShootCombo N_attack3;
    private ShootCombo DashN1_attack;
    private ShootCombo DashN2_attack;
    private ShootCombo DashN3_attack;
    private ShootCombo_Extra Charge_Attack;
    private ShootCombo_Extra Charge_DashAttack;
    private DashAttack drivedash;
    private enum States
    {
        none,

        // 登場
        entry,

        // 待機
        idle,

        // 移動
        move,

        // 回避
        dodge,

        // 通常攻撃
        N1_attack, N2_attack, N3_attack,

        // 強攻撃
        S_attack, S_chargeAttack,

        // ドライブダッシュ
        drivedash,

        // ダッシュ通常攻撃
        dashN1_Attack, dashN2_Attack, dashN3_Attack,

        // ダッシュ強攻撃
        S_dashAttack,

        // チャージ攻撃
        charge_Attack, charge_DashAttack,

        // ジャンプ
        jumpstart,
        jumpfallAim,
        fall,
        fallAttack,

        // スタン
        stan,

        // 死亡
        dead

        // テスト用
        , Shoot
    }

    private enum Triggers
    {
        none,
        entry,
        dodgeInput,
        dodgeCancel,
        moveInput,
        moveCancel,
        dashInput,
        dashCancel,
        jumpInput,
        jumpInputNext,
        jumpAir,
        jumpConplete,
        jumpCancel,
        attackInput,
        attackConplete,
        chargeAttackInput,
        stan,
        died
    }

    // ステート登録
    protected override void RegisterStats()
    {
        // トランスミッショングループを作成
        var idleTrigger = new[]
        {
            (Triggers.moveInput, States.move, "MoveInput"),
            (Triggers.attackInput, States.N1_attack, "AttackInput"),
            (Triggers.chargeAttackInput, States.charge_Attack, "ChargeInput"),
            (Triggers.dashInput, States.drivedash, "DashInput"),
            (Triggers.dodgeInput, States.dodge, "DodgeInput"),
            (Triggers.jumpInput, States.jumpstart, "JumpInput"),
            (Triggers.died, States.dead, "Dide")
        };
        var moveTrigger = new[]
        {
            (Triggers.moveCancel, States.idle,"MoveEnd"),
            (Triggers.attackInput, States.N1_attack,"AttackInput"),
            (Triggers.chargeAttackInput, States.charge_Attack, "ChargeInput"),
            (Triggers.dashInput, States.drivedash, "DashInput"),
            (Triggers.dodgeInput, States.dodge, "DodgeInput"),
            (Triggers.jumpInput, States.jumpstart,"JumpInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var dodgeTrigger = new[]
        {
            (Triggers.dodgeCancel, States.idle,"DodgeEnd"),
            (Triggers.moveInput, States.move,"MoveInput"),
            (Triggers.attackInput, States.N1_attack,"AttackInput"),
            (Triggers.chargeAttackInput, States.charge_Attack, "ChargeInput"),
            (Triggers.dashInput, States.drivedash,"DashInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var drivedashTrigger = new[]
        {
            (Triggers.dashCancel, States.idle,"DashEnd"),
            (Triggers.attackInput, States.dashN1_Attack,"AttackInput"),
            (Triggers.chargeAttackInput, States.charge_DashAttack, "ChargeInput"),
            (Triggers.died, States.dead,"Dide")
        };

        #region   ===== N_Attack Triggers =====
        var n1_attackTrigger = new[]
        {
            (Triggers.moveInput, States.move, "MoveInput"),
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.N2_attack,"AttackInput"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var n2_attackTrigger = new[]
        {
            (Triggers.moveInput, States.move, "MoveInput"),
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.N3_attack,"AttackInput"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var n3_attackTrigger = new[]
        {
            (Triggers.moveInput, States.move, "MoveInput"),
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        #endregion

        #region   ===== DashN_Attack Triggers =====
        var dashn1_attackTrigger = new[]
        {
            (Triggers.moveInput, States.move, "MoveInput"),
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.dashN2_Attack,"AttackInput"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var dashn2_attackTrigger = new[]
        {
            (Triggers.moveInput, States.move, "MoveInput"),
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.dashN3_Attack,"AttackInput"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var dashn3_attackTrigger = new[]
        {
            (Triggers.moveInput, States.move, "MoveInput"),
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        #endregion

        #region   ===== ChargeAttack Triggers =====
        var chargeAttackTrigger = new[]
        {
            (Triggers.moveInput, States.move, "MoveInput"),
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var chargeDashAttackTrigger = new[]
        {
            (Triggers.moveInput, States.move, "MoveInput"),
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        #endregion

        #region   ===== Jump Triggers =====
        var jumpstartTrigger = new[]
        {
            (Triggers.jumpAir, States.jumpfallAim,""),
            (Triggers.died, States.dead,"Dide")
        };
        var jumpfallAimTrigger = new[]
        {
            (Triggers.jumpInputNext, States.fall,"JumpInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var fallTrigger = new[]
        {
            (Triggers.jumpConplete, States.fallAttack,""),
            (Triggers.died, States.dead,"Dide")
        };
        var fallAttackTrigger = new[]
        {
            (Triggers.jumpCancel, States.idle,"JumpEnd"),
            (Triggers.died, States.dead,"Dide")
        };
        #endregion

        // ステートマシンにStatesの移動先の追加
        _stateMachine
            .AddTransition(States.idle, idleTrigger)
            .AddTransition(States.move, moveTrigger)
            .AddTransition(States.dodge, dodgeTrigger)
            .AddTransition(States.drivedash, drivedashTrigger)
            .AddTransition(States.N1_attack, n1_attackTrigger)
            .AddTransition(States.N2_attack, n2_attackTrigger)
            .AddTransition(States.N3_attack, n3_attackTrigger)
            .AddTransition(States.dashN1_Attack, dashn1_attackTrigger)
            .AddTransition(States.dashN2_Attack, dashn2_attackTrigger)
            .AddTransition(States.charge_Attack, chargeAttackTrigger)
            .AddTransition(States.charge_DashAttack, chargeDashAttackTrigger)
            .AddTransition(States.dashN3_Attack, dashn3_attackTrigger)
            .AddTransition(States.jumpstart, jumpstartTrigger)
            .AddTransition(States.jumpfallAim, jumpfallAimTrigger)
            .AddTransition(States.fall, fallTrigger)
            .AddTransition(States.fallAttack, fallAttackTrigger);

        /* 待機 */
        var idle = new Idle();
        _stateMachine.AddState(States.idle, idle, new string[] { "CanDodge", "CanJump" });

        /* 移動 */
        var move = new MoveFree(true);
        move.SetAccel(_accel);
        move.SetDecel(_decel);
        _stateMachine.AddState(States.move, move, new string[] { "MV", "CanDodge", "CanJump" });

        /* 回避 */
        var dodge = new MoveInvincible(_invincibleTime, _dodgeRecoveryTime, false, Triggers.dodgeCancel.ToString());
        dodge.SetAccel(_dodgeAccel);
        dodge.SetDecel(_dodgeDecel);
        dodge.OnCompleted += RecheckMoveInput;
        _stateMachine.AddState(States.dodge, dodge, new string[] { "DG" , TAG_NO_STAMINA_RECOVERY});

        /* ドライブダッシュ */
        drivedash = new DashAttack(Dash_bulletData, AttackLayer, true, Triggers.dashCancel.ToString());
        drivedash.SetGameObject(_muzzle);
        drivedash.SetRB2(Rigidbody2D);
        drivedash.SetCreatMisalignment(0);
        drivedash.SetBlockThoroughTag(_dashAttackTag);
        drivedash.OnBeGinning += () =>
        {
            if (CameraDirector.instance != null) CameraDirector.instance.PlayFreezeEffect(0.15f).Forget(); ;
            PlaySE(_DashSE.SEName, _DashSE.Volume);
        };
        drivedash.OnCompleted += RecheckMoveInput;
        _stateMachine.AddState(States.drivedash, drivedash, new string[] { "DD" , TAG_NO_STAMINA_RECOVERY});

        #region   ===== N_Attack State =====

        /* 通常攻撃1 */
        N_attack1 = new ShootCombo(N1_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        N_attack1.SetTime(N1_inputReceptionTime, N1_stateChangeTime, N1_inputEndTime, N1_attackStartTime);
        N_attack1.SetGameObject(_muzzle);
        N_attack1.SetCreatMisalignment(_createPos);
        N_attack1.SetRB2(Rigidbody2D);
        N_attack1.SetAccel(_attackAccel);
        N_attack1.onShootComplete.AddListener(() =>
        {
            PlaySE(_N1_AttackSE.SEName, _N1_AttackSE.Volume);
        });
        N_attack1.OnCompleted += RecheckMoveInput;
        N_attack1.OnTransitionOpened += RecheckMoveInput;
        _stateMachine.AddState(States.N1_attack, N_attack1, new string[] { "NA", "NA1", "CanDodge" });

        /* 通常攻撃2 */
        N_attack2 = new ShootCombo(N2_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        N_attack2.SetTime(N2_inputReceptionTime, N2_stateChangeTime, N2_inputEndTime, N2_attackStartTime);
        N_attack2.SetGameObject(_muzzle);
        N_attack2.SetCreatMisalignment(_createPos);
        N_attack2.SetRB2(Rigidbody2D);
        N_attack2.SetAccel(_attackAccel);
        N_attack2.onShootComplete.AddListener(() =>
        {
            PlaySE(_N2_AttackSE.SEName, _N2_AttackSE.Volume);
        });
        N_attack2.OnCompleted += RecheckMoveInput;
        N_attack2.OnTransitionOpened += RecheckMoveInput;
        _stateMachine.AddState(States.N2_attack, N_attack2, new string[] { "NA", "NA2", "CanDodge" });

        /* 通常攻撃3 */
        N_attack3 = new ShootCombo(N3_bulletData, AttackLayer, Triggers.attackConplete.ToString(), "");
        N_attack3.SetTime(N3_inputReceptionTime, N3_stateChangeTime, N3_inputEndTime, N3_attackStartTime);
        N_attack3.SetGameObject(_muzzle);
        N_attack3.SetCreatMisalignment(_createPos);
        N_attack3.SetRB2(Rigidbody2D);
        N_attack3.SetAccel(_attackAccel);
        N_attack3.onShootComplete.AddListener(() =>
        {
            PlaySE(_N3_AttackSE.SEName, _N3_AttackSE.Volume);
        });
        N_attack3.OnCompleted += RecheckMoveInput;
        N_attack3.OnTransitionOpened += RecheckMoveInput;
        _stateMachine.AddState(States.N3_attack, N_attack3, new string[] { "NA", "NA3", "CanDodge" });
        #endregion

        #region   ===== DashN_Attack State =====

        /* ダッシュ通常攻撃1 */
        DashN1_attack = new ShootCombo(DashN1_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        DashN1_attack.SetTime(DashN1_inputReceptionTime, DashN1_stateChangeTime, DashN1_inputEndTime, DashN1_attackStartTime);
        DashN1_attack.SetGameObject(_muzzle);
        DashN1_attack.SetCreatMisalignment(_createPos);
        DashN1_attack.SetRB2(Rigidbody2D);
        DashN1_attack.SetAccel(_attackAccel * 1.5f);
        DashN1_attack.onShootComplete.AddListener(() =>
        {
            PlaySE(_N1_AttackSE.SEName, _N1_AttackSE.Volume);
        });
        DashN1_attack.OnCompleted += RecheckMoveInput;
        DashN1_attack.OnTransitionOpened += RecheckMoveInput;
        _stateMachine.AddState(States.dashN1_Attack, DashN1_attack, new string[] { _dashAttackTag, "DA1", "CanDodge" });

        /* ダッシュ通常攻撃2 */
        DashN2_attack = new ShootCombo(DashN2_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        DashN2_attack.SetTime(DashN2_inputReceptionTime, DashN2_stateChangeTime, DashN2_inputEndTime, DashN2_attackStartTime);
        DashN2_attack.SetGameObject(_muzzle);
        DashN2_attack.SetCreatMisalignment(_createPos);
        DashN2_attack.SetRB2(Rigidbody2D);
        DashN2_attack.SetAccel(_attackAccel * 1.5f);
        DashN2_attack.onShootComplete.AddListener(() =>
        {
            PlaySE(_N2_AttackSE.SEName, _N2_AttackSE.Volume);
        });
        DashN2_attack.OnCompleted += RecheckMoveInput;
        DashN2_attack.OnTransitionOpened += RecheckMoveInput;
        _stateMachine.AddState(States.dashN2_Attack, DashN2_attack, new string[] { _dashAttackTag, "DA2", "CanDodge" });

        /* ダッシュ通常攻撃3 */
        DashN3_attack = new ShootCombo(DashN3_bulletData, AttackLayer, Triggers.attackConplete.ToString(), "");
        DashN3_attack.SetTime(DashN3_inputReceptionTime, DashN3_stateChangeTime, DashN3_inputEndTime, DashN3_attackStartTime);
        DashN3_attack.SetGameObject(_muzzle);
        DashN3_attack.SetCreatMisalignment(_createPos);
        DashN3_attack.SetRB2(Rigidbody2D);
        DashN3_attack.SetAccel(_attackAccel * 1.5f);
        DashN3_attack.onShootComplete.AddListener(() =>
        {
            PlaySE(_N3_AttackSE.SEName, _N3_AttackSE.Volume);
        });
        DashN3_attack.OnCompleted += RecheckMoveInput;
        DashN3_attack.OnTransitionOpened += RecheckMoveInput;
        _stateMachine.AddState(States.dashN3_Attack, DashN3_attack, new string[] { _dashAttackTag, "DA3", "CanDodge" });
        #endregion

        #region   ===== Charge_Attack State =====
        /* チャージ攻撃 */
        Charge_Attack = new ShootCombo_Extra(_Charge_bulletData, _Charge_Extra_bulletData, AttackLayer, Triggers.attackConplete.ToString(), "");
        Charge_Attack.SetTime(_Charge_inputReceptionTime, _Charge_inputEndTime, _Charge_stateChangeTime, _Charge_attackStartTime);
        Charge_Attack.SetGameObject(_muzzle);
        Charge_Attack.SetCreatMisalignment(_createPos);
        Charge_Attack.SetRB2(Rigidbody2D);
        Charge_Attack.SetAccel(_attackAccel);
        Charge_Attack.IsStopInExit = true;
        Charge_Attack.onShootComplete.AddListener(() =>
        {
            if (CameraDirector.instance != null) CameraDirector.instance.PlayShake();
            PlaySE(_Charge_AttackSE.SEName, _Charge_AttackSE.Volume);
        });
        Charge_Attack.OnCompleted += RecheckMoveInput;
        _stateMachine.AddState(States.charge_Attack, Charge_Attack, new string[] { "CA", "CanDodge" });

        /* チャージダッシュ攻撃 */
        Charge_DashAttack = new ShootCombo_Extra(_ChargeDash_bulletData, _ChargeDash_Extra_bulletData, AttackLayer, Triggers.attackConplete.ToString(), "");
        Charge_DashAttack.SetTime(_ChargeDash_inputReceptionTime, _ChargeDash_inputEndTime, _ChargeDash_stateChangeTime, _ChargeDash_attackStartTime);
        Charge_DashAttack.SetGameObject(_muzzle);
        Charge_DashAttack.SetCreatMisalignment(_createPos);
        Charge_DashAttack.SetRB2(Rigidbody2D);
        Charge_DashAttack.SetAccel(_attackAccel * 1.5f);
        Charge_DashAttack.IsStopInExit = true;
        Charge_DashAttack.onShootComplete.AddListener(() =>
        {
            if (CameraDirector.instance != null) CameraDirector.instance.PlayShake();
            PlaySE(_Charge_DashAttackSE.SEName, _Charge_DashAttackSE.Volume);
        });
        Charge_DashAttack.OnCompleted += RecheckMoveInput;
        _stateMachine.AddState(States.charge_DashAttack, Charge_DashAttack, new string[] { "CA", "CDA", "CanDodge" });
        #endregion

        #region   ===== Jump State =====

        /* ジャンプ開始 */
        var jumpstart = new Idle_LazyChange(Triggers.jumpAir.ToString(), _jumpStartTime);
        jumpstart.OnCompleted += () =>
        {
            // 発動中すり抜け
            if (_coll2D != null)
                _coll2D.isTrigger = true;
            // 無敵付与
            SetInvincible(true);

            if (EffectManager.instance != null) _activeFallAimEffect = EffectManager.instance.Play("AimCursor", transform.position, transform);
        };
        _stateMachine.AddState(States.jumpstart, jumpstart, TAG_NO_STAMINA_RECOVERY);

        /* 落下狙い */
        var jumpfallAim = new MoveFree(true);
        jumpfallAim.SetAccel(_fallAimAccel);
        jumpfallAim.SetDecel(_fallAimDecel);
        jumpfallAim.SetLazyChange(Triggers.jumpInputNext.ToString(), _fallAutoChangeTIme);
        jumpfallAim.OnCompleted += () =>
        {
            _activeFallAimEffect.Stop();
            _activeFallAimEffect = null;
        };
        _stateMachine.AddState(States.jumpfallAim, jumpfallAim, TAG_NO_STAMINA_RECOVERY);

        /* 落下 */
        var fall = new Idle_LazyChange(Triggers.jumpConplete.ToString(), _fallTime);
        fall.OnCompleted += () =>
        {
            if (_activeJumpEffect != null)
            {
                _activeJumpEffect.Stop();
                _activeJumpEffect = null;
            }

            if (EffectManager.instance != null) _activeFallAttackBoosterEffect = EffectManager.instance.Play("FallAttackBooster", _FallAttackBoosterEffectPoint.transform.position, _FallAttackBoosterEffectPoint.transform, this.transform);

            // すり抜け解除
            if (_coll2D != null)
                _coll2D.isTrigger = false;
            // 無敵解除
            SetInvincible(false);
        };
        _stateMachine.AddState(States.fall, fall, TAG_NO_STAMINA_RECOVERY);

        /* 落下攻撃 */
        var fallAttack = new ShootForward_LazyChange(FallAttack_bulletData, AttackLayer, Triggers.jumpCancel.ToString(), _fallAttackTime);
        fallAttack.SetGameObject(_muzzle);
        fallAttack.onShootComplete.AddListener(() =>
        {
            if (CameraDirector.instance != null) CameraDirector.instance.PlayZoom();
            PlaySE(_JumpAttackSE.SEName, _JumpAttackSE.Volume);
        });
        fallAttack.OnCompleted += () =>
        {
            if (_activeFallAttackBoosterEffect != null)
            {
                _activeFallAttackBoosterEffect.Stop();
                _activeFallAttackBoosterEffect = null;
            }
        };
        fallAttack.OnCompleted += RecheckMoveInput;
        _stateMachine.AddState(States.fallAttack, fallAttack, new string[] { "SPA" });
        #endregion

        /* 死亡 */
        var dead = new Idle_MoveStop(Rigidbody2D);
        _stateMachine.AddState(States.dead, dead);
    }
}
