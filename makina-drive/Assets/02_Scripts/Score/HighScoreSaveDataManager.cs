using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor.Overlays;
using UnityEngine;

public class HighScoreSaveDataManager : SingletonBehavior<HighScoreSaveDataManager>
{
    private HighScoreSaveData _currentSaveData = new HighScoreSaveData();
    private readonly string _saveFileName = "savedata.dat";
    private readonly string _encryptionKey = "YourSecretKey123";
    private readonly byte[] _key = Encoding.UTF8.GetBytes("64109634827847798456398782385437");

    protected override void Awake()
    {
        base.Awake();
        Load();
    }

    // ハイスコア取得
    public int GetHighScore(string stageId)
    {
        var entry = _currentSaveData.StageScores.FirstOrDefault(s => s.StageId == stageId);
        return entry.HighScore;
    }

    // ハイスコア更新
    public void UpdateHighScore(string stageId, int newScore)
    {
        int index = _currentSaveData.StageScores.FindIndex(s => s.StageId == stageId);
        if(index >= 0)
        {
            if(newScore > _currentSaveData.StageScores[index].HighScore)
            {
                var entry = _currentSaveData.StageScores[index];
                entry.HighScore = newScore;
                _currentSaveData.StageScores[index] = entry;
                Save();
            }
        }
        else
        {
            _currentSaveData.StageScores.Add(new HighScoreSaveData.StageScoreEntry{ StageId = stageId, HighScore = newScore });
            Save();
        }
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(_currentSaveData);
        byte[] encryptedData = Encrypt(json);
        File.WriteAllBytes(GetSavePath(), encryptedData);
    }

    private void Load()
    {
        string path = GetSavePath();
        if(!File.Exists(path)) return;

        byte[] encryptedData = File.ReadAllBytes(path);
        string json = Decrypt(encryptedData);
        _currentSaveData = JsonUtility.FromJson<HighScoreSaveData>(json);
    }

    private string GetSavePath() => Path.Combine(Application.persistentDataPath, _saveFileName);

    // 暗号化
    private byte[] Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = _key;
            aes.GenerateIV(); // ランダムなIVを生成
            byte[] iv = aes.IV;

            using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
            using(var ms = new MemoryStream())
            {
                // 戦闘にIVを書き込む
                ms.Write(iv, 0, iv.Length);

                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using(var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return ms.ToArray();
            }
        }
    }

    // 暗号化の復元
    private string Decrypt(byte[] cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = _key;
            byte[]iv = new byte[aes.BlockSize / 8];

            // IVを抽出
            Array.Copy(cipherText, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
