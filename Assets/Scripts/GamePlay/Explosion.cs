using UnityEngine;

namespace GamePlay
{
    public class Explosion : MonoBehaviour
    {
        [SerializeField] private float explosionForce;
        [SerializeField] private float explosionRadius;
        [SerializeField] private GameObject slicedBlock;
    
   

        public void Explode()
        {
            foreach (var overlappedCollider in Physics.OverlapSphere(transform.position, explosionRadius))
            {
                if (!overlappedCollider.gameObject.CompareTag("Block"))
                    continue;
                var block = overlappedCollider.gameObject;
                var color = block.GetComponent<MeshRenderer>().material.color;
                Destroy(block);
                var slicedInstantiatedBlock = Instantiate(slicedBlock, block.transform.position, Quaternion.identity);
                foreach (Transform child in slicedInstantiatedBlock.transform)
                {
                    child.gameObject.GetComponent<MeshRenderer>().material.color = color;
                }
            
                foreach (Transform child in slicedInstantiatedBlock.transform)
                {
                    child.gameObject.GetComponent<Rigidbody>()
                        .AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
            
                Destroy(slicedInstantiatedBlock,5);
            }
            Destroy(gameObject);
        }
    }
}