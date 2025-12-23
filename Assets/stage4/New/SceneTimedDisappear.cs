using UnityEngine;
using System.Collections;

public class SceneTimedDisappear : MonoBehaviour
{
    [Header("消えるタイミング設定")]
    [Tooltip("シーン開始から何秒後に消え始めるか")]
    public float waitTime = 3.0f; // 例: 3秒後に消え始める

    [Tooltip("消えるのにかける時間（今回は0.4秒）")]
    public float fadeDuration = 0.4f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // コルーチン（時間差処理）を開始
        StartCoroutine(DisappearProcess());
    }

    IEnumerator DisappearProcess()
    {
        // 1. 指定した時間だけ待つ
        yield return new WaitForSeconds(waitTime);

        // 2. 0.4秒かけて透明にする
        if (spriteRenderer != null)
        {
            float timer = 0f;
            Color startColor = spriteRenderer.color;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                // 現在の進行度 (0 ～ 1)
                float progress = timer / fadeDuration;

                // アルファ値（透明度）を 1(不透明) から 0(透明) へ変化させる
                float newAlpha = Mathf.Lerp(startColor.a, 0f, progress);

                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

                yield return null;
            }
        }
        else
        {
            // 画像がないオブジェクトの場合は、透明化処理をスキップして時間だけ待つ
            // (もし縮小させたい場合はここに縮小処理を書く)
            yield return new WaitForSeconds(fadeDuration);
        }

        // 3. 完全に消滅させる（オブジェクトを削除）
        Destroy(gameObject);
    }
}