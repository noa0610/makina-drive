using System;

/// <summary>
/// ステータス強化情報
/// </summary>
[Serializable]
public struct StatusOverride
{
    public Status type;
    public float multiplier;
    public float addition;
}
