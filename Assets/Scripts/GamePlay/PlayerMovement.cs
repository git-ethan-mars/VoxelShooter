using System;
using Core;
using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody Rigidbody { get; set; }
    private bool _onGround;
    [SerializeField] private float speed;
    private float _previousFallingSpeed;
    private bool _isFell;
    private bool _isJumpButtonPressed;
    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.freezeRotation = true;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
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

        if (verticalInput != 0 && horizontalInput != 0)
        {
            verticalInput /= (float)Math.Sqrt(2);
            horizontalInput /= (float)Math.Sqrt(2);
        }

        var playerTransform = transform;
        var moveDirection = (verticalInput * playerTransform.forward + horizontalInput * playerTransform.right) 
                            * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _isJumpButtonPressed = true;
        }

        var newVelocity = new Vector3(moveDirection.x * speed, Rigidbody.velocity.y, moveDirection.z * speed);
        Rigidbody.velocity = newVelocity;
    }

    private void FixedUpdate()
    {
        if (_previousFallingSpeed == Rigidbody.velocity.y)
        {
            _isFell = true;
            if (_isJumpButtonPressed && _onGround)
            {
                Rigidbody.AddForce(Vector3.up * 7, ForceMode.VelocityChange);
            }
        }
        else
        {
            _isFell = false;
        }
        _isJumpButtonPressed = false;
        _previousFallingSpeed = Rigidbody.velocity.y;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.GetComponent<ChunkRenderer>() && _isFell)
        {
            _onGround = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.GetComponent<ChunkRenderer>())
        {
            _onGround = false;
        }
    }
}