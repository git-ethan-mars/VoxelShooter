using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IEntityFactory : IService
    {
        GameObject CreateTnt(Vector3 position, Quaternion rotation);
        
        GameObject CreateGrenade(Vector3 position, Quaternion rotation);
    }
}