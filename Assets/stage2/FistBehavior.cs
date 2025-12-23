using UnityEngine;
using System.Collections;

public class FistBehavior : MonoBehaviour
{
    [Header("オーディオ設定")]
    public AudioClip soundSmash; // 振り下ろし時の音

    // ★追加: 音量調整用のスライダー（0.0 ～ 1.0）
    [Range(0f, 1f)]
    public float smashVolume = 0.5f; // デフォルトは半分くらいの大きさ

    private AudioSource audioSource;

    private float targetY;
    private float speed;
    private bool isMoving = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void MoveToHeight(float y, float moveSpeed)
    {
        targetY = y;
        speed = moveSpeed;
        isMoving = true;
    }

    public void MoveWithVibration(float y, float moveSpeed, float vibrateDuration, float vibrateStrength)
    {
        // ★変更: 第2引数に volume (音量) を渡す
        if (soundSmash != null)
        {
            audioSource.PlayOneShot(soundSmash, smashVolume);
        }

        StartCoroutine(VibrateSequence(y, moveSpeed, vibrateDuration, vibrateStrength));
    }

    IEnumerator VibrateSequence(float y, float moveSpeed, float duration, float strength)
    {
        isMoving = false;
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float xOffset = Random.Range(-1f, 1f) * strength;
            transform.position = new Vector3(startPos.x + xOffset, transform.position.y, transform.position.z);
            yield return null;
            elapsed += Time.deltaTime;
        }

        transform.position = new Vector3(startPos.x, transform.position.y, transform.position.z);
        MoveToHeight(y, moveSpeed);
    }

    void Update()
    {
        if (isMoving)
        {
            float newY = Mathf.MoveTowards(transform.position.y, targetY, speed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            if (Mathf.Abs(transform.position.y - targetY) < 0.01f)
            {
                isMoving = false;
            }
        }
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