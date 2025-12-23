using UnityEngine;

/// <summary>
/// 時間を加算するタイマー
/// </summary>
public class TimerCount : MonoBehaviour
{
    private float _currentTime = 0;

    public bool timerStop = false;
    public float currentTime => _currentTime;

    void Update()
    {
        if (timerStop) return;

        CountUp();
    }

    private void CountUp()
    {
        _currentTime += Time.deltaTime;
    }


    private void OnEnable()
    {
        GameStateManager.OnStateChanged += TimerStop;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= TimerStop;
    }

    private void TimerStop(GameState gameState)
    {
        if (gameState != GameState.Play)
        {
            timerStop = true;
        }
        else
        {
            timerStop = false;
        }
    }
}
