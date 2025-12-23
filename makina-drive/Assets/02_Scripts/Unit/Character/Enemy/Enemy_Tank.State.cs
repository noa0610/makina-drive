using UnityEngine;

public partial class Enemy_Tank
{
    
    private MoveFree chase;
    private Stun stun;
    private Blowback blowback;
    private enum States
    {
        none,
        idle,
        chase,
        tackle,
        stan,
        blowback,
        dead
    }
    private enum Triggers
    {
        none,
        toIdle,
        toChase,
        toAttack,
        toStan,
        toBlowback,
        died
    }
    protected override void RegisterStats()
    {
        // トランスミッショングループを作成
        var idleTrigger = new[]
        {
            (Triggers.toChase, States.chase, ""),
            (Triggers.toStan, States.stan, ""),
            (Triggers.toBlowback, States.blowback, ""),
            (Triggers.died, States.dead, "")
        };
        var chaseTrigger = new[]
        {
            (Triggers.toIdle, States.idle, ""),
            (Triggers.toAttack, States.tackle, ""),
            (Triggers.toStan, States.stan, ""),
            (Triggers.toBlowback, States.blowback, ""),
            (Triggers.died, States.dead, "")
        };
        var attackTrigger = new[]
        {
            (Triggers.toChase, States.chase, ""),
            (Triggers.toStan, States.stan, ""),
            (Triggers.toBlowback, States.blowback, ""),
            (Triggers.died, States.dead, "")
        };
        var blowbackTrigger = new[]
        {
            (Triggers.toChase, States.chase, ""),
            (Triggers.died, States.dead, "")
        };
        var stanTrigger = new[]
        {
            (Triggers.toIdle, States.idle, ""),
            (Triggers.toBlowback, States.blowback, ""),
            (Triggers.died, States.dead, "")
        };

        // ステートマシンにStatesの移動先の追加
        _stateMachine
            .AddTransition(States.idle, idleTrigger)
            .AddTransition(States.chase, chaseTrigger)
            .AddTransition(States.tackle, attackTrigger)
            .AddTransition(States.stan, stanTrigger)
            .AddTransition(States.blowback, blowbackTrigger);
            
        
        /* 待機 */
        _stateMachine.AddState(States.idle, new Idle());

        /* 追跡 */
        chase = new MoveFree(true);
        chase.SetRB2(Rigidbody2D);
        chase.SetAccel(_accel);
        chase.SetDecel(_decel);
        _stateMachine.AddState(States.chase, chase);
        
        // TODO 突進攻撃（DashAttack）に変更する
        /* タックル */
        var tackle = new DashAttack(_attackBulletData, AttackLayer, true, Triggers.toChase.ToString());
        tackle.SetStateDashTime(_stateChangeTime);
        tackle.SetGameObject(_muzzle);
        tackle.SetRB2(Rigidbody2D);
        tackle.SetCreatMisalignment(_attackCreatePos);
        tackle.OnCompleted += () =>
        {
            _timer = 0;
        };
        _stateMachine.AddState(States.tackle, tackle);

        /* スタン */
        stun = new Stun(Rigidbody2D, Triggers.toIdle.ToString(), _stanTime, false);
        _stateMachine.AddState(States.stan, stun);

        /* 吹き飛ばし */
        blowback = new Blowback(Rigidbody2D, Triggers.toChase.ToString(), 1f, true);
        blowback.SetMaxDistance(_blowbackMaxDistance);
        _stateMachine.AddState(States.blowback, blowback);

        /* 死亡 */
        var dead = new Idle();
        _stateMachine.AddState(States.dead, dead);
    }
}
