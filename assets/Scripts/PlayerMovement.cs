using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviourPun
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -15f;
    
    [Header("Camera")]
    public Transform playerCamera;
    public float lookSensitivity = 1.5f;
    private float verticalRotation = 0f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // Mobile Input Refs (set via UI buttons/joystick)
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public Vector2 lookInput;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (!photonView.IsMine)
        {
            // Only control local player
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            return;
        }
        
        // Hide cursor if testing on PC
        // Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        ApplyMovement();
        ApplyRotation();
    }

    private void ApplyMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get movement from joystick input
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ApplyRotation()
    {
        // Rotate Player (Horizontal)
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);

        // Rotate Camera (Vertical)
        verticalRotation -= lookInput.y * lookSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
}
