using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public enum CameraState { Normal, Aim, Dialogue }
    public CameraState state = CameraState.Normal;

    public Transform target; // 따라갈 플레이어
    public Vector3 normalOffset = new Vector3(0, 2, -4);
    public Vector3 aimOffset = new Vector3(0, 1.5f, -2.5f);
    public Vector3 dialogueOffset = new Vector3(0, 1.8f, -3f);

    public float followSpeed = 10f;
    public float rotationSpeed = 5f;
    public float minYAngle = -30f;
    public float maxYAngle = 70f;
    public float collisionRadius = 0.2f;
    public LayerMask collisionMask;

    private float currentYaw = 0f;
    private float currentPitch = 20f;
    private Vector3 currentOffset;
    private Vector3 desiredPosition;

    void Start()
    {
        if (target == null)
            Debug.LogWarning("ThirdPersonCamera: target이 할당되지 않았습니다.");
        currentOffset = normalOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 상태별 오프셋 적용
        switch (state)
        {
            case CameraState.Normal: currentOffset = normalOffset; break;
            case CameraState.Aim: currentOffset = aimOffset; break;
            case CameraState.Dialogue: currentOffset = dialogueOffset; break;
        }

        // 2. 마우스 입력(오비탈 회전)
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);

        // 3. 목표 위치 계산
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 targetOffset = rotation * currentOffset;
        desiredPosition = target.position + targetOffset;

        // 4. 충돌 감지(카메라 클리핑 방지)
        RaycastHit hit;
        Vector3 dir = (desiredPosition - target.position).normalized;
        float maxDist = currentOffset.magnitude;
        Vector3 finalPosition = desiredPosition;
        if (Physics.SphereCast(target.position, collisionRadius, dir, out hit, maxDist, collisionMask))
        {
            finalPosition = target.position + dir * (hit.distance - collisionRadius);
        }

        // 5. 부드러운 이동
        transform.position = Vector3.Lerp(transform.position, finalPosition, followSpeed * Time.deltaTime);

        // 6. 타겟 바라보기
        transform.LookAt(target.position + Vector3.up * currentOffset.y * 0.5f);

        // 7. 상태 전환(예시: 우클릭 시 조준, 대화 등)
        if (Input.GetMouseButton(1)) state = CameraState.Aim;
        else if (Input.GetKey(KeyCode.C)) state = CameraState.Dialogue;
        else state = CameraState.Normal;

        // 8. 카메라 흔들림(기본 구조)
        // transform.position += Random.insideUnitSphere * shakeAmount;
    }
} 