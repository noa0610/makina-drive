using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StateInfo
{
    public string Name;
    public IState Instance;
    public string[] Tags;

    // APIを統一するためにkeyプロパティを追加
    public string key => Name;
    public IState state => Instance;
    public StateInfo(string name, IState instance, params string[] tags)
    {
        Name = name;
        Instance = instance;
        Tags = tags;
    }

    public StateInfo(string name, IState instance, string tag)
    {
        Name = name;
        Instance = instance;
        Tags = new string[] { tag };
    }

    public StateInfo(string name, IState instance)
    {
        Name = name;
        Instance = instance;
        Tags = Array.Empty<string>();
    }

    /// <summary>
    /// タグが含まれるかどうか。完全一致ではない
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public bool HasTag(params string[] tags)
    {
        if (Tags == null || Tags.Length == 0) return false;
        if (tags == null || tags.Length == 0) return false;
        var set = new HashSet<string>(tags);
        foreach (var t in Tags)
        {
            if (set.Contains(t)) return true;
        }
        return false;
    }
}
