using UnityEngine;
using System.Collections;

public class BouncingObject : MonoBehaviour
{
    public enum ObjectType
    {
        Damage, // 障害物
        Heal    // 回復アイテム
    }

    [Header("設定")]
    public ObjectType type = ObjectType.Damage; // 種類設定
    public float rotationSpeed = 360f; // 回転速度
    public int damageAmount = 1;       // ダメージ量
    public int healAmount = 1;         // 回復量

    [Header("消滅設定")] // ★追加項目
    [Tooltip("射出されてから完全に消えるまでの合計時間")]
    public float lifeTime = 5.0f;
    [Tooltip("消える直前に小さくなるアニメーションの時間")]
    public float shrinkDuration = 0.5f;

    private Rigidbody2D rb;
    private Vector3 originalScale; // 最初の大きさを覚えておく

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale; // 開始時の大きさを保存

        // コルーチン（寿命の管理）を開始
        StartCoroutine(LifeCycleSequence());
    }

    void Update()
    {
        // くるくる回転させる
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    // ★追加: 生成から消滅までの流れ
    IEnumerator LifeCycleSequence()
    {
        // 1. 「小さくなり始める時間」まで待つ
        // (合計寿命 - 縮小にかかる時間) = 待機時間
        float waitTime = Mathf.Max(0, lifeTime - shrinkDuration);
        yield return new WaitForSeconds(waitTime);

        // 2. 徐々に小さくする（シュッと消える演出）
        float timer = 0f;
        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            // 1(元の大きさ) から 0(消滅) へ滑らかに変化させる
            float progress = timer / shrinkDuration;
            float scaleMultiplier = Mathf.Lerp(1f, 0f, progress);

            transform.localScale = originalScale * scaleMultiplier;
            yield return null; // 1フレーム待つ
        }

        // 3. 完全に消す
        transform.localScale = Vector3.zero; // 見た目を完全に消す
        Destroy(gameObject); // オブジェクトを削除
    }

    // プレイヤーに当たった時の処理
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kyara"))
        {
            hpdesu2 playerHP = collision.GetComponent<hpdesu2>();
            if (playerHP != null)
            {
                if (type == ObjectType.Damage)
                {
                    playerHP.TakeDamage(damageAmount);
                }
                else if (type == ObjectType.Heal)
                {
                    playerHP.Heal(healAmount);
                }
            }
            // プレイヤーに当たった場合は即座に消す
            Destroy(gameObject);
        }
    }
}