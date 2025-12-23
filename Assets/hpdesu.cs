using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hpdesu : MonoBehaviour
{
    public Sprite[] hpSprites; // HPゲージ画像（6枚: 5→0）

    private Image hpImage; // UIのImageコンポーネントへの参照
    private RectTransform rectTransform; // HPゲージのRect Transformへの参照

    // 振動設定のための変数
    public float shakeDuration = 0.4f; // 振動させる時間
    public float shakeMagnitude = 5.0f; // 振動の強さ（ピクセル単位）
    public float shakeSpeed = 0.05f; // 振動の切り替え速度（秒）

    private Vector2 originalPosition; // HPゲージの元の位置を保存

    private Coroutine shakeCoroutine;

    // ★追加：HPゲージの表示演出用変数★
    private CanvasGroup hpGaugeCanvasGroup; // このGameObject自身のCanvasGroupへの参照
    public float initialDisplayDelay = 5.0f; // シーン開始からHPゲージが表示されるまでの遅延時間（秒）
    public float fadeInDuration = 0.5f; // フェードインにかかる時間（秒）


    void Start()
    {
        hpImage = GetComponent<Image>(); // このGameObjectにアタッチされているImageコンポーネントを取得
        rectTransform = GetComponent<RectTransform>(); // RectTransformを取得
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition; // 元の位置を保存
        }
        else
        {
            Debug.LogError("HPゲージのRectTransformコンポーネントが見つかりません。", this);
        }

        // ★追加：CanvasGroupを取得し、初期状態を透明に設定、コルーチンを開始★
        hpGaugeCanvasGroup = GetComponent<CanvasGroup>();
        if (hpGaugeCanvasGroup == null)
        {
            // CanvasGroupがなければ追加する（エディタで手動で追加するのが推奨）
            // Runtimeで追加することも可能だが、Inspectorでの設定の方が確実
            Debug.LogWarning("HPゲージにCanvasGroupコンポーネントが見つかりません。自動で追加します。", this);
            hpGaugeCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        hpGaugeCanvasGroup.alpha = 0f; // 最初は完全に透明
        StartCoroutine(ShowHPGaugeWithDelay()); // 遅延表示＆フェードインのコルーチンを開始
    }

    public void UpdateHPImage(int currentHPValue)
    {
        if (hpImage == null)
        {
            Debug.LogError("HPゲージのImageコンポーネントが見つかりません。", this);
            return;
        }

        int spriteIndex = currentHPValue;
        spriteIndex = Mathf.Clamp(spriteIndex, 0, hpSprites.Length - 1);

        if (currentHPValue <= 0)
        {
            spriteIndex = 0;
        }

        hpImage.sprite = hpSprites[spriteIndex];

        // HPが減少したときに振動を開始する
        StartShake();
    }

    // 振動を開始するメソッド
    public void StartShake()
    {
        if (rectTransform == null)
        {
            Debug.LogWarning("RectTransformがないため、HPゲージを振動できません。", this);
            return;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        rectTransform.anchoredPosition = originalPosition; // 元の位置に戻してから新しいコルーチンを始める

        shakeCoroutine = StartCoroutine(ShakeEffect());
    }

    // 振動処理を行うコルーチン
    IEnumerator ShakeEffect()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * shakeMagnitude;
            float y = originalPosition.y + Random.Range(-1f, 1f) * shakeMagnitude;

            rectTransform.anchoredPosition = new Vector2(x, y);

            yield return new WaitForSeconds(shakeSpeed);

            elapsed += shakeSpeed;
        }

        rectTransform.anchoredPosition = originalPosition;
        shakeCoroutine = null;
    }

    // ★追加：HPゲージの遅延表示とフェードインを制御するコルーチン★
    IEnumerator ShowHPGaugeWithDelay()
    {
        // 指定された遅延時間待機
        yield return new WaitForSeconds(initialDisplayDelay);

        Debug.Log("HPゲージのフェードインを開始します。");

        // フェードイン演出
        float timer = 0f;
        float startAlpha = hpGaugeCanvasGroup.alpha; // 現在のアルファ値（0f）から開始
        while (timer < fadeInDuration)
        {
            float t = timer / fadeInDuration;
            hpGaugeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t); // 透明度を現在の値から1へ
            timer += Time.deltaTime;
            yield return null;
        }
        hpGaugeCanvasGroup.alpha = 1f; // 最後に完全に不透明に設定

        Debug.Log("HPゲージが表示されました。");
    }
}