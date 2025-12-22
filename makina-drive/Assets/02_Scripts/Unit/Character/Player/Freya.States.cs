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
        jumpInputNext,
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
            (Triggers.attackInput, States.N1_attack, "AttackInput"),
            (Triggers.dashInput, States.drivedash, ""),
            (Triggers.dodgeInput, States.dodge, "DodgeInput"),
            (Triggers.jumpInput, States.jumpstart, "JumpInput"),
            (Triggers.died, States.dead, "Dide")
            ,(Triggers.TestShoot, States.Shoot,"")
        };
        var moveTrigger = new[]
        {
            (Triggers.moveCancel, States.idle,""),
            (Triggers.attackInput, States.N1_attack,"AttackInput"),
            (Triggers.dashInput, States.drivedash, ""),
            (Triggers.dodgeInput, States.dodge, "DodgeInput"),
            (Triggers.jumpInput, States.jumpstart,"JumpInput"),
            (Triggers.died, States.dead,"Dide")
            ,(Triggers.TestShoot, States.Shoot,"")
        };
        var dodgeTrigger = new[]
        {
            (Triggers.dodgeCancel, States.idle,"DodgeEnd"),
            (Triggers.moveInput, States.move,"DodgeEnd"),
            (Triggers.attackInput, States.N1_attack,"AttackInput"),
            (Triggers.dashInput, States.drivedash,"DodgeEnd"),
            (Triggers.died, States.dead,"Dide")
        };
        var drivedashTrigger = new[]
        {
            (Triggers.dashCancel, States.idle,""),
            (Triggers.attackInput, States.dashN1_Attack,"AttackInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var n1_attackTrigger = new[]
        {
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.N2_attack,"AttackInput"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var n2_attackTrigger = new[]
        {
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.N3_attack,"AttackInput"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var n3_attackTrigger = new[]
        {
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var dashn1_attackTrigger = new[]
        {
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.dashN2_Attack,"AttackInput"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var dashn2_attackTrigger = new[]
        {
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.attackInput, States.dashN3_Attack,"AttackInput"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
        var dashn3_attackTrigger = new[]
        {
            (Triggers.attackConplete, States.idle,"AttackEnd"),
            (Triggers.dodgeInput, States.dodge,"DodgeInput"),
            (Triggers.died, States.dead,"Dide")
        };
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
        var ShootTrigger = new[]
        {
            (Triggers.ShootEnd, States.idle,""),
            (Triggers.died, States.dead,"Dide")
        };

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
            .AddTransition(States.dashN3_Attack, dashn3_attackTrigger)
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

        #region   ===== N_Attack State =====

        /* 通常攻撃1 */
        N_attack1 = new ShootCombo(N1_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        N_attack1.SetTime(N1_inputReceptionTime, N1_stateChangeTime, N1_inputEndTime, N1_attackStartTime);
        N_attack1.SetGameObject(_muzzle);
        N_attack1.SetCreatMisalignment(_createPos);
        N_attack1.SetRB2(rb);
        N_attack1.SetAccel(_attackAccel);
        // N_attack1.onShootComplete.AddListener(() =>
        // {
            
        // });
        _stateMachine.AddState(States.N1_attack, N_attack1);

        /* 通常攻撃2 */
        N_attack2 = new ShootCombo(N2_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        N_attack2.SetTime(N2_inputReceptionTime, N2_stateChangeTime, N2_inputEndTime, N2_attackStartTime);
        N_attack2.SetGameObject(_muzzle);
        N_attack2.SetCreatMisalignment(_createPos);
        N_attack2.SetRB2(rb);
        N_attack2.SetAccel(_attackAccel);
        _stateMachine.AddState(States.N2_attack, N_attack2);

        /* 通常攻撃3 */
        N_attack3 = new ShootCombo(N3_bulletData, AttackLayer, Triggers.attackConplete.ToString(), "");
        N_attack3.SetTime(N3_inputReceptionTime, N3_stateChangeTime, N3_inputEndTime, N3_attackStartTime);
        N_attack3.SetGameObject(_muzzle);
        N_attack3.SetCreatMisalignment(_createPos);
        N_attack3.SetRB2(rb);
        N_attack3.SetAccel(_attackAccel);
        _stateMachine.AddState(States.N3_attack, N_attack3);
        #endregion

        #region   ===== DashN_Attack State =====

        /* ダッシュ通常攻撃1 */
        DashN1_attack = new ShootCombo(DashN1_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        DashN1_attack.SetTime(DashN1_inputReceptionTime, DashN1_stateChangeTime, DashN1_inputEndTime, DashN1_attackStartTime);
        DashN1_attack.SetGameObject(_muzzle);
        DashN1_attack.SetCreatMisalignment(_createPos);
        DashN1_attack.SetRB2(rb);
        DashN1_attack.SetAccel(_attackAccel * 1.5f);
        _stateMachine.AddState(States.dashN1_Attack, DashN1_attack);

        /* ダッシュ通常攻撃2 */
        DashN2_attack = new ShootCombo(DashN2_bulletData, AttackLayer, Triggers.attackConplete.ToString(), Triggers.attackInput.ToString());
        DashN2_attack.SetTime(DashN2_inputReceptionTime, DashN2_stateChangeTime, DashN2_inputEndTime, DashN2_attackStartTime);
        DashN2_attack.SetGameObject(_muzzle);
        DashN2_attack.SetCreatMisalignment(_createPos);
        DashN2_attack.SetRB2(rb);
        DashN2_attack.SetAccel(_attackAccel * 1.5f);
        _stateMachine.AddState(States.dashN2_Attack, DashN2_attack);

        /* ダッシュ通常攻撃3 */
        DashN3_attack = new ShootCombo(DashN3_bulletData, AttackLayer, Triggers.attackConplete.ToString(), "");
        DashN3_attack.SetTime(DashN3_inputReceptionTime, DashN3_stateChangeTime, DashN3_inputEndTime, DashN3_attackStartTime);
        DashN3_attack.SetGameObject(_muzzle);
        DashN3_attack.SetCreatMisalignment(_createPos);
        DashN3_attack.SetRB2(rb);
        DashN3_attack.SetAccel(_attackAccel * 1.5f);
        _stateMachine.AddState(States.dashN3_Attack, DashN3_attack);
        #endregion


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
            IsRecovery = true;
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
