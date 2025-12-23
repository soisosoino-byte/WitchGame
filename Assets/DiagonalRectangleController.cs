using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UI要素を扱うために必要

public class DiagonalRectangleController : MonoBehaviour
{
    // ★Inspectorで設定するUI Imageへの参照★
    public Image diagonalImage; // 斜め四角形のImageコンポーネント

    // ★表示・移動設定★
    public float displayDuration = 4.0f; // 斜め四角形を表示する時間
    public float moveOutDuration = 0.5f; // 画面外へ移動する時間
    public Vector2 moveOutDirection = new Vector2(1f, 0f); // 画面外へ移動する方向 (例: 右へなら (1,0), 上へなら (0,1))
                                                           // (1,0)は画面右へ、(-1,0)は画面左へ、(0,1)は画面上へ

    private RectTransform rectTransform; // 四角形のRectTransform
    private Vector2 originalPosition; // 四角形の初期位置を保存

    void Awake()
    {
        // ImageコンポーネントとRectTransformを取得
        if (diagonalImage == null)
        {
            diagonalImage = GetComponent<Image>();
            if (diagonalImage == null)
            {
                Debug.LogError("DiagonalImageが設定されていません。Imageコンポーネントがアタッチされているか確認してください。", this);
                this.enabled = false;
                return;
            }
        }
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition; // 現在のRectTransformの位置を保存
        }
        else
        {
            Debug.LogError("DiagonalRectangleにRectTransformがありません。", this);
            this.enabled = false;
            return;
        }

        // 初期状態：完全に透明
        Color initialColor = diagonalImage.color;
        initialColor.a = 0f;
        diagonalImage.color = initialColor;
    }

    void Start()
    {
        // シーン開始時に処理を開始
        StartCoroutine(ManageDiagonalRectangle());
    }

    IEnumerator ManageDiagonalRectangle()
    {
        // 四角形を表示 (Alphaを1にする)
        Color targetColor = diagonalImage.color;
        targetColor.a = 1f;
        diagonalImage.color = targetColor;

        Debug.Log($"斜め四角形を {displayDuration} 秒間表示します。");
        // 指定された時間表示
        yield return new WaitForSeconds(displayDuration);

        Debug.Log($"斜め四角形を {moveOutDuration} 秒かけて画面外へ移動させます。");

        // 画面外への移動アニメーション
        float timer = 0f;
        Vector2 startPosition = rectTransform.anchoredPosition; // 現在の位置
        // 画面外への移動距離を計算 (画面の対角線の長さなどを基準にすると確実)
        // ここでは、RectTransformのサイズと移動方向に応じて十分な距離を設定
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        // 画面の対角線を利用して十分な距離を確保
        float maxMoveDistance = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);

        Vector2 endPosition = startPosition + moveOutDirection.normalized * maxMoveDistance; // 移動方向へ十分な距離移動

        while (timer < moveOutDuration)
        {
            float t = timer / moveOutDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);

            // オプション: 移動しながら透明度を徐々に下げる
            // Color currentColor = diagonalImage.color;
            // currentColor.a = Mathf.Lerp(1f, 0f, t);
            // diagonalImage.color = currentColor;

            timer += Time.deltaTime;
            yield return null;
        }

        // 移動完了後、最終的に非表示にする
        diagonalImage.color = new Color(diagonalImage.color.r, diagonalImage.color.g, diagonalImage.color.b, 0f);
        gameObject.SetActive(false); // GameObjectを非アクティブにするか、Destroy(gameObject); で削除
        Debug.Log("斜め四角形の処理が完了しました。");
    }
}