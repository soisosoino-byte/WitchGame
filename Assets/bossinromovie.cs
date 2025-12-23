using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI要素を扱うために必要
using UnityEngine.Video; // VideoPlayerを扱うために必要

public class BossIntroVideo : MonoBehaviour
{
    // ★Inspectorで設定するUI要素への参照★
    public RawImage bossVideoRawImage; // 映像を表示するRawImageコンポーネント
    public VideoPlayer videoPlayer; // 映像再生用のVideoPlayerコンポーネント

    // ★映像ファイルとレンダーテクスチャの参照（Inspectorで設定）★
    public VideoClip bossVideoClip; // 再生する映像ファイル
    public RenderTexture videoRenderTexture; // 映像の出力先となるレンダーテクスチャ

    // ★演出の設定★
    public float displayDuration = 3.0f; // 映像が表示されている時間（フェードアウト開始までの時間）
    public float fadeOutDuration = 1.0f; // フェードアウトにかかる時間
    public float slideInDuration = 0.5f; // スライドインにかかる時間 (オプション)

    // ★映像の表示位置調整用★
    // RectTransformのアンカーが左端になっている前提
    public float targetXPosition = 100f; // スライドイン後の画像のX座標（左端からのピクセル数など）

    void Start()
    {
        if (bossVideoRawImage == null || videoPlayer == null || bossVideoClip == null || videoRenderTexture == null)
        {
            Debug.LogError("必要なコンポーネントまたはアセットが設定されていません。Inspectorを確認してください。", this);
            this.enabled = false;
            return;
        }

        // RawImageにレンダーテクスチャを割り当て
        bossVideoRawImage.texture = videoRenderTexture;

        // VideoPlayerの設定
        videoPlayer.clip = bossVideoClip;
        videoPlayer.targetTexture = videoRenderTexture;
        videoPlayer.isLooping = false; // 繰り返し再生しない
        videoPlayer.playOnAwake = false; // 自動再生しない (スクリプトで制御するため)

        // 初期設定：完全に透明にし、画面左外に配置 (スライドイン演出のため)
        Color startColor = bossVideoRawImage.color;
        startColor.a = 0f; // 透明に
        bossVideoRawImage.color = startColor;

        // RawImageのRectTransformを取得し、初期位置を設定
        RectTransform rectTransform = bossVideoRawImage.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // アンカーが左端（min.x=0, max.x=0）になっていることを想定
            // 初期位置は画面左外
            rectTransform.anchoredPosition = new Vector2(-rectTransform.rect.width, rectTransform.anchoredPosition.y);
        }
        else
        {
            Debug.LogError("RawImageにRectTransformがありません。", this);
            this.enabled = false;
            return;
        }

        // コルーチンを開始して演出を始める
        StartCoroutine(ShowBossIntroVideo());
    }

    IEnumerator ShowBossIntroVideo()
    {
        // VideoPlayerの準備
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null; // 準備が完了するまで待機
        }

        // VideoPlayerの再生を開始
        videoPlayer.Play();

        // RectTransformを取得
        RectTransform rectTransform = bossVideoRawImage.GetComponent<RectTransform>();
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(targetXPosition, rectTransform.anchoredPosition.y);

        // ★スライドイン＆フェードイン演出★
        float timer = 0f;
        while (timer < slideInDuration)
        {
            float t = timer / slideInDuration;
            // X座標を補間
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            // フェードインも同時に行う
            Color currentColor = bossVideoRawImage.color;
            currentColor.a = Mathf.Lerp(0f, 1f, t); // 透明度を0から1へ
            bossVideoRawImage.color = currentColor;

            timer += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = targetPosition; // 最終位置に正確に設定
        bossVideoRawImage.color = new Color(bossVideoRawImage.color.r, bossVideoRawImage.color.g, bossVideoRawImage.color.b, 1f); // 完全に不透明に

        Debug.Log("ボス紹介映像が表示されました。");

        // 指定時間表示を維持 (映像が再生されている間、またはdisplayDurationが経過するまで)
        float currentDisplayTime = 0f;
        while (videoPlayer.isPlaying && currentDisplayTime < displayDuration)
        {
            currentDisplayTime += Time.deltaTime;
            yield return null;
        }
        // もし映像がdisplayDurationより短い場合は、映像が終わるのを待つ
        // もし映像がdisplayDurationより長い場合は、displayDurationで切り上げる

        // 映像が再生中の場合は停止させる
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        // ★フェードアウト演出★
        timer = 0f;
        Color fullColor = bossVideoRawImage.color;
        while (timer < fadeOutDuration)
        {
            float t = timer / fadeOutDuration;
            fullColor.a = Mathf.Lerp(1f, 0f, t); // 透明度を1から0へ
            bossVideoRawImage.color = fullColor;
            timer += Time.deltaTime;
            yield return null;
        }
        fullColor.a = 0f; // 完全に透明に
        bossVideoRawImage.color = fullColor;

        Debug.Log("ボス紹介映像がフェードアウトしました。");

        // オプション: フェードアウト後、映像オブジェクトを非表示にするか、Destroyするか
        bossVideoRawImage.gameObject.SetActive(false); // 無効化して非表示にする
        // Destroy(bossVideoRawImage.gameObject); // GameObjectごと削除したい場合
    }
}