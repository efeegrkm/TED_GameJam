using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraStateController : MonoBehaviour
{
    public static CameraStateController Instance { get; private set; }

    private CinemachineInputAxisController cinemachineInput;

    [Header("FOV Ayarlarý")]
    [Tooltip("Cinemachine CAM")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [Tooltip("FOV lerp time")]
    [SerializeField] private float transitionDuration = 1.5f;

    private float originalFOV;
    private Coroutine fovCoroutine;
    private Coroutine orbitCoroutine;

    [Header("Orbiting methot settings")]
    [Tooltip("Orbit target")]
    [SerializeField] private GameObject orbitTarget;
    [Tooltip("Orbit duration")]
    [SerializeField] private float orbitDuration = 8f;
    [SerializeField] private float orbitSpeed = 60f;
    [SerializeField] private float targetVerticalAngle = 45;
    public CinemachineOrbitalFollow orbitalFollow;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        cinemachineInput = GetComponent<CinemachineInputAxisController>();

        if (cinemachineCamera != null)
        {
            originalFOV = cinemachineCamera.Lens.FieldOfView;
        }
    }

    public void IncreaseFOVTo(float targetFOV)
    {
        if (fovCoroutine != null) StopCoroutine(fovCoroutine);
        fovCoroutine = StartCoroutine(ChangeFOVRoutine(targetFOV));
    }

    public void ResetFOV()
    {
        if (fovCoroutine != null) StopCoroutine(fovCoroutine);
        fovCoroutine = StartCoroutine(ChangeFOVRoutine(originalFOV));
    }

    private IEnumerator ChangeFOVRoutine(float targetFOV)
    {
        if (cinemachineCamera == null) yield break;
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / transitionDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            var lens = cinemachineCamera.Lens;
            lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            cinemachineCamera.Lens = lens;

            yield return null;
        }
        var finalLens = cinemachineCamera.Lens;
        finalLens.FieldOfView = targetFOV;
        cinemachineCamera.Lens = finalLens;
    }

    private void orbitTest()
    {
        StartOrbitSequence(orbitTarget, orbitDuration, orbitSpeed, targetVerticalAngle);
    }

    public void StartOrbitSequence(GameObject target, float duration, float rotationSpeed, float targetVerticalAngle)
    {
        if (orbitCoroutine != null) StopCoroutine(orbitCoroutine);
        orbitCoroutine = StartCoroutine(OrbitRoutine(target, duration, rotationSpeed, targetVerticalAngle));
    }

    public void StartOrbitSequence(GameObject target)
    {
        if (orbitCoroutine != null) StopCoroutine(orbitCoroutine);
        orbitCoroutine = StartCoroutine(OrbitRoutine(target, orbitDuration, orbitSpeed, targetVerticalAngle));
    }

    private IEnumerator OrbitRoutine(GameObject target, float duration, float speed, float vAngle)
    {
        if (cinemachineCamera == null || orbitalFollow == null) yield break;
        if (cinemachineInput != null) cinemachineInput.enabled = false;

        Transform originalFollow = cinemachineCamera.Target.TrackingTarget;
        Transform originalLookAt = cinemachineCamera.Target.LookAtTarget;

        cinemachineCamera.Target.TrackingTarget = target.transform;
        cinemachineCamera.Target.LookAtTarget = target.transform;

        float elapsed = 0f;
        float startVAngle = orbitalFollow.VerticalAxis.Value;

        float verticalTransitionDuration = 1.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            orbitalFollow.HorizontalAxis.Value += speed * Time.deltaTime;

            if (elapsed < verticalTransitionDuration)
            {
                float t = elapsed / verticalTransitionDuration;
                orbitalFollow.VerticalAxis.Value = Mathf.Lerp(startVAngle, vAngle, Mathf.SmoothStep(0, 1, t));
            }
            else
            {
                orbitalFollow.VerticalAxis.Value = vAngle;
            }

            yield return null;
        }

        cinemachineCamera.Target.TrackingTarget = originalFollow;
        cinemachineCamera.Target.LookAtTarget = originalLookAt;

        yield return new WaitForSeconds(1.5f);

        if (cinemachineInput != null) cinemachineInput.enabled = true;
    }
}