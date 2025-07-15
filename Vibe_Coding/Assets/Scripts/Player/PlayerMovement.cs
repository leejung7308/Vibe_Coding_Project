using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public Transform cameraTransform; // 3인칭 카메라 Transform
    public Transform playerBody; // 플레이어 몸통(1인칭 이동 기준)

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 카메라의 Y축(수평) 회전만 반영
        Vector3 move;
        if (playerBody != null)
        {
            // 1인칭: 플레이어 몸통 기준
            Vector3 bodyForward = playerBody.forward;
            Vector3 bodyRight = playerBody.right;
            bodyForward.y = 0f;
            bodyRight.y = 0f;
            bodyForward.Normalize();
            bodyRight.Normalize();
            move = bodyRight * x + bodyForward * z;
        }
        else
        {
            // 3인칭: 카메라 기준
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            move = camRight * x + camForward * z;
        }

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
} 