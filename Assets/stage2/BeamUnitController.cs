using UnityEngine;
using System.Collections;

public class BeamUnitController : MonoBehaviour
{
    [Header("移動設定")]
    public Vector3 targetPosition;
    public float moveDuration = 2.0f;

    [Header("画面揺れ設定")]
    public float shakeMagnitude = 0.1f;

    [Header("画像切り替え")]
    public Sprite attackSprite;
    private SpriteRenderer spriteRenderer;

    [Header("ビーム設定")]
    public GameObject beamPrefab;
    public Transform rotator;
    public Transform firePoint;
    [Range(-180, 180)] public float startAngle = 0f;
    public float rotateAngle = 90.0f;
    public float rotateDuration = 3.0f;

    [Header("オーディオ設定")] // ★追加
    public AudioClip soundMove; // 移動中の音（ウィーン...）
    public AudioClip soundBeam; // ビーム中の音（ビーーーッ！）
    private AudioSource audioSource;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ★追加: AudioSourceを取得
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(ActionSequence());
    }

    IEnumerator ActionSequence()
    {
        // 1. 画面揺らし開始
        SimpleShaker shaker = Camera.main.GetComponent<SimpleShaker>();
        if (shaker == null)
        {
            // CameraShakerがついている場合の対策
            CameraShaker oldShaker = Camera.main.GetComponent<CameraShaker>();
            if (oldShaker != null) oldShaker.Shake(moveDuration, shakeMagnitude);
        }
        else
        {
            shaker.Shake(moveDuration, shakeMagnitude);
        }

        // ★追加: 移動音を開始（ループ再生）
        if (soundMove != null)
        {
            audioSource.clip = soundMove;
            audioSource.loop = true; // ループ有効
            audioSource.Play();
        }

        // 2. 移動処理
        Vector3 startPos = transform.position;
        float elapsed = 0;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;

        // ★追加: 移動音を停止
        audioSource.Stop();
        audioSource.loop = false; // ループ解除

        // 3. 画像切り替え
        if (attackSprite != null) spriteRenderer.sprite = attackSprite;

        yield return new WaitForSeconds(0.5f);

        rotator.rotation = Quaternion.Euler(0, 0, startAngle);

        // 4. ビーム生成
        GameObject beamObj = null;
        if (beamPrefab != null)
        {
            beamObj = Instantiate(beamPrefab, firePoint.position, rotator.rotation);
            beamObj.transform.SetParent(rotator);
        }

        // ★追加: ビーム音を開始（ループ再生）
        if (soundBeam != null)
        {
            audioSource.clip = soundBeam;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 5. 回転
        Quaternion startRot = rotator.rotation;
        Quaternion endRot = rotator.rotation * Quaternion.Euler(0, 0, rotateAngle);
        elapsed = 0;
        while (elapsed < rotateDuration)
        {
            rotator.rotation = Quaternion.Slerp(startRot, endRot, elapsed / rotateDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ★追加: ビーム音を停止
        audioSource.Stop();

        // 6. 終了処理
        if (beamObj != null) Destroy(beamObj);

        float fadeTime = 1.0f;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(1, 0, t / fadeTime);
            spriteRenderer.color = c;
            yield return null;
        }
        Destroy(gameObject);
    }
}
