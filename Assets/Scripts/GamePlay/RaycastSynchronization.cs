using Infrastructure.Factory;
using Mirror;
using UnityEngine;

namespace GamePlay
{
    public class RaycastSynchronization : NetworkBehaviour
    {
        private IGameFactory _gameFactory;
        
        public void Construct(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
        }
        
        [Command]
        public void ApplyRaycast(Vector3 origin, Vector3 direction, float range, int damage)
        {
            var ray = new Ray(origin, direction);
            var raycastResult = Physics.Raycast(ray, out var rayHit, range);
            if (raycastResult)
            {
                var playerHealth = rayHit.collider.transform.parent.gameObject.GetComponent<HealthSystem>();
                if (playerHealth)
                {
                    playerHealth.Health -= damage;
                    if (playerHealth.Health <= 0)
                    {
                        Debug.Log("DEAD");
                    }
                }

                _gameFactory.CreateBulletHole(rayHit.point, Quaternion.Euler(rayHit.normal.y * -90,
                    rayHit.normal.x * 90 + rayHit.normal.z * -180, 0));
            }
        }
    }
}