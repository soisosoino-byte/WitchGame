using UnityEngine;

public class ShurikenMover : MonoBehaviour
{
    [Header("手裏剣の設定")]
    public float speed = 15f;   // 速さ
    public float lifetime = 3f; // 消えるまでの時間

    [Header("回転の設定")] // ★追加項目
    [Tooltip("回転する速さ（度/秒）。値が大きいほど速く回ります。マイナスで逆回転。")]
    public float rotationSpeed = 1080f; // ★追加: 1秒間に3回転くらいさせる初期値

    [Header("追従（狙い）の調整")]
    [Tooltip("狙いのブレ幅（度数）。数値を上げると狙いがバラけて避けやすくなります")]
    public float aimSpread = 15f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.isKinematic = false;

        // ★追加: ここで回転速度を与えます。あとは物理演算で勝手に回り続けます。
        // ※ angularVelocity は「1秒間に何度回るか」を指定します
        rb.angularVelocity = rotationSpeed;

        GameObject target = GameObject.FindGameObjectWithTag("Kyara");

        if (target != null)
        {
            // 1. 本来の正確な方向を計算
            Vector2 direction = (target.transform.position - transform.position).normalized;

            // 2. ランダムな角度のズレを作る
            float randomAngle = Random.Range(-aimSpread, aimSpread);

            // 3. ズレた角度を元の方向に足す
            Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);
            Vector2 finalDirection = rotation * direction;

            // 4. 速度を与える
            rb.velocity = finalDirection * speed;

            // 【削除】進行方向に向ける処理は、回転させるので削除しました
            // float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
            // transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            rb.velocity = Vector2.right * speed;
        }

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}