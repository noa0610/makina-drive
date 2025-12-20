using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 指定されたユニットを特定の法則で生成するコンポーネント用インターフェース
/// 実装によって画面外ランダム生成、画面内エリア生成などを行う。
/// </summary>
public interface ISpawnComponent
{
    public GameObject target{set; get;}
    public List<UnitBase> Execute(List<UnitBase> pool);
}
