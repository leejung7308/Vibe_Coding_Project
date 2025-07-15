using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraMode { ThirdPerson, FirstPerson }
    public CameraMode mode = CameraMode.ThirdPerson;

    public Transform player; // Player Transform
    public Transform thirdPersonPivot; // 3인칭 기준점(플레이어 뒤)
    public Transform firstPersonPivot; // 1인칭 기준점(플레이어 머리)
    public float switchSmooth = 10f;

    private ThirdPersonCamera thirdPersonCamera;
    private PlayerLook playerLook;
    private Camera cam;

    void Awake()
    {
        thirdPersonCamera = GetComponent<ThirdPersonCamera>();
        playerLook = GetComponent<PlayerLook>();
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        SetCameraMode(mode);
    }

    void Update()
    {
        // 전환 키(예: V)
        if (Input.GetKeyDown(KeyCode.V))
        {
            mode = (mode == CameraMode.ThirdPerson) ? CameraMode.FirstPerson : CameraMode.ThirdPerson;
            SetCameraMode(mode);
        }

        // 카메라 위치/회전 스무스 전환
        if (mode == CameraMode.ThirdPerson)
        {
            if (thirdPersonPivot != null)
            {
                transform.position = Vector3.Lerp(transform.position, thirdPersonPivot.position, Time.deltaTime * switchSmooth);
                transform.rotation = Quaternion.Slerp(transform.rotation, thirdPersonPivot.rotation, Time.deltaTime * switchSmooth);
            }
        }
        else
        {
            if (firstPersonPivot != null && player != null && playerLook != null)
            {
                // 위치는 머리 피벗에 맞춤
                transform.position = Vector3.Lerp(transform.position, firstPersonPivot.position, Time.deltaTime * switchSmooth);
                // 회전: 플레이어의 Y축(좌우) + 카메라의 X축(상하)
                float xRot = playerLook.CurrentXRotation; // PlayerLook에서 public 프로퍼티로 노출 필요
                Quaternion targetRot = Quaternion.Euler(xRot, player.eulerAngles.y, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * switchSmooth);
            }
        }
    }

    void SetCameraMode(CameraMode newMode)
    {
        if (newMode == CameraMode.ThirdPerson)
        {
            if (thirdPersonCamera != null) thirdPersonCamera.enabled = true;
            if (playerLook != null) playerLook.enabled = false;
            // 3인칭: 카메라를 thirdPersonPivot(혹은 루트)로 이동
            if (thirdPersonPivot != null)
            {
                transform.SetParent(null); // 씬 루트로 이동(필요시 thirdPersonPivot으로 변경)
            }
        }
        else
        {
            if (thirdPersonCamera != null) thirdPersonCamera.enabled = false;
            if (playerLook != null) playerLook.enabled = true;
            // 1인칭: 카메라를 firstPersonPivot의 자식으로 이동
            if (firstPersonPivot != null)
            {
                transform.SetParent(firstPersonPivot, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
    }
} 