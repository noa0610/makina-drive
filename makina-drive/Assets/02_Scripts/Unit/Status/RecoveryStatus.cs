using UnityEngine;

public class RecoveryStatus
{
    [SerializeField] private StatusManager _statusManager;
    [SerializeField] private Status _targetStatus = Status.Stamina;
    [SerializeField] private float _recoveryRatePerSecond = 5f;
    [SerializeField] private float _recoveryDelay = 2f;
    
    private float _timeSinceLastDamage = 0f;
    private StatusInfo _statusInfo;

    public RecoveryStatus(StatusManager statusManager, Status targetStatus ,float recoveryPerSecond, float recoveryDelay)
    {
        _statusManager = statusManager;
        _targetStatus = targetStatus;
        _recoveryRatePerSecond = recoveryPerSecond;
        _recoveryDelay = recoveryDelay;

        _statusInfo = _statusManager.GetStatus(_targetStatus);

        if(_statusInfo != null)
        {
            _statusInfo.OnAmountChanged += OnRecoveryAmountChanged;
        }
        else
        {
            Debug.LogError($"RecoveryStatus: Status {_targetStatus} not found in StatusManager.");
        }
    }
    public RecoveryStatus(){}


    private void OnRecoveryAmountChanged(float before, float after)
    {
        // 値が減少した場合、タイマーをリセット
        if(after < before)
        {
            _timeSinceLastDamage = 0f; // Reset timer on damage
        }
    }

    public void Tick(float deltaTime)
    {
        // 回復条件の確認
        if (_statusInfo == null || _statusInfo.CurrentAmount >= _statusInfo.DefaultAmount)
        {
            // Debug.Log("RecoveryStatus: No recovery needed or status info is null.");
            return; // Staminaが最大値に達している、または情報がない場合は処理しない
        }

        // ディレイ（遅延）の経過を待つ
        if (_timeSinceLastDamage < _recoveryDelay)
        {
            Debug.Log("RecoveryStatus: Waiting for recovery delay.");
            _timeSinceLastDamage += deltaTime;
            return;
        }

        // 回復処理
        float recoverAmount = _recoveryRatePerSecond * deltaTime;
        
        _statusInfo.CurrentAmount += recoverAmount;
    }
}
