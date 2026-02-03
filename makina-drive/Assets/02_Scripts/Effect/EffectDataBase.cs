using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MakinaDrive/EffectDataBase")]
public class EffectDataBase : ScriptableObject
{
    [Header("Visual Asset")]
    public string effectName;

    [Header("Playback")]
    public float startDelay = 0f;
    public float duration = 2.0f;
    public bool loopForever = false;
    public EffectStopType stopType = EffectStopType.Destroy;

    [Header("Positioning")]
    public EffectAttachType attachType = EffectAttachType.FixedPosition;
    public Vector3 offset = Vector3.zero;
    public bool useRandomOffset = false;
    public float randomRange = 0.2f;

    [Header("Orientation")]
    public Vector3 initialEulerAngles; // デフォルトの回転角
    public bool ignoreFlip = false; // ユニット反転後も反転させない
    public bool mirrorRotation = false; // 向きに合わせて回転を鏡写しにする
}
