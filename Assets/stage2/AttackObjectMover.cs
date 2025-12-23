using UnityEngine;

public class AttackObjectMover : MonoBehaviour
{
    [Header("設定")]
    public float moveSpeed = 5.0f;    // 移動する速さ
    public float lifeTime = 3.0f;     // 消えるまでの時間（秒）
    public Vector3 moveDirection = Vector3.left; // 移動する方向（左ならVector3.left）

    void Start()
    {
        // 生成されてから lifeTime 秒後に自動的に消滅させる
        Destroy(this.gameObject, lifeTime);
    }

    void Update()
    {
        // 毎フレーム指定した方向に移動させる
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    // 当たり判定（プレイヤーに当たったら...などの処理はここに追加）
    // 当たり判定（何かに触れたら呼ばれる）
    void OnTriggerEnter2D(Collider2D collision)
    {
        // ぶつかった相手のタグが "Kyara" かどうか確認
        if (collision.CompareTag("Kyara"))
        {
            Debug.Log("キャラに命中！弾を消します。");

            // 自分自身（この弾）を破壊して消す
            Destroy(this.gameObject);
        }
    }
}