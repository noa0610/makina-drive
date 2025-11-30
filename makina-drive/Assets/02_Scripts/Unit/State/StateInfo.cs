using UnityEngine;

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
}
