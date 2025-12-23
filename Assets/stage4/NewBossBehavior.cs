using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Listを使うために必要

public class NewBossBehavior : MonoBehaviour
{
    [Header("基本設定")]
    public float startWaitTime = 2.0f;
    public float intervalTime = 2.0f;

    // --- ★ここから追加: 共通オーディオ設定 ---
    [Header("オーディオ設定")]
    private AudioSource audioSource;

    // 音を鳴らすための便利関数
    void PlaySE(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
    // -------------------------------------

    [Header("パターン1設定：爆弾と釘")]
    public GameObject bombPrefab;
    public Vector3 teleportPos = new Vector3(0, 3, 0);
    public string animThrow = "Throw";
    public string animIdle = "Idle";

    // ★追加: パターン1の効果音
    [Header("パターン1 効果音")]
    public AudioClip seBombThrow; // 爆弾を投げる音
    [Range(0, 1)] public float volBombThrow = 1.0f; // 音量

    public AudioClip seNailThrow; // 釘を投げる音
    [Range(0, 1)] public float volNailThrow = 1.0f;

    public AudioClip seDash; // 横突撃の音
    [Range(0, 1)] public float volDash = 1.0f;

    // 爆弾の速度設定
    [Header("爆弾の速度ランダム設定")]
    [Tooltip("左に投げる爆弾の最小速度(X, Y)")]
    public Vector2 bomb1ForceMin = new Vector2(-6f, 4f);
    [Tooltip("左に投げる爆弾の最大速度(X, Y)")]
    public Vector2 bomb1ForceMax = new Vector2(-4f, 6f);

    [Tooltip("右に投げる爆弾の最小速度(X, Y)")]
    public Vector2 bomb2ForceMin = new Vector2(4f, 4f);
    [Tooltip("右に投げる爆弾の最大速度(X, Y)")]
    public Vector2 bomb2ForceMax = new Vector2(6f, 6f);

    [Header("パターン1続き：釘攻撃移動")]
    public GameObject nailPrefab;
    public Vector3 nailMovePos1 = new Vector3(0, 2, 0);
    public Vector3 nailMovePos2 = new Vector3(3, 2, 0);
    public string animMove = "Move"; // 通常移動アニメ
    public float moveSpeed = 5.0f;
    public float nailSpeed = 10.0f;
    public bool aimAtPlayer = true;
    public float spreadAngle = 30f;

    [Header("パターン1続き：急降下＆突撃")]
    public string animDive = "Dive"; // 急降下アニメ名
    public string animDash = "Dash"; // 突撃アニメ名
    public float diveSpeed = 15.0f;  // 急降下の速さ
    public float dashSpeed = 20.0f;  // 突撃の速さ

    [Tooltip("急降下して着地した後の硬直時間")]
    public float waitAfterDive = 0.5f;

    [Tooltip("1回目の突撃後、折り返す前のタメ時間")]
    public float waitAfterDash = 0.3f;

    public Vector3 diveTargetPos = new Vector3(3, -3, 0); // 急降下の着地点（地面）
    public Vector3 dashTargetPos1 = new Vector3(-5, -3, 0); // 1回目の突撃の終点
    public Vector3 dashTargetPos2 = new Vector3(5, -3, 0);  // 2回目の突撃の終点（折り返し）

    [Tooltip("突撃アニメだけ左右が逆になってしまう場合はチェックを入れてください")]
    public bool flipDashFace = false;

    [Tooltip("ステップ1で作った足元の煙パーティクルをここに登録")]
    public ParticleSystem footSmokeParticle;

    private Animator animator;
    private Transform playerTransform;
    private Vector3 originalScale; // 元の向きを記憶

    // 攻撃パターンの管理用変数
    private int lastPattern = -1;      // 直前に出した技の番号
    private int consecutiveCount = 0;  // 同じ技が何回続いたか
    private int bannedPattern = -1;    // 現在禁止されている技の番号
    private int banTurnCount = 0;      // 禁止があと何回続くか

    [Header("パターン2設定：魔法陣と誘導爆弾")]
    public Vector3 pattern2AppearPos = new Vector3(0, 3, 0); // 出現位置

    [Tooltip("ステップ3で作った魔法陣プレハブ")]
    public GameObject magicCirclePrefab;
    [Tooltip("魔法陣を出す位置のズレ（ボスの背後になるようにZをプラスに）")]
    public Vector3 magicCircleOffset = new Vector3(0, 0, 1);

    [Tooltip("ステップ2で作った誘導爆弾プレハブ")]
    public GameObject homingBombPrefab;

    [Tooltip("大きくなって現れるまでにかかる時間")]
    public float appearDuration = 1.0f;

    public int bombCount = 2; // 発射する数
    public float bombInterval = 0.5f; // 発射間隔（時間差）
    public string animMagicAttack = "MagicAttack";

    // ★追加: パターン2の効果音
    [Header("パターン2 効果音")]
    public AudioClip seMagicCircle; // 魔法陣出現音
    [Range(0, 1)] public float volMagicCircle = 1.0f;

    public AudioClip seHomingShoot; // 誘導爆弾発射音
    [Range(0, 1)] public float volHomingShoot = 1.0f;


    [Header("パターン3設定：ブーメラン")]
    public GameObject boomerangPrefab; // ブーメランのプレハブ

    [Tooltip("右端に出現する座標")]
    public Vector3 posRightEdge = new Vector3(6, 0, 0);
    [Tooltip("左端に出現する座標")]
    public Vector3 posLeftEdge = new Vector3(-6, 0, 0);

    public float boomerangDistance = 8.0f; // 前に飛ぶ距離
    public float boomerangWidth = 4.0f;    // 横に広がる幅（弧の大きさ）
    public float boomerangDuration = 2.0f; // 行って戻るまでの時間

    public string animBoomerang = "AttackBoomerang"; // 投げるアニメ名

    [Tooltip("ブーメラン攻撃の時だけ向きが逆になる場合はチェックを入れてください")]
    public bool flipBoomerangFace = false;

    // ★追加: パターン3の効果音
    [Header("パターン3 効果音")]
    public AudioClip seBoomerangThrow; // ブーメラン投げ音
    [Range(0, 1)] public float volBoomerangThrow = 1.0f;


    [Header("パターン4設定：3連斬り＆特大斬撃")]
    public Vector3 pattern4StartPos = new Vector3(-5, -2, 0); // 攻撃開始位置
    public GameObject slashSmallPrefab; // 小さい斬撃
    public GameObject slashHugePrefab;  // 特大斬撃

    [Tooltip("斬撃アニメだけ左右が逆になる場合はチェック")]
    public bool flipSlashFace = false;

    // ★追加: 特大斬撃の飛ぶ速さ
    [Tooltip("特大斬撃が飛んでいく速さ")]
    public float slashHugeSpeed = 10.0f;

    public string animSlash = "AttackSlash"; // 斬撃のアニメーション名

    [Tooltip("斬撃の際、ボスの前方にどれくらいズラして出すか")]
    public float slashSpawnOffset = 1.5f;

    [Tooltip("攻撃ごとの前進距離")]
    public float stepMoveDistance = 2.0f;

    [Tooltip("攻撃ごとの前進にかける時間")]
    public float stepMoveDuration = 0.2f;

    [Tooltip("斬撃間の待ち時間")]
    public float slashInterval = 0.5f;
    [Tooltip("攻撃時の前進方向が逆（後ろに下がる）場合はチェックを入れてください")]
    public bool reverseStepDirection = false;
    [Tooltip("特大斬撃の飛ぶ方向だけを逆にしたい場合はチェックを入れてください")]
    public bool flipHugeSlashDirection = false;

    // ★追加: パターン4の効果音
    [Header("パターン4 効果音")]
    public AudioClip seSlashSmall; // 小斬撃音
    [Range(0, 1)] public float volSlashSmall = 1.0f;

    public AudioClip seSlashHuge; // 特大斬撃音
    [Range(0, 1)] public float volSlashHuge = 1.0f;


    void Start()
    {
        animator = GetComponent<Animator>();
        playerTransform = GameObject.FindGameObjectWithTag("Kyara")?.transform;
        originalScale = transform.localScale; // 開始時の向きを記憶

        // ★追加: AudioSourceコンポーネント取得（なければ追加）
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        StartCoroutine(BattleLoop());
    }

    IEnumerator BattleLoop()
    {
        // 最初の待機
        yield return new WaitForSeconds(startWaitTime);

        while (true)
        {
            // 1. 次にどの技を出すか決める（ランダム＆禁止ロジック）
            int nextPattern = DecideNextPattern();

            // 2. 選ばれたパターンを実行
            switch (nextPattern)
            {
                case 1:
                    yield return StartCoroutine(ActionPattern1());
                    break;
                case 2:
                    yield return StartCoroutine(ActionPattern2());
                    break;
                case 3:
                    yield return StartCoroutine(ActionPattern3());
                    break;
                case 4:
                    yield return StartCoroutine(ActionPattern4());
                    break;
            }

            // 3. 待機状態に戻す
            if (animator != null) animator.Play(animIdle);
            transform.localScale = originalScale; // 向きをリセット

            // 4. インターバル時間（休憩）
            yield return new WaitForSeconds(intervalTime);
        }
    }

    // パターン1全工程
    IEnumerator ActionPattern1()
    {
        Debug.Log("行動1開始");

        // --- フェーズ1: 爆弾投擲 ---
        transform.position = teleportPos;
        if (animator != null) animator.Play(animThrow);

        // ★音再生: 爆弾投げ
        PlaySE(seBombThrow, volBombThrow);

        yield return new WaitForSeconds(0.5f);

        if (bombPrefab != null)
        {
            // 左用
            Vector2 force1 = new Vector2(
                Random.Range(bomb1ForceMin.x, bomb1ForceMax.x),
                Random.Range(bomb1ForceMin.y, bomb1ForceMax.y)
            );
            SpawnBomb(force1);

            // 右用
            Vector2 force2 = new Vector2(
                Random.Range(bomb2ForceMin.x, bomb2ForceMax.x),
                Random.Range(bomb2ForceMin.y, bomb2ForceMax.y)
            );
            SpawnBomb(force2);
        }

        yield return new WaitForSeconds(1.0f);

        // --- フェーズ2: 空中移動＆釘発射×2 ---
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, nailMovePos1) > 0.1f) { transform.position = Vector3.MoveTowards(transform.position, nailMovePos1, moveSpeed * Time.deltaTime); yield return null; }
        if (animator != null) animator.Play(animThrow);
        ShootNails3Way(); // ★音はShootNails3Wayの中で鳴らす
        yield return new WaitForSeconds(0.5f);

        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, nailMovePos2) > 0.1f) { transform.position = Vector3.MoveTowards(transform.position, nailMovePos2, moveSpeed * Time.deltaTime); yield return null; }
        if (animator != null) animator.Play(animThrow);
        ShootNails3Way(); // ★音はShootNails3Wayの中で鳴らす
        yield return new WaitForSeconds(0.5f);

        // --- フェーズ3 急降下＆突撃 ---
        Debug.Log("急降下！");
        if (animator != null) animator.Play(animDive);
        while (Vector3.Distance(transform.position, diveTargetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, diveTargetPos, diveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = diveTargetPos;

        if (animator != null) animator.Play(animIdle);
        yield return new WaitForSeconds(waitAfterDive);


        // 2. 横に突撃（1回目）
        Debug.Log("突撃1回目！");
        if (animator != null) animator.Play(animDash);

        FaceDirection(dashTargetPos1);
        if (footSmokeParticle != null) footSmokeParticle.Play();

        // ★音再生: 突撃
        PlaySE(seDash, volDash);

        while (Vector3.Distance(transform.position, dashTargetPos1) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dashTargetPos1, dashSpeed * Time.deltaTime);
            yield return null;
        }

        if (footSmokeParticle != null) footSmokeParticle.Stop();
        yield return new WaitForSeconds(waitAfterDash);


        // 3. 逆方向を向いて突撃（2回目）
        Debug.Log("突撃2回目（折り返し）！");
        FaceDirection(dashTargetPos2);
        if (animator != null) animator.Play(animDash);

        if (footSmokeParticle != null) footSmokeParticle.Play();

        // ★音再生: 突撃
        PlaySE(seDash, volDash);

        while (Vector3.Distance(transform.position, dashTargetPos2) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dashTargetPos2, dashSpeed * Time.deltaTime);
            yield return null;
        }

        if (footSmokeParticle != null) footSmokeParticle.Stop();

        Debug.Log("行動1完了");
    }

    IEnumerator ActionPattern2()
    {
        Debug.Log("行動2開始: 魔法陣出現");
        if (animator != null) animator.Play(animIdle);

        // 1. 姿を消して指定座標へ移動
        transform.localScale = Vector3.zero;
        transform.position = pattern2AppearPos;

        // ★音再生: 魔法陣出現
        PlaySE(seMagicCircle, volMagicCircle);

        // 2. 魔法陣を背後に生成
        GameObject magicCircle = null;
        if (magicCirclePrefab != null)
        {
            magicCircle = Instantiate(magicCirclePrefab, transform.position + magicCircleOffset, Quaternion.identity);
        }

        // 3. 徐々に大きくなって現れる
        float timer = 0f;
        Vector3 targetScale = originalScale;

        while (timer < appearDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / appearDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
            yield return null;
        }
        transform.localScale = targetScale;

        yield return new WaitForSeconds(0.5f);


        // 4. 時間差で爆弾を発射
        Debug.Log("誘導爆弾発射！");
        for (int i = 0; i < bombCount; i++)
        {
            if (animator != null)
            {
                animator.Play(animMagicAttack, 0, 0f);
            }

            // ★音再生: 爆弾発射
            PlaySE(seHomingShoot, volHomingShoot);

            if (homingBombPrefab != null)
            {
                Instantiate(homingBombPrefab, transform.position, Quaternion.identity);
            }

            if (i < bombCount - 1)
            {
                yield return new WaitForSeconds(bombInterval);
            }
        }

        yield return new WaitForSeconds(1.0f);

        // 5. 魔法陣を消す
        if (magicCircle != null)
        {
            var fader = magicCircle.GetComponent<MagicCircleFader>();
            if (fader != null)
            {
                fader.StartFadeOut();
            }
            else
            {
                Destroy(magicCircle);
            }
        }
        Debug.Log("行動2完了");
    }

    IEnumerator ActionPattern3()
    {
        Debug.Log("行動3開始: ブーメラン");
        if (animator != null) animator.Play(animIdle);

        // 1. 左右どちらに行くかランダムで決める
        bool isRightSide = (Random.Range(0, 2) == 0); // 0なら右、1なら左
        Vector3 targetPos = isRightSide ? posRightEdge : posLeftEdge;

        // 2. 姿を消して移動
        transform.localScale = Vector3.zero;
        transform.position = targetPos;

        // 3. 向きの調整
        float baseScaleX = Mathf.Abs(originalScale.x);
        if (flipBoomerangFace)
        {
            baseScaleX = -baseScaleX;
        }

        if (isRightSide)
        {
            transform.localScale = new Vector3(-baseScaleX, originalScale.y, originalScale.z);
        }
        else
        {
            transform.localScale = new Vector3(baseScaleX, originalScale.y, originalScale.z);
        }

        // 4. 大きくなって出現
        float timer = 0f;
        Vector3 currentTargetScale = transform.localScale;

        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, currentTargetScale, timer / 0.5f);
            yield return null;
        }
        transform.localScale = currentTargetScale;

        yield return new WaitForSeconds(0.3f);

        // 5. ブーメランを2つ投げる
        if (animator != null) animator.Play(animBoomerang);

        // ★音再生: ブーメラン投げ
        PlaySE(seBoomerangThrow, volBoomerangThrow);

        if (boomerangPrefab != null)
        {
            Vector3 forwardDir = isRightSide ? Vector3.left : Vector3.right;
            SpawnBoomerang(forwardDir, Vector3.up);
            SpawnBoomerang(forwardDir, Vector3.down);
        }

        yield return new WaitForSeconds(boomerangDuration);
        yield return new WaitForSeconds(0.5f);

        Debug.Log("行動3完了");
    }

    IEnumerator ActionPattern4()
    {
        Debug.Log("行動4開始: 3連斬撃");

        if (animator != null) animator.Play(animMove);

        while (Vector3.Distance(transform.position, pattern4StartPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pattern4StartPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = pattern4StartPos;

        if (playerTransform != null)
        {
            FaceDirection(playerTransform.position);
            if (flipSlashFace)
            {
                Vector3 s = transform.localScale;
                s.x *= -1;
                transform.localScale = s;
            }
        }
        yield return new WaitForSeconds(0.5f);

        // 3連撃
        for (int i = 0; i < 3; i++)
        {
            if (animator != null) animator.Play(animSlash, 0, 0f);

            // ★音再生: 小斬撃
            PlaySE(seSlashSmall, volSlashSmall);

            SpawnSlash(slashSmallPrefab, 0f);

            float direction = Mathf.Sign(transform.localScale.x);
            if (reverseStepDirection)
            {
                direction *= -1;
            }

            Vector3 targetStepPos = transform.position + new Vector3(direction * stepMoveDistance, 0, 0);

            float timer = 0f;
            Vector3 startStepPos = transform.position;
            while (timer < stepMoveDuration)
            {
                timer += Time.deltaTime;
                transform.position = Vector3.Lerp(startStepPos, targetStepPos, timer / stepMoveDuration);
                yield return null;
            }
            transform.position = targetStepPos;

            yield return new WaitForSeconds(slashInterval);
        }

        // 特大斬撃
        Debug.Log("特大斬撃！");
        if (animator != null) animator.Play(animSlash, 0, 0f);

        // ★音再生: 大斬撃
        PlaySE(seSlashHuge, volSlashHuge);

        SpawnSlash(slashHugePrefab, slashHugeSpeed, false, flipHugeSlashDirection);

        yield return new WaitForSeconds(1.0f);

        Debug.Log("行動4完了");
    }

    // --- 各種ヘルパー関数 ---

    void SpawnBomb(Vector2 force)
    {
        GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
        if (rb != null) rb.AddForce(force, ForceMode2D.Impulse);
    }

    void ShootNails3Way()
    {
        // ★音再生: 釘投げ
        PlaySE(seNailThrow, volNailThrow);

        if (nailPrefab == null) return;
        Vector3 baseDirection = Vector3.down;
        if (aimAtPlayer && playerTransform != null)
        {
            baseDirection = (playerTransform.position - transform.position).normalized;
        }
        SpawnNail(baseDirection);
        SpawnNail(ApplyRotation(baseDirection, spreadAngle));
        SpawnNail(ApplyRotation(baseDirection, -spreadAngle));
    }

    void SpawnNail(Vector3 direction)
    {
        GameObject nail = Instantiate(nailPrefab, transform.position, Quaternion.identity);
        NailProjectile np = nail.GetComponent<NailProjectile>();
        if (np != null)
        {
            np.Setup(direction, nailSpeed);
        }
    }

    Vector3 ApplyRotation(Vector3 forward, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * forward;
    }

    void FaceDirection(Vector3 targetPos)
    {
        float diffX = targetPos.x - transform.position.x;
        float baseScaleX = Mathf.Abs(originalScale.x);
        if (flipDashFace)
        {
            baseScaleX = -baseScaleX;
        }

        if (diffX > 0)
        {
            transform.localScale = new Vector3(baseScaleX, originalScale.y, originalScale.z);
        }
        else if (diffX < 0)
        {
            transform.localScale = new Vector3(-baseScaleX, originalScale.y, originalScale.z);
        }
    }

    void SpawnBoomerang(Vector3 forward, Vector3 side)
    {
        GameObject obj = Instantiate(boomerangPrefab, transform.position, Quaternion.identity);
        BoomerangProjectile bp = obj.GetComponent<BoomerangProjectile>();
        if (bp != null)
        {
            bp.Setup(transform.position, forward, side, boomerangDistance, boomerangWidth, boomerangDuration);
        }
    }

    void SpawnSlash(GameObject prefab, float speed, bool forceRight = false, bool invert = false)
    {
        if (prefab == null) return;

        float directionSign = Mathf.Sign(transform.localScale.x);
        float visualDir = directionSign;
        if (flipSlashFace) visualDir *= -1;

        Vector3 spawnPos = transform.position + new Vector3(visualDir * slashSpawnOffset, 0, 0);

        float moveDirSign = directionSign;
        if (reverseStepDirection) moveDirSign *= -1;
        if (invert) moveDirSign *= -1;

        GameObject slash = Instantiate(prefab, spawnPos, Quaternion.identity);
        SlashEffect se = slash.GetComponent<SlashEffect>();

        if (se != null)
        {
            Vector3 moveDir;

            if (forceRight)
            {
                moveDir = Vector3.right;
            }
            else
            {
                moveDir = new Vector3(moveDirSign, 0, 0);
            }

            se.SetupMove(moveDir, speed);
        }
    }

    int DecideNextPattern()
    {
        if (banTurnCount > 0)
        {
            banTurnCount--;
            if (banTurnCount == 0)
            {
                bannedPattern = -1;
            }
        }

        List<int> availablePatterns = new List<int>();

        for (int i = 1; i <= 4; i++)
        {
            if (i != bannedPattern)
            {
                availablePatterns.Add(i);
            }
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

        Debug.Log($"次の攻撃: パターン{selectedPattern} (禁止中: {bannedPattern}, 残り{banTurnCount}回)");
        return selectedPattern;
    }
}