using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour {
    public static CameraManager Instance;
    
    static HelicopterController.Directions directions;
    HelicopterController.Direction _currentCameraFacingDirection;
    public HelicopterController.Direction currentCameraFacingDirection { get=>_currentCameraFacingDirection; }

    [SerializeField] Camera mainCamera;
    [SerializeField] CinemachineVirtualCamera activeCinemachineCamera;
    [SerializeField] float maxZoomOut = 50, maxZoomIn = 10, zoomTime = 0.5f, zoomMultiplier = 10f;
    [SerializeField] float rotationTime = 0.5f;
    [SerializeField] AnimationCurve cameraMovementCurve;
    IEnumerator zoomCoroutine, rotationCoroutine;

    InputManager inputManager;

    public event Action<HelicopterController.Direction> OnFacingChanged;
    
    private void Awake() {
        HelicopterController.Direction N, S, E, W;
        #region Directions Initialization
        N = new HelicopterController.Direction(
            new Vector3(-1, 0, -1),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, -1),
            new Vector3(-1, 0, 1)
        );
        S = new HelicopterController.Direction(
            new Vector3(1, 0, 1),
            new Vector3(-1, 0, -1),
            new Vector3(-1, 0, 1),
            new Vector3(1, 0, -1)
        );
        E = new HelicopterController.Direction(
            new Vector3(1, 0, -1),
            new Vector3(-1, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(-1, 0, -1)
        );
        W = new HelicopterController.Direction(
            new Vector3(-1, 0, 1),
            new Vector3(1, 0, -1),
            new Vector3(-1, 0, -1),
            new Vector3(1, 0, 1)
        );
        #endregion
        directions = new HelicopterController.Directions(N, S, E, W);
        _currentCameraFacingDirection = S;
        Instance = this;
    }
    
    private void Start() {
        mainCamera = mainCamera ? mainCamera : Camera.main;
        activeCinemachineCamera = activeCinemachineCamera ? activeCinemachineCamera : mainCamera.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
        cameraMovementCurve = cameraMovementCurve != null ? cameraMovementCurve : AnimationCurve.EaseInOut(0,0,1,1);
        inputManager = inputManager ? inputManager : InputManager.Instance;
        inputManager.OnTiltLeft += ToLeft;
        inputManager.OnTiltRight += ToRight;
        inputManager.OnScroll += Scroll;
    }

    void Scroll(float scroll) {
        float oldZoom = activeCinemachineCamera.m_Lens.OrthographicSize;
        float newZoom = activeCinemachineCamera.m_Lens.OrthographicSize - scroll * zoomMultiplier;
        if(oldZoom != newZoom) {
            if(zoomCoroutine!=null) StopCoroutine(zoomCoroutine);
            zoomCoroutine = Zoom(oldZoom, newZoom);
            StartCoroutine(zoomCoroutine);
        }
    }

    IEnumerator Zoom(float oldVal, float newVal) {
        float i = 0;
        while(i < zoomTime) {
            activeCinemachineCamera.m_Lens.OrthographicSize = Mathf.Lerp(oldVal, newVal, i/zoomTime);
            if(activeCinemachineCamera.m_Lens.OrthographicSize > maxZoomOut)
                activeCinemachineCamera.m_Lens.OrthographicSize = maxZoomOut;
            else if(activeCinemachineCamera.m_Lens.OrthographicSize < maxZoomIn)
                activeCinemachineCamera.m_Lens.OrthographicSize = maxZoomIn;
            i += Time.deltaTime;
            yield return null;
        }
    }

    void ToLeft() {
        if(rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        rotationCoroutine = RotateCamera(directions.GetLeftOf(_currentCameraFacingDirection));
        StartCoroutine(rotationCoroutine);
    }

    void ToRight() {
        if(rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        rotationCoroutine = RotateCamera(directions.GetRightOf(_currentCameraFacingDirection));
        StartCoroutine(rotationCoroutine);
    }

    IEnumerator RotateCamera(HelicopterController.Direction toDirection) {
        CinemachineOrbitalTransposer transposer = activeCinemachineCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();

        float bias = transposer.m_Heading.m_Bias;
        float targetBias = GetBiasOfFacing(toDirection);
        if(bias != 180) {
            if((targetBias < 0 && bias > 0) || (targetBias > 0 && bias < 0))
                targetBias *= -1;
        } else {
            if(targetBias < 0)
                bias *= -1;
        }

        float i = 0;
        while (i < rotationTime) {
            i += Time.deltaTime;
            transposer.m_Heading.m_Bias = Mathf.Lerp(bias, targetBias, cameraMovementCurve.Evaluate(i/rotationTime));
            if(transposer.m_Heading.m_Bias >= 180 || transposer.m_Heading.m_Bias <= -180)
                transposer.m_Heading.m_Bias = 180;
            yield return null;
        }
        _currentCameraFacingDirection = toDirection;
        OnFacingChanged?.Invoke(_currentCameraFacingDirection);
    }

    static float GetBiasOfFacing(HelicopterController.Direction d) {
        if(d.Equals(directions.S)) return 0;
        else if(d.Equals(directions.W)) return -90;
        else if(d.Equals(directions.E)) return 90;
        else return 180;
    }
}
