using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 0.5f;

    // ★追加: 移動速度（0なら動かない）
    [Header("移動設定")]
    public float speed = 0f;

    // 移動方向（初期値は右）
    private Vector3 moveDirection = Vector3.right;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // ★追加: 設定された速度で移動する
        if (speed > 0)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }
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

    // ★追加: 外部から方向と速度をセットする関数
    public void SetupMove(Vector3 direction, float moveSpeed)
    {
        moveDirection = direction;
        speed = moveSpeed;

        // 画像の向きも合わせる（左に進むなら反転）
        if (direction.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}