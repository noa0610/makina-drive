using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 見下ろし視点の背景を無限ループさせる
/// </summary>
public class BackgroundTileLooper : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _baseTile; // もしくはImage?
    [SerializeField] private Transform _target;
    private float _tileWidth;
    private float _tileHeight;
    private List<Transform> _tileTransforms = new List<Transform>();

    private void Start()
    {
        if (_baseTile == null || _target == null)
        {
            Debug.Log("SpriteRenderer または Target が設定されていません。");
            return;
        }

        // 縦横サイズ取得
        _tileWidth = _baseTile.bounds.size.x;
        _tileHeight = _baseTile.bounds.size.y;

        // 9×9グリッドタイル生成
        InitializeTiles();
    }

    private void InitializeTiles()
    {
        Vector3 basePos = _baseTile.transform.position;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // (0,0)の場合は元のタイルを使用、それ以外は複製
                if (x == 0 && y == 0)
                {
                    _tileTransforms.Add(_baseTile.transform);
                }
                else
                {
                    GameObject newTile = Instantiate(_baseTile.gameObject, transform);
                    newTile.transform.position = basePos + new Vector3(x * _tileWidth, y * _tileHeight, 0f);
                    _tileTransforms.Add(newTile.transform);
                }
            }
        }
    }

    private void Update()
    {
        if (_target == null) return;

        UpdateTilePositions();
    }

    private void UpdateTilePositions()
    {
        Vector3 targetPos = _target.position;

        // タイルグリッド全体の縦横サイズ
        float totalWidth = _tileWidth * 3f;
        float totalHeight = _tileHeight * 3f;

        foreach (var tile in _tileTransforms)
        {
            Vector3 pos = tile.position;

            // ターゲットとの距離の差分
            float diffX = targetPos.x - pos.x;
            float diffY = targetPos.y - pos.y;
            
            // 1.5枚分以上の差ができたら、3枚分移動させる

            // X軸のループ移動処理
            if (Mathf.Abs(diffX) > _tileWidth * 1.5f)
            {
                pos.x += Mathf.Sign(diffX) * totalWidth;
            }

            // Y軸のループ移動処理
            if (Mathf.Abs(diffY) > _tileHeight * 1.5f)
            {
                pos.y += Mathf.Sign(diffY) * totalHeight;
            }

            tile.position = pos;
        }
    }
}
