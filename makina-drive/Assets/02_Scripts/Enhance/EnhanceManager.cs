using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3択の選択肢を用意
/// </summary>
public class EnhanceManager
{
    [SerializeField] private List<EnhanceData> allEnhances;

    public EnhanceManager(List<EnhanceData> enhanceDatas)
    {
        allEnhances = enhanceDatas;
    }
    
    public List<EnhanceData> GetRandomChoices(EnhanceInventory inventory, int count = 3)
    {
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
        
        return candidates.GetRange(0, Mathf.Min(count, candidates.Count));
    }

}
