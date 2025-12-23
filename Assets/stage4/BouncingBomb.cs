using UnityEngine;
using System.Collections;

public class BouncingBomb : MonoBehaviour
{
    [Header("挙動設定")]
    public float moveDuration = 2.0f; // バウンドして移動する時間
    public float waitBeforeExplode = 0.5f; // 停止してから爆発するまでの溜め時間
    public float rotationSpeed = 360f; // ★追加: 1秒あたりの回転角度
    public Sprite explodeWarningSprite; // 爆発直前に切り替わる画像

    [Header("参照")]
    public GameObject crossExplosionPrefab;
    public ParticleSystem smokeEffect;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isStopped = false; // ★追加: 停止中かどうかのフラグ

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(BombSequence());
    }

    // ★追加: 毎フレーム回転させる処理
    void Update()
    {
        // まだ止まっていないなら回す
        if (!isStopped)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    IEnumerator BombSequence()
    {
        // 1. 指定時間は物理演算でバウンド移動（煙が出ている状態）
        if (smokeEffect != null) smokeEffect.Play();

        yield return new WaitForSeconds(moveDuration);

        // 2. 空中でピタッと停止
        isStopped = true; // ★追加: 回転を止めるフラグON
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f; // 物理的な回転力もゼロに
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 角度を0（正位置）に戻す（警告画像を見やすくするため）
        transform.rotation = Quaternion.identity;

        if (smokeEffect != null) smokeEffect.Stop();

        // 3. 画像を切り替えて警告
        if (explodeWarningSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = explodeWarningSprite;
        }

        // 少し待つ
        yield return new WaitForSeconds(waitBeforeExplode);

        // 4. 十字爆発を生成
        if (crossExplosionPrefab != null)
        {
            Instantiate(crossExplosionPrefab, transform.position, Quaternion.identity);
        }

        // 5. 消滅
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Kyara"))
        {
            var playerHP = collision.gameObject.GetComponent<hpdesu2>();
            if (playerHP != null) playerHP.TakeDamage(1);
        }
    }
}