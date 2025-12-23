using UnityEngine;
using UnityEngine.UI; // Textを使うため

public class MedalManager : MonoBehaviour
{
    [Header("UI設定")]
    public Text medalText; // メダル数を表示するテキスト

    void Start()
    {
        // ゲーム開始時に表示を更新
        UpdateDisplay();
    }

    // 画面の枚数表示を更新する関数
    public void UpdateDisplay()
    {
        int currentMedals = PlayerPrefs.GetInt("MedalCount", 0); // 保存データを読み込む

        if (medalText != null)
        {
            medalText.text = "メダル: " + currentMedals + "枚";
        }
    }

    // メダルがあるか確認する関数 (はい: true / いいえ: false)
    public bool CanPlayGacha()
    {
        int currentMedals = PlayerPrefs.GetInt("MedalCount", 0);
        return currentMedals > 0; // 1枚以上あれば OK
    }

    // メダルを1枚消費する関数
    public void ConsumeMedal()
    {
        int currentMedals = PlayerPrefs.GetInt("MedalCount", 0);
        if (currentMedals > 0)
        {
            currentMedals = currentMedals - 1; // 1枚減らす
            PlayerPrefs.SetInt("MedalCount", currentMedals); // 保存
            PlayerPrefs.Save();

            UpdateDisplay(); // 表示も更新
        }
    }

    // 【テスト用】右クリックでメダルを10枚増やす魔法
    [ContextMenu("メダルを10枚追加")]
    public void AddTestMedals()
    {
        int currentMedals = PlayerPrefs.GetInt("MedalCount", 0);
        PlayerPrefs.SetInt("MedalCount", currentMedals + 10);
        PlayerPrefs.Save();
        UpdateDisplay();
        Debug.Log("メダルを10枚追加しました！");
    }
}
