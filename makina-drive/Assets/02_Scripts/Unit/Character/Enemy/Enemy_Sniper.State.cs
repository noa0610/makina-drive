using Unity.VisualScripting;
using UnityEngine;

public partial class Enemy_Sniper
{
    private MoveFree chase;
    private Stun stun;
    private enum States
    {
        none,
        idle,
        chase,
        shoot,
        stan,
        dead
    }
    private enum Triggers
    {
        none,
        toIdle,
        toChase,
        toShoot,
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
            (Triggers.toShoot, States.shoot, ""),
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
            .AddTransition(States.shoot, attackTrigger)
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
        var shoot = new ShootForward(_ShootBulletData, AttackLayer);
        shoot.SetGameObject(_muzzle);
        shoot.SetCreatMisalignment(_ShootCreatePos);
        shoot.onShootComplete.AddListener(() =>
        {
            _shootTimer = 0;
            _stateMachine.ChangeState(Triggers.toChase);
        });
        _stateMachine.AddState(States.shoot, shoot);

        /* スタン */
        stun = new Stun(rb, Triggers.toIdle.ToString(), _stanTime, false);
        _stateMachine.AddState(States.stan, stun);

        /* 死亡 */
        var dead = new Idle();
        _stateMachine.AddState(States.dead, dead);
    }
}
