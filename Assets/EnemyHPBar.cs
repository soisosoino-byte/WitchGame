using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI要素を扱うために必要

public class EnemyHPBar : MonoBehaviour
{
    // ★Inspectorで設定するUI要素への参照★
    public Image hpBarFillImage; // HPバーの伸び縮みする部分 (Image Type: Filled, Horizontal, Fill Origin: Left)

    // ★HPバーの最大・現在の値★
    [SerializeField] private float maxHP = 100f; // 最大HP（Inspectorで変更可能）
    [SerializeField] private float currentHP; // 現在のHP（Inspectorで確認用）

    // ★シーン開始時のアニメーション設定★
    public float initialAnimationDuration = 3.0f; // 0からMaxHPまで伸びる時間
    // ★追加：アニメーション開始前の遅延時間★
    public float initialAnimationDelay = 2.0f; // アニメーション開始前の待機時間（秒）

    // ★HP減少時の振動設定★
    public float shakeDurationOnDamage = 0.3f; // 減少時の振動時間
    public float shakeMagnitude = 5.0f; // 振動の強さ（ピクセル単位）
    public float shakeSpeed = 0.05f; // 振動の切り替え速度（秒）

    private RectTransform hpBarRectTransform; // HPバーのRectTransform
    private Vector2 originalPosition; // HPバーの元の位置を保存
    private Coroutine shakeCoroutine; // 振動コルーチンへの参照

    void Awake()
    {
        // ImageコンポーネントとRectTransformを取得
        if (hpBarFillImage == null)
        {
            hpBarFillImage = GetComponent<Image>();
            if (hpBarFillImage == null)
            {
                Debug.LogError("HPBarFillImageが設定されていません。Imageコンポーネントがアタッチされているか確認してください。", this);
                this.enabled = false;
                return;
            }
        }
        hpBarRectTransform = hpBarFillImage.GetComponent<RectTransform>();
        if (hpBarRectTransform != null)
        {
            originalPosition = hpBarRectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogError("HPバーにRectTransformがありません。", this);
            this.enabled = false;
            return;
        }

        // 初期状態を必ず0にしておく
        currentHP = 0f;
        hpBarFillImage.fillAmount = 0f;
    }

    void Start()
    {
        // ★修正：コルーチンに遅延時間を渡すのではなく、コルーチン内で遅延させる★
        StartCoroutine(AnimateHPBarAfterDelay(initialAnimationDelay, initialAnimationDuration));
    }

    // HPバーの表示を更新する外部から呼び出すメソッド
    public void UpdateHP(float newHP)
    {
        float oldHP = currentHP;
        currentHP = Mathf.Clamp(newHP, 0f, maxHP);

        hpBarFillImage.fillAmount = currentHP / maxHP;

        // HPが減少した場合のみ振動させる
        if (currentHP < oldHP)
        {
            StartShake();
        }

        Debug.Log($"Enemy HP Updated: {currentHP}/{maxHP}");
    }

    // HP減少時の振動を開始するメソッド
    private void StartShake()
    {
        if (hpBarRectTransform == null) return;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        hpBarRectTransform.anchoredPosition = originalPosition;

        shakeCoroutine = StartCoroutine(ShakeEffect());
    }

    // 振動処理を行うコルーチン
    IEnumerator ShakeEffect()
    {
        float elapsed = 0f;

        while (elapsed < shakeDurationOnDamage)
        {
            float x = originalPosition.x + Random.Range(-0.2f, 0.2f) * shakeMagnitude;
            float y = originalPosition.y + Random.Range(-0.3f, 0.3f) * shakeMagnitude;

            hpBarRectTransform.anchoredPosition = new Vector2(x, y);

            yield return new WaitForSeconds(shakeSpeed);

            elapsed += shakeSpeed;
        }

        hpBarRectTransform.anchoredPosition = originalPosition;
        shakeCoroutine = null;
    }

    // ★追加・修正：遅延後にHPバーをアニメーションさせるコルーチン★
    IEnumerator AnimateHPBarAfterDelay(float delayDuration, float animationDuration)
    {
        // まず指定された遅延時間待機
        Debug.Log($"HPバーのアニメーションを {delayDuration} 秒後に開始します。");
        yield return new WaitForSeconds(delayDuration);

        Debug.Log("HPバーのアニメーションを開始しました。");

        float timer = 0f;
        // currentHPとfillAmountはAwake()で0に初期化されている
        while (timer < animationDuration)
        {
            float t = timer / animationDuration;
            currentHP = Mathf.Lerp(0f, maxHP, t);
            hpBarFillImage.fillAmount = currentHP / maxHP;

            timer += Time.deltaTime;
            yield return null;
        }

        currentHP = maxHP;
        hpBarFillImage.fillAmount = 1f;

        Debug.Log("Enemy HP Bar initial animation complete. HP: " + currentHP);
    }

    // HPバーの最大値を設定する（必要であれば外部から呼び出す）
    public void SetMaxHP(float newMaxHP)
    {
        maxHP = newMaxHP;
        UpdateHP(currentHP);
    }
}