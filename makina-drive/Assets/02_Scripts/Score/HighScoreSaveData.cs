using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HighScoreSaveData 
{
    // ステージIDをキー、ハイスコアを値とする辞書（保存時はList）
    public List<StageScoreEntry> StageScores = new List<StageScoreEntry>();

    [Serializable]
    public struct StageScoreEntry
    {
        public string StageId;
        public int HighScore;
    }
}
