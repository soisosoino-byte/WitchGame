using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // ★追加: シーン遷移のために必要★
using UnityEngine.UI; // ★追加: 暗転用UIのために必要★

public class hpdesu2 : MonoBehaviour
{
    public hpdesu hpGaugeController;

    // ★追加：HP管理のための変数★
    public int maxHP = 5;       // 最大HP
    public int currentHP;       // 現在のHP
    private bool isDead = false; // ★追加: プレイヤーが死亡状態かを示すフラグ★

    // ★追加：無敵状態管理のための変数★
    private bool isInvincible = false; // 無敵フラグ
    public float invincibleDuration = 0.6f; // 無敵時間（Inspectorで設定可能）

    // ★追加：キャラクターの点滅処理のための変数★
    private SpriteRenderer characterSpriteRenderer; // キャラクター自身のSpriteRenderer
    private Coroutine invincibilityVisualCoroutine; // 点滅コルーチンの参照
    private Color originalColor;

    // ★★★ここからサウンド関連の追加★★★
    [Header("Audio Settings")]
    public AudioClip damageSound; // ダメージを受けた時の効果音
    public AudioClip healSound;   // ★追加: 回復した時の効果音
    private AudioSource audioSource; // このGameObjectにアタッチされたAudioSource
    // ★★★ここまでサウンド関連の追加★★★

    // ★★★ここから敗北・ゲームオーバー関連の追加★★★
    [Header("Defeat & Game Over Settings")]
    public Animator playerAnimator; // プレイヤーのAnimatorコンポーネント (Inspectorで設定)
    public string defeatAnimationTrigger = "Defeat"; // 敗北アニメーションのトリガー名

    public float disableControlDuration = 3.0f; // 敗北アニメーション再生と操作無効化の継続時間
    public Image fadePanel; // 暗転用のUI Image (Canvasの子要素に配置、Inspectorで設定)
    public float fadeDuration = 0.5f; // 暗転にかかる時間 (秒)
    public string gameOverSceneName = "GameOverScene"; // ゲームオーバー画面のシーン名

    // プレイヤーの操作を制御するスクリプトへの参照
    private control playerControl;
    // ★★★ここまで敗北・ゲームオーバー関連の追加★★★


    void Awake() // StartからAwakeに変更：他のスクリプトがStartで参照する場合に備える
    {
        // hpGaugeControllerの初期化はStartで行う（FindObjectOfTypeはシーンが完全にロードされてからの方が安全な場合がある）
        // キャラクター自身のSpriteRendererを取得
        characterSpriteRenderer = GetComponent<SpriteRenderer>();
        if (characterSpriteRenderer == null)
        {
            Debug.LogError("このGameObjectにはSpriteRendererがアタッチされていません！キャラクターにSpriteRendererがあるか確認してください。");
        }
        else
        {
            originalColor = characterSpriteRenderer.color; // キャラクターの元の色を保存する
        }

        // ★追加：AudioSourceコンポーネントの取得★
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0;
            audioSource.playOnAwake = false;
        }

