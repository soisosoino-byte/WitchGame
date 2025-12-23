using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossBehaviorFinal : MonoBehaviour
{
    // =========================================================
    [Header("デバッグ設定")]
    [Tooltip("0ならランダム。1~5を指定するとその行動を繰り返します。")]
    public int debugFixedPattern = 0; // Inspectorのボタンからこの変数を操作します
    // =========================================================
    // ★新機能: 開始時の演出設定 (Intro & Shake)
    // =========================================================
    [Header("開始演出設定 (Intro & Shake)")]
    [Tooltip("開始待機時間（この間、演出が再生されます）")]
    public float startWaitTime = 3.0f;

    [Tooltip("この時に再生するボスの登場アニメーション名")]
    public string animIntro = "Intro";

    [Tooltip("同時に再生させたい別オブジェクトのアニメーター（例：警告エフェクトやゲートなど）")]
    public Animator introEffectAnimator;
    [Tooltip("別オブジェクトで再生するアニメーション名")]
    public string introEffectAnimName = "Open";

    [Tooltip("画面揺れの強さ（0で揺れなし）")]
    public float shakeMagnitude = 0.2f;

    // =========================================================
    // 以下、既存の設定
    // =========================================================

    [Header("基本設定")]
    public float intervalTime = 2.0f;

    [Header("オーディオ設定")]
    private AudioSource audioSource;

    void PlaySE(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    [Header("パターン1設定：爆弾と釘")]
    public GameObject bombPrefab;
    public Vector3 teleportPos = new Vector3(0, 3, 0);
    public string animThrow = "Throw";
    public string animIdle = "Idle";

    [Header("パターン1 効果音")]
    public AudioClip seBombThrow;
    [Range(0, 1)] public float volBombThrow = 1.0f;
    public AudioClip seNailThrow;
    [Range(0, 1)] public float volNailThrow = 1.0f;
    public AudioClip seDash;
    [Range(0, 1)] public float volDash = 1.0f;

    [Header("爆弾の速度ランダム設定")]
    public Vector2 bomb1ForceMin = new Vector2(-6f, 4f);
    public Vector2 bomb1ForceMax = new Vector2(-4f, 6f);
    public Vector2 bomb2ForceMin = new Vector2(4f, 4f);
    public Vector2 bomb2ForceMax = new Vector2(6f, 6f);

    [Header("パターン1続き：釘攻撃移動")]
    public GameObject nailPrefab;
    public Vector3 nailMovePos1 = new Vector3(0, 2, 0);
    public Vector3 nailMovePos2 = new Vector3(3, 2, 0);
    public string animMove = "Move";
    public float moveSpeed = 5.0f;
    public float nailSpeed = 10.0f;
    public bool aimAtPlayer = true;
    public float spreadAngle = 30f;

    [Header("パターン1続き：急降下＆突撃")]
    public string animDive = "Dive";
    public string animDash = "Dash";
    public float diveSpeed = 15.0f;
    public float dashSpeed = 20.0f;
    public float waitAfterDive = 0.5f;
    public float waitAfterDash = 0.3f;
    public Vector3 diveTargetPos = new Vector3(3, -3, 0);
    public Vector3 dashTargetPos1 = new Vector3(-5, -3, 0);
    public Vector3 dashTargetPos2 = new Vector3(5, -3, 0);
    public bool flipDashFace = false;
    public ParticleSystem footSmokeParticle;

    private Animator animator;
    private Transform playerTransform;
    private Vector3 originalScale;

    // パターン管理
    private int lastPattern = -1;
    private int consecutiveCount = 0;
    private int bannedPattern = -1;
    private int banTurnCount = 0;

    [Header("パターン2設定：魔法陣と回転ビーム")]
    public Vector3 pattern2AppearPos = new Vector3(0, 3, 0);
    public GameObject magicCirclePrefab;
    public Vector3 magicCircleOffset = new Vector3(0, 0, 1);
    public float appearDuration = 1.0f;

    [Tooltip("ステップ2で作ったビームセットのプレハブ")]
    public GameObject beamPrefab;
    [Tooltip("ビームを出している時間")]
    public float beamDuration = 5.0f;
    [Tooltip("ビーム攻撃中のアニメーション名")]
    public string animBeamAttack = "MagicAttack";

    [Tooltip("ビームを出す位置のズレ（ボスの中心からどれくらいずらすか）")]
    public Vector3 beamSpawnOffset = Vector3.zero;
    [Tooltip("ビームの開始角度（度数法：90なら真横から始まる等）")]
    public float beamInitialAngle = 0f;

    [Header("パターン2 効果音")]
    public AudioClip seMagicCircle;
    [Range(0, 1)] public float volMagicCircle = 1.0f;

    public AudioClip seBeam;
    [Range(0, 1)] public float volBeam = 1.0f;

    [Header("パターン3設定：ブーメラン")]
    public GameObject boomerangPrefab;
    public Vector3 posRightEdge = new Vector3(6, 0, 0);
    public Vector3 posLeftEdge = new Vector3(-6, 0, 0);
    public float boomerangDistance = 8.0f;
    public float boomerangWidth = 4.0f;
    public float boomerangDuration = 2.0f;
    public string animBoomerang = "AttackBoomerang";
    public bool flipBoomerangFace = false;

    [Header("パターン3 効果音")]
    public AudioClip seBoomerangThrow;
    [Range(0, 1)] public float volBoomerangThrow = 1.0f;

    [Header("パターン4設定：設置型時限攻撃")]
    public Vector3 pattern4StartPos = new Vector3(0, 0, 0);
    public GameObject delayedAttackPrefab;
    public Sprite delayedAttackTargetSprite;
    public Transform[] attackSpawnPoints;
    public float attackDelayTime = 2.0f;
    public float spawnInterval = 0.1f;
    public AudioClip seAttackActivate;
    [Range(0, 1)] public float volAttackActivate = 1.0f;

    // =========================================================
    // ★追加: パターン5設定（落下＋棘波攻撃）
    // =========================================================
    [Header("パターン5設定：落下と棘の波")]
    [Tooltip("落下ターゲットX座標（2つ設定してください）")]
    public float[] dropTargetXPositions = new float[2];
    [Tooltip("落下開始位置の高さ")]
    public float dropStartYPosition = 10f;
    [Tooltip("落下の速度")]
    public float dropSpeed = 20f;
    [Tooltip("着地する地面の高さ")]
    public float groundYPosition = 0f;

    // ★追加・変更箇所
    [Tooltip("落下位置へ移動する際のアニメーション名")]
    public string animMoveToStart = "MoveUp"; // 新しいアニメーション名
    [Tooltip("落下位置へ移動するのにかける時間")]
    public float moveToStartDuration = 0.3f;  // 0.3秒で移動

    // ★追加：落下アニメーション名
    [Tooltip("落下中のアニメーション名")]
    public string animDrop = "Drop";

    [Header("パターン5 棘の設定")]
    public GameObject spikePrefab; // ★Animatorなし、SpikeController付きのプレハブ
    public int spikesPerSide = 5;
    public float spikeSpacing = 1.5f;
    public float waveDelay = 0.1f;

    [Tooltip("棘の飛び出す高さ")]
    public float spikeHeight = 3.0f;
    [Tooltip("棘の動く速さ")]
    public float spikeMoveSpeed = 10.0f;
    [Tooltip("棘の滞空時間")]
    public float spikeStayTime = 0.2f;
    [Tooltip("棘の出現開始Y座標（地面より下にする）")]
    public float spikeStartY = -1.0f;

    // ★追加：土煙プレハブ
    [Tooltip("棘の出現時に出る土煙のプレハブ")]
    public GameObject dustPrefab;

    [Header("パターン5 効果音")]
    public AudioClip seSpikeAppear;
    [Range(0, 1)] public float volSpikeAppear = 1.0f;


    void Start()
    {
        animator = GetComponent<Animator>();
        playerTransform = GameObject.FindGameObjectWithTag("Kyara")?.transform;
        originalScale = transform.localScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        StartCoroutine(BattleLoop());
    }

    IEnumerator BattleLoop()
    {
        // 開始演出
        Debug.Log("ボスバトル開始演出");
        if (animator != null && !string.IsNullOrEmpty(animIntro)) animator.Play(animIntro);
        if (introEffectAnimator != null && !string.IsNullOrEmpty(introEffectAnimName)) introEffectAnimator.Play(introEffectAnimName);
        if (shakeMagnitude > 0) StartCoroutine(ScreenShake(startWaitTime, shakeMagnitude));

        yield return new WaitForSeconds(startWaitTime);

        // バトルループ
        while (true)
        {
            int nextPattern = DecideNextPattern();

            switch (nextPattern)
            {
                case 1: yield return StartCoroutine(ActionPattern1()); break;
                case 2: yield return StartCoroutine(ActionPattern2()); break;
                case 3: yield return StartCoroutine(ActionPattern3()); break;
                case 4: yield return StartCoroutine(ActionPattern4()); break;
                // ★追加: パターン5の分岐
                case 5: yield return StartCoroutine(ActionPattern5()); break;
            }

            if (animator != null) animator.Play(animIdle);
            transform.localScale = originalScale;
            yield return new WaitForSeconds(intervalTime);
        }
    }



    IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.position;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.position = originalPos;
    }

    // --- パターン1~4は変更なし (省略せずに記述します) ---

    IEnumerator ActionPattern1()
    {
        Debug.Log("行動1開始");
        transform.position = teleportPos;
        if (animator != null) animator.Play(animThrow);
        PlaySE(seBombThrow, volBombThrow);
        yield return new WaitForSeconds(0.5f);
        if (bombPrefab != null)
        {
            Vector2 force1 = new Vector2(Random.Range(bomb1ForceMin.x, bomb1ForceMax.x), Random.Range(bomb1ForceMin.y, bomb1ForceMax.y));
            SpawnBomb(force1);
            Vector2 force2 = new Vector2(Random.Range(bomb2ForceMin.x, bomb2ForceMax.x), Random.Range(bomb2ForceMin.y, bomb2ForceMax.y));
            SpawnBomb(force2);
        }
        yield return new WaitForSeconds(1.0f);
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, nailMovePos1) > 0.1f) { transform.position = Vector3.MoveTowards(transform.position, nailMovePos1, moveSpeed * Time.deltaTime); yield return null; }
        if (animator != null) animator.Play(animThrow);
        ShootNails3Way();
        yield return new WaitForSeconds(0.5f);
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, nailMovePos2) > 0.1f) { transform.position = Vector3.MoveTowards(transform.position, nailMovePos2, moveSpeed * Time.deltaTime); yield return null; }
        if (animator != null) animator.Play(animThrow);
        ShootNails3Way();
        yield return new WaitForSeconds(0.5f);
        Debug.Log("急降下！");
        if (animator != null) animator.Play(animDive);
        while (Vector3.Distance(transform.position, diveTargetPos) > 0.1f) { transform.position = Vector3.MoveTowards(transform.position, diveTargetPos, diveSpeed * Time.deltaTime); yield return null; }
        transform.position = diveTargetPos;
        if (animator != null) animator.Play(animIdle);
        yield return new WaitForSeconds(waitAfterDive);
        Debug.Log("突撃1回目！");
        if (animator != null) animator.Play(animDash);
        FaceDirection(dashTargetPos1);
        if (footSmokeParticle != null) footSmokeParticle.Play();
        PlaySE(seDash, volDash);
        while (Vector3.Distance(transform.position, dashTargetPos1) > 0.1f) { transform.position = Vector3.MoveTowards(transform.position, dashTargetPos1, dashSpeed * Time.deltaTime); yield return null; }
        if (footSmokeParticle != null) footSmokeParticle.Stop();
        yield return new WaitForSeconds(waitAfterDash);
        Debug.Log("突撃2回目！");
        FaceDirection(dashTargetPos2);
        if (animator != null) animator.Play(animDash);
        if (footSmokeParticle != null) footSmokeParticle.Play();
        PlaySE(seDash, volDash);
        while (Vector3.Distance(transform.position, dashTargetPos2) > 0.1f) { transform.position = Vector3.MoveTowards(transform.position, dashTargetPos2, dashSpeed * Time.deltaTime); yield return null; }
        if (footSmokeParticle != null) footSmokeParticle.Stop();
        Debug.Log("行動1完了");
    }

    IEnumerator ActionPattern2()
    {
        Debug.Log("行動2開始: ビーム攻撃");
        if (animator != null) animator.Play(animIdle);
        transform.localScale = Vector3.zero;
        transform.position = pattern2AppearPos;
        PlaySE(seMagicCircle, volMagicCircle);
        GameObject magicCircle = null;
        if (magicCirclePrefab != null) magicCircle = Instantiate(magicCirclePrefab, transform.position + magicCircleOffset, Quaternion.identity);
        float timer = 0f;
        Vector3 targetScale = originalScale;
        while (timer < appearDuration) { timer += Time.deltaTime; transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / appearDuration); yield return null; }
        transform.localScale = targetScale;
        yield return new WaitForSeconds(0.5f);
        Debug.Log("回転ビーム発射！");
        if (animator != null) animator.Play(animBeamAttack);
        if (beamPrefab != null)
        {
            if (seBeam != null && audioSource != null) { audioSource.clip = seBeam; audioSource.volume = volBeam; audioSource.loop = true; audioSource.Play(); }
            Vector3 spawnPos = transform.position + beamSpawnOffset;
            Quaternion initialRot = Quaternion.Euler(0, 0, beamInitialAngle);
            GameObject beam = Instantiate(beamPrefab, spawnPos, initialRot);
            RotatingBeam rb = beam.GetComponent<RotatingBeam>();
            if (rb != null) { rb.duration = beamDuration; if (Random.Range(0, 2) == 0) rb.rotationSpeed *= -1; }
        }
        yield return new WaitForSeconds(beamDuration);
        if (audioSource != null) { if (audioSource.clip == seBeam) { audioSource.Stop(); audioSource.loop = false; audioSource.clip = null; } }
        yield return new WaitForSeconds(0.5f);
        if (magicCircle != null) { var fader = magicCircle.GetComponent<MagicCircleFader>(); if (fader != null) fader.StartFadeOut(); else Destroy(magicCircle); }
        Debug.Log("行動2完了");
    }

    IEnumerator ActionPattern3()
    {
        Debug.Log("行動3開始");
        if (animator != null) animator.Play(animIdle);
        bool isRightSide = (Random.Range(0, 2) == 0);
        Vector3 targetPos = isRightSide ? posRightEdge : posLeftEdge;
        transform.localScale = Vector3.zero;
        transform.position = targetPos;
        float baseScaleX = Mathf.Abs(originalScale.x);
        if (flipBoomerangFace) baseScaleX = -baseScaleX;
        if (isRightSide) transform.localScale = new Vector3(-baseScaleX, originalScale.y, originalScale.z);
        else transform.localScale = new Vector3(baseScaleX, originalScale.y, originalScale.z);
        float timer = 0f;
        Vector3 currentTargetScale = transform.localScale;
        while (timer < 0.5f) { timer += Time.deltaTime; transform.localScale = Vector3.Lerp(Vector3.zero, currentTargetScale, timer / 0.5f); yield return null; }
        transform.localScale = currentTargetScale;
        yield return new WaitForSeconds(0.3f);
        if (animator != null) animator.Play(animBoomerang);
        PlaySE(seBoomerangThrow, volBoomerangThrow);
        if (boomerangPrefab != null) { Vector3 forwardDir = isRightSide ? Vector3.left : Vector3.right; SpawnBoomerang(forwardDir, Vector3.up); SpawnBoomerang(forwardDir, Vector3.down); }
        yield return new WaitForSeconds(boomerangDuration);
        yield return new WaitForSeconds(0.5f);
        Debug.Log("行動3完了");
    }

    IEnumerator ActionPattern4()
    {
        Debug.Log("行動4開始: 設置型攻撃");
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, pattern4StartPos) > 0.1f) { transform.position = Vector3.MoveTowards(transform.position, pattern4StartPos, moveSpeed * Time.deltaTime); yield return null; }
        transform.position = pattern4StartPos;
        if (animator != null) animator.Play(animIdle);
        if (delayedAttackPrefab != null && attackSpawnPoints != null)
        {
            float elapsedSpawnTime = 0f;
            for (int i = 0; i < attackSpawnPoints.Length; i++)
            {
                Transform spawnPoint = attackSpawnPoints[i];
                if (spawnPoint != null)
                {
                    GameObject obj = Instantiate(delayedAttackPrefab, spawnPoint.position, spawnPoint.rotation);
                    DelayedAttackObject dao = obj.GetComponent<DelayedAttackObject>();
                    if (dao != null) { float myDelay = attackDelayTime - elapsedSpawnTime; if (myDelay < 0) myDelay = 0f; dao.Setup(delayedAttackTargetSprite, myDelay, 1); }
                }
                if (i < attackSpawnPoints.Length - 1) { yield return new WaitForSeconds(spawnInterval); elapsedSpawnTime += spawnInterval; }
            }
            float remainingWait = attackDelayTime - elapsedSpawnTime;
            if (remainingWait > 0) { yield return new WaitForSeconds(remainingWait); }
        }
        else { yield return new WaitForSeconds(attackDelayTime); }
        PlaySE(seAttackActivate, volAttackActivate);
        yield return new WaitForSeconds(0.2f + 0.5f);
        Debug.Log("行動4完了");
    }

    // =========================================================
    // ★更新: パターン5（アニメーション、位置移動）
    // =========================================================
    IEnumerator ActionPattern5()
    {
        Debug.Log("行動5開始: 落下棘攻撃");

        int randomIndex = Random.Range(0, dropTargetXPositions.Length);
        float targetX = dropTargetXPositions[randomIndex];
        Vector3 targetDropPos = new Vector3(targetX, groundYPosition, transform.position.z); // 着地予定地点
        Vector3 startDropPos = new Vector3(targetX, dropStartYPosition, transform.position.z); // 落下開始地点（上空）

        // -----------------------------------------------------
        // 1. 落下開始位置へ移動 (0.3秒かけて移動)
        // -----------------------------------------------------

        // 移動用のアニメーション再生
        if (animator != null && !string.IsNullOrEmpty(animMoveToStart))
        {
            animator.Play(animMoveToStart);
        }

        Vector3 currentPos = transform.position;
        float timer = 0f;

        // 指定時間(0.3秒)かけて現在地から上空へ移動
        while (timer < moveToStartDuration)
        {
            timer += Time.deltaTime;
            // Lerpで滑らかに移動
            transform.position = Vector3.Lerp(currentPos, startDropPos, timer / moveToStartDuration);
            yield return null;
        }
        transform.position = startDropPos; // ズレ防止のため最終位置を確定

        // -----------------------------------------------------
        // 2. 落下アニメーション再生 & 落下開始
        // -----------------------------------------------------
        if (animator != null && !string.IsNullOrEmpty(animDrop))
        {
            animator.Play(animDrop);
        }

        // 3. 落下処理（地面に着くまで）
        while (transform.position.y > groundYPosition)
        {
            transform.Translate(Vector3.down * dropSpeed * Time.deltaTime);
            yield return null;
        }
        // 位置補正
        transform.position = targetDropPos;

        // 着地パーティクル
        if (footSmokeParticle != null) footSmokeParticle.Play();

        // 4. 着地後、棘攻撃開始（アニメーションはIdleに戻す）
        if (animator != null) animator.Play(animIdle);

        yield return StartCoroutine(SpawnSpikeWave(targetDropPos));

        yield return new WaitForSeconds(0.5f);
        Debug.Log("行動5完了");
    }

    private IEnumerator SpawnSpikeWave(Vector3 centerPos)
    {
        for (int i = 0; i <= spikesPerSide; i++)
        {
            Vector3 rightPos = new Vector3(centerPos.x + (i * spikeSpacing), spikeStartY, 0);
            SpawnSingleSpike(rightPos);

            if (i > 0)
            {
                Vector3 leftPos = new Vector3(centerPos.x - (i * spikeSpacing), spikeStartY, 0);
                SpawnSingleSpike(leftPos);
            }

            if (i < spikesPerSide)
            {
                yield return new WaitForSeconds(waveDelay);
            }
        }
    }

    // ★更新: 土煙を追加
    private void SpawnSingleSpike(Vector3 position)
    {
        // 1. 土煙の生成
        if (dustPrefab != null)
        {
            // 土煙は地面の高さ(groundYPosition)に出す
            Vector3 dustPos = new Vector3(position.x, groundYPosition, 0);
            Instantiate(dustPrefab, dustPos, Quaternion.identity);
            // DustControllerがついているので自動で消える
        }

        // 2. 棘の生成
        if (spikePrefab != null)
        {
            GameObject spike = Instantiate(spikePrefab, position, Quaternion.identity);
            SpikeController spikeCtrl = spike.GetComponent<SpikeController>();
            if (spikeCtrl != null)
            {
                spikeCtrl.Initialize(spikeHeight, spikeMoveSpeed, spikeStayTime);
            }
            PlaySE(seSpikeAppear, volSpikeAppear);
        }
    }


    // --- ヘルパー関数群 ---
    void SpawnBomb(Vector2 force) { GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity); Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>(); if (rb != null) rb.AddForce(force, ForceMode2D.Impulse); }
    void ShootNails3Way() { PlaySE(seNailThrow, volNailThrow); if (nailPrefab == null) return; Vector3 baseDirection = Vector3.down; if (aimAtPlayer && playerTransform != null) { baseDirection = (playerTransform.position - transform.position).normalized; } SpawnNail(baseDirection); SpawnNail(ApplyRotation(baseDirection, spreadAngle)); SpawnNail(ApplyRotation(baseDirection, -spreadAngle)); }
    void SpawnNail(Vector3 direction) { GameObject nail = Instantiate(nailPrefab, transform.position, Quaternion.identity); NailProjectile np = nail.GetComponent<NailProjectile>(); if (np != null) { np.Setup(direction, nailSpeed); } }
    Vector3 ApplyRotation(Vector3 forward, float angle) { return Quaternion.Euler(0, 0, angle) * forward; }
    void FaceDirection(Vector3 targetPos) { float diffX = targetPos.x - transform.position.x; float baseScaleX = Mathf.Abs(originalScale.x); if (flipDashFace) baseScaleX = -baseScaleX; if (diffX > 0) transform.localScale = new Vector3(baseScaleX, originalScale.y, originalScale.z); else if (diffX < 0) transform.localScale = new Vector3(-baseScaleX, originalScale.y, originalScale.z); }
    void SpawnBoomerang(Vector3 forward, Vector3 side) { GameObject obj = Instantiate(boomerangPrefab, transform.position, Quaternion.identity); BoomerangProjectile bp = obj.GetComponent<BoomerangProjectile>(); if (bp != null) bp.Setup(transform.position, forward, side, boomerangDistance, boomerangWidth, boomerangDuration); }

    int DecideNextPattern()
    {
        if (banTurnCount > 0)
        {
            banTurnCount--;
            if (banTurnCount == 0) bannedPattern = -1;
        }

        List<int> availablePatterns = new List<int>();
        // ★修正: ループ回数を 4 から 5 に変更
        for (int i = 1; i <= 5; i++)
        {
            if (i != bannedPattern) availablePatterns.Add(i);
        }

        int randomIndex = Random.Range(0, availablePatterns.Count);
        int selectedPattern = availablePatterns[randomIndex];

        if (selectedPattern == lastPattern)
        {
            consecutiveCount++;
            if (consecutiveCount >= 2)
            {
                bannedPattern = selectedPattern;
                banTurnCount = 2;
                consecutiveCount = 0;
            }
        }
        else
        {
            lastPattern = selectedPattern;
            consecutiveCount = 1;
        }

        Debug.Log($"次の攻撃: パターン{selectedPattern}");
        return selectedPattern;
    }
}