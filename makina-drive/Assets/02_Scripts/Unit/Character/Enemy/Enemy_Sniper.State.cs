using Unity.VisualScripting;
using UnityEngine;

public partial class Enemy_Sniper
{
    private MoveFree chase;
    private Stun stun;
    private Blowback blowback;
    private enum States
    {
        none,
        idle,
        chase,
        chaseStop,
        shoot,
        stan,
        blowback,
        dead
    }
    private enum Triggers
    {
        none,
        toIdle,
        toChase,
        toStop,
        toShoot,
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
            (Triggers.toStop, States.chaseStop, "toIdle"),
            (Triggers.toShoot, States.shoot, "toAttack"),
            (Triggers.toStan, States.stan, "toIdle"),
            (Triggers.toBlowback, States.blowback, "toHit"),
            (Triggers.died, States.dead, "")
        };
        var chaseStopTrigger = new[]
        {
            (Triggers.toChase, States.chase, "toMove"),
            (Triggers.toShoot, States.shoot, "toAttack"),
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
            .AddTransition(States.chaseStop, chaseStopTrigger)
            .AddTransition(States.shoot, attackTrigger)
            .AddTransition(States.blowback, blowbackTrigger)
            .AddTransition(States.stan, stanTrigger);
        
        /* 待機 */
        _stateMachine.AddState(States.idle, new Idle());

        /* 追跡 */
        chase = new MoveFree(true);
        chase.SetRB2(Rigidbody2D);
        chase.SetAccel(_accel);
        chase.SetDecel(_decel);
        _stateMachine.AddState(States.chase, chase);

        /* 追跡停止 */
        var stop = new Idle_MoveStop(Rigidbody2D);
        _stateMachine.AddState(States.chaseStop, stop);

        /* 攻撃 */
        var shoot = new ShootForward(_ShootBulletData, AttackLayer);
        shoot.SetGameObject(_muzzle);
        shoot.SetCreatMisalignment(_ShootCreatePos);
        shoot.onShootComplete.AddListener(() =>
        {
            _shootTimer = 0;
            PlaySE(_AttackSE.SEName, _AttackSE.Volume);
            _stateMachine.ChangeState(Triggers.toChase);
        });
        _stateMachine.AddState(States.shoot, shoot);

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
