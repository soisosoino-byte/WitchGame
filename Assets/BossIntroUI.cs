using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI要素を扱うために必要

public class BossIntroUI : MonoBehaviour
{
    // ★Inspectorで設定するUI要素への参照★
    public Image bossImage; // ボス紹介画像を表示するImageコンポーネント

    // ★演出の設定★
    public float displayDuration = 3.0f; // 画像が表示されている時間（フェードアウト開始までの時間）
    public float fadeOutDuration = 1.0f; // フェードアウトにかかる時間
    public float slideInDuration = 0.5f; // スライドインにかかる時間 (オプション)

    // ★画像の表示位置調整用★
    // RectTransformのアンカーが左端になっている前提
    public float targetXPosition = 100f; // スライドイン後の画像のX座標（左端からのピクセル数など）

    void Start()
    {
        if (bossImage == null)
        {
            Debug.LogError("Boss Imageが設定されていません！Inspectorで設定してください。", this);
            this.enabled = false; // スクリプトを無効化してエラーを避ける
            return;
        }

        // 初期設定：完全に透明にし、画面左外に配置 (スライドイン演出のため)
        Color startColor = bossImage.color;
        startColor.a = 0f; // 透明に
        bossImage.color = startColor;

        // 画像のRectTransformを取得し、初期位置を設定
        RectTransform rectTransform = bossImage.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // アンカーが左端（min.x=0, max.x=0）になっていることを想定
            // 初期位置は画面左外
            rectTransform.anchoredPosition = new Vector2(-rectTransform.rect.width, rectTransform.anchoredPosition.y);
        }
        else
        {
            Debug.LogError("Boss ImageにRectTransformがありません。", this);
            this.enabled = false;
            return;
        }

        // コルーチンを開始して演出を始める
        StartCoroutine(ShowBossIntro());
    }

    IEnumerator ShowBossIntro()
    {
        // RectTransformを取得
        RectTransform rectTransform = bossImage.GetComponent<RectTransform>();
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(targetXPosition, rectTransform.anchoredPosition.y);

        // ★スライドイン演出★
        float timer = 0f;
        while (timer < slideInDuration)
        {
            float t = timer / slideInDuration;
            // X座標を補間
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            // フェードインも同時に行う
            Color currentColor = bossImage.color;
            currentColor.a = Mathf.Lerp(0f, 1f, t);
            bossImage.color = currentColor;

            timer += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = targetPosition; // 最終位置に正確に設定
        bossImage.color = new Color(bossImage.color.r, bossImage.color.g, bossImage.color.b, 1f); // 完全に不透明に

        Debug.Log("ボス紹介画像が表示されました。");

        // 指定時間表示を維持
        yield return new WaitForSeconds(displayDuration);

        // ★フェードアウト演出★
        timer = 0f;
        Color fullColor = bossImage.color;
        while (timer < fadeOutDuration)
        {
            float t = timer / fadeOutDuration;
            fullColor.a = Mathf.Lerp(1f, 0f, t); // 透明度を1から0へ
            bossImage.color = fullColor;
            timer += Time.deltaTime;
            yield return null;
        }
        fullColor.a = 0f; // 完全に透明に
        bossImage.color = fullColor;

        Debug.Log("ボス紹介画像がフェードアウトしました。");

        // オプション: フェードアウト後、画像オブジェクトを非表示にするか、Destroyするか
        bossImage.gameObject.SetActive(false); // 無効化して非表示にする
        // Destroy(bossImage.gameObject); // GameObjectごと削除したい場合
    }
}