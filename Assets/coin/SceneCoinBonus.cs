using UnityEngine;

public class SceneCoinBonus : MonoBehaviour
{
    [Header("基本設定")]
    public int bonusAmount = 1;       // 増やす枚数
    public string saveKey = "MedalCount"; // 保存するデータの名前（前回のコードに合わせています）

    [Header("一回きりにする場合")]
    public bool isOneTimeOnly = false; // チェックを入れると、最初の一回しか貰えなくなる
    public string bonusID = "Scene1_Bonus"; // ★重要★ シーンごとに違う名前にしてください（例: Stage2_Bonus）

    void Start()
    {
        GiveBonus();
    }

    void GiveBonus()
    {
        // 1. 「一回限定モード」かつ「すでに受け取り済み」なら何もしない
        if (isOneTimeOnly && PlayerPrefs.GetInt(bonusID, 0) == 1)
        {
            Debug.Log("このシーンのボーナスは受け取り済みです。");
            return;
        }

        // 2. 現在のコインを取得して増やす
        int currentCoins = PlayerPrefs.GetInt(saveKey, 0);
        currentCoins += bonusAmount;

        // 3. 保存する
        PlayerPrefs.SetInt(saveKey, currentCoins);

        // 4. 「一回限定モード」なら、受け取ったことを記録する
        if (isOneTimeOnly)
        {
            PlayerPrefs.SetInt(bonusID, 1); // 1 = 受け取り済み
        }

        PlayerPrefs.Save();
        Debug.Log("ボーナス！コインを " + bonusAmount + " 枚獲得しました。現在: " + currentCoins + "枚");
    }
}