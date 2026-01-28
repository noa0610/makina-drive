using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MakinaDrive/EffectDataBase")]
public class EffectDataBase : ScriptableObject
{
    [Serializable]
    public struct EffectEntry
    {
        public string effectName; // 名前
        public GameObject prefab; // エフェクトプレハブ
        public float duration;    // 自動回収時間
        public bool usePooling;   // プーリング
    }

    public List<EffectEntry> effects;
    public GameObject GetPrefab(string name) => effects.Find(e => e.effectName == name).prefab;
}
