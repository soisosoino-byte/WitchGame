using UnityEngine.SceneManagement;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ★追加: これがないとシーン移動できません
using System.Collections;

public class ScreenFadeManager : MonoBehaviour
{
    public static ScreenFadeManager Instance;

    [Header("フェード設定")]
    public Image fadePanel; // 画面全体を覆う黒いパネル
    public float fadeDuration = 1.0f; // 暗転にかかる時間

    void Awake()
    {
        // 親子関係を解除して、ルート（一番上）に移動させる
        transform.SetParent(null);

        // その後に実行する
        DontDestroyOnLoad(this.gameObject);
    }

    // ★追加: 外部からこの関数を呼ぶと、暗転してシーン移動します
    public void ChangeScene(string nextSceneName)
    {
        StartCoroutine(FadeAndLoadSequence(nextSceneName));
    }

    // 暗転 → シーン移動 → 明転 の流れを行うコルーチン
    IEnumerator FadeAndLoadSequence(string sceneName)
    {
        // 1. フェードアウト（暗くする）
        yield return StartCoroutine(FadeOut());

        // 2. シーン読み込み
        SceneManager.LoadScene(sceneName);

        // 3. フェードイン（明るくする）
        // ※シーンが切り替わった後、自動的に画面を明るく戻します
        yield return StartCoroutine(FadeIn());
    }

    // 単体のフェードアウト処理
    public IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float timer = 0f;

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            SetAlpha(alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        SetAlpha(1f);
    }

    // 単体のフェードイン処理
    public IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            SetAlpha(alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0f);
        fadePanel.gameObject.SetActive(false);
    }

    void SetAlpha(float alpha)
    {
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = alpha;
            fadePanel.color = c;
        }
    }
}