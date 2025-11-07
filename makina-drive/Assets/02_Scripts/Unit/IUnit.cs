using UnityEngine;

public interface IUnit
{

}

public interface IPlayable
{
    void InputReject();
    void AllowedInput();
}
// ポーズ中に行う処理（UnitManagerから呼び出す想定）
public interface IPausable
{
    void Pause();
    void Play();
}
