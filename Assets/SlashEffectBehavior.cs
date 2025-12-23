using UnityEngine;
using System.Collections;
using System;

public class SlashEffectBehavior : MonoBehaviour
{
    public event Action OnSlashEffectDestroyed;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float moveDuration;
    private float fadeOutDuration;
    private float initialRotationZ;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource; // AudioSourceコンポーネントへの参照

    // ★追加箇所ここから★
    [Header("Sound Effects")] // Unityエディタで見やすくするための表示
    public AudioClip spawnSound; // 出現時の効果音
    [Range(0f, 1f)] // 0から1の範囲でスライダーを表示
    public float spawnSoundVolume = 1f; // 出現時の効果音の音量
    // ★追加箇所ここまで★

    public void SetupSlashEffect(Vector3 startPos, Vector3 endPos, float moveDur, float fadeOutDur, float initialRotZ)
    {
        startPosition = startPos;
        endPosition = endPos;
        moveDuration = moveDur;
        fadeOutDuration = fadeOutDur;
        initialRotationZ = initialRotZ;

        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(0, 0, initialRotationZ);

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SlashEffectBehavior: SpriteRendererが見つかりません。フェードアウトが機能しません。", this);
        }

        // ★変更箇所ここから★
        audioSource = GetComponent<AudioSource>(); // AudioSourceを取得
        if (audioSource == null)
        {
            Debug.LogWarning("SlashEffectBehavior: AudioSourceがアタッチされていません！自動的に追加します。");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // ★変更箇所ここまでに★

        StartCoroutine(AnimateSlashEffect());
    }

    private IEnumerator AnimateSlashEffect()
    {
        // ★変更箇所ここから★
        // 出現時の効果音を再生
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound, spawnSoundVolume); // 音量を指定して再生
        }
        // ★変更箇所ここまで★

        // 移動フェーズ
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPosition; // 最終位置に確定

        // フェードアウトフェーズ
        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeOutDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f); // 完全に透明に
        }

        // 破壊
        OnSlashEffectDestroyed?.Invoke(); // イベント通知
        Destroy(gameObject);
    }
}