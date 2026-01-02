using UnityEngine;
using UnityEditor;

// EnhanceDataクラスをカスタム表示するためのエディタスクリプト
[CustomEditor(typeof(EnhanceData))]
public class EnhanceDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 元のデータの参照を取得
        EnhanceData data = (EnhanceData)target;

        // 共通項目の表示
        data.enhanceName = EditorGUILayout.TextField("項目名", data.enhanceName);
        
        EditorGUILayout.LabelField("説明");
        data.discription = EditorGUILayout.TextArea(data.discription, GUILayout.Height(60));
        
        data.maxLevel = EditorGUILayout.IntField("最大レベル(0で無限)", data.maxLevel);
        
        // 区切り線
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("強化設定", EditorStyles.boldLabel);

        // 列挙型の選択
        data.type = (EnhanceType)EditorGUILayout.EnumPopup("強化タイプ", data.type);

        // 選択されたタイプに応じて表示を切り替える
        switch (data.type)
        {
            case EnhanceType.statusConstantUp:
            case EnhanceType.StatusRatioUp:
                // ステータス強化用の項目を表示
                data.targetStatus = (Status)EditorGUILayout.EnumPopup("対象ステータス", data.targetStatus);
                data.ratioPerLevel = EditorGUILayout.FloatField("上昇値 / 倍率", data.ratioPerLevel);
                break;

            case EnhanceType.Heal:
                // 回復用の項目を表示
                data.healRatio = EditorGUILayout.Slider("回復割合 (0-1)", data.healRatio, 0f, 1f);
                break;
        }

        // 変更を保存するために必要
        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }
}