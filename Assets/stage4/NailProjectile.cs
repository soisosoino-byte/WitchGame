using UnityEngine;

public class NailProjectile : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 5.0f;

    [Header("エフェクト設定")]
    public GameObject brokenEffectPrefab;

    [Tooltip("砕けた画像が出る位置のズレ（X, Y, Z）。Yをマイナスにすると下にずれます")]
    public Vector3 effectOffset = new Vector3(0, -0.5f, 0); // ★ここを追加！デフォルトで少し下(-0.5)に設定

    public void Setup(Vector3 direction, float speed)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifeTime);
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
            Destroy(gameObject);
        }
        // 地面に当たった場合
        else if (collision.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (brokenEffectPrefab != null)
            {
                // ★修正: 現在の位置(transform.position) に ズレ(effectOffset) を足す
                Vector3 spawnPos = transform.position + effectOffset;

                Instantiate(brokenEffectPrefab, spawnPos, transform.rotation);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
