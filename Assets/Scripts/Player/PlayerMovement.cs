using UnityEngine;
using Photon.Pun;

namespace CyberStrike.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviourPun
    {
        [Header("Movement Stats")]
        public float moveSpeed = 6f;
        public float jumpHeight = 2.5f;
        public float gravity = -20f;

        [Header("Mobile Controls")]
        public VariableJoystick joystick; // Reference to virtual joystick
        
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            
            // Disable movement if not ours
            if (!photonView.IsMine)
            {
                // Optionally disable camera holder or other components
                return;
            }

            // Find joystick in UI if not assigned
            if (joystick == null)
                joystick = FindObjectOfType<VariableJoystick>();
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Joystick Input
            float x = joystick ? joystick.Horizontal : Input.GetAxis("Horizontal");
            float z = joystick ? joystick.Vertical : Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * moveSpeed * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        public void Jump()
        {
            if (photonView.IsMine && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
    }
}
