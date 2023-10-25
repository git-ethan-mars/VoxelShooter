using UnityEngine;

namespace UI
{
    public class BillBoard : MonoBehaviour
    {
        private Transform _cameraTransform;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + _cameraTransform.rotation * Vector3.forward,
                _cameraTransform.rotation * Vector3.up);
        }
    }
}