        // プレイヤーのAnimatorコンポーネントの取得
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }
        if (playerAnimator == null)
        {
            Debug.LogWarning("PlayerHealth: Animatorコンポーネントが見つかりません。プレイヤーにAnimatorがあるか確認してください。", this);
        }

        // プレイヤーのコントロールスクリプトの参照を取得
        playerControl = GetComponent<control>();
        if (playerControl == null)
        {
            Debug.LogError("PlayerHealth: controlスクリプトが見つかりません。同じGameObjectにアタッチされていますか？", this);
        }

        // 暗転パネルの初期設定
        if (fadePanel == null)
        {
            Debug.LogError("PlayerHealth: Fade Panel (UI Image)が設定されていません。Canvasの子要素に黒いImageを設定し、Inspectorで参照してください。", this);
        }
        else
        {
            // 初期状態では透明にしておく (ただし、Inspectorで初期α=0にしておくのが推奨)
            // ここでコードで設定する場合、初期状態が不透明だと一瞬表示される可能性あり
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 0);
            fadePanel.gameObject.SetActive(false); // 最初は非アクティブにしておく
        }
    }
    private PlayerDash dashScript;
    void Start()
    {
        // ゲーム開始時にhpGaugeControllerが設定されているか確認
        if (hpGaugeController == null)
        {
            hpGaugeController = FindObjectOfType<hpdesu>();
            if (hpGaugeController == null)
            {
                Debug.LogError("HP Gauge Controller (hpdesu) がシーンに見つかりませんでした。Inspectorで設定してください！");
            }
        }

        // 初期HPを設定し、HPゲージの画像を更新
        currentHP = maxHP;
        if (hpGaugeController != null)
        {
            hpGaugeController.UpdateHPImage(currentHP); // 現在のHPで画像を更新
        }
        dashScript = GetComponent<PlayerDash>();
    }

    void Update()
    {
        // Updateはここでは特に変更なし
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 死亡状態ならダメージを受け付けない
        if (isDead)
        {
            return;
        }
        // 無敵状態ならダメージを受け付けない
        if (isInvincible)
        {
            Debug.Log("無敵時間中のためダメージを受けません。");
            return; // 無敵中なので処理を中断
        }

        // Tagが"Enemy"または"Hidan"のオブジェクトに接触した場合にダメージを与える
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Hidan"))
        {
            TakeDamage(1); // HPを1減らす
            Debug.Log($"キャラクターが{(collision.gameObject.CompareTag("Enemy") ? "敵" : "Hidan")}と衝突！現在のHP: {currentHP}");
            // 必要に応じて、衝突したオブジェクトを消滅させる場合は以下を有効に
            // Destroy(collision.gameObject);
        }
    }

    // ★★★ここを追加してください★★★
    // Trigger（弾などのすり抜ける当たり判定）と接触した時の処理
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 死亡・無敵チェック
        if (isDead || isInvincible) return;

        // "Enemy" または "Hidan" タグならダメージ
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Hidan"))
        {
            TakeDamage(1);
            Debug.Log("Trigger判定でダメージを受けました！");
        }
    }
    // ★★★ここまで★★★

    // ★変更点：HPを減らすメソッド（ここに効果音再生を追加）★
    public void TakeDamage(int amount)
    {
        // 死亡・無敵チェック
        if (isDead)
        {
            Debug.Log("死亡済みのためダメージ無効");
            return;
        }
        if (dashScript != null && dashScript.isDashing)
        {
            return;
        }
        if (isInvincible)
        {
            Debug.Log("無敵状態のためダメージを無効化しました");
            return;
        }

        // ★追加: デバッグログ
        Debug.Log("TakeDamage呼び出し: ダメージ量 " + amount + " / 現在HP " + currentHP);

        // ダメージ音
        if (damageSound != null) PlaySound(damageSound);

        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        // HPゲージ更新（安全策を追加）
        if (hpGaugeController != null)
        {
            hpGaugeController.UpdateHPImage(currentHP);
        }
        else
        {
            Debug.LogWarning("HP Gauge Controllerが設定されていません！Inspectorで確認してください。");
        }

        if (currentHP <= 0)
        {
            StartCoroutine(HandlePlayerDefeat());
        }
        else
        {
            StartInvincibility();
        }
    }
    // ★追加: 回復メソッド
    public void Heal(int amount)
    {
        if (isDead) return;

        int oldHP = currentHP;
        currentHP += amount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        if (currentHP > oldHP)
        {
            // 回復音再生
            PlaySound(healSound);
            Debug.Log($"回復しました！ 現在のHP: {currentHP}");

            // HPゲージ更新
            if (hpGaugeController != null)
            {
                hpGaugeController.UpdateHPImage(currentHP);
            }
        }
    }

    // ★追加：無敵状態を開始するメソッド★
    void StartInvincibility()
    {
        isInvincible = true; // 無敵フラグをON
        Debug.Log("無敵状態開始！");

        // キャラクターの点滅処理を開始
        StartInvincibilityVisual(invincibleDuration);

        // 指定した時間後に無敵状態を解除するコルーチンを開始
        StartCoroutine(EndInvincibilityAfterDelay(invincibleDuration));
    }

    // ★追加：無敵状態を指定時間後に解除するコルーチン★
    IEnumerator EndInvincibilityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // delay秒待つ
        isInvincible = false; // 無敵フラグをOFF
        Debug.Log("無敵状態終了。");
    }

    // ★追加：キャラクターの点滅処理を開始するメソッド★
    void StartInvincibilityVisual(float duration)
    {
        if (characterSpriteRenderer == null) return;

        // 既に点滅コルーチンが動いていたら停止（新しいダメージでリセットするため）
        if (invincibilityVisualCoroutine != null)
        {
            StopCoroutine(invincibilityVisualCoroutine);
        }
        invincibilityVisualCoroutine = StartCoroutine(InvincibilityVisualEffect(duration));
    }

    // ★追加：赤くするのと点滅処理を行うコルーチン★
    IEnumerator InvincibilityVisualEffect(float duration)
    {
        float timer = 0f;
        float blinkInterval = 0.1f; // 点滅の間隔
        Color invincibleColor = new Color(1f, 0f, 0f, 0.6f); // 赤色で透明度60%

        while (timer < duration)
        {
            if (characterSpriteRenderer != null)
            {
                // 点滅と色の変更を交互に行う
                if (characterSpriteRenderer.enabled) // 現在表示状態なら
                {
                    characterSpriteRenderer.enabled = false; // 非表示（点滅）
                }
                else // 現在非表示状態なら
                {
                    characterSpriteRenderer.enabled = true; // 表示
                    characterSpriteRenderer.color = invincibleColor; // 赤くする
                }
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // 無敵時間終了後に元の表示状態と色に戻す
        if (characterSpriteRenderer != null)
        {
            characterSpriteRenderer.enabled = true; // 必ず表示状態に戻す
            characterSpriteRenderer.color = originalColor; // 元の色に戻す
        }
        invincibilityVisualCoroutine = null;
    }

    // ★★★ここからサウンド再生用のヘルパーメソッド追加★★★

    // 一度だけ再生するサウンド
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    // ★★★ここまでサウンド再生用のヘルパーメソッド追加★★★


    // ★★★ここから敗北・ゲームオーバー関連のメソッド追加★★★

    /// <summary>
    /// プレイヤーの敗北処理
    /// </summary>
    private IEnumerator HandlePlayerDefeat()
    {
        isDead = true; // 死亡フラグを立てる
        Debug.Log("プレイヤーが倒れました！");

        // 進行中の無敵コルーチンがあれば停止
        if (invincibilityVisualCoroutine != null)
        {
            StopCoroutine(invincibilityVisualCoroutine);
            invincibilityVisualCoroutine = null;
        }
        if (characterSpriteRenderer != null)
        {
            characterSpriteRenderer.enabled = true; // 点滅を停止し、表示状態に戻す
            characterSpriteRenderer.color = originalColor; // 元の色に戻す
        }

        // 1. 全ての操作を数秒間無効化
        SetPlayerControlsEnabled(false);
        Debug.Log("プレイヤーの操作を無効にしました。");

        // 2. 敗北アニメーションを再生
        if (playerAnimator != null && !string.IsNullOrEmpty(defeatAnimationTrigger))
        {
            // ★修正: HasParameterの代わりにTryGetAnimatorParameterを使用★
            if (TryGetAnimatorParameter(playerAnimator, defeatAnimationTrigger, AnimatorControllerParameterType.Trigger))
            {
                // 他のすべてのアニメーショントリガーをリセットしておく（任意）
                // 例: TryResetAnimatorTrigger(playerAnimator, "Move"); TryResetAnimatorTrigger(playerAnimator, "Attack");
                playerAnimator.SetTrigger(defeatAnimationTrigger);
                Debug.Log($"プレイヤーAnimator: Trigger '{defeatAnimationTrigger}' を設定しました。");
            }
            else
            {
                Debug.LogWarning($"PlayerHealth: Animatorにトリガー '{defeatAnimationTrigger}' が見つかりません。", this);
            }
        }
        else
        {
            Debug.LogWarning("PlayerHealth: プレイヤーAnimatorまたは敗北アニメーションのトリガーが設定されていません。", this);
        }

        // 3. 数秒間、アニメーション再生と操作無効化を継続
        yield return new WaitForSeconds(disableControlDuration);
        Debug.Log("操作無効期間が終了しました。");

        // 4. 暗転処理
        yield return StartCoroutine(FadeToBlack());

        // 5. ゲームオーバー画面へ移行
        SceneManager.LoadScene(gameOverSceneName);
    }

    /// <summary>
    /// プレイヤーの操作を有効/無効にする
    /// controlスクリプトのenabledプロパティを切り替える
    /// </summary>
    /// <param name="enabled">trueで有効、falseで無効</param>
    private void SetPlayerControlsEnabled(bool enabled)
    {
        if (playerControl != null)
        {
            playerControl.enabled = enabled;
            // 操作無効時は移動音も停止
            if (!enabled)
            {
                playerControl.StopMoveSound();
            }
        }
    }

    /// <summary>
    /// 画面を暗転させるコルーチン
    /// </summary>
    private IEnumerator FadeToBlack()
    {
        if (fadePanel == null)
        {
            Debug.LogError("Fade Panelが設定されていません。暗転できません。");
            yield break;
        }

        fadePanel.gameObject.SetActive(true); // パネルをアクティブにする
        Color panelColor = fadePanel.color;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            panelColor.a = Mathf.Lerp(0, 1, timer / fadeDuration); // 透明から不透明へ
            fadePanel.color = panelColor;
            yield return null;
        }
        panelColor.a = 1; // 完全に不透明にする
        fadePanel.color = panelColor;
        Debug.Log("画面が完全に暗転しました。");
    }

    // プレイヤーが死亡状態かチェックするプロパティ (他のスクリプトから参照するため)
    public bool IsDead()
    {
        return isDead;
    }

    /// <summary>
    /// Animatorの指定された名前と型のパラメータが存在するかどうかをチェックするヘルパーメソッド
    /// </summary>
    private bool TryGetAnimatorParameter(Animator anim, string name, AnimatorControllerParameterType type)
    {
        if (anim == null || string.IsNullOrEmpty(name)) return false;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == name && param.type == type)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Animatorの指定されたトリガーをリセットするヘルパーメソッド (存在する場合のみ)
    /// </summary>
    private void TryResetAnimatorTrigger(Animator anim, string triggerName)
    {
        if (anim != null && !string.IsNullOrEmpty(triggerName))
        {
            if (TryGetAnimatorParameter(anim, triggerName, AnimatorControllerParameterType.Trigger))
            {
                anim.ResetTrigger(triggerName);
            }
        }
    }
    // ★★★ここまで敗北・ゲームオーバー関連のメソッド追加★★★
}
