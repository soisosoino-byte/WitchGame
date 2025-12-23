using UnityEngine;

public class HomingBomb : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 8f; // 飛ぶ速さ
    [Tooltip("誘導性能。0だと直進、数値を上げるとグイグイ曲がって追いかけます")]
    public float homingSensitivity = 1.5f;

    [Header("爆発条件")]
    [Tooltip("このX座標より左に出たら爆発")]
    public float minX = -10f;
    [Tooltip("このX座標より右に出たら爆発")]
    public float maxX = 10f;

    [Header("参照")]
    public GameObject explosionPrefab; // ステップ1で作った爆発プレハブ

    private Transform playerTransform;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // プレイヤーを探す
        GameObject player = GameObject.FindGameObjectWithTag("Kyara");
        if (player != null)
        {
            playerTransform = player.transform;
            // 最初にプレイヤーの方を向いて発射
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.velocity = direction * speed;
        }
        else
        {
            // プレイヤーがいなければ真下に発射
            rb.velocity = Vector2.down * speed;
        }
    }

    void FixedUpdate()
    {
        // 簡易的な誘導処理（現在の進行方向を、徐々にプレイヤーの方向へ曲げていく）
        if (playerTransform != null && homingSensitivity > 0)
        {
            Vector2 targetDirection = (playerTransform.position - transform.position).normalized;
            // 現在の速度ベクトルとターゲット方向のベクトルを混ぜ合わせる（Slerpで滑らかに）
            Vector2 newVelocity = Vector3.Slerp(rb.velocity.normalized, targetDirection, homingSensitivity * Time.fixedDeltaTime);
            rb.velocity = newVelocity * speed;
        }

        // 画像の向きを進行方向に合わせる
        if (rb.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void Update()
    {
        // X座標範囲外のチェック
        if (transform.position.x < minX || transform.position.x > maxX)
        {
            Explode();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 地面(Ground) または プレイヤー(Kyara) に当たったら爆発
        // ※Layerの判定も入れる場合は || collision.gameObject.layer == LayerMask.NameToLayer("Ground") を追加
        if (collision.CompareTag("Ground") || collision.CompareTag("Kyara"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // 爆発エフェクト生成
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        // 自分を削除
        Destroy(gameObject);
    }
}