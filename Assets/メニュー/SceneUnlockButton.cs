using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // コルーチンに必要

public class SceneUnlockButton : MonoBehaviour
{
    [Header("条件設定")]
    [Tooltip("SceneVisitRecorderで設定したIDと一字一句同じにしてください")]
    public string requiredVisitID = "Stage1_Visited";

    [Header("見た目の設定")]
    public Image buttonImage;       // ボタン本体の画像（最初はLockedを表示）
    public Sprite lockedSprite;     // まだ行ってない時の画像
    public Sprite unlockedSprite;   // 行ったあとの画像

    [Header("演出設定（今回追加）")]
    [Tooltip("下からせり上がる演出用の画像（ボタンの子要素に配置してください）")]
    public Image transitionOverlayImage;
    [Tooltip("キラキラエフェクトのプレハブ")]
    public GameObject sparklePrefab;
    [Tooltip("操作を不能にするための透明パネル（Canvas内の一番手前に配置）")]
    public GameObject inputBlockerPanel;
    public float animationDuration = 1.5f; // 画像が変化する時間

    [Header("移動先")]
    public string nextSceneName = "BossBattleScene";

    private bool isUnlocked = false;

    void Start()
    {
        CheckUnlockStatus();
    }

    void CheckUnlockStatus()
    {
        // 1. まず「行ったことがあるか」チェック
        bool visited = PlayerPrefs.GetInt(requiredVisitID, 0) == 1;

        // 2. 「演出をもう見たか」チェック（キー名はID + "_AnimShown" とします）
        string animKey = requiredVisitID + "_AnimShown";
        bool animShown = PlayerPrefs.GetInt(animKey, 0) == 1;

        if (visited)
        {
            if (!animShown)
            {
                // ★パターンA: 行ったことあるけど、演出はまだ -> 演出開始！
                StartCoroutine(PlayUnlockSequence(animKey));
            }
            else
            {
                // ★パターンB: 演出も見た済み -> 最初から解放状態
                SetUnlockedState();
            }
        }
        else
        {
            // ★パターンC: まだ行ってない -> ロック状態
            if (buttonImage != null && lockedSprite != null)
            {
                buttonImage.sprite = lockedSprite;
            }
            if (transitionOverlayImage != null) transitionOverlayImage.gameObject.SetActive(false);
        }
    }

    // 最初から解放状態にする関数
    void SetUnlockedState()
    {
        isUnlocked = true;
        if (buttonImage != null && unlockedSprite != null)
        {
            buttonImage.sprite = unlockedSprite;
        }
        // 重ねる用の画像は非表示に
        if (transitionOverlayImage != null) transitionOverlayImage.gameObject.SetActive(false);
    }

    // ★演出のコルーチン
    IEnumerator PlayUnlockSequence(string animKey)
    {
        // 1. 初期状態設定（見た目はロック状態）
        if (buttonImage != null) buttonImage.sprite = lockedSprite;

        // 2. 操作をブロックする
        if (inputBlockerPanel != null) inputBlockerPanel.SetActive(true);

        Debug.Log("解放演出開始：操作無効化");

        // 少し待つ（シーン遷移直後のため）
        yield return new WaitForSeconds(0.5f);

        // 3. キラキラエフェクト再生
        if (sparklePrefab != null)
        {
            // ボタンの位置にエフェクトを生成
            Instantiate(sparklePrefab, transform.position, Quaternion.identity, transform);
        }

        // 4. 下から画像が変わるアニメーション
        if (transitionOverlayImage != null && unlockedSprite != null)
        {
            transitionOverlayImage.gameObject.SetActive(true);
            transitionOverlayImage.sprite = unlockedSprite;

            // 重要：ここでFilled設定にする（コードで強制設定）
            transitionOverlayImage.type = Image.Type.Filled;
            transitionOverlayImage.fillMethod = Image.FillMethod.Vertical;
            transitionOverlayImage.fillOrigin = 0; // 0 = Bottom
            transitionOverlayImage.fillAmount = 0f; // 最初は空っぽ

            float timer = 0f;
            while (timer < animationDuration)
            {
                timer += Time.deltaTime;
                // 徐々に満たしていく
                transitionOverlayImage.fillAmount = Mathf.Lerp(0f, 1f, timer / animationDuration);
                yield return null;
            }
            transitionOverlayImage.fillAmount = 1f;
        }

        // 5. データの更新（完了）
        SetUnlockedState(); // 見た目を完全に解放状態にする

        // 「演出見たよ」と記録する
        PlayerPrefs.SetInt(animKey, 1);
        PlayerPrefs.Save();

        // 6. 操作ブロック解除
        if (inputBlockerPanel != null) inputBlockerPanel.SetActive(false);

        Debug.Log("解放演出終了：操作有効化");
    }

    public void OnClickButton()
    {
        if (isUnlocked)
        {
            Debug.Log("条件クリア済み。シーンへ移動します: " + nextSceneName);
            if (ScreenFadeManager.Instance != null)
            {
                ScreenFadeManager.Instance.ChangeScene(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            Debug.Log("まだその場所に行っていません！");
        }
    }

    // デバッグ用：リセット（未訪問＆演出未再生に戻す）
    [ContextMenu("未訪問状態にする（リセット）")]
    public void ResetStatus()
    {
        PlayerPrefs.DeleteKey(requiredVisitID);
        PlayerPrefs.DeleteKey(requiredVisitID + "_AnimShown"); // 演出フラグも消す
        Debug.Log("リセットしました。");
    }

    // デバッグ用：訪問済みだが演出は見ていない状態にする
    [ContextMenu("訪問済み・演出前（デバッグ）")]
    public void SetVisitedOnly()
    {
        PlayerPrefs.SetInt(requiredVisitID, 1);
        PlayerPrefs.DeleteKey(requiredVisitID + "_AnimShown");
        PlayerPrefs.Save();
        Debug.Log("訪問済みにしました（演出は次回再生されます）。");
    }
}