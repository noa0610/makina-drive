using UnityEngine;
using UnityEngine.InputSystem;


public partial class Freya
{
    private ShootCombo N_attack1;
    private ShootCombo N_attack2;
    private ShootCombo N_attack3;
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
        N_attack1, N_attack2, N_attack3,

        // 強攻撃
        S_attack, S_chargeAttack,

        // ドライブダッシュ
        drivedash,

        // ダッシュ通常攻撃
        N_dashAttack,

        // ダッシュ強攻撃
        S_dashAttack,

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
        ,Shoot
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
        jumpAir,
        jumpConplete,
        jumpCancel,
        attackInput,
        attackConplete,
        stan,
        died
        ,TestShoot
        ,ShootEnd
    }

    // ステート登録
    protected override void RegisterStats()
    {
        // トランスミッショングループを作成
        var idleTrigger = new[]
        {
            (Triggers.moveInput, States.move, ""),
            (Triggers.attackInput, States.N_attack1, "AttackInput"),
            (Triggers.dashInput, States.drivedash, ""),
            (Triggers.dodgeInput, States.dodge, "DodgeInput"),
            (Triggers.jumpInput, States.jumpstart, "JumpInput"),
            (Triggers.died, States.dead, "")
            ,(Triggers.TestShoot, States.Shoot,"")
        };
        var moveTrigger = new[]
        {
            (Triggers.moveCancel, States.idle,""),
            (Triggers.attackInput, States.N_attack1,"AttackInput"),
            (Triggers.dashInput, States.drivedash, ""),
            (Triggers.dodgeInput, States.dodge, "DodgeInput"),
            (Triggers.jumpInput, States.jumpstart,"JumpInput"),
            (Triggers.died, States.dead,"")
            ,(Triggers.TestShoot, States.Shoot,"")
        };
        var dodgeTrigger = new[]
        {
            (Triggers.dodgeCancel, States.idle,"DodgeEnd"),
            (Triggers.moveInput, States.move,"DodgeEnd"),
            (Triggers.attackInput, States.N_attack1,"AttackInput"),
            (Triggers.dashInput, States.drivedash,"DodgeEnd"),
            (Triggers.died, States.dead,"")
        };
        var drivedashTrigger = new[]
        {
            (Triggers.dashCancel, States.idle,""),
            (Triggers.attackInput, States.N_attack1,"AttackInput"),
            (Triggers.died, States.dead,"")
        };
        var n_attack1Trigger = new[]
        {
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.N_attack2,"AttackInput"),
            (Triggers.died, States.dead,"")
        };
        var n_attack2Trigger = new[]
        {
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.N_attack3,"AttackInput"),
            (Triggers.died, States.dead,"")
        };
        var n_attack3Trigger = new[]
        {
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.died, States.dead,"")
        };
        var jumpstartTrigger = new[]
        {
            (Triggers.jumpAir, States.jumpfallAim,""),
            (Triggers.died, States.dead,"")
        };
        var jumpfallAimTrigger = new[]
        {
            (Triggers.jumpInput, States.fall,"JumpInput"),
            (Triggers.died, States.dead,"")
        };
        var fallTrigger = new[]
        {
            (Triggers.jumpConplete, States.fallAttack,""),
            (Triggers.died, States.dead,"")
        };
        var fallAttackTrigger = new[]
        {
            (Triggers.jumpCancel, States.idle,"JumpEnd"),
            (Triggers.died, States.dead,"")
        };
        var ShootTrigger = new[]
        {
            (Triggers.ShootEnd, States.idle,""),
            (Triggers.died, States.dead,"")
        };

        // ステートマシンにStatesの移動先の追加
        _stateMachine
            .AddTransition(States.idle, idleTrigger)
            .AddTransition(States.move, moveTrigger)
            .AddTransition(States.dodge, dodgeTrigger)
            .AddTransition(States.drivedash, drivedashTrigger)
            .AddTransition(States.N_attack1, n_attack1Trigger)
            .AddTransition(States.N_attack2, n_attack2Trigger)
            .AddTransition(States.N_attack3, n_attack3Trigger)
            .AddTransition(States.jumpstart, jumpstartTrigger)
            .AddTransition(States.jumpfallAim, jumpfallAimTrigger)
            .AddTransition(States.fall, fallTrigger)
            .AddTransition(States.fallAttack, fallAttackTrigger)
            .AddTransition(States.Shoot, ShootTrigger);

        /* 待機 */
        var idle = new Idle();
        _stateMachine.AddState(States.idle, idle);

        /* 移動 */
        var move = new MoveFree(true);
        move.SetAccel(_accel);
        move.SetDecel(_decel);
        _stateMachine.AddState(States.move, move);

        /* 回避 */
        var dodge = new MoveInvincible(_invincibleTime, _dodgeRecoveryTime, false, Triggers.dodgeCancel.ToString());
        dodge.SetAccel(_dodgeAccel);
        dodge.SetDecel(_dodgeDecel);
        _stateMachine.AddState(States.dodge, dodge);

        /* ドライブダッシュ */
        drivedash = new DashAttack(Dash_bulletData, AttackLayer, true);
        drivedash.SetGameObject(_muzzle);
        drivedash.SetRB2(rb);
        drivedash.SetCreatMisalignment(0);
        _stateMachine.AddState(States.drivedash, drivedash);

        /* 通常攻撃1 */
        N_attack1 = new ShootCombo(N1_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        N_attack1.SetTime(N1_inputReceptionTime, N1_stateChangeTime, N1_inputEndTime, N1_attackStartTime);
        N_attack1.SetGameObject(_muzzle);
        N_attack1.SetCreatMisalignment(_createPos);
        // N_attack1.onShootComplete.AddListener(() =>
        // {
            
        // });
        _stateMachine.AddState(States.N_attack1, N_attack1);

        /* 通常攻撃2 */
        N_attack2 = new ShootCombo(N2_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        N_attack2.SetTime(N2_inputReceptionTime, N2_stateChangeTime, N2_inputEndTime, N2_attackStartTime);
        N_attack2.SetGameObject(_muzzle);
        N_attack2.SetCreatMisalignment(_createPos);
        _stateMachine.AddState(States.N_attack2, N_attack2);

        /* 通常攻撃3 */
        N_attack3 = new ShootCombo(N3_bulletData, AttackLayer, Triggers.attackConplete.ToString(), "");
        N_attack3.SetTime(N3_inputReceptionTime, N3_stateChangeTime, N3_inputEndTime, N3_attackStartTime);
        N_attack3.SetGameObject(_muzzle);
        N_attack3.SetCreatMisalignment(_createPos);
        _stateMachine.AddState(States.N_attack3, N_attack3);

        /* ジャンプ開始 */
        var jumpstart = new Idle_LazyChange(Triggers.jumpAir.ToString(), _jumpStartTime);
        jumpstart.OnCompleted += () =>
        {
            // 無敵付与
            SetInvincible(true);
        };
        _stateMachine.AddState(States.jumpstart, jumpstart);

        /* 落下狙い */
        var jumpfallAim = new MoveFree(true);
        jumpfallAim.SetAccel(_fallAimAccel);
        jumpfallAim.SetDecel(_fallAimDecel);
        _stateMachine.AddState(States.jumpfallAim, jumpfallAim);

        /* 落下 */
        var fall = new Idle_LazyChange(Triggers.jumpConplete.ToString(), _fallTime);
        fall.OnCompleted += () =>
        {
            // 無敵解除
            SetInvincible(false);
        };
        _stateMachine.AddState(States.fall, fall);

        /* 落下攻撃 */
        var fallAttack = new ShootForward_LazyChange(FallAttack_bulletData, AttackLayer, Triggers.jumpCancel.ToString(), _fallAttackTime);
        fallAttack.SetGameObject(_muzzle);
        _stateMachine.AddState(States.fallAttack, fallAttack);

        /* テスト用 */
        var shoot = new ShootForward(N1_bulletData, AttackLayer);
        shoot.SetGameObject(_muzzle);
        shoot.SetCreatMisalignment(_createPos);
        shoot.onShootComplete.AddListener(() =>
        {
            _stateMachine.ChangeState(Triggers.ShootEnd);
        });
        _stateMachine.AddState(States.Shoot, shoot);
    }
}
