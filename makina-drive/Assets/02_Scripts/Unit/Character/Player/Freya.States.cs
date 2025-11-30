using UnityEngine;
using UnityEngine.InputSystem;


public partial class Freya
{
    [SerializeField] private ShootForward N_attack1;
    private enum States
    {
        none,

        // 登場
        entry,

        // 待機
        idle,

        // 移動
        move,
        dodge,

        // 通常攻撃
        N_attack1,
        N_attack2,
        N_attack3,

        // 強攻撃
        S_attack,
        S_chargeAttack,

        // ドライブダッシュ
        dlivedash,

        // ダッシュ通常攻撃
        N_dashAttack,

        // ダッシュ強攻撃
        S_dashAttack,

        // ジャンプ
        jumpstart,
        jumpfallAim,
        fallAttack,

        // スタン
        stan,
        dead
    }

    private enum Triggers
    {
        none,
        entry,
        moveInput,
        moveCancel,
        dashInput,
        dashCancel,
        jumpInput,
        jumpCancel,
        jumpAir,
        jumpConplete,
        attackInput,
        attackConplete,
        stan,
        died
    }

    // ステート登録
    protected override void RegisterStats()
    {
        // トランスミッショングループを作成
        var idleTrigger = new[]
        {
            (Triggers.moveInput, States.move, ""),
            (Triggers.attackInput, States.N_attack1, ""),
            (Triggers.died, States.dead, "")
        };
        var moveTrigger = new[]
        {
            (Triggers.moveCancel, States.idle,""),
            (Triggers.attackInput, States.N_attack1,""),
            (Triggers.died, States.dead,"")
        };
        var n_attack1Trigger = new[]
        {
            (Triggers.attackConplete, States.idle,""),
            (Triggers.attackInput, States.N_attack1,""),
            (Triggers.died, States.dead,"")
        };

        // ステートマシンにStatesの移動先の追加
        _stateMachine
            .AddTransition(States.idle, idleTrigger)
            .AddTransition(States.move, moveTrigger)
            .AddTransition(States.N_attack1, n_attack1Trigger);

        /* 待機 */
        var idle = new Idle();
        _stateMachine.AddState(States.idle, idle);

        /* 移動 */
        var move = new MoveFree(true);
        move.SetAccel(_accel);
        move.SetDecel(_decel);
        _stateMachine.AddState(States.move, move);

        /* 通常攻撃1 */
        N_attack1 = new ShootForward(bulletData, AttackLayer);
        N_attack1.SetGameObject(_muzzle);
        N_attack1.onShootComplete.AddListener(() =>
        {
            _stateMachine.ChangeState(Triggers.attackConplete);
        });
        _stateMachine.AddState(States.N_attack1, N_attack1);
    }
}
