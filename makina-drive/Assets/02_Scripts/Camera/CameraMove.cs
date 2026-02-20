using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UniRx;
using Cysharp.Threading.Tasks;

/// <summary>
/// カメラ移動
/// </summary>
public class CameraMove : MonoBehaviour
{
    [Header("Player GameObject")]
    [SerializeField] private float SmoothSpeed = 0.125f; // カメラの追従速度を滑らかにするための係数
    [SerializeField] private Vector3 OFFSET; // カメラとプレイヤーの相対的な位置を設定するオフセット

    [Header("Stage CameraOut")]
    [SerializeField] private float MinX; // ステージの左端
    [SerializeField] private float MaxX; // ステージの右端
    [SerializeField] private float MinY; // ステージの下端
    [SerializeField] private float MaxY; // ステージの上端


    private GameObject _player;
    private Vector3 desiredPosition;
    private Camera _cam;
    private bool _isFrozen = false;
    private float _defaultSize;
    private CameraMove _cameraMove;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _defaultSize = _cam.orthographicSize;
    }

    void Start()
    {
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");
        if (_player != null) transform.position = _player.transform.position + OFFSET;
    }

    void LateUpdate()
    {
        if (_player == null || _isFrozen) return;

        // プレイヤーの位置にオフセットを適用し、ターゲット位置を設定
        desiredPosition = _player.transform.position + OFFSET;

        // ステージの境界内にカメラの位置を制限する
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, MinX, MaxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, MinY, MaxY);

        desiredPosition.z = transform.position.z;

        // 現在のカメラ位置からターゲット位置への移動を滑らかにする
        transform.position = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);
    }

    public void SetFreeze(bool freeze) => _isFrozen = freeze;

    // ズーム効果
    public void Zoom(float targetSize, float duration, Ease ease = Ease.OutExpo)
    {
        _cam.DOOrthoSize(targetSize, duration).SetEase(ease);
    }
    public void ResetZoom(float duration)
    {
        _cam.DOOrthoSize(_defaultSize, duration).SetEase(Ease.OutSine);
    }

    // 揺れ効果
    public void Shake(float duration, float strength)
    {
        transform.DOShakePosition(duration, strength);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        { 
            // _cameraMove = this.gameObject.GetComponent<CameraMove>();
            // _cameraMove.enabled = false;
        }
    }
}