using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using System;

public class UnitManager : SingletonBehavior<UnitManager>
{
    [SerializeField] private bool _isdamageTextView = false;
    [SerializeField] private Canvas _parentCanvas;  // Canvasの設定はオーバーレイ
    [SerializeField] private TextMeshProUGUI _textPrefab;
    [SerializeField] private float _textViewTime = 0.5f;
    [SerializeField] private UnitTags _displayTags = UnitTags.Enemy; // 表示対象
    public static event Action<UnitBase, UnitBase, BulletStatus?> OnUnitDamaged; // ダメージ発生イベント（被弾側、攻撃側、弾情報）

    [Header("Debug")]
    [SerializeField] private bool _damegeLog;

    private List<UnitBase> _unitList = new List<UnitBase>();
    public void AddUnit(UnitBase unit)
    {
        _unitList.Add(unit);
    }
    public void RemoveUnit(UnitBase unit)
    {
        _unitList.Remove(unit);
    }
    public void Clear()
    {
        _unitList.Clear();
    }

    public List<UnitBase> GetUnitList()
    {
        return _unitList;
    }

    /// <summary>
    /// ダメージを与える
    /// </summary>
    /// <param name="target">ダメージを受ける側</param>
    /// <param name="from">ダメージを与える側</param>
    /// <param name="damage"></param>
    public void AddDamage(UnitBase target, UnitBase from, float damage, Vector2 pushdir, float knockbackForce = 0, BulletStatus? bulletStatus = null) // BulletStatus? → 弾情報がないダメージ配慮
    {
        float finalDamage = FinalDamageCalculation(damage,
                                                   from.statusManager.ReadValue(Status.ATK),
                                                   target.statusManager.ReadValue(Status.DEF),
                                                   target.statusManager.ReadValue(Status.DamageRatio));
        if (_damegeLog)
        {
            Debug.Log($"{target.name} : Take Damage {finalDamage}.  HP: {target.statusManager.ReadValue(Status.HP) - damage} /{target.statusManager.ReadValue(Status.MaxHP)}");
        }
        target.TakeDamage(from, finalDamage, pushdir, knockbackForce);

        if (bulletStatus.HasValue)
        {
            if (bulletStatus.Value.performHitStop)
            {
                // ヒットストップ
                target.HitStop(0.1f).Forget();
                from.HitStop(bulletStatus.Value.hitStopTime).Forget();
            }
        }

        // イベント発火
        OnUnitDamaged?.Invoke(target, from, bulletStatus);

        if (_isdamageTextView)
        {
            DamageTextView(finalDamage, target);
        }
    }

    public void AddDamage(UnitBase target, UnitBase from, float damage)
    {
        float finalDamage = FinalDamageCalculation(damage,
                                                   from.statusManager.ReadValue(Status.ATK),
                                                   target.statusManager.ReadValue(Status.DEF),
                                                   target.statusManager.ReadValue(Status.DamageRatio));
        if (_damegeLog)
        {
            Debug.Log($"{target.name} : Take Damage {finalDamage}.  HP: {target.statusManager.ReadValue(Status.HP) - damage} /{target.statusManager.ReadValue(Status.MaxHP)}");
        }
        target.TakeDamage(from, finalDamage, Vector2.zero, 0);
    }

    // ダメージ計算式
    public float FinalDamageCalculation(float damage, float atkRate, float def, float damageRate)
    {
        float rn = UnityEngine.Random.Range(0.9f, 1.1f); // 乱数
        float calculationDamage = (damage * atkRate - def) * damageRate; // (弾ダメージ × 攻撃倍率 - 防御力) * 被ダメージ倍率
        float finalDamage = Mathf.Ceil(calculationDamage * rn); // 乱数端数切り上げ
        return finalDamage;
    }

    // ダメージ量をテキスト表示
    public void DamageTextView(float damage, UnitBase target)
    {
        if (_parentCanvas == null && _textPrefab == null) return;

        // 表示対象のタグを識別
        if ((target.UnitStatusData.tags & _displayTags) == 0) return;

        TextMeshProUGUI instanceText = Instantiate(_textPrefab, _parentCanvas.transform);


        instanceText.text = $"{damage}";

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
        instanceText.transform.position = screenPos;

        // テキストのアニメーション演出
        // 上に移動
        instanceText.transform.DOMoveY(screenPos.y + 50f, _textViewTime).SetEase(Ease.OutQuad);
        // フェードアウト
        instanceText.DOFade(0, _textViewTime)
        .SetEase(Ease.InQuint)
        // 破棄
        .OnComplete(() => Destroy(instanceText.gameObject));
    }

    public void Pause(bool pause, bool isTimeStop = true)
    {
        UnitBase._isPlaying = !pause;
        if (isTimeStop)
            Time.timeScale = pause ? 0f : 1f;
        foreach (var unit in _unitList)
        {
            if (unit is IPausable pausable)
            {
                if (pause)
                    pausable.Pause();
                else
                    pausable.Play();
            }
        }
    }
}