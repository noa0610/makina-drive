using JetBrains.Annotations;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Linq;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using UnityEngine.PlayerLoop;

/// <summary>
/// プレイヤーユニット
/// </summary>
[RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
public partial class Freya : UnitBase, IPausable
{
    [Header("固有設定")]
    private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
    [SerializeField] private CircleCollider2D _coll2D;


    [Header("レベルアップ")]
    [SerializeField] private float _baseExp = 20;
    [SerializeField] private float _linearWeight = 8f;      // レベルのに比例して増える分
    [SerializeField] private float _quadraticWeight = 2f;   // レベルの2乗で増える分
    [SerializeField] private bool _isImmediateEnhancement = true; // 強化項目を即座に表示するか
    public PlayerLevel _level;
    public EnhanceInventory _inventory;
    public Action<int> OnEnhancementRequest;

    [Header("移動")]
    [SerializeField] private float _accel = 30f;
    [SerializeField] private float _decel = 20f;

    [Header("回避")]
    [SerializeField] private float _dodgeAccel = 10f;
    [SerializeField] private float _dodgeDecel = 10f;
    [SerializeField] private float _invincibleTime = 0.5f;   // 無敵時間
    [SerializeField] private float _dodgeRecoveryTime = 0.7f; // 無敵解除後の後隙回復時間
    [SerializeField] private float _dodgeStaminaLostAmount = 20;

    [Header("ドライブダッシュ")]
    [SerializeField] private BulletData Dash_bulletData;
    [SerializeField] private float _dashStaminaFrameLostAmount = 0.05f;


    [Header("攻撃共通")]
    [SerializeField] private float _createPos = 4f;
    [SerializeField] private float _attackAccel = 5f;
    [SerializeField] private GameObject _arrowPrefab; // 攻撃方向を表示する矢印
    [SerializeField] private float _arrowDistance = 1.5f;
    [SerializeField] private bool _showArrow = true;
    private DirectionIndicator _indicatorInstance;


    [Header("通常攻撃１")]
    [SerializeField] private BulletData N1_bulletData;
    [SerializeField] private float N1_inputReceptionTime = 0.2f;
    [SerializeField] private float N1_stateChangeTime = 0.7f;
    [SerializeField] private float N1_inputEndTime = 0.9f;
    [SerializeField] private float N1_attackStartTime = 0.3f;

    [Header("通常攻撃２")]
    [SerializeField] private BulletData N2_bulletData;
    [SerializeField] private float N2_inputReceptionTime = 0.2f;
    [SerializeField] private float N2_stateChangeTime = 0.7f;
    [SerializeField] private float N2_inputEndTime = 0.8f;
    [SerializeField] private float N2_attackStartTime = 0.3f;

    [Header("通常攻撃３")]
    [SerializeField] private BulletData N3_bulletData;
    [SerializeField] private float N3_inputReceptionTime = 0.2f;
    [SerializeField] private float N3_stateChangeTime = 0.8f;
    [SerializeField] private float N3_inputEndTime = 1.2f;
    [SerializeField] private float N3_attackStartTime = 0.4f;


    [Header("ダッシュ通常攻撃１")]
    [SerializeField] private BulletData DashN1_bulletData;
    [SerializeField] private float DashN1_inputReceptionTime = 0.1f;
    [SerializeField] private float DashN1_stateChangeTime = 0.6f;
    [SerializeField] private float DashN1_inputEndTime = 0.8f;
    [SerializeField] private float DashN1_attackStartTime = 0.2f;

    [Header("ダッシュ通常攻撃２")]
    [SerializeField] private BulletData DashN2_bulletData;
    [SerializeField] private float DashN2_inputReceptionTime = 0.2f;
    [SerializeField] private float DashN2_stateChangeTime = 0.6f;
    [SerializeField] private float DashN2_inputEndTime = 0.9f;
    [SerializeField] private float DashN2_attackStartTime = 0.2f;

    [Header("ダッシュ通常攻撃３")]
    [SerializeField] private BulletData DashN3_bulletData;
    [SerializeField] private float DashN3_inputReceptionTime = 0.3f;
    [SerializeField] private float DashN3_stateChangeTime = 0.6f;
    [SerializeField] private float DashN3_inputEndTime = 1f;
    [SerializeField] private float DashN3_attackStartTime = 0.4f;

    [Header("チャージ")]
    [SerializeField] private float _chargeThresholdTIme = 1.5f; // チャージ完了時間
    private float _chargeTimer = 0f;
    private bool _isPressingFire = false;
    private bool _isChargeCompleted = false;

    [Header("チャージ攻撃")]
    [SerializeField] private BulletData _Charge_bulletData;

    [SerializeField] private BulletData _Charge_Extra_bulletData;
    [SerializeField] private float _Charge_inputReceptionTime = 0.8f;
    [SerializeField] private float _Charge_stateChangeTime = 0.9f;
    [SerializeField] private float _Charge_inputEndTime = 1.4f;
    [SerializeField] private float _Charge_attackStartTime = 0.25f;


    [Header("チャージダッシュ攻撃")]
    [SerializeField] private BulletData _ChargeDash_bulletData;

    [SerializeField] private BulletData _ChargeDash_Extra_bulletData;
    [SerializeField] private float _ChargeDash_inputReceptionTime = 0.8f;
    [SerializeField] private float _ChargeDash_stateChangeTime = 0.9f;
    [SerializeField] private float _ChargeDash_inputEndTime = 1.4f;
    [SerializeField] private float _ChargeDash_attackStartTime = 0.25f;


    [Header("ジャンプ開始")]
    [SerializeField] private float _jumpStartTime = 1f;
    [SerializeField] private float _jumpStaminaLostAmount = 80f;

    [Header("落下狙い")]
    [SerializeField] private float _fallAimAccel = 40f;
    [SerializeField] private float _fallAimDecel = 30f;
    [SerializeField] private float _fallAutoChangeTIme = 5f;

    [Header("落下")]
    [SerializeField] private float _fallTime = 0.12f;

    [Header("落下攻撃")]
    [SerializeField] private BulletData FallAttack_bulletData;
    [SerializeField] private float _fallAttackTime = 1.0f;

    [Header("死亡")]
    [SerializeField] private float _deadGameOverDelay = 1f;
    private bool _isDead = false;

    [Header("エフェクト")]
    [SerializeField] private Transform _jumpBoosterEffectPoint;
    [SerializeField] private Transform _FallAttackBoosterEffectPoint;
    private EffectInstance _activeJumpEffect;
    private EffectInstance _activeFallAimEffect;
    private EffectInstance _activeFallAttackBoosterEffect;
    private EffectInstance _activeChargeEffect;
    private EffectInstance _activeLevelUPEffect;
    private EffectInstance _activeHealEffect;
    private EffectInstance _activeDamageEffect;
    private GameObject _childParticle;

    [Header("SE")]
    [SerializeField] private VisualInfo _N1_AttackSE;
    [SerializeField] private VisualInfo _N2_AttackSE;
    [SerializeField] private VisualInfo _N3_AttackSE;
    [SerializeField] private VisualInfo _DodgeSE;
    [SerializeField] private VisualInfo _DashSE;
    [SerializeField] private VisualInfo _JumpSE;
    [SerializeField] private VisualInfo _JumpAttackSE;
    [SerializeField] private VisualInfo _Charge_AttackSE;
    [SerializeField] private VisualInfo _Charge_DashAttackSE;
    [SerializeField] private VisualInfo _ChargeCompletedSE;
    [SerializeField] private VisualInfo _levelUpSE;
    [SerializeField] private VisualInfo _DamageSE;
    [SerializeField] private VisualInfo _DaedSE;

    private string _dashAttackTag = "DA";

    private Vector2 _dashDirection = Vector2.right;
    private bool _inputDash = false;


    #region   ===== Initialization Process =====
    protected override void AfterAwake()
    {
        base.AfterAwake();
        _level = new PlayerLevel(this, _baseExp, _linearWeight, _quadraticWeight);
        _inventory = new EnhanceInventory();
        Rigidbody2D.freezeRotation = true;

        // 攻撃方向UI表示
        if(_arrowPrefab != null)
        {
            GameObject obj = Instantiate(_arrowPrefab);
            _indicatorInstance = obj.GetComponent<DirectionIndicator>();

            if(_indicatorInstance == null)
            {
                _indicatorInstance = obj.AddComponent<DirectionIndicator>();
            }

            _indicatorInstance.Setup(this, _arrowDistance, _showArrow);
        }
    }

    protected override void Start()
    {
        ApplySettings();
    }

    // 設定を反映させる
    private void ApplySettings()
    {
        if(VisualSettingsManager.instance != null)
        {
            _isImmediateEnhancement = VisualSettingsManager.instance.Settings.isImmediateEnhancement;
        }
    }
    #endregion

    #region   ===== Update Process =====
    protected override void AfterUpdate()
    {
        base.AfterUpdate();

        if (!_isPlaying) return;

        // ボタン押しっぱなしによるチャージ計測
        if (_isPressingFire)
        {
            _chargeTimer += Time.deltaTime;
            if (_chargeTimer >= _chargeThresholdTIme && !_isChargeCompleted)
            {
                PlaySE(_ChargeCompletedSE.SEName, _ChargeCompletedSE.Volume);
                StartCharge();

                _isChargeCompleted = true;
            }
        }
    }

    protected override void BeforeFixedUpdate()
    {
        TurnAround();

        // Debug.Log("inputDash: " + _inputDash);

        if (IsMatchingState(States.drivedash))
        {
            if (_inputDash == false || statusManager.ReadValue(Status.Stamina) <= 0)
            {
                _recoveryStatus.SetLock(Status.Stamina, false);
                stateMachine.ChangeState(Triggers.dashCancel);
            }
            statusManager.AddValue(Status.Stamina, -_dashStaminaFrameLostAmount);
            _dashDirection = drivedash.GetDashDirection();
        }
    }

    /// <summary>
    ///  振り向き
    /// </summary>
    private void TurnAround()
    {
        if (Direction.x != 0)
        {
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (Direction.x > 0 ? 1 : -1);
            transform.localScale = scale;
        }
    }
    #endregion

    #region   ===== Status =====
    // 外部（経験値アイテム）から経験値取得する窓口
    public override void GainExp(float amount)
    {
        _level.AddExp(amount);
    }

    protected override void OnTakeDamage(IUnit from, float damage, Vector2 pushdir, float knockbackForce = 0)
    {
        base.OnTakeDamage(from, damage, pushdir, knockbackForce);
        PlaySE(_DamageSE.SEName, _DamageSE.Volume);
        if (EffectManager.instance != null) _activeDamageEffect = EffectManager.instance.Play("DamageHit", transform.position, transform);
    }

    // HPが0のときに処理
    public override void OnDeath()
    {
        base.OnDeath();

        stateMachine.ChangeState(Triggers.died);
        OnGameOver();
    }

    // ゲームオーバー処理
    public async void OnGameOver()
    {
        if (_isDead) return;

        PlaySE(_DaedSE.SEName, _DaedSE.Volume);

        _isDead = true;
        _coll2D.isTrigger = true;
        IsRecovery = false;
        Pause();

        await UniTask.Delay(TimeSpan.FromSeconds(_deadGameOverDelay));


        GameStateManager.instance.ChangeState(GameState.GameOver);

        if (SoundManager.instance == null) return;
        SoundManager.instance.AllStopBGM();
    }
    #endregion

    #region   ===== Event =====
    // イベントを購読
    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        _level.OnLevelUp += HandleLevelUp;
        statusManager.OnHeal += HandleHeal;
    }

    // 購読解除
    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        _level.OnLevelUp -= HandleLevelUp;
        statusManager.OnHeal -= HandleHeal;
    }

    // ゲーム状態遷移ハンドル
    private void HandleStateChanged(GameState newState)
    {
        if (newState == GameState.Clear)
        {
            Rigidbody2D.linearVelocity = Vector2.zero;

            Pause();

            if (!IsMatchingState(States.idle))
            {
                _stateMachine.SetStateDirectLazy(States.idle.ToString());
            }
        }
        else if (newState == GameState.Pause || newState == GameState.TutorialPause || newState == GameState.EnhanceSelect)
        {
            Pause();
        }
        else if (newState == GameState.Play || newState == GameState.TutorialPlay)
        {
            Play();
        }
    }

    private void HandleLevelUp(int level)
    {
        PlaySE(_levelUpSE.SEName, _levelUpSE.Volume);

        if (_isImmediateEnhancement)
        {
            TryOpenEnhanceUI();
        }
        else
        {
            if (EffectManager.instance != null) _activeLevelUPEffect = EffectManager.instance.Play("LevelUp", transform.position, transform);
        }
    }

    private void HandleHeal()
    {
        if (EffectManager.instance != null) _activeHealEffect = EffectManager.instance.Play("Heal", transform.position, transform);
    }
    #endregion

    // 強化項目UIを表示
    private void TryOpenEnhanceUI()
    {
        if (_level.EnhancementPoints > 0)
        {
            OnEnhancementRequest?.Invoke(_level.EnhancementPoints);
        }
    }

    // チャージを開始する
    private void StartCharge()
    {
        if (EffectManager.instance != null) _activeChargeEffect = EffectManager.instance.Play("Charge", transform.position, transform);
    }

    // チャージ状況をリセットする
    public void ResetCharge()
    {
        _chargeTimer = 0f;
        _isChargeCompleted = false;

        if (_activeChargeEffect != null)
        {
            _activeChargeEffect.Stop();
            _activeChargeEffect = null;
        }
    }

    private void RecheckMoveInput()
    {
        if(MoveDirection != Vector2.zero)
        {
            _stateMachine.LazyChange(Triggers.moveInput);
        }
    }


    /// <summary>
    /// 現在ステートの判別
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private bool IsMatchingState(States state)
    {
        return _stateMachine.CurrentState.key == _stateNames[state];
    }
    
    // --- シーンビューに方向を描画(テスト用) ---
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_dashDirection);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)Direction);
    }
}
