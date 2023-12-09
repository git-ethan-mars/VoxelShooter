using UnityEngine;

public class TransformFollower : MonoBehaviour
{
    public Transform Target { get; set; }

    private void Update()
    {
        if (Target != null)
        {
            transform.position = Target.transform.position;
        }
    }
}