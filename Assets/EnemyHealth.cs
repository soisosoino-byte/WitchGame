// EnemyHealth.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移のために追加
using UnityEngine.Events; // ★★★これが必要です！追加してください★★★

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f; // 敵の最大HP
    [SerializeField] private float currentHealth; // 敵の現在のHP (Inspectorで確認用)

    public EnemyHPBar enemyHPBar;

    public float damageAmountPerWazaHit = 10f; // 技1回あたりのダメージ量

    public float damageCooldown = 0.1f;
    private bool canTakeDamage = true;

    private SpriteRenderer enemySpriteRenderer;
    public Color hitColor = new Color(1f, 0.5f, 0.5f, 1f);
    private Color originalColor;
    public float flashDuration = 0.2f;
    public float flashInterval = 0.05f;
    private Coroutine flashCoroutine;

    public GameObject bloodEffectPrefab; // 流血アニメーションのPrefab
    public float bloodEffectDuration = 0.3f; // 流血アニメーションの再生時間

    [Header("Blood Effect Offsets")] // Inspectorでグループ化表示
    public Vector2 bloodEffectOffsetWhenHitFromRight = Vector2.zero; // 右から攻撃された時のオフセット (Xは負の値で設定することも可能)
    public Vector2 bloodEffectOffsetWhenHitFromLeft = Vector2.zero;  // 左から攻撃された時のオフセット (Xは負の値で設定することも可能)

    [Header("Audio Settings")]
    public AudioClip enemyHitSound; // 敵がダメージを受けた時の効果音
    private AudioSource audioSource; // このGameObjectにアタッチされたAudioSource

    // ★★★ここから追加する変数★★★
    [Header("Defeat Settings")]
    public Vector3 defeatMovePosition = new Vector3(0, 0, 0); // やられ時に移動する目標座標
    public float defeatMoveDuration = 0.5f; // やられ移動にかかる時間
    public string defeatAnimationTrigger = "Defeat"; // やられアニメーションのトリガー名
    public float delayBeforeFade = 2.0f; // やられアニメーション再生後の暗転までの待機時間
    public string nextSceneName = "GameOverScene"; // 遷移先のシーン名

    private Animator animator; // アニメーターコンポーネネント
    private EnemyMovementController enemyMovementController; // 敵の移動を停止させるため
    private EnemyAttackManager enemyAttackManager; // 敵の攻撃管理スクリプトへの参照

    private bool isDefeated = false; // ★追加：敵が倒されたことを示すフラグ★

    // ★★★ここまで追加する変数★★★

    void Awake() // Start()からAwake()に変更する方が安全です
    {
        currentHealth = maxHealth;

        enemySpriteRenderer = GetComponent<SpriteRenderer>();
        if (enemySpriteRenderer != null)
        {
            originalColor = enemySpriteRenderer.color;
        }
        else
        {
            Debug.LogWarning("EnemyHealth: SpriteRendererがアタッチされていません。", this);
        }

        if (enemyHPBar == null)
        {
            enemyHPBar = FindObjectOfType<EnemyHPBar>();
            if (enemyHPBar == null)
            {
                Debug.LogError("シーンにEnemyHPBarコンポーネントが見つかりません！");
                this.enabled = false;
                return;
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0;
            audioSource.playOnAwake = false;
        }

        // ★追加：AnimatorとEnemyMovementController, EnemyAttackManagerの取得★
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("EnemyHealth: Animatorコンポーネントが見つかりません。", this);
        }

        enemyMovementController = GetComponent<EnemyMovementController>();
        if (enemyMovementController == null)
        {
            Debug.LogWarning("EnemyHealth: EnemyMovementControllerコンポーネントが見つかりません。", this);
        }

        // EnemyAttackManagerは通常、シーン上の別のゲームオブジェクトにアタッチされているはずなので、FindObjectOfTypeで探す
        // ★修正: FindObjectOfTypeで探すのではなく、GetComponentInChildrenなども検討する。
        // 今回はシンプルにFindObjectOfTypeのままで良いが、アタッチされているGameObjectを確認すること。
        enemyAttackManager = FindObjectOfType<EnemyAttackManager>();
        if (enemyAttackManager == null)
        {
            Debug.LogWarning("EnemyHealth: シーンにEnemyAttackManagerコンポーネントが見つかりません。", this);
        }
        // ★ここまで追加★
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // HPが0以下の場合はダメージを受け付けない
        if (isDefeated) return; // ★追加：倒れていたらダメージを受け付けない★
        if (currentHealth <= 0) return;

        if (other.CompareTag("Waza") && canTakeDamage)
        {
            TakeDamage(damageAmountPerWazaHit);
            StartCoroutine(DamageCooldownCoroutine());

            Vector3 hitPosition = other.bounds.center;
            bool attackedFromLeft = (other.transform.position.x < transform.position.x);
            PlayBloodEffect(hitPosition, attackedFromLeft);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // HPが0以下の場合はダメージを受け付けない
        if (isDefeated) return; // ★追加：倒れていたらダメージを受け付けない★
        if (currentHealth <= 0) return;

        if (collision.gameObject.CompareTag("Waza") && canTakeDamage)
        {
            TakeDamage(damageAmountPerWazaHit);
            StartCoroutine(DamageCooldownCoroutine());

            Vector3 hitPosition = collision.contacts[0].point;
            bool attackedFromLeft = (collision.transform.position.x < transform.position.x);
            PlayBloodEffect(hitPosition, attackedFromLeft);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDefeated) return; // ★追加：倒れていたらダメージを受け付けない★

        float oldHealth = currentHealth;
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        if (currentHealth < oldHealth)
        {
            PlaySound(enemyHitSound);

            if (enemyHPBar != null)
            {
                enemyHPBar.UpdateHP(currentHealth);
            }

            Debug.Log($"Enemy took {damage} damage. Current HP: {currentHealth}");

            if (enemySpriteRenderer != null)
            {
                if (flashCoroutine != null)
                {
                    StopCoroutine(flashCoroutine);
                    enemySpriteRenderer.color = originalColor;
                }
                flashCoroutine = StartCoroutine(FlashColorCoroutine());
            }
        }

        if (currentHealth <= 0)
        {
            if (isDefeated) return; // ★再確認：既に死亡処理が開始されていたら重複しないようにする★
            isDefeated = true; // ★フラグを立てる★

            // 攻撃マネージャーの行動を停止
            if (enemyAttackManager != null)
            {
                enemyAttackManager.StopAttackBehavior();
            }

            // 敵自身の移動コルーチンを停止
            if (enemyMovementController != null)
            {
                enemyMovementController.StopAllCoroutines(); // ★全てのコルーチンを停止させる★
                // 移動コルーチンが停止したら、Rigidbodyの速度もリセットして、完全に動きを止める
                Rigidbody2D rb = enemyMovementController.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0;
                }
                enemyMovementController.SetIdle(); // アニメーションをIdleに戻しておく (やられアニメーションへの遷移準備)
            }

            // 点滅コルーチンを停止し、色を元に戻す
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                enemySpriteRenderer.color = originalColor;
            }

            // Colliderを無効化してこれ以上ダメージを受けないようにする
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // HPが0になったら死亡処理シーケンスを開始
            StartCoroutine(DieSequence());
            this.enabled = false; // このスクリプト自体を無効にする (念のため、これ以上Updateなどが呼ばれないように)
        }
    }

    // ★★★ここからDie()メソッドをコルーチンとして再定義★★★
    private IEnumerator DieSequence()
    {
        Debug.Log("Enemy has been defeated! Initiating defeat sequence.");

        // 1. 敵を特定の座標へ移動させる (アニメーションは行わず、純粋に位置移動のみ)
        if (enemyMovementController != null)
        {
            if (defeatMoveDuration > 0)
            {
                Debug.Log($"敵がやられ座標 {defeatMovePosition} へ移動開始。");
                // 移動中にアニメーションを再生しないように、空のトリガー名を渡す
                yield return StartCoroutine(enemyMovementController.SetTriggerAndMoveToPosition(
                    defeatMovePosition,
                    defeatMoveDuration,
                    "" // ★変更：移動中はアニメーションを再生しない★
                ));
            }
            else // 移動時間が0の場合、即座に位置を設定
            {
                transform.position = defeatMovePosition;
                Debug.Log($"敵がやられ座標 {defeatMovePosition} へ即座に移動しました。");
            }
        }
        else
        {
            Debug.LogWarning("EnemyMovementControllerが設定されていません。やられ時の移動をスキップします。", this);
        }

        // 2. やられアニメーションを再生する (移動が完了してから)
        if (animator != null && !string.IsNullOrEmpty(defeatAnimationTrigger))
        {
            animator.SetTrigger(defeatAnimationTrigger);
            Debug.Log($"やられアニメーション '{defeatAnimationTrigger}' を再生します。(移動完了後)");
        }
        else
        {
            Debug.LogWarning("Animatorまたはやられアニメーションのトリガーが設定されていません。やられアニメーションをスキップします。", this);
        }

        // 3. 指定された時間待機 (アニメーションの再生時間も考慮に入れる)
        Debug.Log($"やられアニメーション再生後 {delayBeforeFade} 秒待機します...");
        yield return new WaitForSeconds(delayBeforeFade);

        // 4. 画面全体を暗転させる
        Debug.Log("画面を暗転させます...");
        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(FadeManager.Instance.FadeOut()); // フェードアウト処理
        }

        else
        {
            Debug.LogError("シーンにFadeManagerのインスタンスが見つかりません！フェードアウトをスキップします。");
        }
        Debug.Log("画面を暗転させます...");
        // ★変更: ScreenFadeManager を使うように書き換え
        if (ScreenFadeManager.Instance != null)
        {
            // ここで一連の流れ（暗転→移動→明転）をお任せする
            ScreenFadeManager.Instance.ChangeScene(nextSceneName);
        }
        else
        {
            Debug.LogError("シーンにScreenFadeManagerのインスタンスが見つかりません！");
        }

        // 5. 別のシーンに移動させる
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"次のシーン '{nextSceneName}' へ遷移します。");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("遷移先のシーン名が設定されていません。", this);
            // シーン遷移がない場合、敵オブジェクトを破棄 (通常はシーン遷移するので不要)
            Destroy(gameObject);
        }
    }
    // ★★★ここまでDie()メソッドをコルーチンとして再定義★★★

    IEnumerator DamageCooldownCoroutine()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    IEnumerator FlashColorCoroutine()
    {
        float timer = 0f;
        while (timer < flashDuration)
        {
            enemySpriteRenderer.color = (enemySpriteRenderer.color == originalColor) ? hitColor : originalColor;

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        enemySpriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private void PlayBloodEffect(Vector3 position, bool isAttackedFromLeft)
    {
        if (bloodEffectPrefab != null)
        {
            Vector3 spawnPosition;
            Vector3 effectLocalScale = bloodEffectPrefab.transform.localScale;

            if (isAttackedFromLeft)
            {
                spawnPosition = position + (Vector3)bloodEffectOffsetWhenHitFromLeft;
                effectLocalScale.x = Mathf.Abs(effectLocalScale.x);
            }
            else
            {
                spawnPosition = position + (Vector3)bloodEffectOffsetWhenHitFromRight;
                effectLocalScale.x = -Mathf.Abs(effectLocalScale.x);
            }

            GameObject bloodEffect = Instantiate(bloodEffectPrefab, spawnPosition, Quaternion.identity);
            bloodEffect.transform.localScale = effectLocalScale;
            Destroy(bloodEffect, bloodEffectDuration);
        }
        else
        {
            Debug.LogWarning("Blood Effect Prefabが設定されていません。", this);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}