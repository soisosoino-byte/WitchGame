using UnityEngine;
using System.Collections;

public class MinionBehavior : MonoBehaviour
{
    [Header("ステータス")]
    public int maxHP = 3;
    private int currentHP;

    [Header("移動設定")]
    public float moveSpeed = 3.0f;
    public int direction = -1; // -1:左, 1:右

    [Header("攻撃設定")]
    public float attackRange = 1.5f; // プレイヤーを感知する距離
    public float attackDuration = 1.0f; // 攻撃モーションの時間
    public float attackCooldown = 2.0f; // 次の攻撃までの待機時間

    [Header("ダメージ演出")]
    public Color damageColor = new Color(1f, 0.5f, 0.5f, 1f); // 赤色
    public float flashDuration = 0.1f;

    [Header("アニメーション名")]
    public string animMove = "Move";
    public string animAttack = "Attack";

    private bool isAttacking = false; // 攻撃中フラグ
    private bool isDead = false;      // 死亡フラグ

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Transform playerTransform;

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        // プレイヤーを探しておく
        GameObject player = GameObject.FindGameObjectWithTag("Kyara");
        if (player != null) playerTransform = player.transform;

        // 最初のアニメーション
        if (animator != null) animator.Play(animMove);
    }

    void Update()
    {
        if (isDead) return;

        // 攻撃中は何もしない（移動も停止）
        if (isAttacking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 1. プレイヤーとの距離チェック
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            // プレイヤーが近くにいて、かつ攻撃クールダウン中でなければ攻撃開始
            if (distance <= attackRange)
            {
                StartCoroutine(AttackSequence());
                return; // ここで処理を中断（移動させない）
            }
        }

        // 2. 横移動
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        // 向きの反転（画像の向き）
        if (direction > 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1); // 右向き
        else transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);  // 左向き

        // 移動アニメーション維持
        // (Animatorがステートマシンなら SetBool("IsMoving", true) などが良いですが、今回はPlayで管理します)
    }

    // 攻撃の一連の流れ
    IEnumerator AttackSequence()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero; // 完全に停止

        // 攻撃アニメーション再生
        if (animator != null) animator.Play(animAttack);
        Debug.Log("雑魚: 攻撃開始！");

        // アニメーション再生中待機
        yield return new WaitForSeconds(attackDuration);

        // クールタイム（棒立ちの時間）
        if (animator != null) animator.Play(animMove); // 待機モーションがあればそれに変える
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false; // 移動再開
    }

    // 壁に当たったら反転
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // "Kabe" タグに当たったら方向転換
        if (collision.gameObject.CompareTag("Kabe"))
        {
            direction *= -1; // 1なら-1に、-1なら1にする
            Debug.Log("壁に当たったので反転します");
        }
    }

    // ダメージ判定（技に当たった時）
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // "Waza" タグに当たったらダメージ
        if (collision.CompareTag("Waza"))
        {
            TakeDamage(1);
        }
    }

    void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"雑魚HP: {currentHP}");

        // 赤く点滅
        StartCoroutine(FlashRed());

        if (currentHP <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false; // 当たり判定を消す

        // やられアニメーションがあればここで再生
        // animator.Play("Die");

        Debug.Log("雑魚を倒した！");
        Destroy(gameObject, 0.5f); // 0.5秒後に消滅
    }
}