using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// WaveDataをExcel/CSVから読み込んだ情報で生成するエディタ用インポーター
/// </summary>
public class WaveDataImporter : EditorWindow
{
    private const string SAVE_PATH = "Assets/07_Datas/Waves/";
    private const string PREFAB_SEARCH_PATH = "Assets/06_Prefabs/Unit/Character/Enemy/EnemyComp";

    [MenuItem("Tools/Import WaveData from CSV")]
    public static void Import()
    {
        string path = EditorUtility.OpenFilePanel("Select CSV", "Assets", "csv");
        if (string.IsNullOrEmpty(path)) return;

        // フォルダが無い場合は作成
        if (!Directory.Exists(SAVE_PATH)) Directory.CreateDirectory(SAVE_PATH);

        // 全ての行を読み込む
        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1) return;

        // ウェーブ名ごとに情報をまとめる
        var waveGroups = new Dictionary<string, List<string[]>>();

        // 一行目はスキップ（ヘッダー用）
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] data = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            // クォーテーションを除去
            for (int j = 0; j < data.Length; j++)
                data[j] = data[j].Trim(' ', '\"');

            if (data.Length < 2) continue;

            string waveName = data[0];
            if (!waveGroups.ContainsKey(waveName))
                waveGroups[waveName] = new List<string[]>();

            waveGroups[waveName].Add(data);
        }

        foreach (var group in waveGroups)
        {
            UpdateOrCreateWaveAsset(group.Key, group.Value);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("WaveDataのインポートが完了しました。");
    }

    private static string[] ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"') inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else current += c;
        }
        result.Add(current.Trim());
        return result.ToArray();
    }

    
    /// <summary>
    /// ウェーブデータを作成または更新
    /// </summary>
    /// <param name="waveName"></param>
    /// <param name="spawnInfos"></param>
    private static void UpdateOrCreateWaveAsset(string waveName, List<string[]> rows)
    {
        string assetPath = $"{SAVE_PATH}{waveName}.asset";
        WaveData wave = AssetDatabase.LoadAssetAtPath<WaveData>(assetPath);

        // 同名のウェーブデータが存在しない場合は作成
        if (wave == null)
        {
            wave = ScriptableObject.CreateInstance<WaveData>();
            AssetDatabase.CreateAsset(wave, assetPath);
        }

        wave.waveName = waveName;
        wave.spawnInfos = new List<UnitSpawnInfo>();

        foreach (var data in rows)
        {
            if(data.Length < 14)
            {
                Debug.LogWarning($"列が足りない行があります。Wave: {waveName}");
                continue;
            }

            UnitSpawnInfo info = new UnitSpawnInfo();

            // 1: PrefabName
            info.unitBase = FindEnemyPrefab(data[1]);
            // 2: Count (生成処理回数)
            info.spawnCount = int.Parse(data[2]);
            // 3: Min (同時最小数)
            info.minSpawnCount = int.Parse(data[3]);
            // 4: Max (同時最大数)
            info.maxSpawnCount = int.Parse(data[4]);
            // 5: Start (開始時間)
            info.spawnTime = float.Parse(data[5]);
            // 6: End (終了時間)
            info.spawnEndTime = float.Parse(data[6]);
            // 7: Interval (間隔)
            info.spawnInterval = float.Parse(data[7]);
            // 8: StatusRate (ウェーブ強化倍率)
            info.waveIncreaseRate = float.Parse(data[8]);
            // 9: Destroy (自動消去)
            info.destroyTime = float.Parse(data[9]);
            // 10: Effect
            info.spawneEffectName = data[10];
            // 11: SE
            info.FirstSpawneSEName = data[11];
            // 12: IsClear
            info.isClearTarget = bool.Parse(data[12]);

            // 13: StatusOverrides の解析 ("HP:1.2:10;ATK:1.1:0")
            info.statusOverrides = ParseStatusOverrides(data[13]);

            // 14: CompClass (生成ロジック) のインスタンス化
            info.comp = CreateSpawnComponent(data[14]);

            wave.spawnInfos.Add(info);
        }

        EditorUtility.SetDirty(wave);
    }

    /// <summary>
    /// StatusOverrideをデータから取得
    /// </summary>
    /// <param name="rawData"></param>
    /// <returns></returns>
    private static List<StatusOverride> ParseStatusOverrides(string rawData)
    {
        var list = new List<StatusOverride>();
        if (string.IsNullOrEmpty(rawData)) return list;

        // セミコロンとカンマの両方を区切り文字として扱う
        string[] entries = rawData.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            string[] parts = entry.Split(':');
            if (parts.Length >= 2)
            {
                StatusOverride so = new StatusOverride();
                if (Enum.TryParse(parts[0], out Status type))
                {
                    so.type = type;
                    so.multiplier = float.Parse(parts[1]);
                    so.addition = parts.Length > 2 ? float.Parse(parts[2]) : 0;
                    list.Add(so);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 生成法則のデータを名前から取得
    /// </summary>
    /// <param name="className"></param>
    /// <returns></returns>
    private static ISpawnComponent CreateSpawnComponent(string className)
    {
        if (string.IsNullOrEmpty(className)) return null;

        // プロジェクト内のすべての型から検索（名前空間がある場合は注意）
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .FirstOrDefault(p => typeof(ISpawnComponent).IsAssignableFrom(p) && p.Name == className);

        if (type != null)
        {
            return (ISpawnComponent)Activator.CreateInstance(type);
        }
        
        Debug.LogWarning($"SpawnComponent '{className}' が見つかりませんでした。");
        return null;
    }


    /// <summary>
    /// エネミーのプレハブを探す
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static UnitBase FindEnemyPrefab(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        string[] guids = AssetDatabase.FindAssets($"{name} t:Prefab", new[] { PREFAB_SEARCH_PATH });
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return go != null ? go.GetComponent<UnitBase>() : null;
        }
        Debug.LogWarning($"{name} プレハブが見つかりません。");
        return null;
    }
}
