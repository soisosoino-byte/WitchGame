using UnityEngine;
using System.Collections;

public class MagicCircleFader : MonoBehaviour
{
    [Header("フェード設定")]
    public float fadeInTime = 0.5f;  // 現れる時間
    public float fadeOutTime = 0.5f; // 消える時間

    private SpriteRenderer spriteRenderer;
    private ParticleSystem ps;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 子要素にあるパーティクルも取得
        ps = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {
        // 生成された瞬間にフェードイン開始
        StartCoroutine(FadeInProcess());
    }

    // ★フェードイン処理（自動実行）
    IEnumerator FadeInProcess()
    {
        float timer = 0f;
        Color color = spriteRenderer.color;

        // 最初は透明(0)にする
        color.a = 0f;
        spriteRenderer.color = color;

        // パーティクル再生開始
        if (ps != null) ps.Play();

        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            // アルファ値を 0 -> 1 に滑らかに変化
            color.a = Mathf.Lerp(0f, 1f, timer / fadeInTime);
            spriteRenderer.color = color;
            yield return null;
        }

        // 念のため完全に不透明(1)にする
        color.a = 1f;
        spriteRenderer.color = color;
    }

    // ★フェードアウト処理（外部から呼ばれる）
    public void StartFadeOut()
    {
        StartCoroutine(FadeOutProcess());
    }

    IEnumerator FadeOutProcess()
    {
        // パーティクルの新規発生を止める（残っている粒は自然に消える）
        if (ps != null) ps.Stop();

        float timer = 0f;
        Color color = spriteRenderer.color;
        float startAlpha = color.a;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            // アルファ値を 現在の値 -> 0 に変化
            color.a = Mathf.Lerp(startAlpha, 0f, timer / fadeOutTime);
            spriteRenderer.color = color;
            yield return null;
        }

        // 完全に透明になったら自分自身を削除
        Destroy(gameObject);
    }
}
