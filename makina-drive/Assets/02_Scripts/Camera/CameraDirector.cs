using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// カメラ演出管理用シングルトン
/// </summary>
public class CameraDirector : SingletonBehavior<CameraDirector>
{
    [SerializeField] private CameraMove cameraMove;

    public async UniTaskVoid PlayFreezeEffect(float duration)
    {
        cameraMove.SetFreeze(true);
        await UniTask.Delay((int) (duration * 1000));
        cameraMove.SetFreeze(false);
    }

    public void PlayZoom(float size = 5.0f, float zoomDuration = 0.05f, float resetDuration = 0.3f)
    {
        Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => 0, x => {}, 0, 0.1f))     // ディレイ
               .AppendCallback(() => cameraMove.Zoom(size, zoomDuration)) // ズームイン
               .AppendInterval(0.2f)
               .AppendCallback(() => cameraMove.ResetZoom(resetDuration)); // 戻す
    }

    public void PlayShake(float duration = 0.2f, float strength = 0.5f)
    {
        cameraMove.Shake(duration, strength);
    }
}
