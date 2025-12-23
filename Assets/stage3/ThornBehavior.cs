using UnityEngine;
using System.Collections;

public class ThornBehavior : MonoBehaviour
{
    [Header("オーディオ設定")]
    public AudioClip moveSound; // 地響きの音
    [Range(0, 1)] public float moveVolume = 1.0f;
    private AudioSource audioSource;

    private Vector3 startPos;
    private Vector3 endPos;
    private float moveDuration;

    private float vibrationStrength = 0.5f;
    private float vibrationSpeed = 20.0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Setup(Vector3 start, Vector3 end, float duration, float vibStrength)
    {
        startPos = start;
        endPos = end;
        moveDuration = duration;
        vibrationStrength = vibStrength;

        StartCoroutine(MoveSequence());
    }

    IEnumerator MoveSequence()
    {
        // ★移動開始：音を再生（ループ）
        if (moveSound != null)
        {
            audioSource.clip = moveSound;
            audioSource.volume = moveVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            Vector3 currentBasePos = Vector3.Lerp(startPos, endPos, t);

            float xOffset = Mathf.Sin(elapsed * vibrationSpeed) * vibrationStrength;

            transform.position = currentBasePos + new Vector3(xOffset, 0, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        // ★移動終了：音を停止
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
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