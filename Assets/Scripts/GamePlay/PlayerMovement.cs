using Mirror;
using UnityEngine;

namespace GamePlay
{
    public class PlayerMovement : NetworkBehaviour
    {
        private Rigidbody Rigidbody { get; set; }
        [SerializeField] private float speed;

        private void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            if (!isOwned) return;
            var verticalInput = 0f;
            if (Input.GetKey(KeyCode.W))
                verticalInput += 1;
            if (Input.GetKey(KeyCode.S))
                verticalInput -= 1;
            var horizontalInput = 0f;
            if (Input.GetKey(KeyCode.D))
            {
                horizontalInput += 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                horizontalInput -= 1;
            }

            var moveDirection = verticalInput * transform.forward + horizontalInput * transform.right;
            Rigidbody.velocity = moveDirection * speed;
            if (Input.GetKey(KeyCode.Space))
                Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, speed, Rigidbody.velocity.z);
            if (Input.GetKey(KeyCode.LeftShift))
                Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, -speed, Rigidbody.velocity.z);
        }
    }
}