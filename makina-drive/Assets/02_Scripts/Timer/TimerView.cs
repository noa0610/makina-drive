using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// タイマー時間をテキストに表示するUI用クラス
/// </summary>
public class TimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TimerCount timer;
    [SerializeField] private GameStateManager _GameStateManager;
    private float _currentTime = 0f;

    private void Start()
    {
        if (_GameStateManager == null)
        {
            _GameStateManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();
        }
        
        UpdateTimerView();
    }

    private void Update()
    {
        _currentTime = timer.currentTime;
        UpdateTimerView();
    }

    private void UpdateTimerView()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(_currentTime);
        timerText.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
    }
}
