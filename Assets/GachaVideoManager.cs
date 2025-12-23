using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class GachaVideoManager : MonoBehaviour
{
    [Header("コイン設定")]
    public Text coinText;             // コイン枚数を表示するテキスト
    public int coinCost = 1;          // ガチャ1回に必要なコイン数

    [Header("UI設定")]
    public RawImage displayScreen;      // 映像を映すRawImage
    public GameObject gachaUIObject;    // 画面全体（表示/非表示用）

    [Header("プレイヤー設定（2つ必要）")]
    public VideoPlayer openingPlayer;   // 1つ目のVideoPlayer（オープニング用）
    public VideoPlayer resultPlayer;    // 2つ目のVideoPlayer（結果用）

    [Header("動画URL設定")]
    public string openingVideoUrl;      // 演出動画URL
    public string[] resultVideoUrls;    // 武器ごとの動画URL（5種）

    private bool isResultLooping = false;

    // ゲーム開始時に呼ばれる
    void Start()
    {
        // 画面のコイン表示を更新する
        UpdateCoinUI();
    }

    // ガチャボタンから呼ばれる関数
    public void StartGachaPerformance()
    {
        // 1. まずコインが足りているかチェック！
        // ★修正点: "CoinCount" を "MedalCount" に変更
        int currentCoins = PlayerPrefs.GetInt("MedalCount", 0);

        if (currentCoins < coinCost)
        {
            Debug.Log("コインが足りません！");
            return; // ここで処理を強制終了（ガチャは回らない）
        }

        // 2. コインを消費する
        currentCoins -= coinCost;

        // ★修正点: "CoinCount" を "MedalCount" に変更
        PlayerPrefs.SetInt("MedalCount", currentCoins); // 保存
        PlayerPrefs.Save();
        UpdateCoinUI(); // 画面の枚数表示を更新

        Debug.Log("【確認】ガチャボタンが押されました。残りコイン: " + currentCoins);

        // 3. 抽選 (0～4)
        int weaponIndex = Random.Range(1, 6);

        // 4. データ保存
        PlayerPrefs.SetInt("Weapon_" + weaponIndex, 1);
        PlayerPrefs.Save();
        Debug.Log("【成功】武器ID: " + weaponIndex + " を保存しました");

        // 5. 演出開始コルーチンへ
        StartCoroutine(PlaySeamlessSequence(weaponIndex));
    }

    // ★追加機能：コイン枚数の表示を更新する関数
    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            // ★修正点: "CoinCount" を "MedalCount" に変更
            int coins = PlayerPrefs.GetInt("MedalCount", 0);
            coinText.text = "コイン: " + coins + "枚";
        }
    }

    // ★追加機能：テスト用（ボタンに割り当てるとコインが増えます）
    public void DebugAddCoin()
    {
        // ★修正点: "CoinCount" を "MedalCount" に変更
        int coins = PlayerPrefs.GetInt("MedalCount", 0);
        coins += 1;
        PlayerPrefs.SetInt("MedalCount", coins);
        PlayerPrefs.Save();
        UpdateCoinUI();
        Debug.Log("コインを1枚増やしました");
    }

    IEnumerator PlaySeamlessSequence(int index)
    {
        isResultLooping = false;
        gachaUIObject.SetActive(true);

        // --- 手順1：オープニングの準備と再生 ---
        openingPlayer.source = VideoSource.Url;
        openingPlayer.url = openingVideoUrl;
        openingPlayer.isLooping = false;

        // 画面の出力先をオープニング用プレイヤーにセット
        displayScreen.texture = openingPlayer.texture;

        openingPlayer.Prepare();
        // 準備待ち
        while (!openingPlayer.isPrepared) yield return null;

        // 再生開始
        openingPlayer.Play();
        displayScreen.texture = openingPlayer.texture; // 念のため再セット

        // --- 手順2：再生中に、裏で次の動画を準備する（重要！） ---

        // 結果用プレイヤーの設定
        resultPlayer.source = VideoSource.Url;
        // ※もしエラーが出る場合は配列の範囲外アクセスなので、URLの数が5個あるか確認してください
        resultPlayer.url = resultVideoUrls[index];
        resultPlayer.isLooping = true; // 結果はループ

        // 裏で読み込み開始！
        resultPlayer.Prepare();

        // --- 手順3：オープニングが終わるのを待つ ---
        while (openingPlayer.isPlaying)
        {
            yield return null;
        }

        // --- 手順4：即座に切り替え ---

        // もしネットが遅くてまだ準備できてなかったら、ここで少し待つことになる
        while (!resultPlayer.isPrepared) yield return null;

        // 表示を結果プレイヤーに切り替えて再生
        displayScreen.texture = resultPlayer.texture;
        resultPlayer.Play();

        // オープニングは停止
        openingPlayer.Stop();

        isResultLooping = true;
    }

    public void OnScreenClick()
    {
        if (isResultLooping)
        {
            // 両方止める
            openingPlayer.Stop();
            resultPlayer.Stop();
            gachaUIObject.SetActive(false);
            isResultLooping = false;
            Debug.Log("演出終了");
        }
    }
}