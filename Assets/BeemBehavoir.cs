using System;
using System.Collections;
using UnityEngine;

public class BeemBehavior : MonoBehaviour
{
    private SpriteRenderer spriteRenderer; // BeemVisualの子オブジェクトのSpriteRenderer
    private Transform beemVisualTransform; // BeemVisualの子オブジェクトのTransform
    private AudioSource audioSource; // AudioSourceコンポーネントへの参照

    // EnemyAttackManagerから受け取る設定
    private Vector3 _initialSpawnPosition;
    private Vector3 _targetPosition;
    private float _initialRotationZ;
    private float _targetRotationZ;
    private float _moveDuration;
    private float _fadeInDuration;
    private float _fadeOutDuration;
    private Vector2 _pivotOffset; // ★追加: 回転の中心点オフセット★

    // ★追加箇所ここから★
    [Header("Sound Effects")] // Unityエディタで見やすくするための表示
    public AudioClip moveLoopSound; // 移動中にループ再生する効果音
    [Range(0f, 1f)] // 0から1の範囲でスライダーを表示
    public float moveLoopSoundVolume = 1f; // 移動中にループ再生する効果音の音量
    // ★追加箇所ここまで★

    // Beemが完全に消失したことを通知するイベント
    public event Action OnBeemDestroyed;

    /// <summary>
    /// Beemの挙動を設定し、開始します。
    /// </summary>
    /// <param name="initialSpawnPos">初期出現座標 (BeemParentの座標)</param>
    /// <param name="targetPos">移動目標座標 (BeemParentの座標)</param>
    /// <param name="initialRotationZ">初期回転角度（Z軸）(BeemParentの回転)</param>
    /// <param name="targetRotationZ">目標回転角度（Z軸）(BeemParentの回転)</param>
    /// <param name="moveDur">移動にかかる時間</param>
    /// <param name="fadeInDur">透明から不透明になるまでの時間</param>
    /// <param name="fadeOutDur">不透明から透明になるまでの時間</param>
    /// <param name="pivotOffset">ビーム画像がBeemParentの中心からずれるオフセット★追加★</param>
    public void SetupBeem(Vector3 initialSpawnPos, Vector3 targetPos, float initialRotationZ, float targetRotationZ, float moveDur, float fadeInDur, float fadeOutDur, Vector2 pivotOffset) // ★変更: pivotOffset引数を追加★
    {
        _initialSpawnPosition = initialSpawnPos;
        _targetPosition = targetPos;
        _initialRotationZ = initialRotationZ;
        _targetRotationZ = targetRotationZ;
        _moveDuration = moveDur;
        _fadeInDuration = fadeInDur;
        _fadeOutDuration = fadeOutDur;
        _pivotOffset = pivotOffset; // ★追加: pivotOffsetを設定★

        // 子オブジェクトのSpriteRendererとTransformを取得
        // BeemBehaviorはBeemParentにアタッチされているため、子からSpriteRendererを探す
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("BeemBehavior: 子オブジェクトにSpriteRendererが見つかりません！BeemPrefabの構造を確認してください。", this);
            return;
        }
        beemVisualTransform = spriteRenderer.transform; // SpriteRendererを持つ子オブジェクトのTransform

        // ★変更箇所ここから★
        audioSource = GetComponent<AudioSource>(); // AudioSourceを取得
        if (audioSource == null)
        {
            Debug.LogWarning("BeemBehavior: AudioSourceがアタッチされていません！自動的に追加します。");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // ★変更箇所ここまでに★

        // 親の初期位置と回転を設定
        transform.position = _initialSpawnPosition;
        transform.rotation = Quaternion.Euler(0, 0, _initialRotationZ);

        // 子オブジェクト（ビーム画像）のローカルポジションを設定
        // ここでpivotOffsetを適用することで、親の回転の中心が変わったように見える
        // BeemPrefabを編集する際に、BeemVisualのローカルPositionをpivotOffsetと同じ値にしておく必要があります
        beemVisualTransform.localPosition = _pivotOffset;


        // 最初は完全に透明にしておく
        Color currentColor = spriteRenderer.color;
        currentColor.a = 0f;
        spriteRenderer.color = currentColor;

        StartCoroutine(BeemSequence());
    }

    private IEnumerator BeemSequence()
    {
        Debug.Log($"Beemが初期座標 {transform.position}、初期角度 {transform.rotation.eulerAngles.z} に出現準備中...");

        // フェードイン (透明 -> 不透明)
        float timer = 0f;
        Color startColor = spriteRenderer.color; // 現在の色 (A=0)
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // 不透明な色
        while (timer < _fadeInDuration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, timer / _fadeInDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = endColor; // 確実に不透明に設定
        Debug.Log($"Beemが完全に不透明になりました (フェードイン時間: {_fadeInDuration}秒)。");

        // 移動と回転 (親オブジェクトのTransformを操作)
        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, _targetRotationZ);

        // ★変更箇所ここから★
        // 移動開始時に効果音を再生
        if (moveLoopSound != null && audioSource != null)
        {
            audioSource.clip = moveLoopSound; // 再生するクリップを設定
            audioSource.loop = true; // ループ再生を有効にする
            audioSource.volume = moveLoopSoundVolume; // 音量を設定
            audioSource.Play(); // 再生開始
        }
        // ★変更箇所ここまで★

        timer = 0f;
        while (timer < _moveDuration)
        {
            float t = timer / _moveDuration;
            transform.position = Vector3.Lerp(startPos, _targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = _targetPosition; // 確実に目標位置に設定
        transform.rotation = endRotation;      // 確実に目標角度に設定
        Debug.Log($"Beemが目標座標 {transform.position} へ移動完了、目標角度 {transform.rotation.eulerAngles.z} に到達しました。");

        // ★追加箇所ここから★
        // 移動完了時に効果音を停止
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        // ★追加箇所ここまで★

        // フェードアウト (不透明 -> 透明)
        startColor = spriteRenderer.color; // 現在の色 (A=1)
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 透明な色
        timer = 0f;
        while (timer < _fadeOutDuration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, timer / _fadeOutDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = endColor; // 確実に透明に設定
        Debug.Log($"Beemが完全に透明になりました (フェードアウト時間: {_fadeOutDuration}秒)。");

        // 役割を終えたBeemを削除する前にイベントを発火
        OnBeemDestroyed?.Invoke();
        Destroy(gameObject);
        Debug.Log("Beemオブジェクトを削除しました。");
    }
}
