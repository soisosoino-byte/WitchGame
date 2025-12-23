using System.Collections;
using UnityEngine;

public class SpawnedObjectBehavior : MonoBehaviour
{
    // ★ Inspectorで設定する要素 ★
    [Header("Sprite Settings")]
    public Sprite initialSprite; // 出現時の画像
    public Sprite hitGroundSprite; // GroundまたはKyaraに触れた後の画像

    [Header("Fade Out Settings")]
    public float fadeOutDuration = 0.3f; // 画像が透明になるまでの時間

    // ★追加箇所ここから★
    [Header("Sound Effects")]
    public AudioClip hitSound; // Groundなどに接触した時に鳴らす効果音
    [Range(0f, 1f)] // 0から1の範囲でスライダー表示
    public float hitSoundVolume = 1.0f; // 効果音のボリューム (0:無音, 1:最大)
    // ★追加箇所ここまで★

    // ★ 内部変数 ★
    private SpriteRenderer spriteRenderer; // このオブジェクトのSpriteRenderer
    private AudioSource audioSource; // AudioSourceコンポーネントへの参照 ★追加★
    private bool hasEffectStarted = false; // ★変更：演出が既に開始されたかどうかの汎用フラグ★

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpawnedObjectBehavior: SpriteRendererがアタッチされていません。", this);
            this.enabled = false;
            return;
        }

        // ★追加箇所ここから★
        audioSource = GetComponent<AudioSource>(); // AudioSourceを取得
        if (audioSource == null)
        {
            Debug.LogWarning("SpawnedObjectBehavior: AudioSourceがアタッチされていません！自動的に追加します。");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // ★追加箇所ここまで★

        // 初期スプライトを設定
        if (initialSprite != null)
        {
            spriteRenderer.sprite = initialSprite;
        }
        else
        {
            Debug.LogWarning("SpawnedObjectBehavior: Initial Spriteが設定されていません。", this);
        }

        // 初期状態で完全に不透明にしておく
        Color currentColor = spriteRenderer.color;
        currentColor.a = 1f;
        spriteRenderer.color = currentColor;
    }

    // 地面（Ground）とKyaraとの物理的な衝突を検出
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 既に演出が開始されている場合は何もしない
        if (hasEffectStarted)
        {
            return;
        }

        // Tagが"Ground"、"Kyara"、または"Enemy"のオブジェクトに接触した場合
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Kyara") || collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log(collision.gameObject.name + " に接触しました。演出を開始します。");
            hasEffectStarted = true; // 演出開始フラグを立てる

            // ★追加箇所ここから★
            // 効果音を再生
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound, hitSoundVolume); // ボリュームを指定して再生
            }
            // ★追加箇所ここまで★

            // オブジェクトの動きを止める（必要であれば）
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true; // 物理演算を無効にする
            }

            // 画像を変化させる
            if (hitGroundSprite != null)
            {
                spriteRenderer.sprite = hitGroundSprite;
            }
            else
            {
                Debug.LogWarning("SpawnedObjectBehavior: Hit Ground Spriteが設定されていません。", this);
            }

            // フェードアウト処理を開始
            StartCoroutine(FadeOutAndDestroy());

            // ここに、Kyaraに当たった際の追加処理（例：ダメージ処理など）を記述します。
            if (collision.gameObject.CompareTag("Kyara"))
            {
                // 例: KyaraのHPを減らす、効果音を鳴らす、パーティクルを出すなど。
                // KyaraHealth kyaraHealth = collision.gameObject.GetComponent<KyaraHealth>();
                // if (kyaraHealth != null)
                // {
                //    kyaraHealth.TakeDamage(damageAmount); // ダメージを与える
                // }
            }
        }
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

        spriteRenderer.color = endColor;
        Destroy(gameObject);
        Debug.Log("オブジェクトを削除しました。");
    }
}