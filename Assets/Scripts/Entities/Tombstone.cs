using UnityEngine;

namespace Entities
{
    public class Tombstone : PushableObject
    {
        public override void Push()
        {
            transform.position += Vector3.up;
        }

        public override void Fall()
        {
        }
    }
}