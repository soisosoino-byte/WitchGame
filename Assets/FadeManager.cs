using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // シーン遷移のために追加

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; } // シングルトンパターン

    [SerializeField] private Image fadePanel; // 暗転用Imageコンポーネント
    [SerializeField] private float fadeDuration = 0.5f; // フェードにかかる時間

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移しても破棄されないようにする
        }
        else
        {
            Destroy(gameObject);
        }

        if (fadePanel == null)
        {
            // シーン内のFadePanelを自動で探す (HierarchyにImageコンポーネントを持つGameObjectとして配置しておく)
            // あるいはInspectorで設定できるようにpublicにする
            GameObject panelObject = GameObject.Find("FadePanel"); // ここは作成したImageの名前と一致させる
            if (panelObject != null)
            {
                fadePanel = panelObject.GetComponent<Image>();
                if (fadePanel != null)
                {
                    fadePanel.color = new Color(0, 0, 0, 0); // 初期は透明
                    fadePanel.raycastTarget = false; // クリックなどをブロックしないようにする
                }
                else
                {
                    Debug.LogError("FadePanelオブジェクトにImageコンポーネントが見つかりません！", this);
                }
            }
            else
            {
                Debug.LogError("シーンに'FadePanel'という名前のGameObjectが見つかりません！", this);
            }
        }
    }

    /// <summary>
    /// 画面を暗転させるコルーチン
    /// </summary>
    /// <param name="onCompleteSceneName">フェードアウト完了後にロードするシーン名 (nullの場合はシーン遷移しない)</param>
    public IEnumerator FadeOut(string onCompleteSceneName = null)
    {
        if (fadePanel == null)
        {
            Debug.LogError("FadePanelが設定されていません。フェードアウトできません。", this);
            yield break;
        }

        fadePanel.raycastTarget = true; // フェード中はクリックなどをブロックする

        Color startColor = fadePanel.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // 不透明な黒
        float timer = 0f;

        while (timer < fadeDuration)
        {
            fadePanel.color = Color.Lerp(startColor, targetColor, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadePanel.color = targetColor; // 完全に不透明にする

        if (!string.IsNullOrEmpty(onCompleteSceneName))
        {
            SceneManager.LoadScene(onCompleteSceneName);
        }
    }

    /// <summary>
    /// 画面をフェードイン（明るく）するコルーチン
    /// </summary>
    public IEnumerator FadeIn()
    {
        if (fadePanel == null)
        {
            Debug.LogError("FadePanelが設定されていません。フェードインできません。", this);
            yield break;
        }

        Color startColor = fadePanel.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 透明
        float timer = 0f;

        while (timer < fadeDuration)
        {
            fadePanel.color = Color.Lerp(startColor, targetColor, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadePanel.color = targetColor; // 完全に透明にする
        fadePanel.raycastTarget = false; // クリックなどをブロックしないように戻す
    }
}