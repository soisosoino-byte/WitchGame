using UnityEngine;
using System.Collections;

public class TemporaryEffect : MonoBehaviour
{
    [Tooltip("表示を維持する時間（秒）")]
    public float visibleTime = 0.05f;
    [Tooltip("透明になるまでにかかる時間（秒）")]
    public float fadeOutTime = 0.2f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 画像コンポーネントがあればフェードアウト処理を開始
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            // 画像がない場合は時間経過で単純に削除
            Destroy(gameObject, visibleTime + fadeOutTime);
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        // 1. 一瞬だけそのまま表示
        yield return new WaitForSeconds(visibleTime);

        // 2. 徐々に透明にする
        float timer = 0f;
        Color initialColor = spriteRenderer.color;
        // 現在のアルファ値（透明度）を取得
        float startAlpha = initialColor.a;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            // 時間経過に合わせてアルファ値を 0 に近づける
            float newAlpha = Mathf.Lerp(startAlpha, 0f, timer / fadeOutTime);
            spriteRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, newAlpha);
            yield return null;
        }

        // 3. 完全に透明になったら削除
        Destroy(gameObject);
    }
}