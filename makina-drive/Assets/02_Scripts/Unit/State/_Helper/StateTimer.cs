using UnityEngine;

public class StateTimer
{
    private float _time;
    public float ElapsedTime => _time;

    public void Update(float deltaTime) => _time += deltaTime;
    public void Reset() => _time = 0;

    // 指定時間が経過したか
    public bool HasReached(float targetTime) => _time >= targetTime;

    // 特定の期間内にいるか
    public bool IsInWindow(float startTime, float endTime) 
        => _time >= startTime && _time <= endTime;
}