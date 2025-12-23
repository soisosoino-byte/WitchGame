using UnityEngine;
using System.Collections;

public class CrossExplosion : MonoBehaviour
{
    [Header("時間設定")]
    [Tooltip("爆発が出現してから縮小し始めるまでの時間")]
    public float existDuration = 0.5f;

    [Tooltip("縮小にかかる時間（小さくなって消える）")]
    public float shrinkDuration = 0.3f;

    public int damage = 1;

    private Vector3 initialScale; // 最初の大きさを記憶

    void Start()
    {
        initialScale = transform.localScale;
        // 自動消滅のシーケンスを開始
        StartCoroutine(ExplosionLifecycle());
    }

    IEnumerator ExplosionLifecycle()
    {
        // 1. 一定時間はそのままの大きさで留まる
        yield return new WaitForSeconds(existDuration);

        // 2. 徐々に小さくする
        float timer = 0f;
        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / shrinkDuration;

            // 1(元の大きさ) から 0(消滅) へ滑らかに変化
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, progress);

            yield return null;
        }

        // 3. 完全に小さくなったら削除
        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kyara"))
        {
            var playerHP = collision.GetComponent<hpdesu2>();
            if (playerHP != null)
            {
                playerHP.TakeDamage(damage);
            }
        }
    }
}