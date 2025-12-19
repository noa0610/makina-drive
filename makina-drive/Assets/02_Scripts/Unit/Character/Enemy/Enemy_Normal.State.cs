using UnityEngine;

public partial class Enemy_Normal
{
    private MoveFree chase;
    private Stun stun;
    private enum States
    {
        none,
        idle,
        chase,
        attack,
        stan,
        dead
    }
    private enum Triggers
    {
        none,
        toIdle,
        toChase,
        toAttack,
        toStan,
        died
    }
    protected override void RegisterStats()
    {
        // トランスミッショングループを作成
        var idleTrigger = new[]
        {
            (Triggers.toChase, States.chase, ""),
            (Triggers.toStan, States.stan, ""),
            (Triggers.died, States.dead, "")
        };
        var chaseTrigger = new[]
        {
            (Triggers.toIdle, States.idle, ""),
            (Triggers.toAttack, States.attack, ""),
            (Triggers.toStan, States.stan, ""),
            (Triggers.died, States.dead, "")
        };
        var attackTrigger = new[]
        {
            (Triggers.toChase, States.chase, ""),
            (Triggers.toStan, States.stan, ""),
            (Triggers.died, States.dead, "")
        };
        var stanTrigger = new[]
        {
            (Triggers.toIdle, States.idle, ""),
            (Triggers.died, States.dead, "")
        };

        // ステートマシンにStatesの移動先の追加
        _stateMachine
            .AddTransition(States.idle, idleTrigger)
            .AddTransition(States.chase, chaseTrigger)
            .AddTransition(States.attack, attackTrigger)
            .AddTransition(States.stan, stanTrigger);
        
        /* 待機 */
        _stateMachine.AddState(States.idle, new Idle());

        /* 追跡 */
        chase = new MoveFree(true);
        chase.SetRB2(rb);
        chase.SetAccel(_accel);
        chase.SetDecel(_decel);
        _stateMachine.AddState(States.chase, chase);
        
        /* 攻撃 */
        var attack = new ShootCombo(_attackBulletData, AttackLayer, Triggers.toChase.ToString());
        attack.SetTime(1f, 1f, _stateChangeTime, _attackTime);
        attack.SetGameObject(_muzzle);
        attack.SetCreatMisalignment(_attackCreatePos);
        _stateMachine.AddState(States.attack, attack);

        /* スタン */
        stun = new Stun(rb, Triggers.toIdle.ToString(), _stanTime, false);
        _stateMachine.AddState(States.stan, stun);

        /* 死亡 */
        var dead = new Idle();
        _stateMachine.AddState(States.dead, dead);
    }
}