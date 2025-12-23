using System.Collections;
using UnityEngine;

public class EffectBehavior : MonoBehaviour
{
    public float fadeOutDuration = 0.3f; // 透明になるまでの時間

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("EffectBehavior: SpriteRendererがアタッチされていません。", this);
            this.enabled = false;
            return;
        }

        // 初期状態で完全に不透明にしておく
        Color currentColor = spriteRenderer.color;
        currentColor.a = 1f;
        spriteRenderer.color = currentColor;

        // フェードアウト処理を開始
        StartCoroutine(FadeOutAndDestroy());
    }

    // フェードアウトしてオブジェクトを削除するコルーチン
    IEnumerator FadeOutAndDestroy()
    {
        float timer = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = startColor;
        endColor.a = 0f;

        while (timer < fadeOutDuration)
        {
            float t = timer / fadeOutDuration;
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);
            timer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = endColor; // 確実に完全透明にする
        Destroy(gameObject);
        Debug.Log("エフェクトオブジェクトを削除しました。");
    }
}
