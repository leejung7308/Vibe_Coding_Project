using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float followSmoothTime = 0.1f;

    [Header("Orbit")]
    public float orbitSensitivity = 3f;
    public float minYAngle = -30f;
    public float maxYAngle = 70f;

    public enum CameraState { Normal, Dialogue }
    public CameraState state = CameraState.Normal;

    [Header("State Offsets")]
    public Vector3 normalOffset = new Vector3(0, 2, -5);
    public Vector3 dialogueOffset = new Vector3(0, 1.8f, -2.5f);
    public float normalSensitivity = 3f;
    public float dialogueSensitivity = 2f;

    private Vector3 currentVelocity;
    private float yaw;
    private float pitch;
    private Renderer playerRenderer;
    private Material originalMaterial;
    private Material transparentMaterial;
    private bool isOccluded = false;
    private Vector3 currentOffset;
    private float currentSensitivity;

    // 카메라 흔들림 변수
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.2f;
    private float shakeDamping = 1.0f;
    private Vector3 shakeOffset = Vector3.zero;

    void Awake()
    {
        // 플레이어 Renderer 및 머티리얼 캐싱 (필요시 직접 할당 가능)
        if (target != null)
        {
            playerRenderer = target.GetComponentInChildren<Renderer>();
            if (playerRenderer != null)
            {
                originalMaterial = playerRenderer.material;
                // 투명 머티리얼은 프로젝트에 미리 준비하거나, 기존 머티리얼을 복제해 알파만 낮추는 방식 사용
                transparentMaterial = new Material(originalMaterial);
                transparentMaterial.SetFloat("_Mode", 2); // Fade 모드
                Color c = transparentMaterial.color;
                c.a = 0.3f;
                transparentMaterial.color = c;
                transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                transparentMaterial.SetInt("_ZWrite", 0);
                transparentMaterial.DisableKeyword("_ALPHATEST_ON");
                transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
                transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                transparentMaterial.renderQueue = 3000;
            }
        }
    }

    void Start()
    {
        currentOffset = normalOffset;
        currentSensitivity = normalSensitivity;
    }

    void LateUpdate()
    {
        // 상태 전환 입력
        if (Input.GetKey(KeyCode.C)) state = CameraState.Dialogue;
        else state = CameraState.Normal;

        // 상태별 목표값 계산
        Vector3 targetOffset = normalOffset;
        float targetSensitivity = normalSensitivity;
        switch (state)
        {
            case CameraState.Dialogue:
                targetOffset = dialogueOffset;
                targetSensitivity = dialogueSensitivity;
                break;
            case CameraState.Normal:
            default:
                targetOffset = normalOffset;
                targetSensitivity = normalSensitivity;
                break;
        }

        // 오프셋/민감도 부드럽게 전환
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * 8f);
        currentSensitivity = Mathf.Lerp(currentSensitivity, targetSensitivity, Time.deltaTime * 8f);

        // 마우스 입력으로 오비탈 회전
        yaw += Input.GetAxis("Mouse X") * currentSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * currentSensitivity;
        pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

        // 회전 적용
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 목표 위치 계산 (상태별 오프셋)
        Vector3 desiredPosition = target.position + rotation * currentOffset;

        // 충돌 감지 (SphereCast)
        Vector3 direction = (desiredPosition - target.position).normalized;
        float maxDistance = currentOffset.magnitude;
        float minDistance = 0.5f;
        RaycastHit hit;
        bool occluded = false;
        if (Physics.SphereCast(target.position, 0.2f, direction, out hit, maxDistance))
        {
            float hitDist = Mathf.Max(hit.distance, minDistance);
            desiredPosition = target.position + direction * hitDist;
            if (hit.transform != null && hit.transform != target && playerRenderer != null)
            {
                occluded = true;
            }
        }
        if (playerRenderer != null)
        {
            if (occluded && !isOccluded)
            {
                playerRenderer.material = transparentMaterial;
                isOccluded = true;
            }
            else if (!occluded && isOccluded)
            {
                playerRenderer.material = originalMaterial;
                isOccluded = false;
            }
        }
        // 스무스 이동
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);
        // LookAt 타겟(일반/대화 상태)
        Vector3 lookTarget = target.position + Vector3.up * currentOffset.y;
        transform.LookAt(lookTarget);

        // === 디버그/테스트용 ===
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 테스트용 핫키(L키)로 흔들림 트리거
        if (Input.GetKeyDown(KeyCode.L))
        {
            Shake(0.1f, 0.4f, 2.0f); // 강도, 지속시간, 감쇠
        }
#endif
        // === 디버그/테스트용 끝 ===

        // 카메라 흔들림 적용
        if (shakeDuration > 0f)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime * shakeDamping;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
        transform.position += shakeOffset;
    }

    // 흔들림 트리거용 public 메서드
    // 사용법: Shake(강도, 지속시간, 감쇠);
    // 예시: Shake(0.3f, 0.4f, 2.0f); // 강도, 지속시간, 감쇠(감쇠는 생략 가능)
    public void Shake(float magnitude, float duration, float damping = 1.0f)
    {
        shakeMagnitude = magnitude;
        shakeDuration = duration;
        shakeDamping = damping;
    }
} 