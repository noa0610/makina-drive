using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// オブジェクトに接触した回数を管理するクラス
/// </summary>
public class ObjectHitCounter
{
    // ヒット情報を記録
    private struct HitRecord
    {
        public int count;        // 現在のヒット回数
        public float lastHitTime; // 最後にヒットした時間
    }

    // GameObjectのInstanceIDをキーとする
    private readonly Dictionary<int, HitRecord> _hitHistory = new();

    /// <summary>
    /// 接触した対象を識別しカウント、最大接触回数による判定を返す（上限でfalse）
    /// </summary>
    public bool TryRegisterHit(GameObject target, int maxHits, float delay)
    {
        if (target == null) return false;

        int id = target.GetInstanceID();
        float currentTime = Time.time;

        // 既に記録があるか確認
        if (_hitHistory.TryGetValue(id, out HitRecord record))
        {
            if (record.count >= maxHits) return false; // 規定回数を超えている

            // ディレイチェック（前回ヒットからの経過時間）
            if (currentTime - record.lastHitTime < delay) return false;

            // 接触数を加算
            record.count++;
            record.lastHitTime = currentTime;
            _hitHistory[id] = record;
        }
        else
        {
            // 初接触
            _hitHistory.Add(id, new HitRecord { count = 1, lastHitTime = currentTime });
        }

        return true;
    }

    public void Clear() => _hitHistory.Clear();
}
