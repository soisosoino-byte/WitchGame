using System.Collections;
using UnityEngine;

public class DustController : MonoBehaviour
{
    public float fadeDuration = 0.2f; // 消えるまでの時間

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeAndDestroy());
        }
        else
        {
            Destroy(gameObject, fadeDuration); // 画像がない場合は時間経過で消すだけ
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        float timer = 0f;
        Color startColor = spriteRenderer.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // アルファ値を1から0へ変化させる
            float alpha = Mathf.Lerp(startColor.a, 0f, timer / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject); // 完全に透明になったら削除
    }
}