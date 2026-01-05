using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 🔹 シングルトン（全スクリプトからアクセス可能）
    public static UIManager Instance;

    // 🔹 登録するUIスクリプトの参照
    public UI_Slide Slide { get; private set; }
    public InputButton InputButton { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        RegisterAllUI();
    }

    private void RegisterAllUI()
    {
        Slide = FindFirstObjectByType<UI_Slide>();
        InputButton = FindAnyObjectByType<InputButton>();

        if (Slide == null)
        {
            Debug.LogWarning("UI_Slide がシーン内に見つかりません。");
            return;
        }
        if (InputButton == null)
        {
            Debug.LogWarning("InputButton がシーン内に見つかりません。");
            return;
        }

        // 🔹 イベント接続
        Slide.OnSlideComplete += InputButton.CompleteUI_Slide;

        Debug.Log("UIManager: UI_Slide と InputButton を自動接続しました。");
    }
}
