using UnityEngine;

namespace GamePlay.Entities
{
	public class SpawningGrenade : Entity
	{
		[SerializeField] 
		private Rigidbody rb;
		
		public void Construct()
		{
			
		}

		public void Throw(Vector3 direction, float throwForce)
		{
			rb.AddForce(direction * throwForce);
		}
	}
}