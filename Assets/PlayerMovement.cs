using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody Rigidbody { get; set; }
    [SerializeField] private float speed;

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.freezeRotation = true;
    }
    
    private void Update()
    {
        var verticalInput = Input.GetAxis("Vertical");
        var horizontalInput = Input.GetAxis("Horizontal");
        var moveDirection = verticalInput * transform.forward + horizontalInput * transform.right;
        Rigidbody.velocity = moveDirection * speed;
        if (Input.GetKey(KeyCode.Space))
            Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, speed, Rigidbody.velocity.z);
        if (Input.GetKey(KeyCode.LeftShift))
            Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, -speed, Rigidbody.velocity.z);
    }
}
