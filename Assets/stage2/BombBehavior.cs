using UnityEngine;
using System.Collections;

public class BombBehavior : MonoBehaviour
{
    [Header("予兆設定")]
    public float vibrationStrength = 0.1f;
    public Color blinkColor = Color.red;
    public float blinkInterval = 0.1f;

    [Header("オーディオ設定")] // ★追加
    public AudioClip soundMove;    // 爆弾が飛んでいく音（ヒュルル...）
    public AudioClip soundExplode; // 爆発音（ドカーン！）
    private AudioSource audioSource;

    private Vector3 targetPosition;
    private float moveDuration;
    private float blinkDuration;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D bombCollider;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        bombCollider = GetComponent<Collider2D>();

        // ★追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (bombCollider != null) bombCollider.enabled = false;
    }

    public void Setup(Vector3 target, float moveTime, float blinkTime)
    {
        targetPosition = target;
        moveDuration = moveTime;
        blinkDuration = blinkTime;
        StartCoroutine(BombSequence());
    }

    IEnumerator BombSequence()
    {
        // ★追加: 移動音を開始（ループ）
        if (soundMove != null)
        {
            audioSource.clip = soundMove;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 1. 移動
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;

        // ★追加: 移動音を停止
        audioSource.Stop();
        audioSource.loop = false;

        // 2. 予兆
        Coroutine vibrateCoroutine = StartCoroutine(Vibrate());
        Coroutine blinkCoroutine = StartCoroutine(Blink());

        yield return new WaitForSeconds(blinkDuration);

        StopCoroutine(vibrateCoroutine);
        StopCoroutine(blinkCoroutine);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        transform.position = targetPosition;

        // 3. 爆発
        Explode();
    }

    IEnumerator Vibrate()
    {
        Vector3 basePos = transform.position;
        while (true)
        {
            float x = Random.Range(-1f, 1f) * vibrationStrength;
            float y = Random.Range(-1f, 1f) * vibrationStrength;
            transform.position = basePos + new Vector3(x, y, 0);
            yield return null;
        }
    }

    IEnumerator Blink()
    {
        if (spriteRenderer == null) yield break;
        bool isRed = false;
        while (true)
        {
            spriteRenderer.color = isRed ? originalColor : blinkColor;
            isRed = !isRed;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    void Explode()
    {
        Debug.Log("爆発！");
        if (bombCollider != null) bombCollider.enabled = true;

        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        // ★追加: 爆発音を鳴らす（ワンショット）
        if (soundExplode != null) audioSource.PlayOneShot(soundExplode);

        Destroy(gameObject, 0.5f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kyara"))
        {
            var playerHP = collision.GetComponent<hpdesu2>();
            if (playerHP != null)
            {
                playerHP.TakeDamage(1);
            }
        }
    }
}