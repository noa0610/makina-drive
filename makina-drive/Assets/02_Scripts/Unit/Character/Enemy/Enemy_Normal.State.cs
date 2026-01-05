using UnityEngine;

public partial class Enemy_Normal
{
    private MoveFree chase;
    private Stun stun;
    private Blowback blowback;
    private enum States
    {
        none,
        idle,
        chase,
        attack,
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
            (Triggers.toChase, States.chase, "toMove"),
            (Triggers.toStan, States.stan, "toIdle"),
            (Triggers.toBlowback, States.blowback, "toHit"),
            (Triggers.died, States.dead, "")
        };
        var chaseTrigger = new[]
        {
            (Triggers.toIdle, States.idle, "toIdle"),
            (Triggers.toAttack, States.attack, "toAttack"),
            (Triggers.toStan, States.stan, "toIdle"),
            (Triggers.toBlowback, States.blowback, "toHit"),
            (Triggers.died, States.dead, "")
        };
        var attackTrigger = new[]
        {
            (Triggers.toChase, States.chase, "toMove"),
            (Triggers.toStan, States.stan, "toIdle"),
            (Triggers.toBlowback, States.blowback, "toHit"),
            (Triggers.died, States.dead, "")
        };
        var blowbackTrigger = new[]
        {
            (Triggers.toChase, States.chase, "toMove"),
            (Triggers.died, States.dead, "")
        };
        var stanTrigger = new[]
        {
            (Triggers.toIdle, States.idle, "toIdle"),
            (Triggers.toBlowback, States.blowback, "toHit"),
            (Triggers.died, States.dead, "")
        };

        // ステートマシンにStatesの移動先の追加
        _stateMachine
            .AddTransition(States.idle, idleTrigger)
            .AddTransition(States.chase, chaseTrigger)
            .AddTransition(States.attack, attackTrigger)
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
        
        /* 攻撃 */
        var attack = new ShootCombo(_attackBulletData, AttackLayer, Triggers.toChase.ToString());
        attack.SetTime(1f, 1f, _stateChangeTime, _attackTime);
        attack.SetGameObject(_muzzle);
        attack.SetCreatMisalignment(_attackCreatePos);
        attack.onShootComplete.AddListener(() =>
        {
            PlaySE(_AttackSE.SEName, _AttackSE.Volume);
        });
        _stateMachine.AddState(States.attack, attack);

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