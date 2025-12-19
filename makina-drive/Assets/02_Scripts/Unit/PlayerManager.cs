using UnityEngine;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    [Tooltip("Awakeのタイミングで、IPlayerFollowerを実装したすべてのコンポーネントにアタッチします")]
    [SerializeField] private UnitBase _playerUnit;

    private void Awake()
    {
        var playerFollower = FindObjectsOfType<MonoBehaviour>().OfType<IPlayerFollower>();
        foreach (var follower in playerFollower)
        {
            follower.SetTarget(_playerUnit);
        }
    }

    public interface IPlayerFollower
    {
        void SetTarget(UnitBase target);
    }
}
