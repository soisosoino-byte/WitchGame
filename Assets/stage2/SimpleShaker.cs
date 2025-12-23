using UnityEngine;
using System.Collections;

public class SimpleShaker : MonoBehaviour
{
    // カメラの「Matrix（投影盤）」を揺らす裏技
    // これならTransform（位置）をいじらないので、追従スクリプトと喧嘩しない！

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeMatrix(duration, magnitude));
    }

    IEnumerator ShakeMatrix(float duration, float magnitude)
    {
        Camera cam = GetComponent<Camera>();
        Matrix4x4 originalMatrix = cam.projectionMatrix;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude * 0.1f; // 少し弱めにする
            float y = Random.Range(-1f, 1f) * magnitude * 0.1f;

            Matrix4x4 newMatrix = originalMatrix;
            newMatrix.m03 += x; // X軸のズレ
            newMatrix.m13 += y; // Y軸のズレ

            cam.projectionMatrix = newMatrix;

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.projectionMatrix = originalMatrix; // 元に戻す
    }
}
