using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control : MonoBehaviour
{
    [Header("Initial Scene Control")]
    public float initialSceneControlDisableDuration = 4.0f;
    private bool isControlEnabled = false;

    public float speed = 3;
    public float jumppower = 8;
    public float effectScale = 1.5f;
    public float effectXOffset = 1.5f;
    float vx = 0;
    bool leftFlag = false;
    bool jumpFlag = false;
    bool groundFlag = false;
    bool extraJumpFlag = true;
    bool attackCooldownFlag = false;
    bool attackEffectFlag = false;
    Rigidbody2D rbody;

    // ★★★ここを変更（配列にしました）★★★
    // public GameObject wazaEffectPrefab; // ←古いやつ
    public GameObject[] wazaEffectPrefabs; // ←新しいやつ（複数入る）
    private int equippedWeaponID = 0;      // 現在の武器IDを保存する変数
    // ★★★ここまで変更★★★

    public Transform effectSpawnPoint;

    public GameObject doubleJumpEffectPrefab;
    public float doubleJumpEffectOffsetY = -0.5f;
    public float doubleJumpEffectDuration = 0.2f;

    [Header("Audio Settings")]
    public AudioClip jumpSound;
    [Range(0f, 1f)]
    public float jumpSoundVolume = 1f;

    public AudioClip doubleJumpSound;
    [Range(0f, 1f)]
    public float doubleJumpSoundVolume = 1f;

    public AudioClip moveSound;
    [Range(0f, 1f)]
    public float moveSoundVolume = 1f;

    public AudioClip attackSound;
    [Range(0f, 1f)]
    public float attackSoundVolume = 1f;

    private AudioSource audioSource;
    private bool isMovingSoundPlaying = false;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        rbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0;
            audioSource.playOnAwake = false;
        }

        // ★★★ここを追加★★★
        // 装備中の武器IDを読み込む（デフォルトは0）
        equippedWeaponID = PlayerPrefs.GetInt("EquippedWeaponID", 0);
        // ★★★ここまで追加★★★

        if (initialSceneControlDisableDuration > 0)
        {
            StartCoroutine(DisableControlTemporarily(initialSceneControlDisableDuration));
        }
        else
        {
            isControlEnabled = true;
        }
    }

    void Update()
    {
        if (!isControlEnabled)
        {
            StopMoveSound();
            return;
        }

        float currentVx = 0;
        bool isMovingInput = false;

        if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
        {
            currentVx = speed;
            leftFlag = false;
            isMovingInput = true;
        }
        if (Input.GetKey("left") || Input.GetKey(KeyCode.A))
        {
            currentVx = -speed;
            leftFlag = true;
            isMovingInput = true;
        }

        if (groundFlag && isMovingInput && !attackCooldownFlag)
        {
            PlayMoveSound();
        }
        else
        {
            StopMoveSound();
        }

        if ((Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.J)) && !attackEffectFlag)
        {
            PlaySound(attackSound, attackSoundVolume);

            attackCooldownFlag = true;
            attackEffectFlag = true;

            if (groundFlag)
            {
                currentVx = 0;
            }

            Invoke("ResetAttackCooldown", 0.2f);

            // ★★★ここを変更★★★
            // 配列から、現在の武器IDに対応するエフェクトを取り出す
            GameObject effectToSpawn = null;

            // エラー防止：IDが配列の範囲内かチェック
            if (wazaEffectPrefabs != null && equippedWeaponID < wazaEffectPrefabs.Length)
            {
                effectToSpawn = wazaEffectPrefabs[equippedWeaponID];
            }
            else if (wazaEffectPrefabs.Length > 0)
            {
                // もしIDに対応するものがなければ、とりあえず0番目を出す（保険）
                effectToSpawn = wazaEffectPrefabs[0];
            }

            // エフェクト生成処理
            if (effectToSpawn != null)
            {
                Vector3 effectPosition = effectSpawnPoint.position;
                effectPosition.x += leftFlag ? -effectXOffset : effectXOffset;

                GameObject effect = Instantiate(effectToSpawn, effectPosition, Quaternion.identity);

                float directionScaleX = leftFlag ? -effectScale : effectScale;
                effect.transform.localScale = new Vector3(directionScaleX, effectScale, 1f);

                Destroy(effect, 0.2f);
            }
            // ★★★ここまで変更★★★

            Invoke("ResetAttackEffect", 0.2f);
        }

        if (attackCooldownFlag && groundFlag)
        {
            vx = 0;
        }
        else
        {
            vx = currentVx;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (groundFlag)
            {
                PlaySound(jumpSound, jumpSoundVolume);
                jumpFlag = true;
                extraJumpFlag = true;
            }
            else if (extraJumpFlag)
            {
                PlaySound(doubleJumpSound, doubleJumpSoundVolume);
                jumpFlag = true;
                extraJumpFlag = false;
                rbody.velocity = new Vector2(rbody.velocity.x, 0);

                if (doubleJumpEffectPrefab != null)
                {
                    Vector3 jumpEffectPos = transform.position;
                    jumpEffectPos.y += doubleJumpEffectOffsetY;

                    GameObject jumpEffect = Instantiate(doubleJumpEffectPrefab, jumpEffectPos, Quaternion.identity);
                    SpriteRenderer jumpEffectRenderer = jumpEffect.GetComponent<SpriteRenderer>();
                    if (jumpEffectRenderer != null)
                    {
                        StartCoroutine(FadeOutAndDestroyEffect(jumpEffectRenderer, doubleJumpEffectDuration));
                    }
                    else
                    {
                        Destroy(jumpEffect, doubleJumpEffectDuration);
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isControlEnabled)
        {
            rbody.velocity = Vector2.zero;
            rbody.angularVelocity = 0;
            return;
        }

        rbody.velocity = new Vector2(vx, rbody.velocity.y);
        this.GetComponent<SpriteRenderer>().flipX = leftFlag;

        if (jumpFlag)
        {
            jumpFlag = false;
            rbody.AddForce(new Vector2(0, jumppower), ForceMode2D.Impulse);
        }
    }

    void ResetAttackCooldown()
    {
        attackCooldownFlag = false;
    }

    void ResetAttackEffect()
    {
        attackEffectFlag = false;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            groundFlag = true;
            extraJumpFlag = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            groundFlag = false;
            StopMoveSound();
        }
    }

    IEnumerator DisableControlTemporarily(float duration)
    {
        isControlEnabled = false;
        Debug.Log("キャラクター操作を " + duration + " 秒間無効化します。");

        rbody.velocity = Vector2.zero;
        rbody.angularVelocity = 0;
        StopMoveSound();

        yield return new WaitForSeconds(duration);

        isControlEnabled = true;
        Debug.Log("キャラクター操作を有効化しました。");
    }

    IEnumerator FadeOutAndDestroyEffect(SpriteRenderer effectRenderer, float duration)
    {
        float timer = 0f;
        Color startColor = effectRenderer.color;
        Color endColor = startColor;
        endColor.a = 0f;

        while (timer < duration)
        {
            float t = timer / duration;
            effectRenderer.color = Color.Lerp(startColor, endColor, t);
            timer += Time.deltaTime;
            yield return null;
        }

        effectRenderer.color = endColor;
        Destroy(effectRenderer.gameObject);
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayMoveSound()
    {
        if (audioSource != null && moveSound != null && !isMovingSoundPlaying)
        {
            audioSource.clip = moveSound;
            audioSource.loop = true;
            audioSource.volume = moveSoundVolume;
            audioSource.Play();
            isMovingSoundPlaying = true;
        }
    }

    public void StopMoveSound()
    {
        if (audioSource != null && isMovingSoundPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
            isMovingSoundPlaying = false;
        }
    }
}