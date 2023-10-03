using Infrastructure.Services.Input;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AnimationPlayer : MonoBehaviour
{
    private const float Gravity = -30f;

    [SerializeField]
    private Transform headPivot;

    [SerializeField]
    private float speed;

    [SerializeField]
    private float jumpMultiplier;

    private IInputService _inputService;
    private Camera _camera;
    private CharacterController _characterController;
    private float _jumpSpeed;

    private void Awake()
    {
        _inputService = new StandaloneInputService();
        _characterController = GetComponent<CharacterController>();
        _camera = Camera.main;
    }

    void Update()
    {
        HandleHeadRotation();
        HandleMovement();
    }

    private void HandleHeadRotation()
    {
        var mousePos2D = Input.mousePosition;
        var screenToCameraDistance = _camera.nearClipPlane + 1;
        var mousePosNearClipPlane = new Vector3(mousePos2D.x, mousePos2D.y, screenToCameraDistance);
        var worldPointPos = _camera.ScreenToWorldPoint(mousePosNearClipPlane);
        headPivot.LookAt(worldPointPos);
    }

    private void HandleMovement()
    {
        var axis = _inputService.Axis;
        var movementDirection = (axis.x * transform.forward + axis.y * transform.right).normalized;
        if (_characterController.isGrounded)
        {
            _jumpSpeed = 0;
            if (_inputService.IsJumpButtonDown())
            {
                _jumpSpeed = jumpMultiplier;
            }
        }

        _jumpSpeed += Gravity * Time.deltaTime;
        var direction = new Vector3(movementDirection.x * speed * Time.deltaTime,
            _jumpSpeed * Time.deltaTime,
            movementDirection.z * Time.deltaTime * speed);
        _characterController.Move(direction);
    }
}