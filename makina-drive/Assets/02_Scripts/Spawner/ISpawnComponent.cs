using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特定の法則で生成座標を返すコンポーネント用インターフェース
/// 実装によって画面外ランダム生成、画面内エリア生成などを行う。
/// </summary>
public interface ISpawnComponent
{
    public GameObject target{set; get;}

    // 新：指定された数分のスポーン座標を返す
    public List<Vector3> GetPositions(int count);
    // 旧：ユニットリストから配置する
    public List<UnitBase> Execute(List<UnitBase> pool);
}
