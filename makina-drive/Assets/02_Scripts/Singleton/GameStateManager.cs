using System;
using UnityEngine;

// 他クラスで変数として所持可能
public enum GameState
{
    None,
    Play,
    Menu,
    Clear,
    GameOver,
    Pause,
    TutorialPlay,
    TutorialPause
}

/// <summary>
/// ゲーム状態管理
/// </summary>
[DefaultExecutionOrder(-3)]
public class GameStateManager : SingletonBehavior<GameStateManager>
{
    public static event Action<GameState> OnStateChanged;
    [SerializeField] private GameState _gameState = GameState.Play;
    public string _currentGameState => _gameState.ToString();
    
    public void ChangeState(GameState nextGameState)
    {
        _gameState = nextGameState;
        OnStateChanged?.Invoke(_gameState);
    }

    /* --------- 使用例 --------- /

    // 状態変更イベントを購読
    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    // 購読解除
    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    // イベント変更時に処理を行うハンドル
    private void HandleStateChanged(GameState newState)
    {
        if(newState == GameState.Clear)
        {
            // クリア画面を表示　など
        }
        else if(newState == GameState.Pause)
        {
            // ゲームの時間を停止　など
        }
    }

    /  -------------------------- */
}
