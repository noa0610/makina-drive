using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 強化の選択肢を用意
/// </summary>
public class EnhanceManager
{
    [SerializeField] private List<EnhanceData> allEnhances;
    private List<EnhanceData> _currentChoices = new List<EnhanceData>(); // 現在提示している選択肢をキャッシュ

    public EnhanceManager(List<EnhanceData> enhanceDatas)
    {
        allEnhances = enhanceDatas;
    }
    
    /// <summary>
    /// 選択肢を生成
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public List<EnhanceData> GetRandomChoices(EnhanceInventory inventory, int count = 3)
    {
        // 選択肢生成済みの場合はそのまま返す
        if(_currentChoices.Count > 0)
        {
            return _currentChoices;
        }

        var candidates = new List<EnhanceData>();

        foreach (var e in allEnhances)
        {
            if(e.maxLevel == 0 || inventory.GetLevel(e) < e.maxLevel)
            {
                candidates.Add(e);
            }
        }

        // シャッフル
        for(int i = 0; i < candidates.Count; i++)
        {
            int r = Random.Range(i, candidates.Count);
            (candidates[i], candidates[r]) = (candidates[r], candidates[i]);
        }

        // 選択肢をキャッシュに保存
        _currentChoices = candidates.GetRange(0, Mathf.Min(count, candidates.Count));
        return candidates.GetRange(0, Mathf.Min(count, candidates.Count));
    }

    /// <summary>
    /// 現在の選択肢を破棄（これにより、次の選択肢生成時に新しい抽選が行われる）
    /// </summary>
    public void ResetChoices()
    {
        _currentChoices.Clear();
    }
}
