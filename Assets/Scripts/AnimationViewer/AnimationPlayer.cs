using Infrastructure.Services.Input;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AnimationPlayer : MonoBehaviour
{
    private const float Gravity = -30f;

    [SerializeField]
    private float speed;

    [SerializeField]
    private float jumpMultiplier;

    private IInputService _inputService;
    private CharacterController _characterController;
    private float _jumpSpeed;

    private void Awake()
    {
        _inputService = new StandaloneInputService();
        _characterController = GetComponent<CharacterController>();
    }

    void Update()
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