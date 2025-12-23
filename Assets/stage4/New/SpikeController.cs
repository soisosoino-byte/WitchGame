using System.Collections;
using UnityEngine;

public class SpikeController : MonoBehaviour
{
    // これらの変数は、生成時にボスのスクリプトから上書きされます
    private float moveHeight = 2.0f; // 飛び出す高さ
    private float speed = 5.0f;      // 移動速度
    private float stayTime = 0.5f;   // 飛び出した後の停止時間

    // 初期化メソッド（ボス側から呼び出してもらう）
    public void Initialize(float height, float moveSpeed, float duration)
    {
        moveHeight = height;
        speed = moveSpeed;
        stayTime = duration;

        // 動きを開始
        StartCoroutine(MoveSequence());
    }

    private IEnumerator MoveSequence()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * moveHeight; // 上方向への目標地点

        // 1. 上へ移動（出現）
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            // 現在地から目標地点へ一定速度で移動
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos; // 位置ズレ補正

        // 2. 突き出たまま待機
        yield return new WaitForSeconds(stayTime);

        // 3. 下へ移動（引っ込む）
        while (Vector3.Distance(transform.position, startPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = startPos; // 位置ズレ補正

        // 4. 自分自身を削除
        Destroy(gameObject);
    }
}