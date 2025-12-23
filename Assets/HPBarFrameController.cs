using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI要素を扱うために必要

public class HPBarFrameController : MonoBehaviour
{
    // ★Inspectorで設定するUI Imageへの参照★
    public Image hpBarFrameImage; // HPバーの枠のImageコンポーネント

    // ★フェードイン演出の設定★
    public float fadeInDelay = 2.0f; // シーン開始からフェードイン開始までの遅延時間（秒）
    public float fadeInDuration = 1.0f; // フェードインにかかる時間（秒）

    void Awake()
    {
        // Imageコンポーネントを取得
        if (hpBarFrameImage == null)
        {
            hpBarFrameImage = GetComponent<Image>();
            if (hpBarFrameImage == null)
            {
                Debug.LogError("HPBarFrameImageが設定されていません。Imageコンポーネントがアタッチされているか確認してください。", this);
                this.enabled = false; // スクリプトを無効化してエラーを避ける
                return;
            }
        }

        // 初期状態：完全に透明
        Color initialColor = hpBarFrameImage.color;
        initialColor.a = 0f;
        hpBarFrameImage.color = initialColor;
    }

    void Start()
    {
        // 遅延表示＆フェードインのコルーチンを開始
        StartCoroutine(FadeInHPBarFrame());
    }

    IEnumerator FadeInHPBarFrame()
    {
        // 指定された遅延時間待機
        Debug.Log($"HPバーの枠のフェードインを {fadeInDelay} 秒後に開始します。");
        yield return new WaitForSeconds(fadeInDelay);

        Debug.Log("HPバーの枠のフェードインを開始しました。");

        // フェードイン演出
        float timer = 0f;
        Color startColor = hpBarFrameImage.color; // 現在の色（a=0）を取得
        Color endColor = startColor;
        endColor.a = 1f; // 目標は完全に不透明

        while (timer < fadeInDuration)
        {
            float t = timer / fadeInDuration;
            hpBarFrameImage.color = Color.Lerp(startColor, endColor, t); // 透明度を0から1へ
            timer += Time.deltaTime;
            yield return null;
        }
        hpBarFrameImage.color = endColor; // 最後に確実に不透明に設定

        Debug.Log("HPバーの枠が表示されました。");
    }
}
