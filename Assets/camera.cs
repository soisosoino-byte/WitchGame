using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    // ★変更: Camera型だとHolderが入らないので、Transform型に変更しました★
    [Tooltip("ここに CameraHolder をドラッグ＆ドロップしてください")]
    public Transform cameraParent;

    // ★追加: ズーム機能のために、内部で本物のカメラを取得する変数★
    private Camera actualCameraComponent;

    public float followStopXRight = 10.0f;
    public float followStopXLeft = -10.0f;

    public float followResumeXRight = 9.0f;
    public float followResumeXLeft = -9.0f;

    public float cameraFixedY = 0f;
    public float cameraFixedZ = -10f;

    public bool useOpeningCamera = true;
    public Vector3 openingCameraPosition = new Vector3(0f, 0f, -10f);
    public float openingCameraDuration = 5.0f;

    public float transitionToFollowDuration = 1.0f;
    public float normalFollowZoomSize = 5.0f;
    public float openingCameraSpecificZoomSize = 3.0f;
    private bool isOpeningCameraActive = false;

    void Start()
    {
        // ★変更: Camera.mainの親を探す、または設定されたParentを使う★
        if (cameraParent == null)
        {
            if (Camera.main != null)
            {
                // MainCameraに親(Holder)がいるならそれを使う
                if (Camera.main.transform.parent != null)
                {
                    cameraParent = Camera.main.transform.parent;
                }
                else
                {
                    // 親がいない場合（Holderを作っていない場合）はカメラ自身を動かす
                    cameraParent = Camera.main.transform;
                }
            }
        }

        if (cameraParent == null)
        {
            Debug.LogError("カメラ（またはCameraHolder）が見つかりません！Inspectorで設定してください。", this);
            return;
        }

        // ★追加: ズーム機能のために、子要素にある本物のカメラコンポーネントを探す★
        actualCameraComponent = cameraParent.GetComponentInChildren<Camera>();
        if (actualCameraComponent == null)
        {
            Debug.LogError("CameraHolderの中にカメラ（Cameraコンポーネント）が入っていません！", this);
        }

        // 初期位置の設定（Parentを動かす）
        Vector3 camPos = cameraParent.position;
        camPos.z = cameraFixedZ;
        cameraParent.position = camPos;

        // 初期ズームの設定（Childのカメラを操作）
        if (actualCameraComponent != null)
        {
            actualCameraComponent.orthographicSize = normalFollowZoomSize;
        }

        if (useOpeningCamera)
        {
            StartCoroutine(OpeningCameraSequence());
        }
    }

    void LateUpdate()
    {
        if (cameraParent == null) return;

        if (isOpeningCameraActive) return;

        float characterX = this.transform.position.x;
        float currentCameraX = cameraParent.position.x; // ★修正: parentの位置を見る

        float newCameraX = characterX;

        // 右端の固定ロジック
        if (characterX > followStopXRight)
        {
            newCameraX = followStopXRight;
        }
        // 左端の固定ロジック
        else if (characterX < followStopXLeft)
        {
            newCameraX = followStopXLeft;
        }
        // 固定解除ロジック
        else if (currentCameraX == followStopXRight && characterX < followResumeXRight)
        {
            newCameraX = characterX;
        }
        else if (currentCameraX == followStopXLeft && characterX > followResumeXLeft)
        {
            newCameraX = characterX;
        }

        // ★修正: 親オブジェクト(Holder)を移動させる★
        cameraParent.position = new Vector3(newCameraX, cameraFixedY, cameraFixedZ);
    }

    IEnumerator OpeningCameraSequence()
    {
        isOpeningCameraActive = true;

        if (cameraParent == null)
        {
            isOpeningCameraActive = false;
            yield break;
        }

        Vector3 startCameraPos = new Vector3(
            openingCameraPosition.x,
            openingCameraPosition.y,
            cameraFixedZ
        );

        // ★修正: 親を移動★
        cameraParent.position = startCameraPos;

        // ★修正: 子（カメラ）をズーム★
        if (actualCameraComponent != null)
        {
            actualCameraComponent.orthographicSize = openingCameraSpecificZoomSize;
        }

        Debug.Log("オープニングカメラ演出開始...");

        yield return new WaitForSeconds(openingCameraDuration);

        Debug.Log("オープニングカメラ演出終了。移動開始...");

        float transitionTimer = 0f;
        Vector3 initialTransitionPos = cameraParent.position; // ★修正

        // ★修正: ズーム初期値取得★
        float initialTransitionOrthoSize = normalFollowZoomSize;
        if (actualCameraComponent != null) initialTransitionOrthoSize = actualCameraComponent.orthographicSize;

        float targetFollowX = this.transform.position.x;
        if (targetFollowX > followStopXRight) targetFollowX = followStopXRight;
        else if (targetFollowX < followStopXLeft) targetFollowX = followStopXLeft;

        Vector3 targetFollowPos = new Vector3(targetFollowX, cameraFixedY, cameraFixedZ);

        while (transitionTimer < transitionToFollowDuration)
        {
            // ★修正: 親を移動★
            cameraParent.position = Vector3.Lerp(initialTransitionPos, targetFollowPos, transitionTimer / transitionToFollowDuration);

            // ★修正: 子（カメラ）をズーム★
            if (actualCameraComponent != null)
            {
                actualCameraComponent.orthographicSize = Mathf.Lerp(initialTransitionOrthoSize, normalFollowZoomSize, transitionTimer / transitionToFollowDuration);
            }

            transitionTimer += Time.deltaTime;
            yield return null;
        }

        // 完了
        cameraParent.position = targetFollowPos;
        if (actualCameraComponent != null)
        {
            actualCameraComponent.orthographicSize = normalFollowZoomSize;
        }

        isOpeningCameraActive = false;
    }
}