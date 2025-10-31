using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ※Send Messages運用なら、下の inputActions は不要です（残しても害はないけど混乱しやすい）
    // private PlayerInputAction inputActions;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;

    private Rigidbody2D rb;
    private Vector2 moveInput;                 // WASDの入力
    private Vector2 lastNonZeroDir = Vector2.right; // 無入力時のダッシュ向きに利用
    private bool isDashing;
    private float nextDashTime;

    private void Awake()
    {
        // inputActions = new PlayerInputAction();    // ← Send Messages方式では不要
        // inputActions.Player.Enable();

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;     // トップダウン想定
        rb.freezeRotation = true; // 回転固定
    }

    // --- Send Messages 用（Action名 "Move" と厳密一致）---
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // 方向キーを離した瞬間は0になるので、直前の向きを覚えておく
        if (moveInput.sqrMagnitude > 0.0001f)
            lastNonZeroDir = moveInput.normalized;
    }

    // --- Send Messages 用（Action名 "Dash" と厳密一致）---
    public void OnDash(InputValue value)
    {
        if (!value.isPressed) return;                    // 押された瞬間のみ反応
        if (isDashing || Time.time < nextDashTime) return;

        StartCoroutine(DashRoutine());
    }

    private System.Collections.IEnumerator DashRoutine()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        float endTime = Time.time + dashDuration;
        Vector2 dashDir = (moveInput.sqrMagnitude > 0.0001f)
            ? moveInput.normalized
            : lastNonZeroDir;

        // ダッシュ中は常に高速で上書き
        while (Time.time < endTime)
        {
            rb.linearVelocity = dashDir * dashSpeed;   // ※互換性のため velocity を使用
            yield return null;
        }

        isDashing = false;
    }

    private void FixedUpdate()
    {
        if (isDashing) return; // ダッシュ中は通常移動で上書きしない

        // 斜め入力の速度過剰にならないよう正規化して一定速度に
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}
