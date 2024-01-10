using UnityEngine;

namespace Entities
{
    public interface IExplosive
    {
        Vector3 Position { get; }
        void Explode();
    }
}