using Infrastructure.Services.Input;
using PlayerLogic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AnimationPlayer : MonoBehaviour
{
    private const float Gravity = -30f;
    private const float NormalSpeed = 1f;
    private const float SneakingSpeed = 0.7f;
    private const float SquattingSpeed = 0.4f;
    private const float SquattingSize = 0.5f;
    private float _stayingHeight;
    private float _squattingHeight;
    private Vector3 _stayingColliderCenter;
    private Vector3 _squattingColliderCenter;

    [SerializeField]
    private Transform headPivot;

    [SerializeField]
    private float speed;

    [SerializeField]
    private float jumpMultiplier;


    private PlayerLegAnimator _legAnimator;
    private IInputService _inputService;
    private Camera _camera;
    private CharacterController _characterController;
    private float _jumpSpeed;
    private float _speedModifier = 1f;

    private void Awake()
    {
        _inputService = new StandaloneInputService();
        _characterController = GetComponent<CharacterController>();
        _legAnimator = new PlayerLegAnimator(GetComponent<Animator>());
        _camera = Camera.main;
        _stayingHeight = _characterController.height;
        _squattingHeight = _stayingHeight - SquattingSize;
        _stayingColliderCenter = _characterController.center;
        _squattingColliderCenter = new Vector3(_stayingColliderCenter.x,
            _stayingColliderCenter.y - SquattingSize / 2, _stayingColliderCenter.z);
    }

    void Update()
    {
        HandleHeadRotation();
        HandleMovement();
        HandleSneaking();
        HandleSquatting();
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
        if (Mathf.Abs(_characterController.velocity.magnitude) < Constants.Epsilon)
        {
            _legAnimator.PlayIdle();
        }
        else
        {
            _legAnimator.PlayMove();
        }

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
        var direction = new Vector3(movementDirection.x * speed * _speedModifier * Time.deltaTime,
            _jumpSpeed * Time.deltaTime,
            movementDirection.z * speed * _speedModifier * Time.deltaTime);
        _characterController.Move(direction);
    }

    private void HandleSneaking()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _speedModifier = SneakingSpeed;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _speedModifier = NormalSpeed;
        }
    }

    private void HandleSquatting()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _speedModifier = SquattingSpeed;
            _characterController.height = _squattingHeight;
            _characterController.center = _squattingColliderCenter;
            _legAnimator.PlayCrouch();
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _speedModifier = NormalSpeed;
            _characterController.height = _stayingHeight;
            _characterController.center = _stayingColliderCenter;
            _legAnimator.StopCrouch();
        }
    }
}