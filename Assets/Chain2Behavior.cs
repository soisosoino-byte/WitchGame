using System;
using System.Collections;
using UnityEngine;

public class Chain2Behavior : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource; // AudioSourceコンポーネントへの参照

    private Vector3 _initialSpawnPosition;
    private Vector3 _targetPosition;
    private float _initialDelayBeforeMove;
    private float _moveDuration;
    private float _initialRotationZ;
    private float _delayBeforeFade;
    private float _fadeDuration;
    private float _fadeInDuration;

    // 新しいエフェクト画像のための設定
    private GameObject _effectPrefab;           // 生成するエフェクトのPrefab
    private Vector3 _effectSpawnOffset;         // Chain2の目標座標からのエフェクトの出現オフセット
    private int _effectCount;                   // 生成するエフェクトの数
    private float _effectInterval;              // エフェクト生成間の時間間隔
    private float _effectFadeOutDuration;       // エフェクトが透明になるまでの時間
    private float _effectScale;                 // エフェクトのスケール

    // ★追加箇所ここから★
    [Header("Sound Effects")] // Unityエディタで見やすくするための表示
    public AudioClip spawnSound; // 出現時の効果音
    [Range(0f, 1f)] // 0から1の範囲でスライダーを表示
    public float spawnSoundVolume = 1f; // 出現時の効果音の音量

    public AudioClip moveCompleteSound; // 移動完了時の効果音
    [Range(0f, 1f)] // 0から1の範囲でスライダーを表示
    public float moveCompleteSoundVolume = 1f; // 移動完了時の効果音の音量
    // ★追加箇所ここまで★

    public event Action OnChain2Destroyed;

    public void SetupChain2(
        Vector3 initialSpawnPos, Vector3 targetPos, float initialDelay, float moveDur,
        float delayFade, float fadeDur, float rotationZ, float fadeInDur,
        GameObject effectPrefab, Vector3 effectSpawnOffset, int effectCount,
        float effectInterval, float effectFadeOutDuration, float effectScale)
    {
        _initialSpawnPosition = initialSpawnPos;
        _targetPosition = targetPos;
        _initialDelayBeforeMove = initialDelay;
        _moveDuration = moveDur;
        _initialRotationZ = rotationZ;
        _delayBeforeFade = delayFade;
        _fadeDuration = fadeDur;
        _fadeInDuration = fadeInDur;

        // エフェクト関連の設定を保存
        _effectPrefab = effectPrefab;
        _effectSpawnOffset = effectSpawnOffset;
        _effectCount = effectCount;
        _effectInterval = effectInterval;
        _effectFadeOutDuration = effectFadeOutDuration;
        _effectScale = effectScale;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Chain2Behavior: SpriteRendererがアタッチされていません！");
            return;
        }

        // ★変更箇所ここから★
        audioSource = GetComponent<AudioSource>(); // AudioSourceを取得
        if (audioSource == null)
        {
            Debug.LogWarning("Chain2Behavior: AudioSourceがアタッチされていません！自動的に追加します。");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // ★変更箇所ここまで★

        Color initialColor = spriteRenderer.color;
        initialColor.a = 0f;
        spriteRenderer.color = initialColor;

        transform.position = _initialSpawnPosition;
        transform.rotation = Quaternion.Euler(0, 0, _initialRotationZ);

        StartCoroutine(Chain2Sequence());
    }

    private IEnumerator Chain2Sequence()
    {
        Debug.Log($"Chain2が初期座標 {_initialSpawnPosition}、初期角度 {_initialRotationZ} に出現しました。");

        // ★変更箇所ここから★
        // 出現時の効果音を再生
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound, spawnSoundVolume); // 音量を指定して再生
        }
        // ★変更箇所ここまで★

        float timer = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
        while (timer < _fadeInDuration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, timer / _fadeInDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = endColor;
        Debug.Log($"Chain2が完全に不透明になりました (フェードイン時間: {_fadeInDuration}秒)。");

        yield return new WaitForSeconds(_initialDelayBeforeMove);
        Debug.Log($"{_initialDelayBeforeMove}秒の待機が終了しました。移動を開始します。");

        Vector3 startPos = transform.position;
        timer = 0f;
        while (timer < _moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, _targetPosition, timer / _moveDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = _targetPosition;
        Debug.Log($"Chain2が目標座標 {_targetPosition} へ移動完了しました。");

        // ★変更箇所ここから★
        // 移動完了時の効果音を再生
        if (moveCompleteSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(moveCompleteSound, moveCompleteSoundVolume); // 音量を指定して再生
        }
        // ★変更箇所ここまで★

        // 移動完了直後にエフェクトを生成・再生
        if (_effectPrefab != null)
        {
            Vector3 effectSpawnPos = _targetPosition + _effectSpawnOffset;
            Debug.Log($"Chain2が移動完了しました。エフェクトを生成します。生成位置: {effectSpawnPos}");
            StartCoroutine(SpawnAndFadeEffects(effectSpawnPos, _effectCount, _effectInterval, _effectFadeOutDuration, _effectScale));
        }

        // その後、Chain2自身の待機とフェードアウト
        yield return new WaitForSeconds(_delayBeforeFade);
        Debug.Log($"{_delayBeforeFade}秒の待機が終了しました。透明化を開始します。");

        startColor = spriteRenderer.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        timer = 0f;
        while (timer < _fadeDuration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, timer / _fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = endColor;
        Debug.Log("Chain2が完全に透明になりました。");

        OnChain2Destroyed?.Invoke();
        Destroy(gameObject);
        Debug.Log("Chain2オブジェクトを削除しました。");
    }

    private IEnumerator SpawnAndFadeEffects(Vector3 spawnPos, int count, float interval, float fadeOutDuration, float scale)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject effectObj = Instantiate(_effectPrefab, spawnPos, Quaternion.identity);

            // スケールを設定
            effectObj.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer effectSpriteRenderer = effectObj.GetComponent<SpriteRenderer>();
            if (effectSpriteRenderer != null)
            {
                // 初期は不透明
                Color initialColor = effectSpriteRenderer.color;
                initialColor.a = 1f;
                effectSpriteRenderer.color = initialColor;

                // フェードアウトコルーチンを別途開始
                StartCoroutine(FadeOutEffect(effectSpriteRenderer, fadeOutDuration));
            }
            else
            {
                Debug.LogWarning($"生成されたエフェクトPrefab '{_effectPrefab.name}' にSpriteRendererがありません。", effectObj);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator FadeOutEffect(SpriteRenderer sr, float duration)
    {
        float timer = 0f;
        Color startColor = sr.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (timer < duration)
        {
            sr.color = Color.Lerp(startColor, endColor, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        sr.color = endColor; // 確実に透明に

        // エフェクトオブジェクトを削除
        Destroy(sr.gameObject);
    }
}