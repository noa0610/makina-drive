using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MakinaDrive/StatusDisplayConfiguration")]
public class StatusDisplayConfiguration : ScriptableObject
{
    public List<StatusDisplaySetting> settings;
}
